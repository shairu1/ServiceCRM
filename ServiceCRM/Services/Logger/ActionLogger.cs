using System.Text;

namespace ServiceCRM.Services.Logger;

public class ActionLogger : IActionLogger
{
    private readonly string _logDir;
    private readonly string _logFile;

    public ActionLogger(IWebHostEnvironment env)
    {
        _logDir = Path.Combine(env.WebRootPath, "data", "logs");
        Directory.CreateDirectory(_logDir);
        _logFile = Path.Combine(_logDir, "actions.txt");
    }

    public async Task LogAsync(string message)
    {
        var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}";
        await File.AppendAllTextAsync(_logFile, line, Encoding.UTF8);
    }
}

