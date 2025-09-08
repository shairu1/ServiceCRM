using Microsoft.Extensions.Localization;
using ServiceCRM.Models;
using ServiceCRM.Services.Identity;

namespace ServiceCRM.Services;

public class LocalizationOrderStatusHelper
{
    private readonly IStringLocalizer _localizer;

    public LocalizationOrderStatusHelper(IStringLocalizerFactory factory)
    {
        var type = typeof(LocalizedIdentityErrorDescriber);
        _localizer = factory.Create("OrderStatus", typeof(LocalizedIdentityErrorDescriber).Assembly.FullName!);
    }

    public string GetOrderStatusTranslation(OrderStatus status)
    {
        Console.WriteLine(status.ToString());
        return status switch
        {
            OrderStatus.New => _localizer["New"],
            OrderStatus.Repair => _localizer["Repair"],
            OrderStatus.Agreement => _localizer["Agreement"],
            OrderStatus.WaitingForParts => _localizer["WaitingForParts"],
            OrderStatus.Ready => _localizer["Ready"],
            _ => status.ToString()
        };
    }
}
