namespace CompanionAgent.App.Views;

using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using CompanionAgent.Core;
using Microsoft.Win32;

public partial class SettingsTab : UserControl
{
    public SettingsTab()
    {
        InitializeComponent();

        // Wire all event handlers once in constructor — never in Loaded
        SliderPoll.ValueChanged += (_, e) => TxtPollValue.Text = $"{(int)e.NewValue}s";
        ComboLang.SelectionChanged += (_, _) =>
        {
            var selected = (ComboLang.SelectedItem as ComboBoxItem)?.Tag as string ?? "pt-BR";
            LangRestartHint.Visibility = selected != I18n.CurrentLanguage
                ? Visibility.Visible : Visibility.Collapsed;
        };
        BtnDetectAc.Click += (_, _) => TxtAcPath.Text = DetectAcPath() ?? TxtAcPath.Text;
        BtnDetectCm.Click += (_, _) => TxtCmPath.Text = DetectCmSessionsPath() ?? TxtCmPath.Text;
        BtnDetectPb.Click += (_, _) => TxtPbPath.Text = DetectPersonalBestPath() ?? TxtPbPath.Text;
        BtnBrowseAc.Click += (_, _) => BrowseFolder(TxtAcPath);
        BtnBrowseCm.Click += (_, _) => BrowseFolder(TxtCmPath);
        BtnBrowsePb.Click += (_, _) => BrowseFile(TxtPbPath, "INI files|*.ini|All files|*.*");
        BtnSave.Click += (_, _) => SaveSettings();
        BtnLogout.Click += (_, _) => DoLogout();
        BtnConnectViaWeb.Click += async (_, _) => await DoDeviceAuthAsync();
        BtnCancelDeviceAuth.Click += (_, _) => CancelDeviceAuth();
        ChkEmailLogin.Checked += (_, _) => PanelEmailLogin.Visibility = Visibility.Visible;
        ChkEmailLogin.Unchecked += (_, _) => PanelEmailLogin.Visibility = Visibility.Collapsed;
        BtnLogin.Click += async (_, _) => await DoLoginAsync();

        Loaded += (_, _) => LoadValues();
    }

    private void LoadValues()
    {
        var vm = App.SettingsVm;

        TxtAcPath.Text = vm.AcPath;
        TxtCmPath.Text = vm.CmSessionsPath;
        TxtPbPath.Text = vm.PersonalBestPath;
        SliderPoll.Value = Math.Clamp(vm.PollSeconds, 10, 120);
        TxtPollValue.Text = $"{vm.PollSeconds}s";
        ChkAutoStart.IsChecked = vm.AutoStart;

        foreach (ComboBoxItem item in ComboMode.Items)
            if ((string)item.Tag == vm.Mode) { ComboMode.SelectedItem = item; break; }
        if (ComboMode.SelectedItem == null) ComboMode.SelectedIndex = 0;

        foreach (ComboBoxItem item in ComboLang.Items)
            if ((string)item.Tag == vm.Language) { ComboLang.SelectedItem = item; break; }
        if (ComboLang.SelectedItem == null) ComboLang.SelectedIndex = 0;

        UpdateAuthView();
    }

    private void SaveSettings()
    {
        var vm = App.SettingsVm;
        vm.AcPath = TxtAcPath.Text.Trim();
        vm.CmSessionsPath = TxtCmPath.Text.Trim();
        vm.PersonalBestPath = TxtPbPath.Text.Trim();
        vm.Mode = (ComboMode.SelectedItem as ComboBoxItem)?.Tag as string ?? "source";
        vm.Language = (ComboLang.SelectedItem as ComboBoxItem)?.Tag as string ?? "pt-BR";
        vm.AutoStart = ChkAutoStart.IsChecked == true;
        vm.PollSeconds = (int)SliderPoll.Value;

        var settings = App.Settings;
        vm.ApplyTo(settings);
        SettingsStore.Save(settings);

        if (settings.AutoStart) AutoStartManager.Enable();
        else AutoStartManager.Disable();

        App.Log.Info(LogCategory.App, "Configurações salvas");

        if (LangRestartHint.Visibility == Visibility.Visible)
            MessageBox.Show("Idioma alterado. Reinicie o aplicativo para aplicar.", "Sim Racing Companion",
                MessageBoxButton.OK, MessageBoxImage.Information);
        else
            MessageBox.Show("Configurações salvas.", "Sim Racing Companion",
                MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private static string? DetectAcPath()
    {
        try
        {
            string? steamPath = null;
#pragma warning disable CA1416
            using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Valve\Steam"))
                steamPath = key?.GetValue("SteamPath") as string;
#pragma warning restore CA1416

            steamPath ??= new[] { @"C:\Program Files (x86)\Steam", @"C:\Program Files\Steam" }
                .FirstOrDefault(Directory.Exists);

            if (steamPath == null) return null;
            var path = Path.Combine(steamPath, "steamapps", "common", "assettocorsa");
            return Directory.Exists(path) ? path : null;
        }
        catch { return null; }
    }

    private static string? DetectCmSessionsPath()
    {
        var path = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AcTools Content Manager", "Progress", "Sessions");
        return Directory.Exists(path) ? path : null;
    }

    private static string? DetectPersonalBestPath()
    {
        var path = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "Assetto Corsa", "personalbest.ini");
        return File.Exists(path) ? path : null;
    }

    private static void BrowseFolder(TextBox target)
    {
        using var dialog = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = "Selecionar pasta",
            UseDescriptionForTitle = true
        };
        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            target.Text = dialog.SelectedPath;
    }

