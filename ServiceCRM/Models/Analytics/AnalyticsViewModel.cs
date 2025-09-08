namespace ServiceCRM.Models;

public class AnalyticsViewModel
{
    public string Period { get; set; } = "month";
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageOrderValue { get; set; }
    public Dictionary<string, int> OrdersByStatus { get; set; } = new();
    public Dictionary<string, decimal> RevenueByMonth { get; set; } = new();
    public Dictionary<string, int> OrdersByDeviceType { get; set; } = new();
    public Dictionary<string, decimal> RevenueByDeviceType { get; set; } = new();
    public int ActiveOrders { get; set; }
}
