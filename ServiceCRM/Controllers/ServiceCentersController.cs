using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceCRM.Data;
using ServiceCRM.Models;

namespace ServiceCRM.Controllers;

[Authorize]
public class ServiceCentersController : Controller
{
    private readonly ServiceCrmContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public ServiceCentersController(ServiceCrmContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Select(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Auth");

        var hasAccess = await _context.UserServiceCenters.AnyAsync(us => us.UserId == user.Id && us.ServiceCenterId == id);
        if (!hasAccess) return Forbid();

        Response.Cookies.Append(
            "SelectedServiceId",
            id.ToString(),
            new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTimeOffset.UtcNow.AddDays(30)
            });

        var referer = Request.Headers["Referer"].ToString();
        if (!string.IsNullOrEmpty(referer))
            return Redirect(referer);

        return RedirectToAction("Index", "Home");
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Auth");

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

    // 📌 Создание сервиса
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
            ServiceCenter serviceCenter = new() { 
                AdminId = user.Id,
                Name = serviceCenterViewModel.Name,
                OrdersCount = 0,
                CreatedAt = DateTime.UtcNow
            };

            _context.ServiceCenters.Add(serviceCenter);
            await _context.SaveChangesAsync();

            // Автоматически добавляем создателя в участники
            _context.UserServiceCenters.Add(new UserServiceCenter
            {
                UserId = user.Id,
                ServiceCenterId = serviceCenter.Id
            });
            await _context.SaveChangesAsync();

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

        // Загружаем сервис вместе с участниками и заказами
        var serviceCenter = await _context.ServiceCenters
            .Include(s => s.Members) // участники
            .Include(s => s.Orders)  // заказы
            .FirstOrDefaultAsync(s => s.Id == id);

        if (serviceCenter == null)
            return NotFound();

        // Проверка: только админ может удалять
        if (serviceCenter.AdminId != user.Id)
            return Forbid();

        // Удаляем все заказы сервиса
        _context.Orders.RemoveRange(serviceCenter.Orders);

        // Удаляем всех участников сервиса
        _context.UserServiceCenters.RemoveRange(serviceCenter.Members);

        // Удаляем сам сервис
        _context.ServiceCenters.Remove(serviceCenter);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var service = await _context.ServiceCenters.FindAsync(id);
        if (service == null) return NotFound();

        var model = new EditServiceCenterViewModel
        {
            Id = service.Id,
            Name = service.Name
        };

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

        // Изменяем имя сервиса
        service.Name = model.Name;

        // Добавляем нового участника по логину
        if (!string.IsNullOrWhiteSpace(model.NewMemberLogin))
        {
            var user = await _userManager.FindByNameAsync(model.NewMemberLogin);
            if (user != null && !service.Members.Any(m => m.UserId == user.Id))
            {
                service.Members.Add(new UserServiceCenter
                {
                    ServiceCenterId = service.Id,
                    UserId = user.Id
                });
            }
            else
            {
                ModelState.AddModelError("NewMemberLogin", "Пользователь не найден или уже добавлен.");
                return View(model);
            }
        }

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Leave(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Auth");
            
        // Загружаем сервис вместе с участниками
        var service = await _context.ServiceCenters
            .Include(s => s.Members)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (service == null)
            return NotFound();

        // Проверка: админ не может покинуть сервис
        if (service.AdminId == user.Id)
        {
            TempData["Message"] = "Админ не может покинуть сервис!";
            return RedirectToAction(nameof(Index));
        }

        // Находим запись участника
        var membership = service.Members.FirstOrDefault(m => m.UserId == user.Id);
        if (membership != null)
        {
            _context.UserServiceCenters.Remove(membership);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}
