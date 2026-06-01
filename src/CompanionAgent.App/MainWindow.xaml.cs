namespace CompanionAgent.App;

using System.Windows;
using System.Windows.Media;
using CompanionAgent.Core;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        WireViewModel();
    }

    private void WireViewModel()
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
                ? new SolidColorBrush(Color.FromRgb(0x22, 0xC5, 0x5E))  // green
                : new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44));  // red
            StatusText.Text = vm.IsConnected ? vm.UserEmail : "Desconectado";
        });
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        // Hide to tray instead of closing
        e.Cancel = true;
        Hide();
    }
}
