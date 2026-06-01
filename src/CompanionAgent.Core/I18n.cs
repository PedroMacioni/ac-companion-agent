namespace CompanionAgent.Core;

using System.Text.Json;

public static class I18n
{
    private static Dictionary<string, string> _strings = [];
    private static string _currentLanguage = "pt-BR";

    public static void Load(string language)
    {
        _currentLanguage = language;
        try
        {
            var path = Path.Combine(AppContext.BaseDirectory, "resources", "strings", $"{language}.json");
            if (!File.Exists(path))
                path = Path.Combine(AppContext.BaseDirectory, "resources", "strings", "pt-BR.json");

            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                _strings = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? [];
            }
        }
        catch { _strings = []; }
    }

    public static string T(string key, params object[] args)
    {
        if (_strings.TryGetValue(key, out var template))
            return args.Length > 0 ? string.Format(template, args) : template;
        return key;
    }

    public static string CurrentLanguage => _currentLanguage;
}
