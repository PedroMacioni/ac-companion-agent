namespace CompanionAgent.App.Views;

using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

public partial class AboutTab : UserControl
{
    public AboutTab()
    {
        InitializeComponent();
        Loaded += (_, _) => Setup();
    }

    private void Setup()
    {
        var vm = App.AboutVm;
        vm.PropertyChanged += (_, _) => Dispatcher.Invoke(() => UpdateFromVm(vm));
        UpdateFromVm(vm);

        BtnGitHub.Click += (_, _) =>
        {
            Process.Start(new ProcessStartInfo("https://github.com/PedroMacioni/ac-companion-agent")
                { UseShellExecute = true });
        };

        BtnInstallUpdate.Click += async (_, _) => await App.DoInstallUpdateAsync();

        BtnViewChangelog.Click += (_, _) =>
        {
            Process.Start(new ProcessStartInfo("https://github.com/PedroMacioni/ac-companion-agent/releases")
                { UseShellExecute = true });
        };
    }

    private void UpdateFromVm(ViewModels.AboutViewModel vm)
    {
        VersionText.Text = $"Versão {vm.CurrentVersion}";
        UpdateStatusText.Text = vm.UpdateStatus;

        if (vm.UpdateAvailable)
        {
            UpdateBanner.Visibility = Visibility.Visible;
            UpdateVersionText.Text = $"Atualização disponível: v{vm.AvailableVersion}";
        }
        else
        {
            UpdateBanner.Visibility = Visibility.Collapsed;
        }
        BtnInstallUpdate.IsEnabled = !vm.IsInstalling;
    }
}