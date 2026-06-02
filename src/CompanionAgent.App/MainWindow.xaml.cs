namespace CompanionAgent.App;

using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using CompanionAgent.App.Views;
using CompanionAgent.Core;

public partial class MainWindow : Window
{
    private OverviewTab? _overviewTab;
    private LogTab? _logTab;
    private SettingsTab? _settingsTab;
    private AboutTab? _aboutTab;
    private DoubleAnimation? _pulseAnimation;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += (_, _) => SetupTabs();
        WireStatusIndicator();
        SetupPulseAnimation();
    }

    private void SetupPulseAnimation()
    {
        _pulseAnimation = new DoubleAnimation(1.0, 0.25, TimeSpan.FromMilliseconds(700))
        {
            AutoReverse = true,
            RepeatBehavior = RepeatBehavior.Forever,
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        };
    }

    private void SetupTabs()
    {
        _overviewTab = new OverviewTab();
        _logTab = new LogTab();
        _settingsTab = new SettingsTab();
        _aboutTab = new AboutTab();

        OverviewContent.Content = _overviewTab;
        LogContent.Content = _logTab;
        SettingsContent.Content = _settingsTab;
        AboutContent.Content = _aboutTab;

        _overviewTab.OnSyncRequested += () => _ = App.Worker?.SyncAsync();
        _overviewTab.OnResyncLapsRequested += () => _ = App.Worker?.ForceResyncLapsAsync();
        _overviewTab.OnDisconnectRequested += () =>
        {
            App.Supabase?.ClearTokens();
            App.Settings.UserToken = "";
            App.Settings.RefreshToken = "";
            SettingsStore.Save(App.Settings);
            App.Worker?.Dispose();
            App.OverviewVm.IsConnected = false;
            App.OverviewVm.UserEmail = "";
            App.Log.Info(LogCategory.Auth, "Conta desconectada");
        };

        _aboutTab.OnInstallUpdateRequested += () =>
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(
                "https://github.com/PedroMacioni/ac-companion-agent/releases/latest")
                { UseShellExecute = true });
        };
    }

    private void WireStatusIndicator()
    {
        var vm = App.OverviewVm;
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(vm.IsConnected) or nameof(vm.UserEmail))
                Dispatcher.Invoke(UpdateHeaderStatus);
            if (e.PropertyName is nameof(vm.CurrentSyncState) or nameof(vm.StatusMessage) or nameof(vm.IsSyncing))
                Dispatcher.Invoke(UpdateStatusBar);
        };
        UpdateHeaderStatus();
        UpdateStatusBar();
    }

    private void UpdateHeaderStatus()
    {
        var vm = App.OverviewVm;
        StatusDot.Fill = vm.IsConnected
            ? new SolidColorBrush(Color.FromRgb(0x22, 0xC5, 0x5E))
            : new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44));
        StatusText.Text = vm.IsConnected ? vm.UserEmail : "Desconectado";
    }

    private void UpdateStatusBar()
    {
        var vm = App.OverviewVm;
        switch (vm.CurrentSyncState)
        {
            case SyncState.Syncing:
                SyncDot.Fill = new SolidColorBrush(Color.FromRgb(0xF5, 0x9E, 0x0B)); // amber
                SyncStateText.Text = vm.StatusMessage;
                SyncStateText.Foreground = new SolidColorBrush(Color.FromRgb(0xF5, 0x9E, 0x0B));
                SyncDot.BeginAnimation(UIElement.OpacityProperty, _pulseAnimation);
                break;
            case SyncState.Error:
                SyncDot.Fill = new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44)); // red
                SyncStateText.Text = vm.StatusMessage;
                SyncStateText.Foreground = new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44));
                SyncDot.BeginAnimation(UIElement.OpacityProperty, null);
                SyncDot.Opacity = 1;
                break;
            case SyncState.Idle:
                SyncDot.Fill = new SolidColorBrush(Color.FromRgb(0x22, 0xC5, 0x5E)); // green
                SyncStateText.Text = "Sincronizado";
                SyncStateText.Foreground = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88));
                SyncDot.BeginAnimation(UIElement.OpacityProperty, null);
                SyncDot.Opacity = 1;
                break;
            default:
                SyncDot.Fill = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88)); // gray
                SyncStateText.Text = vm.IsConnected ? "Pronto" : "Desconectado";
                SyncStateText.Foreground = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88));
                SyncDot.BeginAnimation(UIElement.OpacityProperty, null);
                SyncDot.Opacity = 1;
                break;
        }
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        e.Cancel = true;
        Hide();
    }
}
