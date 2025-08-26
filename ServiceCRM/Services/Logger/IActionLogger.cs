namespace ServiceCRM.Services.Logger;

public interface IActionLogger
{
    Task LogAsync(string message);
}
