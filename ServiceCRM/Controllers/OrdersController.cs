using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceCRM.Data;
using ServiceCRM.Models;
using ServiceCRM.Services.Logger;

namespace ServiceCRM.Controllers;

[Authorize]
public class OrdersController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ServiceCrmContext _db;
    private readonly IActionLogger _logger;

    public OrdersController(UserManager<IdentityUser> userManager, ServiceCrmContext db, IActionLogger logger)
    {
        _userManager = userManager;
        _db = db;
        _logger = logger;
    }

    // GET: /Orders?search=...&status=...&sort=created_desc
    public async Task<IActionResult> Index(string? search, OrderStatus? status, string? sort)
    {
        ViewData["CurrentSearch"] = search;
        ViewData["CurrentStatus"] = status?.ToString() ?? "";
        ViewData["CurrentSort"] = sort ?? "";

        // Получаем выбранный сервис из куки
        int? selectedServiceId = null;
        var cookie = Request.Cookies["SelectedServiceId"];
        if (cookie != null && int.TryParse(cookie, out int serviceId))
            selectedServiceId = serviceId;

        if (!selectedServiceId.HasValue)
        {
            // Если сервис не выбран, возвращаем пустой список
            return View(new List<Order>());
        }

        IQueryable<Order> q = _db.Orders.Where(o => o.ServiceCenterId == selectedServiceId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            q = q.Where(o =>
                o.Id.ToString().Contains(search) ||
                o.DeviceType.Contains(search) ||
                o.Brand.Contains(search) ||
                o.Model.Contains(search) ||
                o.Issue.Contains(search) ||
                o.Counterparty.Contains(search));
        }

        if (status.HasValue)
        {
            q = q.Where(o => o.Status == status.Value);
        }

        q = sort switch
        {
            "created_desc" => q.OrderByDescending(o => o.CreatedAt),
            "created_asc" => q.OrderBy(o => o.CreatedAt),
            "sum_desc" => q.OrderByDescending(o => o.Amount),
            "sum_asc" => q.OrderBy(o => o.Amount),
            "status_asc" => q.OrderBy(o => o.Status),
            "status_desc" => q.OrderByDescending(o => o.Status),
            _ => q.OrderByDescending(o => o.CreatedAt)
        };

        var items = await q.AsNoTracking().ToListAsync();
        return View(items);
    }

    // GET: /Orders/Create
    [HttpGet]
    public IActionResult Create()
    {
        int? selectedServiceId = null;
        var cookie = Request.Cookies["SelectedServiceId"];
        if (cookie != null && int.TryParse(cookie, out int serviceId))
        {
            selectedServiceId = serviceId;
        }

        var order = new Order();
        if (selectedServiceId.HasValue)
            order.ServiceCenterId = selectedServiceId.Value;

        return View(order);
    }

    // POST: /Orders/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Order order)
    {
        var cookie = Request.Cookies["SelectedServiceId"];
        if (cookie != null && int.TryParse(cookie, out int serviceId))
        {
            order.ServiceCenterId = serviceId;
        }
        else
        {
            ModelState.AddModelError("", "Сервис не выбран.");
            return View(order);
        }

        //if (!ModelState.IsValid)
        //    return View(order);

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();
        await _logger.LogAsync($"Создан заказ #{order.Id}, контрагент: {order.Counterparty}, сумма: {order.Amount}");

        return RedirectToAction(nameof(Index));
    }

    // GET: /Orders/Edit?id=123
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var order = await _db.Orders.FindAsync(id);
        if (order == null) return NotFound();

        var model = new EditOrderViewModel
        {
            Id = order.Id,
            DeviceType = order.DeviceType,
            Brand = order.Brand,
            Model = order.Model,
            Issue = order.Issue,
            Counterparty = order.Counterparty,
            Amount = order.Amount,
            Status = order.Status,
            CreatedAt = order.CreatedAt
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditOrderViewModel mOrder)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Auth");

        if (!ModelState.IsValid)
        {
            // возвращаем введённые пользователем значения
            return View(mOrder);
        }

        var order = await _db.Orders
            .Include(o => o.ServiceCenter) // если нужно
            .Where(o => o.Id == mOrder.Id && o.ServiceCenter.Members.Any(m => m.UserId == user.Id))
            .FirstOrDefaultAsync();
        
        if (order == null) return NotFound();

        // обновляем только поля из формы
        order.DeviceType = mOrder.DeviceType;
        order.Brand = mOrder.Brand;
        order.Model = mOrder.Model;
        order.Issue = mOrder.Issue;
        order.Counterparty = mOrder.Counterparty;
        order.Amount = mOrder.Amount;
        order.Status = mOrder.Status;
        order.CreatedAt = mOrder.CreatedAt;

        try
        {
            await _db.SaveChangesAsync();
            TempData["Message"] = "Заказ успешно обновлён!";
        }
        catch
        {
            ModelState.AddModelError("", "Ошибка при сохранении изменений. Попробуйте снова.");
            return View(mOrder);
        }

        return RedirectToAction(nameof(Index));
    }
}
