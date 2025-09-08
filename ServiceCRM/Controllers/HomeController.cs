using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ServiceCRM.Models;
using ServiceCRM.Services.Logger;

namespace ServiceCRM.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IActionLogger _actionLogger;

        public HomeController(ILogger<HomeController> logger, IActionLogger actionLogger)
        {
            _logger = logger;
            _actionLogger = actionLogger;
        }

        public async Task<IActionResult> Index()
        {
            await _actionLogger.LogAsync("HomeController.Index : Page opened");
            return View();
        }

        public async Task<IActionResult> Privacy()
        {
            await _actionLogger.LogAsync("HomeController.Privacy : Page opened");
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Error()
        {
            await _actionLogger.LogAsync("HomeController.Error : Error page opened");
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
