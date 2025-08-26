using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ServiceCRM.Data;
using ServiceCRM.Models;
using System.Security.Claims;

namespace ServiceCRM.Services.UserServiceCenterProvider;

public class UserServiceCenterProvider : IUserServiceCenterProvider
{
    private readonly ServiceCrmContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public UserServiceCenterProvider(ServiceCrmContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<List<ServiceCenter>> GetUserServiceCentersAsync(ClaimsPrincipal userClaims)
    {
        var user = await _userManager.GetUserAsync(userClaims);
        if (user == null) return new List<ServiceCenter>();

        return await _context.UserServiceCenters
            .Where(us => us.UserId == user.Id)
            .Include(us => us.ServiceCenter)
            .Select(us => us.ServiceCenter)
            .ToListAsync();
    }
}
