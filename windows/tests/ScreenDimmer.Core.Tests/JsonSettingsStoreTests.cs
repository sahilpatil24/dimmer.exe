using ScreenDimmer.Core;

namespace ScreenDimmer.Core.Tests;

public class JsonSettingsStoreTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "ScreenDimmerTests-" + Guid.NewGuid());

    private string FilePath => Path.Combine(_dir, "nested", "settings.json");

    public void Dispose()
    {
        if (Directory.Exists(_dir)) Directory.Delete(_dir, recursive: true);
    }

    [Fact]
    public void MissingFile_LoadsDefaults()
    {
        var state = new JsonSettingsStore(FilePath).Load();
        Assert.Equal(new DimmerSettings(), state.Settings);
        Assert.False(state.Active);
    }

    [Fact]
    public void SaveThenLoad_RoundTrips()
    {
        var store = new JsonSettingsStore(FilePath);
        var original = new PersistedState
        {
            Active = true,
            Settings = new DimmerSettings
            {
                Dim = 63,
                FilterEnabled = true,
                Tint = -42,
                PanelLimitEnabled = true,
                PanelLimit = 7,
                StartWithWindows = true,
                ResumeOnLaunch = false,
            },
        };

        store.Save(original);

        Assert.Equal(original, store.Load());
    }

    [Fact]
    public void CorruptFile_FallsBackToDefaults()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        File.WriteAllText(FilePath, "{ this is not json");

        var state = new JsonSettingsStore(FilePath).Load();

        Assert.Equal(new DimmerSettings(), state.Settings);
    }

    [Fact]
    public void OutOfRangeValuesOnDisk_AreClampedOnLoad()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        File.WriteAllText(FilePath, "{\"Settings\":{\"Dim\":999,\"Tint\":-999}}");

        var state = new JsonSettingsStore(FilePath).Load();

        Assert.Equal(100, state.Settings.Dim);
        Assert.Equal(-100, state.Settings.Tint);
    }
}
