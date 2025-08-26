using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ServiceCRM.Models;

namespace ServiceCRM.Data;

public class ServiceCrmContext : IdentityDbContext<IdentityUser>
{
    public ServiceCrmContext(DbContextOptions<ServiceCrmContext> options) : base(options) { }

    public DbSet<Order> Orders { get; set; }
    public DbSet<ServiceCenter> ServiceCenters { get; set; }
    public DbSet<UserServiceCenter> UserServiceCenters { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
