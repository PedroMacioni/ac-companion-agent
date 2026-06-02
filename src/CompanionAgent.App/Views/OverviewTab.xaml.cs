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
        vm.PropertyChanged += (_, _) => Dispatcher.Invoke(() => UpdateFromVm(vm));
        UpdateFromVm(vm);

        BtnSyncNow.Click += (_, _) => OnSyncRequested?.Invoke();
        BtnResyncLaps.Click += (_, _) => OnResyncLapsRequested?.Invoke();
        BtnDisconnect.Click += (_, _) => OnDisconnectRequested?.Invoke();
    }

    private void UpdateFromVm(ViewModels.OverviewViewModel vm)
    {
        // Connection
        var connected = vm.IsConnected;
        ConnDot.Fill = connected
            ? new SolidColorBrush(Color.FromRgb(0x22, 0xC5, 0x5E))
            : new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44));
        ConnText.Text = connected ? vm.UserEmail : "Desconectado";
        ModeText.Text = "Modo: " + (vm.Mode == "source" ? "Fonte de dados" : "Somente visualização");
        BtnDisconnect.Visibility = connected ? Visibility.Visible : Visibility.Collapsed;

        // Stats
        TxtSessions.Text = vm.IsConnected ? vm.SessionCount.ToString() : "—";

        // Watcher status
        var watching = vm.WatcherActive;
        WatcherDot.Fill = watching
            ? new SolidColorBrush(Color.FromRgb(0x22, 0xC5, 0x5E))
            : new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x55));
        TxtWatcher.Text = watching ? "Ativo" : "Inativo";
        TxtWatcher.Foreground = watching
            ? new SolidColorBrush(Color.FromRgb(0x22, 0xC5, 0x5E))
            : new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88));

        // Sync info
        LastSyncText.Text = "Último sync: " + vm.LastSyncText;
        SyncCountText.Text = vm.SessionCount > 0 ? $"{vm.SessionCount} sessões no último sync" : "";
        NextSyncText.Text = vm.IsSourceMode && vm.NextSyncCountdown > 0
            ? $"Próximo sync em {vm.NextSyncCountdown}s" : "";

        // Buttons
        BtnSyncNow.IsEnabled = vm.IsConnected && vm.IsSourceMode && !vm.IsSyncing;
        BtnResyncLaps.IsEnabled = vm.IsConnected && vm.IsSourceMode && !vm.IsSyncing;

        // Sync card accent
        SyncCardAccent.Background = vm.CurrentSyncState switch
        {
            SyncState.Syncing => new SolidColorBrush(Color.FromRgb(0xF5, 0x9E, 0x0B)),
            SyncState.Error   => new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44)),
            SyncState.Idle    => new SolidColorBrush(Color.FromRgb(0x22, 0xC5, 0x5E)),
            _                 => new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33))
        };

        // Status message
        var showStatus = vm.CurrentSyncState is SyncState.Error || vm.IsSyncing;
        StatusMsg.Visibility = showStatus ? Visibility.Visible : Visibility.Collapsed;
        if (showStatus)
        {
            StatusMsg.Text = vm.StatusMessage;
            StatusMsg.Foreground = vm.CurrentSyncState == SyncState.Error
                ? new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44))
                : new SolidColorBrush(Color.FromRgb(0xF5, 0x9E, 0x0B));
        }

        // Device info
        TxtComputer.Text = vm.ComputerName;
        TxtVersion.Text = App.AboutVm.CurrentVersion;
    }

    public event Action? OnSyncRequested;
    public event Action? OnResyncLapsRequested;
    public event Action? OnDisconnectRequested;
}
