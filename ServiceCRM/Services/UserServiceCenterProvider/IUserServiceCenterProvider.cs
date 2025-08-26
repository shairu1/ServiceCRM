using ServiceCRM.Models;
using System.Security.Claims;

namespace ServiceCRM.Services.UserServiceCenterProvider;

public interface IUserServiceCenterProvider
{
    Task<List<ServiceCenter>> GetUserServiceCentersAsync(ClaimsPrincipal user);
}
