using System.Reflection;

namespace CompanionAgent.App.ViewModels;

public sealed class AboutViewModel : ViewModelBase
{
    private string _currentVersion = ReadVersion();
    private string _updateStatus = "Verificando atualizações...";
    private bool _updateAvailable;
    private string _availableVersion = "";
    private bool _isInstalling;

    public string CurrentVersion  { get => _currentVersion;  set => Set(ref _currentVersion, value); }
    public string UpdateStatus    { get => _updateStatus;     set => Set(ref _updateStatus, value); }
    public bool   UpdateAvailable { get => _updateAvailable;  set => Set(ref _updateAvailable, value); }
    public string AvailableVersion{ get => _availableVersion; set => Set(ref _availableVersion, value); }
    public bool   IsInstalling    { get => _isInstalling;     set => Set(ref _isInstalling, value); }

    private static string ReadVersion()
    {
        var v = Assembly.GetEntryAssembly()?.GetName().Version;
        return v is null ? "?" : $"{v.Major}.{v.Minor}.{v.Build}";
    }
}