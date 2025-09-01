using System.ComponentModel.DataAnnotations;

namespace ServiceCRM.Models;

public class EditOrderViewModel
{
    public int Id { get; set; }
    [Required]
    public string DeviceType { get; set; } = string.Empty;

    [Required]
    public string Brand { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;
    public string Issue { get; set; } = string.Empty;
    public string Counterparty { get; set; } = string.Empty;

    public decimal Amount { get; set; }
    public OrderStatus Status { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime CreatedAt { get; set; }
}
