using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json.Serialization;

namespace CompanionAgent.Core;

public sealed class UpdateService : IDisposable
{
    private const string ApiUrl = "https://api.github.com/repos/PedroMacioni/ac-companion-agent/releases/latest";
    private readonly HttpClient _http = new();
    private UpdateInfo? _pending;

    public string CurrentVersion { get; } = ReadVersion();

    private static string ReadVersion()
    {
        var v = Assembly.GetEntryAssembly()?.GetName().Version;
        return v is null ? "0.0.0" : $"{v.Major}.{v.Minor}.{v.Build}";
    }

    public async Task<UpdateInfo?> CheckAsync(CancellationToken ct = default)
    {
        try
        {
            _http.DefaultRequestHeaders.UserAgent.ParseAdd("SimRacingCompanion-Updater/1.0");
            var release = await _http.GetFromJsonAsync<GhRelease>(ApiUrl, ct);
            if (release is null) return null;

            var latest = release.TagName.TrimStart('v');
            if (!IsNewer(latest, CurrentVersion)) return null;

            var asset = release.Assets.FirstOrDefault(a =>
                a.Name.StartsWith("SimRacingCompanion-Setup") && a.Name.EndsWith(".exe"));
            if (asset is null) return null;

            _pending = new UpdateInfo(latest, asset.BrowserDownloadUrl, release.HtmlUrl);
            return _pending;
        }
        catch { return null; }
    }

    public async Task DownloadAndInstallAsync(CancellationToken ct = default)
    {
        if (_pending is null) return;
        var dest = Path.Combine(Path.GetTempPath(), $"SimRacingCompanion-Setup-{_pending.Version}.exe");
        var bytes = await _http.GetByteArrayAsync(_pending.DownloadUrl, ct);
        await File.WriteAllBytesAsync(dest, bytes, ct);
        Process.Start(new ProcessStartInfo(dest) { UseShellExecute = true });
    }

    private static bool IsNewer(string latest, string current)
    {
        if (!Version.TryParse(latest, out var l)) return false;
        if (!Version.TryParse(current, out var c)) return false;
        return l > c;
    }

    public void Dispose() => _http.Dispose();

    private record GhRelease(
        [property: JsonPropertyName("tag_name")]  string TagName,
        [property: JsonPropertyName("html_url")]  string HtmlUrl,
        [property: JsonPropertyName("assets")]    List<GhAsset> Assets);

    private record GhAsset(
        [property: JsonPropertyName("name")]                   string Name,
        [property: JsonPropertyName("browser_download_url")]   string BrowserDownloadUrl);
}

public record UpdateInfo(string Version, string DownloadUrl, string ReleaseUrl);