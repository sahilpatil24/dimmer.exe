using ScreenDimmer.Core;

namespace ScreenDimmer.Core.Tests;

internal sealed class FakeOverlay : IOverlay
{
    public bool Visible { get; private set; }
    public OverlayColor LastColor { get; private set; }
    public int ShowCalls { get; private set; }

    public void Show(OverlayColor color)
    {
        Visible = true;
        LastColor = color;
        ShowCalls++;
    }

    public void Hide() => Visible = false;
}

internal sealed class FakePanel : IPanelBrightness
{
    public bool IsSupported { get; set; } = true;
    public List<int> Limits { get; } = new();
    public int Restores { get; private set; }

    public void Limit(int percent) => Limits.Add(percent);
    public void Restore() => Restores++;
}

internal sealed class FakeStartup : IStartupRegistration
{
    public List<bool> Calls { get; } = new();
    public void SetEnabled(bool enabled) => Calls.Add(enabled);
}

internal sealed class FakeStore : ISettingsStore
{
    private readonly object _gate = new();
    private readonly List<PersistedState> _saved = new();

    public FakeStore(PersistedState? initial = null) => Initial = initial ?? new PersistedState();

    public PersistedState Initial { get; }

    public IReadOnlyList<PersistedState> Saved
    {
        get { lock (_gate) return _saved.ToList(); }
    }

    public PersistedState Load() => Initial;

    public void Save(PersistedState state)
    {
        lock (_gate) _saved.Add(state);
    }
}
