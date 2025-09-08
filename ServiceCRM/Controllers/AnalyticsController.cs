using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceCRM.Data;
using ServiceCRM.Models;
using ServiceCRM.Services.Logger;
using System.Globalization;

namespace ServiceCRM.Controllers;

[Authorize]
public class AnalyticsController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ServiceCrmContext _db;
    private readonly IActionLogger _logger;

    public AnalyticsController(UserManager<IdentityUser> userManager, ServiceCrmContext db, IActionLogger logger)
    {
        _userManager = userManager;
        _db = db;
        _logger = logger;
    }

    public async Task<IActionResult> Index(string period = "month")
    {
        // Получаем выбранный сервис из куки
        int? selectedServiceId = null;
        var cookie = Request.Cookies["SelectedServiceId"];
        if (cookie != null && int.TryParse(cookie, out int serviceId))
            selectedServiceId = serviceId;

        if (!selectedServiceId.HasValue)
        {
            await _logger.LogAsync($"AnalyticsController.Index : Service is not selected");
            return View(new AnalyticsViewModel());
        }

        var now = DateTime.Now;
        IQueryable<Order> query = _db.Orders
            .Where(o => o.ServiceCenterId == selectedServiceId.Value);

        if (period == "year")
        {
            // последние 12 месяцев
            var startDate = now.AddMonths(-11).Date;
            query = query.Where(o => o.CreatedAt >= startDate);
        }
        else // month
        {
            // последние 30 дней
            var startDate = now.AddDays(-30).Date;
            query = query.Where(o => o.CreatedAt >= startDate);
        }

        var orders = await query.AsNoTracking().ToListAsync();

        var model = new AnalyticsViewModel
        {
            Period = period,
            TotalOrders = orders.Count,
            TotalRevenue = orders.Sum(o => o.Amount),
            AverageOrderValue = orders.Any() ? orders.Average(o => o.Amount) : 0,
            OrdersByStatus = GetOrdersByStatus(orders),
            RevenueByMonth = GetRevenueByMonth(orders, period),
            OrdersByDeviceType = GetOrdersByDeviceType(orders),
            RevenueByDeviceType = GetRevenueByDeviceType(orders),
            ActiveOrders = GetActiveOrdersCount(orders)
        };

        return View(model);
    }

    private Dictionary<string, int> GetOrdersByStatus(List<Order> orders)
    {
        return orders
            .GroupBy(o => o.Status)
            .ToDictionary(
                g => g.Key.ToString(),
                g => g.Count()
            );
    }

    private Dictionary<string, decimal> GetRevenueByMonth(List<Order> orders, string period)
    {
        var now = DateTime.Now;
        var result = new Dictionary<string, decimal>();

        if (period == "year")
        {
            for (int i = 11; i >= 0; i--)
            {
                var month = now.AddMonths(-i);
                var monthKey = month.ToString("MMM yyyy", CultureInfo.GetCultureInfo("ru-RU"));
                var revenue = orders
                    .Where(o => o.CreatedAt.Month == month.Month && o.CreatedAt.Year == month.Year)
                    .Sum(o => o.Amount);
                result[monthKey] = revenue;
            }
        }
        else // month
        {
            var startDate = now.AddDays(-30);
            for (int i = 0; i <= 30; i++)
            {
                var date = startDate.AddDays(i);
                var dateKey = date.ToString("dd.MM");
                var revenue = orders
                    .Where(o => o.CreatedAt.Date == date.Date)
                    .Sum(o => o.Amount);
                result[dateKey] = revenue;
            }
        }

        return result;
    }

    private Dictionary<string, int> GetOrdersByDeviceType(List<Order> orders)
    {
        return orders
            .GroupBy(o => o.DeviceType)
            .ToDictionary(
                g => g.Key,
                g => g.Count()
            )
            .OrderByDescending(x => x.Value)
            .Take(10)
            .ToDictionary(x => x.Key, x => x.Value);
    }

    private Dictionary<string, decimal> GetRevenueByDeviceType(List<Order> orders)
    {
        return orders
            .GroupBy(o => o.DeviceType)
            .ToDictionary(
                g => g.Key,
                g => g.Sum(o => o.Amount)
            )
            .OrderByDescending(x => x.Value)
            .Take(10)
            .ToDictionary(x => x.Key, x => x.Value);
    }

    private int GetActiveOrdersCount(List<Order> orders)
    {
        return orders.Count(o => o.Status != OrderStatus.Ready);
    }
}