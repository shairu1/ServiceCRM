using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using ServiceCRM.Data;
using ServiceCRM.Models.Auth;
using ServiceCRM.Services.Cookie;
using ServiceCRM.Services.Identity;
using ServiceCRM.Services.Logger;

namespace ServiceCRM.Controllers;


public class AuthController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ServiceCrmContext _context;
    private readonly Services.Cookie.ICookieManager _cookieManager;
    private readonly IStringLocalizer<AuthController> _localizer;
    private readonly IActionLogger _logger;

    public AuthController(UserManager<IdentityUser> userManager,
                  SignInManager<IdentityUser> signInManager,
                  ServiceCrmContext context,
                  Services.Cookie.ICookieManager cookieManager,
                  IStringLocalizer<AuthController> localizer,
                  IActionLogger logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _cookieManager = cookieManager;
        _logger = logger;

        _localizer = localizer;
    }

    // ------------------ Регистрация ------------------
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = new IdentityUser { UserName = model.Username };
        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            await _logger.LogAsync("AuthController.Register : User created successfully");

            await _signInManager.SignInAsync(user, isPersistent: false);

            // Проверяем и устанавливаем выбранный сервис в cookie
            await _cookieManager.SetDefaultServiceCenterCookieAsync(user, HttpContext);

            return RedirectToAction("Index", "Home");
        }

        foreach (var error in result.Errors)
        {
            await _logger.LogAsync($"AuthController.Register : Error - {error.Description}");
            ModelState.AddModelError("LoginError", error.Description);
        }

        return View(model);
    }

    // ------------------ Вход ------------------
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await _signInManager.PasswordSignInAsync(
            model.Username, model.Password, model.RememberMe, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            var user = await _userManager.FindByNameAsync(model.Username);

            if (user == null)
            {
                await _logger.LogAsync("AuthController.Login : User not found");
                ModelState.AddModelError("LoginError", _localizer["UserNotFound"]);
                return View(model);
            }

            await _logger.LogAsync("AuthController.Login : User logged in successfully");
            await _cookieManager.SetDefaultServiceCenterCookieAsync(user, HttpContext);

            return RedirectToAction("Index", "Home");
        }

        await _logger.LogAsync("AuthController.Login : Authentication failed");
        ModelState.AddModelError("LoginError", _localizer["AuthError"]);
        return View(model);
    }

    // ------------------ Выход ------------------
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        await _logger.LogAsync("AuthController.Logout : User logged out");

        return RedirectToAction("Index", "Home");
    }
}
