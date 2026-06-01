namespace CompanionAgent.App;

using System.Windows;
using CompanionAgent.App.ViewModels;
using CompanionAgent.Core;

public partial class App : Application
{
    public static LogService Log { get; private set; } = null!;
    public static AgentSettings Settings { get; private set; } = null!;
    public static OverviewViewModel OverviewVm { get; private set; } = null!;
    public static LogViewModel LogVm { get; private set; } = null!;
    public static SettingsViewModel SettingsVm { get; private set; } = null!;
    public static AboutViewModel AboutVm { get; private set; } = null!;

    private TrayIconManager? _tray;
    private MainWindow? _mainWindow;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        Settings = SettingsStore.Load();
        Log = new LogService();
        I18n.Load(Settings.Language);

        OverviewVm = new OverviewViewModel();
        LogVm = new LogViewModel();
        SettingsVm = new SettingsViewModel();
        AboutVm = new AboutViewModel();

        SettingsVm.LoadFrom(Settings);

        Log.EntryAdded += entry =>
        {
            Dispatcher.Invoke(() => LogVm.AddEntry(entry));
        };

        _tray = new TrayIconManager(
            onOpen: ShowMainWindow,
            onSync: () => { /* wired in Phase 4 */ },
            onExit: () => { Current.Shutdown(); }
        );

        ShowMainWindow();
    }

    private void ShowMainWindow()
    {
        if (_mainWindow == null || !_mainWindow.IsLoaded)
        {
            _mainWindow = new MainWindow();
        }
        _mainWindow.Show();
        _mainWindow.WindowState = System.Windows.WindowState.Normal;
        _mainWindow.Activate();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _tray?.Dispose();
        base.OnExit(e);
    }
}
