namespace CompanionAgent.Core;

using System.Collections.ObjectModel;

public enum LogLevel { Info, Warning, Error, Success }
public enum LogCategory { Auth, Sync, Sessions, Laps, Tracks, Cars, Update, WebSync, App }

public sealed record LogEntry(DateTime Timestamp, LogLevel Level, LogCategory Category, string Message);

public sealed class LogService
{
    private const int MaxBufferSize = 1000;
    private const int MaxLogAgeDays = 7;

    private readonly Queue<LogEntry> _buffer = new();
    private readonly object _lock = new();
    private readonly string _logDirectory;

    public event Action<LogEntry>? EntryAdded;

    public LogService()
    {
        _logDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SimRacingCompanion", "Logs");
        Directory.CreateDirectory(_logDirectory);
        CleanOldLogs();
    }

    public void Log(LogLevel level, LogCategory category, string message)
    {
        var entry = new LogEntry(DateTime.Now, level, category, message);

        lock (_lock)
        {
            _buffer.Enqueue(entry);
            if (_buffer.Count > MaxBufferSize)
                _buffer.Dequeue();
        }

        WriteToFile(entry);
        EntryAdded?.Invoke(entry);
    }

    public void Info(LogCategory category, string message) => Log(LogLevel.Info, category, message);
    public void Warn(LogCategory category, string message) => Log(LogLevel.Warning, category, message);
    public void Error(LogCategory category, string message) => Log(LogLevel.Error, category, message);
    public void Success(LogCategory category, string message) => Log(LogLevel.Success, category, message);

    public IReadOnlyList<LogEntry> GetBuffer()
    {
        lock (_lock)
            return _buffer.ToList();
    }

    private void WriteToFile(LogEntry entry)
    {
        try
        {
            var logFile = Path.Combine(_logDirectory, $"agent-{DateTime.Now:yyyy-MM-dd}.log");
            var levelStr = entry.Level switch
            {
                LogLevel.Info => "INFO ",
                LogLevel.Warning => "WARN ",
                LogLevel.Error => "ERROR",
                LogLevel.Success => "OK   ",
                _ => "INFO "
            };
            var line = $"{entry.Timestamp:yyyy-MM-ddTHH:mm:ss} {levelStr} [{entry.Category}] {entry.Message}";
            File.AppendAllText(logFile, line + Environment.NewLine);
        }
        catch { /* file logging is non-critical */ }
    }

    private void CleanOldLogs()
    {
        try
        {
            var cutoff = DateTime.Now.AddDays(-MaxLogAgeDays);
            foreach (var file in Directory.GetFiles(_logDirectory, "agent-*.log"))
            {
                if (File.GetCreationTime(file) < cutoff)
                    File.Delete(file);
            }
        }
        catch { }
    }

    public string GetTodayLogPath() =>
        Path.Combine(_logDirectory, $"agent-{DateTime.Now:yyyy-MM-dd}.log");
}
