namespace CompanionAgent.App;

using System.Windows;
using System.Windows.Threading;
using CompanionAgent.App.ViewModels;
using CompanionAgent.Core;

public partial class App : Application
{
    public static LogService Log { get; private set; } = null!;
    public static AgentSettings Settings { get; private set; } = null!;
    public static SupabaseClient Supabase { get; private set; } = null!;
    public static SyncWorker? Worker { get; private set; }

    public static OverviewViewModel OverviewVm { get; private set; } = null!;
    public static LogViewModel LogVm { get; private set; } = null!;
    public static SettingsViewModel SettingsVm { get; private set; } = null!;
    public static AboutViewModel AboutVm { get; private set; } = null!;

    private TrayIconManager? _tray;
    private MainWindow? _mainWindow;
    private DispatcherTimer? _countdownTimer;
    private int _syncCountdownRemaining;
    private readonly UpdateService _updates = new();

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

        Log.EntryAdded += entry => Dispatcher.Invoke(() => LogVm.AddEntry(entry));

        Supabase = new SupabaseClient(Settings.SupabaseUrl, Settings.SupabaseAnonKey);

        if (!string.IsNullOrEmpty(Settings.UserToken))
        {
            Supabase.SetTokens(Settings.UserToken, Settings.RefreshToken);
            if (Supabase.IsConfigured)
            {
                OverviewVm.IsConnected = true;
                OverviewVm.UserEmail = Supabase.UserEmail;
                Settings.UserEmail = Supabase.UserEmail;
            }
        }

        OverviewVm.UpdateMode(Settings.Mode);

        if (Supabase.IsConfigured && Settings.Mode == "source")
            StartSyncWorker();

        _syncCountdownRemaining = Settings.SyncIntervalMinutes * 60;
        _countdownTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _countdownTimer.Tick += (_, _) =>
        {
            if (_syncCountdownRemaining > 0)
            {
                _syncCountdownRemaining--;
                OverviewVm.NextSyncCountdown = _syncCountdownRemaining;
            }
        };
        _countdownTimer.Start();

        _tray = new TrayIconManager(
            onOpen: ShowMainWindow,
            onSync: () => _ = Worker?.SyncAsync(),
            onExit: () => Current.Shutdown()
        );

        ShowMainWindow();

        Log.Info(LogCategory.App, $"Sim Racing Companion v{AboutVm.CurrentVersion} iniciado");
        _ = CheckForUpdatesAsync();
    }

    public static void StartSyncWorker()
    {
        Worker?.Dispose();

        var history = new LocalHistoryService();
        if (!string.IsNullOrEmpty(Settings.CmSessionsPath) || !string.IsNullOrEmpty(Settings.PersonalBestPath))
            history.SetCustomPaths(Settings.CmSessionsPath, Settings.PersonalBestPath);

        var tracks = new LocalTrackService(string.IsNullOrEmpty(Settings.AcPath) ? null : Settings.AcPath);

        Worker = new SyncWorker(Supabase, history, tracks, Log);

        Worker.StateChanged += (state, msg) => Current.Dispatcher.Invoke(() =>
        {
            OverviewVm.CurrentSyncState = state;
            OverviewVm.StatusMessage = msg;
            OverviewVm.IsSyncing = state == SyncState.Syncing;

            if (state == SyncState.Idle)
            {
                var s = SettingsStore.Load();
                OverviewVm.LastSyncText = s.LastSyncAt.HasValue
                    ? FormatRelativeTime(s.LastSyncAt.Value)
                    : "nunca";
                OverviewVm.SessionCount = s.LastSyncSessionCount;
            }
        });

        Supabase.TokensRefreshed += (at, rt) =>
        {
            Settings.UserToken = at;
            Settings.RefreshToken = rt;
            SettingsStore.Save(Settings);
        };

        bool setupWatchers = Settings.Mode == "source";
        OverviewVm.WatcherActive = setupWatchers;
        Worker.Start(Settings.SyncIntervalMinutes, setupWatchers);

        Log.Info(LogCategory.App,
            $"Sync iniciado (modo: {Settings.Mode}, intervalo: {Settings.SyncIntervalMinutes}min)");
    }

    private static string FormatRelativeTime(DateTimeOffset time)
    {
        var diff = DateTimeOffset.Now - time;
        if (diff.TotalMinutes < 1) return "agora";
        if (diff.TotalMinutes < 60) return $"há {(int)diff.TotalMinutes} min";
        if (diff.TotalHours < 24) return $"há {(int)diff.TotalHours}h";
        return $"há {(int)diff.TotalDays} dias";
    }

    private void ShowMainWindow()
    {
        if (_mainWindow == null || !_mainWindow.IsLoaded)
            _mainWindow = new MainWindow();
        _mainWindow.Show();
        _mainWindow.WindowState = WindowState.Normal;
        _mainWindow.Activate();
    }

    private async Task CheckForUpdatesAsync()
    {
        var update = await _updates.CheckAsync();
        Dispatcher.Invoke(() =>
        {
            if (update is not null)
            {
                AboutVm.UpdateAvailable = true;
                AboutVm.AvailableVersion = update.Version;
                AboutVm.UpdateStatus = $"v{update.Version} disponível para instalar";
            }
            else
            {
                AboutVm.UpdateStatus = $"v{AboutVm.CurrentVersion} · versão mais recente";
            }
        });
    }

    public static async Task DoInstallUpdateAsync()
    {
        var app = (App)Current;
        AboutVm.IsInstalling = true;
        AboutVm.UpdateStatus = "Baixando atualização...";
        try
        {
            await app._updates.DownloadAndInstallAsync();
            Current.Shutdown();
        }
        catch
        {
            AboutVm.IsInstalling = false;
            AboutVm.UpdateStatus = "Erro ao baixar. Veja github.com/PedroMacioni/ac-companion-agent";
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Worker?.Dispose();
        _tray?.Dispose();
        _countdownTimer?.Stop();
        _updates.Dispose();
        base.OnExit(e);
    }
}