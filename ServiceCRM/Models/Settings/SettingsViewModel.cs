using System.ComponentModel.DataAnnotations;

namespace ServiceCRM.Models;

public class SettingsViewModel
{
    [Required(ErrorMessage = "CurrentPasswordRequired")]
    [Display(Name = "Username")]
    public string Username { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "EmailAddressError")]
    [Display(Name = "Email")]
    public string? Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "PhoneError")]
    [Display(Name = "Phone")]
    public string? PhoneNumber { get; set; } = string.Empty;
}

