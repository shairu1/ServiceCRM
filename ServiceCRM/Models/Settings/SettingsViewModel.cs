using System.ComponentModel.DataAnnotations;

namespace ServiceCRM.Models;

public class SettingsViewModel
{
    [Required(ErrorMessage = "Обязательно для заполнения"), Display(Name = "Логин")]
    public string Username { get; set; } = string.Empty;

    [EmailAddress, Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Неверный формат телефона"), Display(Name = "Телефон")]
    public string PhoneNumber { get; set; } = string.Empty;
}

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

public class CreateServiceViewModel
{
    [Required(ErrorMessage = "Обязательно для заполнения")]
    [MaxLength(200)]
    [Display(Name = "Название сервиса")]
    public string Name { get; set; } = string.Empty;
}

