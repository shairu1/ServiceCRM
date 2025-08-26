using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ServiceCRM.Data;

namespace ServiceCRM.Services.Cookie;

public class CookieManager : ICookieManager
{
    private readonly ServiceCrmContext _context;

    public CookieManager(ServiceCrmContext context)
    {
        _context = context;
    }

    public async Task SetDefaultServiceCenterCookieAsync(IdentityUser user, HttpContext httpContext)
    {
        // Получаем список доступных сервисов пользователя
        var userServices = await _context.UserServiceCenters
            .Where(us => us.UserId == user.Id)
            .Select(us => us.ServiceCenterId)
            .ToListAsync();

        if (!userServices.Any())
        {
            // Нет сервисов — удаляем cookie
            httpContext.Response.Cookies.Delete("SelectedServiceId");
            return;
        }

        // Проверяем cookie
        int? selectedServiceId = null;
        if (httpContext.Request.Cookies.ContainsKey("SelectedServiceId"))
        {
            if (int.TryParse(httpContext.Request.Cookies["SelectedServiceId"], out int cookieServiceId))
            {
                if (userServices.Contains(cookieServiceId))
                    selectedServiceId = cookieServiceId;
            }
        }

        // Если cookie не задана или недоступна — берём первый сервис
        if (!selectedServiceId.HasValue)
        {
            selectedServiceId = userServices.First();
        }

        // Записываем cookie
        httpContext.Response.Cookies.Append(
            "SelectedServiceId",
            selectedServiceId.Value.ToString(),
            new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTimeOffset.UtcNow.AddDays(30)
            });
    }
}
