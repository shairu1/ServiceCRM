using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using ServiceCRM.Data;
using ServiceCRM.Models;
using ServiceCRM.Services.Logger;

namespace ServiceCRM.Controllers;

[Authorize]
public class SettingsController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ServiceCrmContext _context;
    private readonly IStringLocalizer<SettingsController> _localizer;
    private readonly IActionLogger _logger;

    public SettingsController(UserManager<IdentityUser> userManager,
                              SignInManager<IdentityUser> signInManager,
                              ServiceCrmContext context,
                              IStringLocalizer<SettingsController> localizer,
                              IActionLogger logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _localizer = localizer;
        _logger = logger;
    }

    // GET: Settings
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            await _logger.LogAsync("SettingsController.Index : User not found, redirect to login");
            return RedirectToAction("Login", "Auth");
        }

        await _logger.LogAsync($"SettingsController.Index : User {user.UserName} opened settings");

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
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            await _logger.LogAsync("SettingsController.Index(POST) : User not found, redirect to login");
            return RedirectToAction("Login", "Auth");
        }

        if (user.UserName != model.Username)
        {
            if (string.IsNullOrEmpty(model.Username))
            {
                await _logger.LogAsync("SettingsController.Index(POST) : Username empty, validation failed");
                return View(model);
            }

            var existingUser = await _userManager.FindByNameAsync(model.Username);
            if (existingUser != null && existingUser.Id != user.Id)
            {
                await _logger.LogAsync($"SettingsController.Index(POST) : Username {model.Username} already exists");
                ModelState.AddModelError("SettingsError", "Пользователь с таким логином уже существует");
                return View(model);
            }
        }

        user.UserName = model.Username;
        user.Email = model.Email;
        user.PhoneNumber = model.PhoneNumber;

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            await _logger.LogAsync($"SettingsController.Index(POST) : User {user.UserName} updated successfully");
            TempData["Message"] = _localizer["DataUpdatedSuccessfully"].Value;
            return RedirectToAction(nameof(Index));
        }

        foreach (var error in result.Errors)
        {
            await _logger.LogAsync($"SettingsController.Index(POST) : Error {error.Description}");
            ModelState.AddModelError("SettingsError", error.Description);
        }

        return View(model);
    }

    // GET: ChangePassword
    public IActionResult ChangePassword()
    {
        return View();
    }

    // POST: ChangePassword
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await _logger.LogAsync("SettingsController.ChangePassword : Model validation failed");
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            await _logger.LogAsync("SettingsController.ChangePassword : User not found, redirect to login");
            return RedirectToAction("Login", "Auth");
        }

        var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (result.Succeeded)
        {
            await _logger.LogAsync($"SettingsController.ChangePassword : Password changed for user {user.UserName}");
            await _signInManager.RefreshSignInAsync(user);
            TempData["SettingsMessage"] = _localizer["PasswordChangedSuccessfully"].Value;
            return RedirectToAction(nameof(Index));
        }

        foreach (var error in result.Errors)
        {
            await _logger.LogAsync($"SettingsController.ChangePassword : Error {error.Description}");
            ModelState.AddModelError("LoginError", error.Description);
        }

        return View(model);
    }

    // POST: Logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        await _logger.LogAsync("SettingsController.Logout : User logged out");
        return RedirectToAction("Index", "Home");
    }
}
