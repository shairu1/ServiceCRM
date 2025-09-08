using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ServiceCRM.Models;

public class EditServiceCenterViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Название сервиса обязательно")]
    public string Name { get; set; } = string.Empty;

    public List<IdentityUser> Members { get; set; } = new List<IdentityUser>();
    public IdentityUser Admin { get; set; } = new IdentityUser();
}
