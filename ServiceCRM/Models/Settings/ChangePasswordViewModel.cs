using System.ComponentModel.DataAnnotations;

namespace ServiceCRM.Models;

public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "Обязательно для заполнения"), DataType(DataType.Password)]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Обязательно для заполнения"), DataType(DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Обязательно для заполнения")]
    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "Пароли не совпадают")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

