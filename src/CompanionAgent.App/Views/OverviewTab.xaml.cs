namespace CompanionAgent.App.Views;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CompanionAgent.Core;

public partial class OverviewTab : UserControl
{
    public OverviewTab()
    {
        InitializeComponent();
        Loaded += (_, _) => BindViewModel();
    }

    private void BindViewModel()
    {
        var vm = App.OverviewVm;
        vm.PropertyChanged += (_, e) => Dispatcher.Invoke(() => UpdateFromVm(vm));
        UpdateFromVm(vm);

        BtnSyncNow.Click += (_, _) => OnSyncRequested?.Invoke();
        BtnResyncLaps.Click += (_, _) => OnResyncLapsRequested?.Invoke();
        BtnDisconnect.Click += (_, _) => OnDisconnectRequested?.Invoke();
    }

    private void UpdateFromVm(ViewModels.OverviewViewModel vm)
    {
        // Connection status
        var connected = vm.IsConnected;
        ConnDot.Fill = connected
            ? new SolidColorBrush(Color.FromRgb(0x22, 0xC5, 0x5E))
            : new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44));
        ConnText.Text = connected ? vm.UserEmail : "Desconectado";
        ModeText.Text = "Modo: " + (vm.Mode == "source" ? "Fonte de dados" : "Somente visualização");
        BtnDisconnect.Visibility = connected ? Visibility.Visible : Visibility.Collapsed;

        // Sync info
        LastSyncText.Text = "Último sync: " + vm.LastSyncText;
        SyncCountText.Text = vm.SessionCount > 0 ? $"{vm.SessionCount} sessões · {vm.LapCount} laps" : "";
        NextSyncText.Text = vm.IsSourceMode && vm.NextSyncCountdown > 0
            ? $"Próximo sync em {vm.NextSyncCountdown}s" : "";

        // Buttons
        BtnSyncNow.IsEnabled = vm.IsConnected && vm.IsSourceMode && !vm.IsSyncing;
        BtnResyncLaps.IsEnabled = vm.IsConnected && vm.IsSourceMode && !vm.IsSyncing;

        // Update card accent color based on sync state
        SyncCardAccent.Background = vm.CurrentSyncState switch
        {
            SyncState.Syncing => new SolidColorBrush(Color.FromRgb(0xF5, 0x9E, 0x0B)),
            SyncState.Error => new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44)),
            SyncState.Idle => new SolidColorBrush(Color.FromRgb(0x22, 0xC5, 0x5E)),
            _ => new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33))
        };

        // Status banner
        var showBanner = vm.CurrentSyncState == SyncState.Error || vm.IsSyncing;
        StatusBanner.Visibility = showBanner ? Visibility.Visible : Visibility.Collapsed;
        if (showBanner)
        {
            StatusMsg.Text = vm.StatusMessage;
            StatusMsg.Foreground = vm.CurrentSyncState == SyncState.Error
                ? new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44))
                : new SolidColorBrush(Color.FromRgb(0xF5, 0x9E, 0x0B));
        }
    }

    // Events wired in Phase 4
    public event Action? OnSyncRequested;
    public event Action? OnResyncLapsRequested;
    public event Action? OnDisconnectRequested;
}
