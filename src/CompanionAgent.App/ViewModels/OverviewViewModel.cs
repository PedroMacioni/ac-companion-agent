namespace CompanionAgent.App.ViewModels;

using CompanionAgent.Core;

public sealed class OverviewViewModel : ViewModelBase
{
    private bool _isConnected;
    private string _userEmail = "";
    private string _mode = "source";
    private string _lastSyncText = "Nunca";
    private int _sessionCount;
    private int _lapCount;
    private int _nextSyncCountdown;
    private bool _isSyncing;
    private SyncState _syncState = SyncState.Unconfigured;
    private string _statusMessage = "";
    private bool _watcherActive;

    public bool IsConnected { get => _isConnected; set => Set(ref _isConnected, value); }
    public string UserEmail { get => _userEmail; set => Set(ref _userEmail, value); }
    public string Mode { get => _mode; set => Set(ref _mode, value); }
    public string LastSyncText { get => _lastSyncText; set => Set(ref _lastSyncText, value); }
    public int SessionCount { get => _sessionCount; set => Set(ref _sessionCount, value); }
    public int LapCount { get => _lapCount; set => Set(ref _lapCount, value); }
    public int NextSyncCountdown { get => _nextSyncCountdown; set => Set(ref _nextSyncCountdown, value); }
    public bool IsSyncing { get => _isSyncing; set => Set(ref _isSyncing, value); }
    public SyncState CurrentSyncState { get => _syncState; set => Set(ref _syncState, value); }
    public string StatusMessage { get => _statusMessage; set => Set(ref _statusMessage, value); }
    public bool WatcherActive { get => _watcherActive; set => Set(ref _watcherActive, value); }
    public string ComputerName { get; } = Environment.MachineName;

    public bool IsSourceMode => Mode == "source";

    public void UpdateMode(string mode)
    {
        Mode = mode;
        RaisePropertyChanged(nameof(IsSourceMode));
    }
}