    private static void BrowseFile(TextBox target, string filter)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Selecionar arquivo",
            Filter = filter
        };
        if (dialog.ShowDialog() == true)
            target.Text = dialog.FileName;
    }

    private void UpdateAuthView()
    {
        var connected = App.Supabase?.IsConfigured ?? false;
        PanelLoggedIn.Visibility = connected ? Visibility.Visible : Visibility.Collapsed;
        PanelLogin.Visibility = connected ? Visibility.Collapsed : Visibility.Visible;
        if (connected) TxtLoggedAs.Text = $"Conectado como {App.Settings.UserEmail}";
        if (!connected)
        {
            TxtDeviceAuthStatus.Visibility = Visibility.Collapsed;
            BtnCancelDeviceAuth.Visibility = Visibility.Collapsed;
            BtnConnectViaWeb.IsEnabled = true;
        }
    }

    private void DoLogout()
    {
        App.Supabase?.ClearTokens();
        App.Settings.UserToken = "";
        App.Settings.RefreshToken = "";
        App.Settings.UserEmail = "";
        SettingsStore.Save(App.Settings);
        App.Worker?.Dispose();
        App.OverviewVm.IsConnected = false;
        App.OverviewVm.UserEmail = "";
        App.Log.Info(LogCategory.Auth, "Usuário desconectado");
        UpdateAuthView();
    }

    private CancellationTokenSource? _deviceAuthCts;

    private async Task DoDeviceAuthAsync()
    {
        BtnConnectViaWeb.IsEnabled = false;
        TxtDeviceAuthStatus.Visibility = Visibility.Visible;
        BtnCancelDeviceAuth.Visibility = Visibility.Visible;

        _deviceAuthCts = new CancellationTokenSource();

        using var service = new DeviceAuthService(App.Settings.WebAppUrl);
        service.StatusChanged += msg =>
            Dispatcher.Invoke(() => TxtDeviceAuthStatus.Text = msg);

        try
        {
            var result = await service.AuthenticateAsync(_deviceAuthCts.Token);
            if (result == null)
            {
                // Status already set by service
            }
            else
            {
                App.Supabase.SetTokens(result.Value.AccessToken, result.Value.RefreshToken);
                if (App.Supabase.IsConfigured)
                {
                    App.Settings.UserToken = result.Value.AccessToken;
                    App.Settings.RefreshToken = result.Value.RefreshToken;
                    App.Settings.UserEmail = App.Supabase.UserEmail;
                    SettingsStore.Save(App.Settings);
                    App.OverviewVm.IsConnected = true;
                    App.OverviewVm.UserEmail = App.Supabase.UserEmail;
                    App.Log.Success(LogCategory.Auth, $"Conectado como {App.Supabase.UserEmail} via site");
                    if (App.Settings.Mode == "source")
                        App.StartSyncWorker();
                    UpdateAuthView();
                }
            }
        }
        catch (OperationCanceledException)
        {
            TxtDeviceAuthStatus.Text = "Cancelado.";
        }
        catch (Exception ex)
        {
            TxtDeviceAuthStatus.Text = $"Erro: {ex.Message}";
            App.Log.Error(LogCategory.Auth, $"Falha na autenticação via site: {ex.Message}");
        }
        finally
        {
            BtnConnectViaWeb.IsEnabled = true;
            BtnCancelDeviceAuth.Visibility = Visibility.Collapsed;
            _deviceAuthCts?.Dispose();
            _deviceAuthCts = null;
        }
    }

    private void CancelDeviceAuth()
    {
        _deviceAuthCts?.Cancel();
    }

    private async Task DoLoginAsync()
    {
        var email = TxtEmail.Text.Trim();
        var password = TxtPassword.Password;
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            TxtLoginError.Text = "Preencha e-mail e senha.";
            return;
        }
        BtnLogin.IsEnabled = false;
        TxtLoginError.Text = "Entrando...";
        try
        {
            var result = await App.Supabase.SignInAsync(email, password);
            if (result == null)
            {
                TxtLoginError.Text = "E-mail ou senha incorretos.";
                return;
            }
            App.Settings.UserToken = result.Value.AccessToken;
            App.Settings.RefreshToken = result.Value.RefreshToken;
            App.Settings.UserEmail = App.Supabase.UserEmail;
            SettingsStore.Save(App.Settings);
            App.OverviewVm.IsConnected = true;
            App.OverviewVm.UserEmail = App.Supabase.UserEmail;
            App.Log.Success(LogCategory.Auth, $"Entrou como {email}");
            if (App.Settings.Mode == "source")
                App.StartSyncWorker();
            TxtLoginError.Text = "";
            UpdateAuthView();
        }
        catch (Exception ex)
        {
            TxtLoginError.Text = $"Erro: {ex.Message}";
            App.Log.Error(LogCategory.Auth, $"Falha no login: {ex.Message}");
        }
        finally { BtnLogin.IsEnabled = true; }
    }
}
