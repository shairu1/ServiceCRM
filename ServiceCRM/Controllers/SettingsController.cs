using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServiceCRM.Data;
using ServiceCRM.Models;

namespace ServiceCRM.Controllers;

[Authorize]
public class SettingsController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ServiceCrmContext _context;

    public SettingsController(UserManager<IdentityUser> userManager,
                              SignInManager<IdentityUser> signInManager,
                              ServiceCrmContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
    }

    // GET: Settings
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Auth");

        var model = new SettingsViewModel
        {
            Username = user.UserName!,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber ?? string.Empty
        };
        return View(model);
    }

    // POST: Settings
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(SettingsViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Auth");

        user.UserName = model.Username;
        user.Email = model.Email;
        user.PhoneNumber = model.PhoneNumber;

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            TempData["Message"] = "Данные успешно обновлены";
            return RedirectToAction(nameof(Index));
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError("", error.Description);

        return View(model);
    }

    // GET: ChangePassword
    public IActionResult ChangePassword() => View();

    // POST: ChangePassword
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Auth");

        var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (result.Succeeded)
        {
            await _signInManager.RefreshSignInAsync(user);
            TempData["Message"] = "Пароль успешно изменен";
            return RedirectToAction(nameof(Index));
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError("", error.Description);

        return View(model);
    }

    // GET: CreateService
    public IActionResult CreateService() => View();

    // POST: CreateService
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateService(CreateServiceViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Auth");

        var serviceCenter = new ServiceCenter
        {
            Name = model.Name,
            AdminId = user.Id,
            CreatedAt = DateTime.Now
        };

        _context.ServiceCenters.Add(serviceCenter);
        await _context.SaveChangesAsync();

        // Добавляем доступ пользователю
        _context.UserServiceCenters.Add(new UserServiceCenter
        {
            UserId = user.Id,
            ServiceCenterId = serviceCenter.Id
        });
        await _context.SaveChangesAsync();

        TempData["Message"] = "Сервис создан";
        return RedirectToAction(nameof(Index));
    }

    // POST: Logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}
