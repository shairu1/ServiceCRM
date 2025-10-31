using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using ServiceCRM.Data;
using ServiceCRM.Models;
using ServiceCRM.Services.Logger;

namespace ServiceCRM.Controllers;

[Authorize]
public class ServiceCentersController : Controller
{
    private readonly ServiceCrmContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IStringLocalizer<ServiceCentersController> _localizer;
    private readonly IActionLogger _logger;

    public ServiceCentersController(
        ServiceCrmContext context,
        UserManager<IdentityUser> userManager,
        IStringLocalizer<ServiceCentersController> localizer,
        IActionLogger logger)
    {
        _context = context;
        _userManager = userManager;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task<IActionResult> Select(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Auth");

        var hasAccess = await _context.UserServiceCenters.AnyAsync(us => us.UserId == user.Id && us.ServiceCenterId == id);
        if (!hasAccess)
        {
            await _logger.LogAsync($"ServiceCentersController.Select : Access denied for user {user.Id} to ServiceCenter {id}");
            return Forbid();
        }

        Response.Cookies.Append(
            "SelectedServiceId",
            id.ToString(),
            new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTimeOffset.UtcNow.AddDays(30)
            });

        await _logger.LogAsync($"ServiceCentersController.Select : User {user.Id} selected ServiceCenter {id}");

        var referer = Request.Headers["Referer"].ToString();
        if (!string.IsNullOrEmpty(referer))
            return Redirect(referer);

        return RedirectToAction("Index", "Home");
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Auth");

        await _logger.LogAsync($"ServiceCentersController.Index : User {user.Id} opened ServiceCenters list");

        var services = await _context.ServiceCenters
            .Include(s => s.Admin)
            .Include(s => s.Members).ThenInclude(m => m.User)
            .Where(s => s.AdminId == user.Id || s.Members.Any(m => m.UserId == user.Id))
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        var model = new ServiceCentersListViewModel
        {
            ServiceCenters = services,
            CurrentUserId = user.Id
        };

        return View(model);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateServiceViewModel serviceCenterViewModel)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Auth");

