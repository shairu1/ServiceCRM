using ServiceCRM.Models.ServiceCenter;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceCRM.Models;

public class Order
{
    public int Id { get; set; } // Id заказа (PK)

    [Required(ErrorMessage = "OrderNumberRequired")]
    [Display(Name = "OrderNumber")]
    public string OrderNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "CreatedAtRequired")]
    [Display(Name = "CreatedAt")]
    [DataType(DataType.DateTime)]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Required(ErrorMessage = "StatusRequired")]
    [Display(Name = "Status")]
    public OrderStatus Status { get; set; } = OrderStatus.New;

    [Required(ErrorMessage = "DeviceTypeRequired")] 
    [Display(Name = "DeviceType")]
    public string DeviceType { get; set; } = string.Empty;

    [Required(ErrorMessage = "BrandRequired")] 
    [Display(Name = "Brand")]
    public string Brand { get; set; } = string.Empty;

    [Required(ErrorMessage = "ModelRequired")]
    [Display(Name = "Model")]
    public string Model { get; set; } = string.Empty;

    [Required(ErrorMessage = "IssueRequired")]
    [Display(Name = "Issue")]
    public string Issue { get; set; } = string.Empty;

    [Required(ErrorMessage = "CounterpartyRequired")]
    [Display(Name = "Counterparty")]
    public string Counterparty { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    [Range(-1, 1_000_000)]
    [Display(Name = "Amount")]
    public decimal Amount { get; set; }

    // FK на Service
    [Required]
    public int ServiceCenterId { get; set; }

    [ForeignKey(nameof(ServiceCenterId))]
    public ServiceCenter ServiceCenter { get; set; } = null!;
}
