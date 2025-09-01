using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceCRM.Data;
using ServiceCRM.Models;

namespace ServiceCRM.ViewComponents;

public class UserServiceCentersViewComponent : ViewComponent
{
    private readonly ServiceCrmContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public UserServiceCentersViewComponent(ServiceCrmContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // selectedServiceId передаётся через query string или route
    public async Task<IViewComponentResult> InvokeAsync(int? selectedServiceId = null)
    {
        if (!User.Identity?.IsAuthenticated ?? true)
            return View(new UserServiceCentersViewModel());

        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user == null)
            return View(new UserServiceCentersViewModel());

        var services = await _context.UserServiceCenters
            .Where(us => us.UserId == user.Id)
            .Include(us => us.ServiceCenter)
            .Select(us => us.ServiceCenter)
            .ToListAsync();

        if (services.Count == 0)
            return View(new UserServiceCentersViewModel());

        ServiceCenter selectedService = services[0];

        if (selectedServiceId.HasValue)
        {
            var find = services.FirstOrDefault(s => s.Id == selectedServiceId.Value);
            if (find is not null)
            {
                selectedService = find;
            }
        }

        return View(new UserServiceCentersViewModel
        {
            ServiceCenters = services,
            SelectedServiceCenter = selectedService
        });
    }
}
