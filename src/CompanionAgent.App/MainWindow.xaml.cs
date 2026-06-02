namespace CompanionAgent.App;

using System.Windows;
using System.Windows.Media;
using CompanionAgent.App.Views;
using CompanionAgent.Core;

public partial class MainWindow : Window
{
    private OverviewTab? _overviewTab;
    private LogTab? _logTab;
    private SettingsTab? _settingsTab;
    private AboutTab? _aboutTab;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += (_, _) => SetupTabs();
        WireStatusIndicator();
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

        // Wire overview tab events
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

        // Wire about tab events
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
            if (e.PropertyName is nameof(vm.IsConnected) or nameof(vm.UserEmail) or nameof(vm.StatusMessage))
                UpdateStatusIndicator();
        };
        UpdateStatusIndicator();
    }

    private void UpdateStatusIndicator()
    {
        var vm = App.OverviewVm;
        Dispatcher.Invoke(() =>
        {
            StatusDot.Fill = vm.IsConnected
                ? new SolidColorBrush(Color.FromRgb(0x22, 0xC5, 0x5E))
                : new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44));
            StatusText.Text = vm.IsConnected ? vm.UserEmail : "Desconectado";
        });
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        e.Cancel = true;
        Hide();
    }
}
