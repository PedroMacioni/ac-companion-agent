namespace CompanionAgent.App;

using System.Drawing;
using System.Windows.Forms;

public sealed class TrayIconManager : IDisposable
{
    private readonly NotifyIcon _notifyIcon;
    private readonly Action _onOpen;
    private readonly Action _onSync;
    private readonly Action _onExit;

    public TrayIconManager(Action onOpen, Action onSync, Action onExit)
    {
        _onOpen = onOpen;
        _onSync = onSync;
        _onExit = onExit;

        _notifyIcon = new NotifyIcon
        {
            Text = "Sim Racing Companion",
            Icon = SystemIcons.Application,
            Visible = true
        };

        var menu = new ContextMenuStrip();
        menu.Items.Add("Abrir", null, (_, _) => _onOpen());
        menu.Items.Add("Sincronizar", null, (_, _) => _onSync());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Sair", null, (_, _) => _onExit());
        _notifyIcon.ContextMenuStrip = menu;

        _notifyIcon.DoubleClick += (_, _) => _onOpen();
    }

    public void ShowBalloon(string title, string text, ToolTipIcon icon = ToolTipIcon.Info) =>
        _notifyIcon.ShowBalloonTip(3000, title, text, icon);

    public void Dispose()
    {
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
    }
}
