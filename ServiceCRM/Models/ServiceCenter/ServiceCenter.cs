using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceCRM.Models;

public class ServiceCenter
{
    public int Id { get; set; } // PK

    [Required]
    [Display(Name = "AdminId")]
    public string AdminId { get; set; } = string.Empty; // FK на User

    [ForeignKey(nameof(AdminId))]
    public IdentityUser Admin { get; set; } = null!;

    [Required(ErrorMessage = "NameRequired")]
    [Display(Name = "Name")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "OrdersCount")]
    public int OrdersCount { get; set; } = 0;

    [Display(Name = "CreatedAt")]
    [DataType(DataType.DateTime)]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Заказы
    public ICollection<Order> Orders { get; set; } = new List<Order>();

    // Участники
    public ICollection<UserServiceCenter> Members { get; set; } = new List<UserServiceCenter>();
}
