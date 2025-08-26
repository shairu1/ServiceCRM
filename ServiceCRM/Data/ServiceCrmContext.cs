using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ServiceCRM.Data;

public class ServiceCrmContext : IdentityDbContext<IdentityUser>
{
    public ServiceCrmContext(DbContextOptions<ServiceCrmContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
