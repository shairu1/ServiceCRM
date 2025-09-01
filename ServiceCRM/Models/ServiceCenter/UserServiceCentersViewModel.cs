namespace ServiceCRM.Models;

public class UserServiceCentersViewModel
{
    public List<ServiceCenter> ServiceCenters { get; set; } = new();
    public ServiceCenter SelectedServiceCenter { get; set; } = null!;
}
