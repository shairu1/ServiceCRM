using System.ComponentModel.DataAnnotations;

namespace ServiceCRM.Models;

public class AddMemberViewModel
{
    [Required(ErrorMessage = "Логин пользователя обязателен")]
    public string Username { get; set; } = string.Empty;

    public int ServiceCenterId { get; set; }
}

