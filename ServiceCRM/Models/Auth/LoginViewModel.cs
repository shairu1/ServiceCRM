using System.ComponentModel.DataAnnotations;
using System.Resources;

namespace ServiceCRM.Models.Auth;

public class LoginViewModel
{
    [Required(ErrorMessage = "UsernameRequired")]
    [Display(Name = "Username")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "PasswordRequired")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "RememberMe")]
    public bool RememberMe { get; set; }
}
