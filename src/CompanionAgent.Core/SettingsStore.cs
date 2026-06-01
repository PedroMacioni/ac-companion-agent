namespace CompanionAgent.Core;

using System.Text.Json;

public static class SettingsStore
{
    public static readonly string SettingsPath = System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "SimRacingCompanion", "settings.json");

    private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = true };

    public static AgentSettings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
                return JsonSerializer.Deserialize<AgentSettings>(File.ReadAllText(SettingsPath)) ?? new AgentSettings();
        }
        catch { }
        return new AgentSettings();
    }

    public static void Save(AgentSettings settings)
    {
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(SettingsPath)!);
        File.WriteAllText(SettingsPath, JsonSerializer.Serialize(settings, JsonOpts));
    }
}
