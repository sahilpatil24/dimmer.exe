using System.Text.Json;

namespace ScreenDimmer.Core;

/// <summary>Stores state as JSON, e.g. in %AppData%\ScreenDimmer\settings.json.</summary>
public sealed class JsonSettingsStore : ISettingsStore
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    private readonly string _path;

    public JsonSettingsStore(string path) => _path = path;

    public static string DefaultPath() => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "ScreenDimmer",
        "settings.json");

    public PersistedState Load()
    {
        try
        {
            if (!File.Exists(_path)) return new PersistedState();
            var state = JsonSerializer.Deserialize<PersistedState>(File.ReadAllText(_path), Options);
            return state is null
                ? new PersistedState()
                : state with { Settings = state.Settings.Sanitized() };
        }
        catch (Exception)
        {
            // Corrupt or unreadable file: fall back to defaults rather than crash.
            return new PersistedState();
        }
    }

    public void Save(PersistedState state)
    {
        var dir = Path.GetDirectoryName(_path);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

        var tmp = _path + ".tmp";
        File.WriteAllText(tmp, JsonSerializer.Serialize(state, Options));
        File.Move(tmp, _path, overwrite: true);
    }
}
