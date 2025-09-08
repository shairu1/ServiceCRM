using System.ComponentModel.DataAnnotations;

namespace ServiceCRM.Models;

public enum OrderStatus
{
    [Display(Name = "Issue")]
    New = 0,

    [Display(Name = "Repair")]
    Repair = 1,

    [Display(Name = "Agreement")]
    Agreement = 2,

    [Display(Name = "WaitingForParts")]
    WaitingForParts = 3,

    [Display(Name = "Ready")]
    Ready = 4
}
