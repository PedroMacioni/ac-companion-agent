namespace CompanionAgent.App.ViewModels;

public sealed class AboutViewModel : ViewModelBase
{
    private string _currentVersion = "3.0.0";
    private string _updateStatus = "Verificando atualizações...";
    private bool _updateAvailable;
    private string _availableVersion = "";

    public string CurrentVersion { get => _currentVersion; set => Set(ref _currentVersion, value); }
    public string UpdateStatus { get => _updateStatus; set => Set(ref _updateStatus, value); }
    public bool UpdateAvailable { get => _updateAvailable; set => Set(ref _updateAvailable, value); }
    public string AvailableVersion { get => _availableVersion; set => Set(ref _availableVersion, value); }
}
