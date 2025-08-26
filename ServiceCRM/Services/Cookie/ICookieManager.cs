using Microsoft.AspNetCore.Identity;

namespace ServiceCRM.Services.Cookie;

public interface ICookieManager
{
    Task SetDefaultServiceCenterCookieAsync(IdentityUser user, HttpContext httpContext);
}
