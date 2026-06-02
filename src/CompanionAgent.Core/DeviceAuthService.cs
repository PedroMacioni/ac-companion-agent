namespace CompanionAgent.Core;

using System.Diagnostics;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

public sealed class DeviceAuthService : IDisposable
{
    private readonly string _webAppUrl;
    private readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(15) };

    public event Action<string>? StatusChanged;

    public DeviceAuthService(string webAppUrl)
    {
        _webAppUrl = webAppUrl.TrimEnd('/');
    }

    public async Task<(string AccessToken, string RefreshToken)?> AuthenticateAsync(
        CancellationToken ct = default)
    {
        // 1. Generate credentials
        var deviceId = Guid.NewGuid().ToString("N")[..16];
        var secret   = Guid.NewGuid().ToString("N");
        var deviceName = Environment.MachineName;

        // 2. Create pending request on server
        StatusChanged?.Invoke("Criando solicitação de conexão...");
        try
        {
            var payload = JsonSerializer.Serialize(new { device_id = deviceId, device_secret = secret, device_name = deviceName });
            var resp = await _http.PostAsync(
                $"{_webAppUrl}/api/device-auth",
                new StringContent(payload, Encoding.UTF8, "application/json"), ct);

            if (!resp.IsSuccessStatusCode)
            {
                StatusChanged?.Invoke($"Erro ao criar solicitação: {(int)resp.StatusCode}");
                return null;
            }
        }
        catch (Exception ex)
        {
            StatusChanged?.Invoke($"Erro de rede: {ex.Message}");
            return null;
        }

        // 3. Open browser
        StatusChanged?.Invoke("Abrindo navegador — aprove no site...");
        try
        {
            Process.Start(new ProcessStartInfo($"{_webAppUrl}/connect/{deviceId}") { UseShellExecute = true });
        }
        catch
        {
            StatusChanged?.Invoke("Abra manualmente: " + $"{_webAppUrl}/connect/{deviceId}");
        }

        // 4. Poll until approved or timeout (10 min = 120 polls × 5s)
        var pollUrl = $"{_webAppUrl}/api/device-auth/{deviceId}?secret={Uri.EscapeDataString(secret)}";
        var deadline = DateTime.UtcNow.AddMinutes(10);
        var dots = 0;

        while (DateTime.UtcNow < deadline)
        {
            ct.ThrowIfCancellationRequested();
            await Task.Delay(5000, ct);

            dots = (dots + 1) % 4;
            StatusChanged?.Invoke("Aguardando aprovação no site" + new string('.', dots + 1));

            try
            {
                var pollResp = await _http.GetAsync(pollUrl, ct);

                if (pollResp.StatusCode == System.Net.HttpStatusCode.Gone ||
                    pollResp.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    StatusChanged?.Invoke("Solicitação expirada. Tente novamente.");
                    return null;
                }

                if (!pollResp.IsSuccessStatusCode) continue;

                var json = await pollResp.Content.ReadAsStringAsync(ct);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                var status = root.TryGetProperty("status", out var s) ? s.GetString() : null;
                if (status != "approved") continue;

                var at = root.TryGetProperty("access_token", out var a) ? a.GetString() : null;
                var rt = root.TryGetProperty("refresh_token", out var r) ? r.GetString() : null;

                if (string.IsNullOrEmpty(at) || string.IsNullOrEmpty(rt)) continue;

                StatusChanged?.Invoke("✓ Aprovado! Conectando...");

                // 5. Clean up the request
                try { await _http.DeleteAsync(pollUrl, CancellationToken.None); } catch { }

                return (at, rt);
            }
            catch (OperationCanceledException) { throw; }
            catch { /* poll failure is non-fatal, keep trying */ }
        }

        StatusChanged?.Invoke("Tempo esgotado. Tente novamente.");
        return null;
    }

    public void Dispose() => _http.Dispose();
}