        if (ModelState.IsValid)
        {
            ServiceCenter serviceCenter = new()
            {
                AdminId = user.Id,
                Name = serviceCenterViewModel.Name,
                OrdersCount = 0,
                CreatedAt = DateTime.UtcNow
            };

            _context.ServiceCenters.Add(serviceCenter);
            await _context.SaveChangesAsync();

            _context.UserServiceCenters.Add(new UserServiceCenter
            {
                UserId = user.Id,
                ServiceCenterId = serviceCenter.Id
            });
            await _context.SaveChangesAsync();

            await _logger.LogAsync($"ServiceCentersController.Create : ServiceCenter '{serviceCenter.Name}' created by user {user.Id}");

            await Select(serviceCenter.Id);

            return RedirectToAction(nameof(Index));
        }
        return View(serviceCenterViewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return RedirectToAction("Login", "Auth");

        var serviceCenter = await _context.ServiceCenters
            .Include(s => s.Members)
            .Include(s => s.Orders)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (serviceCenter == null)
            return NotFound();

        if (serviceCenter.AdminId != user.Id)
        {
            await _logger.LogAsync($"ServiceCentersController.Delete : Forbidden delete attempt by user {user.Id} on ServiceCenter {id}");
            return Forbid();
        }

        _context.Orders.RemoveRange(serviceCenter.Orders);
        _context.UserServiceCenters.RemoveRange(serviceCenter.Members);
        _context.ServiceCenters.Remove(serviceCenter);

        await _context.SaveChangesAsync();

        await _logger.LogAsync($"ServiceCentersController.Delete : ServiceCenter '{serviceCenter.Name}' deleted by user {user.Id}");

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var service = await _context.ServiceCenters
            .Include(s => s.Members)
                .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (service == null) return NotFound();

        var users = service.Members.Select(m => m.User).ToList();

        var model = new EditServiceCenterViewModel
        {
            Id = service.Id,
            Name = service.Name,
            Members = users,
            Admin = service.Admin
        };

        await _logger.LogAsync($"ServiceCentersController.Edit(GET) : User opened edit page for ServiceCenter {id}");

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditServiceCenterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var service = await _context.ServiceCenters
            .Include(s => s.Members)
            .FirstOrDefaultAsync(s => s.Id == model.Id);

        if (service == null) return NotFound();

        service.Name = model.Name;
        await _context.SaveChangesAsync();

        await _logger.LogAsync($"ServiceCentersController.Edit(POST) : ServiceCenter {service.Id} renamed to '{service.Name}'");

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddMember(int serviceCenterId, string username)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return RedirectToAction("Login", "Auth");

        var service = await _context.ServiceCenters
            .FirstOrDefaultAsync(s => s.Id == serviceCenterId);

        if (service == null) return NotFound();

        if (service.AdminId != currentUser.Id)
            return Forbid();

        var userToAdd = await _userManager.FindByNameAsync(username);
        if (userToAdd == null)
        {
            await _logger.LogAsync($"ServiceCentersController.AddMember : User '{username}' not found");
            TempData["ErrorMessage"] = string.Format(_localizer["UserNotFound"], username);
            return RedirectToAction("Edit", new { id = serviceCenterId });
        }

        var alreadyMember = await _context.UserServiceCenters
            .AnyAsync(us => us.UserId == userToAdd.Id && us.ServiceCenterId == serviceCenterId);

        if (alreadyMember)
        {
            await _logger.LogAsync($"ServiceCentersController.AddMember : User '{username}' is already a member of ServiceCenter {serviceCenterId}");
            TempData["ErrorMessage"] = string.Format(_localizer["UserAlreadyMember"], username);
            return RedirectToAction("Edit", new { id = serviceCenterId });
        }

        _context.UserServiceCenters.Add(new UserServiceCenter
        {
            UserId = userToAdd.Id,
            ServiceCenterId = serviceCenterId
        });

        await _context.SaveChangesAsync();

        await _logger.LogAsync($"ServiceCentersController.AddMember : User '{username}' added to ServiceCenter {serviceCenterId} by {currentUser.Id}");

        TempData["SuccessMessage"] = string.Format(_localizer["UserSuccessAdd"], username);
        return RedirectToAction("Edit", new { id = serviceCenterId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Leave(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Auth");

        var service = await _context.ServiceCenters
            .Include(s => s.Members)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (service == null)
            return NotFound();

        if (service.AdminId == user.Id)
        {
            await _logger.LogAsync($"ServiceCentersController.Leave : Admin {user.Id} attempted to leave ServiceCenter {id}");
            TempData["ErrorMessage"] = "Администратор не может покинуть сервисный центр";
            return RedirectToAction("Edit", new { id = service.Id });
        }

        var membership = service.Members.FirstOrDefault(m => m.UserId == user.Id);
        if (membership != null)
        {
            _context.UserServiceCenters.Remove(membership);
            await _context.SaveChangesAsync();

            await _logger.LogAsync($"ServiceCentersController.Leave : User {user.Id} left ServiceCenter {id}");

            TempData["SuccessMessage"] = "Вы успешно покинули сервисный центр";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveMember(int serviceCenterId, string userId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return RedirectToAction("Login", "Auth");

        var service = await _context.ServiceCenters
            .Include(s => s.Members)
            .FirstOrDefaultAsync(s => s.Id == serviceCenterId);

        if (service == null)
            return NotFound();

        if (service.AdminId != currentUser.Id)
        {
            await _logger.LogAsync($"ServiceCentersController.RemoveMember : Forbidden attempt by user {currentUser.Id} to remove from ServiceCenter {serviceCenterId}");
            TempData["ErrorMessage"] = "Только администратор может удалять участников";
            return RedirectToAction("Edit", new { id = serviceCenterId });
        }

        if (currentUser.Id == userId)
        {
            await _logger.LogAsync($"ServiceCentersController.RemoveMember : User {currentUser.Id} tried to remove self from ServiceCenter {serviceCenterId}");
            TempData["ErrorMessage"] = "Для выхода из сервиса используйте кнопку 'Покинуть'";
            return RedirectToAction("Edit", new { id = serviceCenterId });
        }

        var membership = service.Members.FirstOrDefault(m => m.UserId == userId);
        if (membership != null)
        {
            _context.UserServiceCenters.Remove(membership);
            await _context.SaveChangesAsync();

            await _logger.LogAsync($"ServiceCentersController.RemoveMember : User {userId} removed from ServiceCenter {serviceCenterId} by {currentUser.Id}");

            TempData["SuccessMessage"] = "Участник успешно удален из сервисного центра";
        }
        else
        {
            await _logger.LogAsync($"ServiceCentersController.RemoveMember : User {userId} not found in ServiceCenter {serviceCenterId}");
            TempData["ErrorMessage"] = "Участник не найден";
        }

        return RedirectToAction("Edit", new { id = serviceCenterId });
    }
}
