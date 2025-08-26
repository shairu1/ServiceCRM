using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace ServiceCRM.Models;

public class UserServiceCenter
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = null!; // FK на IdentityUser

    [ForeignKey(nameof(UserId))]
    public IdentityUser User { get; set; } = null!;

    [Required]
    public int ServiceCenterId { get; set; } // FK на Service

    [ForeignKey(nameof(ServiceCenterId))]
    public ServiceCenter ServiceCenter { get; set; } = null!;
}
