namespace CompanionAgent.App.ViewModels;

using System.Collections.ObjectModel;
using CompanionAgent.Core;

public sealed class LogViewModel : ViewModelBase
{
    private LogLevel? _filterLevel;

    public ObservableCollection<LogEntry> AllEntries { get; } = [];
    public ObservableCollection<LogEntry> FilteredEntries { get; } = [];

    public LogLevel? FilterLevel
    {
        get => _filterLevel;
        set { Set(ref _filterLevel, value); ApplyFilter(); }
    }

    public void AddEntry(LogEntry entry)
    {
        AllEntries.Add(entry);
        if (_filterLevel == null || entry.Level == _filterLevel)
            FilteredEntries.Add(entry);
    }

    public void Clear()
    {
        AllEntries.Clear();
        FilteredEntries.Clear();
    }

    private void ApplyFilter()
    {
        FilteredEntries.Clear();
        foreach (var entry in AllEntries)
        {
            if (_filterLevel == null || entry.Level == _filterLevel)
                FilteredEntries.Add(entry);
        }
    }
}
