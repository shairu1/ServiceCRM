using System.ComponentModel.DataAnnotations;

namespace ServiceCRM.Models;

public class EditServiceCenterViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Название сервиса обязательно")]
    public string Name { get; set; } = string.Empty;

    // Логин нового участника (можно оставить пустым)
    public string? NewMemberLogin { get; set; }
}
