using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceCRM.Data;
using ServiceCRM.Models.Auth;
using ServiceCRM.Services;

namespace ServiceCRM.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ServiceCrmContext _context;
        private readonly IUserServiceCookieManager _cookieManager;

        public AuthController(UserManager<IdentityUser> userManager,
                      SignInManager<IdentityUser> signInManager,
                      ServiceCrmContext context,
                      IUserServiceCookieManager cookieManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _cookieManager = cookieManager;
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
                await _signInManager.SignInAsync(user, isPersistent: false);

                // Проверяем и устанавливаем выбранный сервис в cookie
                await _cookieManager.SetDefaultServiceCookieAsync(user, HttpContext);

                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

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
                    return View(model);

                await _cookieManager.SetDefaultServiceCookieAsync(user, HttpContext);

                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction("Index", "Home");
        }

        // ------------------ Выход ------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
