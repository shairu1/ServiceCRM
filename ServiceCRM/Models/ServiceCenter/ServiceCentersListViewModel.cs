namespace ServiceCRM.Models;

public class ServiceCentersListViewModel
{
    public List<ServiceCenter> ServiceCenters { get; set; } = new();
    public string CurrentUserId { get; set; } = string.Empty;
}
