namespace CompanionAgent.App.ViewModels;

using CompanionAgent.Core;

public sealed class SettingsViewModel : ViewModelBase
{
    private string _acPath = "";
    private string _cmSessionsPath = "";
    private string _personalBestPath = "";
    private string _mode = "source";
    private string _language = "pt-BR";
    private bool _autoStart;
    private int _pollSeconds = 30;
    private bool _languageChanged;

    public string AcPath { get => _acPath; set => Set(ref _acPath, value); }
    public string CmSessionsPath { get => _cmSessionsPath; set => Set(ref _cmSessionsPath, value); }
    public string PersonalBestPath { get => _personalBestPath; set => Set(ref _personalBestPath, value); }
    public string Mode { get => _mode; set => Set(ref _mode, value); }
    public string Language
    {
        get => _language;
        set
        {
            if (_language != value) _languageChanged = true;
            Set(ref _language, value);
        }
    }
    public bool AutoStart { get => _autoStart; set => Set(ref _autoStart, value); }
    public int PollSeconds { get => _pollSeconds; set => Set(ref _pollSeconds, value); }
    public bool LanguageChanged { get => _languageChanged; set => Set(ref _languageChanged, value); }

    public void LoadFrom(AgentSettings settings)
    {
        AcPath = settings.AcPath;
        CmSessionsPath = settings.CmSessionsPath;
        PersonalBestPath = settings.PersonalBestPath;
        Mode = settings.Mode;
        Language = settings.Language;
        AutoStart = settings.AutoStart;
        PollSeconds = settings.WebSyncPollSeconds;
        _languageChanged = false;
    }

    public void ApplyTo(AgentSettings settings)
    {
        settings.AcPath = AcPath;
        settings.CmSessionsPath = CmSessionsPath;
        settings.PersonalBestPath = PersonalBestPath;
        settings.Mode = Mode;
        settings.Language = Language;
        settings.AutoStart = AutoStart;
        settings.WebSyncPollSeconds = PollSeconds;
    }
}
