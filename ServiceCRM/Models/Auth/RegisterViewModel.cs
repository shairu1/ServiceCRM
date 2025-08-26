using System.ComponentModel.DataAnnotations;

namespace ServiceCRM.Models.Auth;

public class RegisterViewModel
{
    [Required(ErrorMessage = "UsernameRequired")]
    [Display(Name = "Username")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "PasswordRequired")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "ConfirmPasswordRequired")]
    [DataType(DataType.Password)]
    [Display(Name = "ConfirmPassword")]
    [Compare("Password", ErrorMessage = "ConfirmPasswordCompare")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
