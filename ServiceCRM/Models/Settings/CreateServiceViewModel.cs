using System.ComponentModel.DataAnnotations;

namespace ServiceCRM.Models;

public class CreateServiceViewModel
{
    [Required(ErrorMessage = "Обязательно для заполнения")]
    [MaxLength(200)]
    [Display(Name = "Название сервиса")]
    public string Name { get; set; } = string.Empty;
}

