using System.Text.Json;

namespace BallsWar.Game;

public static class ConfigStorage
{
    private static readonly string Path = System.IO.Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, "balls-war-config.json");

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    /// Load config from file, or return defaults if file doesn't exist.
    public static GameConfig Load()
    {
        try
        {
            if (File.Exists(Path))
            {
                var json = File.ReadAllText(Path);
                var cfg = JsonSerializer.Deserialize<GameConfig>(json, JsonOpts);
                if (cfg != null) return cfg;
            }
        }
        catch { /* corrupt file → use defaults */ }

        return new GameConfig();
    }

    /// Save config to file.
    public static void Save(GameConfig config)
    {
        try
        {
            var json = JsonSerializer.Serialize(config, JsonOpts);
            File.WriteAllText(Path, json);
        }
        catch { /* silently fail — non-critical */ }
    }
}
