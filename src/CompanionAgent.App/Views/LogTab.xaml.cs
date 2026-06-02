namespace CompanionAgent.App.Views;

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CompanionAgent.Core;

public partial class LogTab : UserControl
{
    private ObservableCollection<LogEntryDisplay> _displayEntries = [];
    private string _filterTag = "";

    public LogTab()
    {
        InitializeComponent();
        Loaded += (_, _) => Setup();
    }

    private void Setup()
    {
        LogList.ItemsSource = _displayEntries;

        FilterCombo.SelectedIndex = 0;
        FilterCombo.SelectionChanged += (_, _) =>
        {
            _filterTag = (FilterCombo.SelectedItem as ComboBoxItem)?.Tag as string ?? "";
            RefreshDisplay();
        };

        BtnClear.Click += (_, _) =>
        {
            App.LogVm.Clear();
            _displayEntries.Clear();
        };

        BtnOpenFile.Click += (_, _) =>
        {
            var path = App.Log.GetTodayLogPath();
            if (File.Exists(path))
                Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            else
                MessageBox.Show("Nenhum log encontrado para hoje.", "Log", MessageBoxButton.OK, MessageBoxImage.Information);
        };

        App.LogVm.AllEntries.CollectionChanged += (_, _) =>
        {
            if (App.LogVm.AllEntries.Count == 0)
            {
                _displayEntries.Clear();
                return;
            }
            var last = App.LogVm.AllEntries[^1];
            if (MatchesFilter(last))
            {
                var display = ToDisplay(last);
                _displayEntries.Add(display);
                Dispatcher.InvokeAsync(() => LogList.ScrollIntoView(display));
            }
        };

        // Load existing entries
        RefreshDisplay();
    }

    private void RefreshDisplay()
    {
        _displayEntries.Clear();
        foreach (var entry in App.LogVm.AllEntries)
            if (MatchesFilter(entry))
                _displayEntries.Add(ToDisplay(entry));

        if (_displayEntries.Count > 0)
            Dispatcher.InvokeAsync(() => LogList.ScrollIntoView(_displayEntries[^1]));
    }

    private bool MatchesFilter(LogEntry entry)
    {
        if (string.IsNullOrEmpty(_filterTag)) return true;
        return entry.Level.ToString() == _filterTag;
    }

    private static LogEntryDisplay ToDisplay(LogEntry e)
    {
        var levelStr = e.Level switch
        {
            LogLevel.Info => "INFO ",
            LogLevel.Warning => "WARN ",
            LogLevel.Error => "ERROR",
            LogLevel.Success => "OK   ",
            _ => "INFO "
        };
        var color = e.Level switch
        {
            LogLevel.Success => new SolidColorBrush(Color.FromRgb(0x22, 0xC5, 0x5E)),
            LogLevel.Warning => new SolidColorBrush(Color.FromRgb(0xF5, 0x9E, 0x0B)),
            LogLevel.Error => new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44)),
            _ => new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88))
        };
        return new LogEntryDisplay(
            $"{e.Timestamp:HH:mm:ss} {levelStr} [{e.Category}] {e.Message}",
            color);
    }
}

public sealed record LogEntryDisplay(string DisplayText, Brush LevelColor);
