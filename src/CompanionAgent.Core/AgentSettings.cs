namespace CompanionAgent.Core;

public sealed class AgentSettings
{
    public string SupabaseUrl { get; set; } = "https://nnhbowhfqjucedjnsvtp.supabase.co";
    public string SupabaseAnonKey { get; set; } = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Im5uaGJvd2hmcWp1Y2Vkam5zdnRwIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NzkyMzEwMjgsImV4cCI6MjA5NDgwNzAyOH0.83_32291NL4y4p4J8FjgpGTy2_XSO03PJ9edMgs8zy8";
    public string WebAppUrl { get; set; } = "https://sim-racing-companion.vercel.app";

    // Auth tokens
    public string UserToken { get; set; } = "";
    public string RefreshToken { get; set; } = "";

    // User info (cached from JWT)
    public string UserEmail { get; set; } = "";

    // Sync config
    public int SyncIntervalMinutes { get; set; } = 5;
    public bool AutoStart { get; set; } = true;
    public DateTimeOffset? LastSyncAt { get; set; }
    public int LastSyncSessionCount { get; set; }

    // NEW v3: App mode — "source" sends data, "viewer" only observes
    public string Mode { get; set; } = "source";

    // NEW v3: UI language — "pt-BR" or "en"
    public string Language { get; set; } = "pt-BR";

    // NEW v3: Data paths (empty = auto-detect via registry/defaults)
    public string AcPath { get; set; } = "";           // AC root folder (contains /content/cars, /content/tracks)
    public string CmSessionsPath { get; set; } = "";   // Content Manager Sessions folder
    public string PersonalBestPath { get; set; } = ""; // personalbest.ini path

    // NEW v3: Web sync poll interval
    public int WebSyncPollSeconds { get; set; } = 30;
}
