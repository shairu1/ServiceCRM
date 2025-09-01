using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace ServiceCRM.Controllers;

public class LanguageController : Controller
{
    [HttpGet]
    public IActionResult Set(string culture, string returnUrl)
    {
        if (!string.IsNullOrEmpty(culture))
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new Microsoft.AspNetCore.Localization.RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );
        }

        // Возврат на предыдущую страницу
        if (string.IsNullOrEmpty(returnUrl))
            returnUrl = "/";

        return LocalRedirect(returnUrl);
    }
}
