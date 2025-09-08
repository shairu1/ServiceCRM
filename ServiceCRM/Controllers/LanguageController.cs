using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using ServiceCRM.Services.Logger;

namespace ServiceCRM.Controllers;

public class LanguageController : Controller
{
    private readonly IActionLogger _logger;

    public LanguageController(IActionLogger logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Set(string culture, string returnUrl)
    {
        if (!string.IsNullOrEmpty(culture))
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            await _logger.LogAsync($"LanguageController.Set : Culture set to {culture}");
        }
        else
        {
            await _logger.LogAsync("LanguageController.Set : Culture is null or empty");
        }

        // Возврат на предыдущую страницу
        if (string.IsNullOrEmpty(returnUrl))
            returnUrl = "/";

        await _logger.LogAsync($"LanguageController.Set : Redirect to {returnUrl}");

        return LocalRedirect(returnUrl);
    }
}
