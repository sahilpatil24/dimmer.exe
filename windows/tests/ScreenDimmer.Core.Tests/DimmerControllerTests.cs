using ScreenDimmer.Core;

namespace ScreenDimmer.Core.Tests;

public class DimmerControllerTests
{
    private readonly FakeOverlay _overlay = new();
    private readonly FakePanel _panel = new();
    private readonly FakeStartup _startup = new();

    private DimmerController Create(PersistedState? state, out FakeStore store)
    {
        store = new FakeStore(state);
        return new DimmerController(store, _overlay, _panel, _startup, TimeSpan.FromMilliseconds(50));
    }

    [Fact]
    public void StartsInactive_ByDefault()
    {
        using var c = Create(null, out _);
        Assert.False(c.IsActive);
        Assert.False(_overlay.Visible);
    }

    [Fact]
    public void Start_ShowsOverlayWithComputedColor_AndStopHidesIt()
    {
        using var c = Create(null, out _);

        c.Start();
        Assert.True(_overlay.Visible);
        Assert.Equal(OverlayMath.ComputeColor(c.Settings), _overlay.LastColor);

        c.Stop();
        Assert.False(_overlay.Visible);
    }

    [Fact]
    public void ChangingSettingsWhileActive_UpdatesTheOverlay()
    {
        using var c = Create(null, out _);
        c.Start();
        var before = _overlay.LastColor;

        c.Update(s => s with { Dim = 90 });

        Assert.NotEqual(before, _overlay.LastColor);
        Assert.Equal(OverlayMath.ComputeColor(c.Settings), _overlay.LastColor);
    }

    [Fact]
    public void ChangingSettingsWhileInactive_DoesNotShowTheOverlay()
    {
        using var c = Create(null, out _);
        c.Update(s => s with { Dim = 90 });
        Assert.False(_overlay.Visible);
    }

    [Fact]
    public void PanelLimit_IsAppliedOnlyWhileActive()
    {
        using var c = Create(null, out _);
        c.Update(s => s with { PanelLimitEnabled = true, PanelLimit = 15 });
        Assert.Empty(_panel.Limits);

        c.Start();
        Assert.Equal(15, _panel.Limits.Last());

        var restoresBefore = _panel.Restores;
        c.Stop();
        Assert.True(_panel.Restores > restoresBefore);
    }

    [Fact]
    public void PanelLimit_IsSkippedWhenUnsupported()
    {
        _panel.IsSupported = false;
        using var c = Create(null, out _);
        c.Update(s => s with { PanelLimitEnabled = true, PanelLimit = 15 });

        c.Start();

        Assert.Empty(_panel.Limits);
    }

    [Fact]
    public void ResumeOnLaunch_RestoresActiveState()
    {
        var saved = new PersistedState { Active = true, Settings = new DimmerSettings { ResumeOnLaunch = true } };
        using var c = Create(saved, out _);
        Assert.True(c.IsActive);
        Assert.True(_overlay.Visible);
    }

    [Fact]
    public void ResumeOnLaunchOff_StartsInactiveEvenIfWasActive()
    {
        var saved = new PersistedState { Active = true, Settings = new DimmerSettings { ResumeOnLaunch = false } };
        using var c = Create(saved, out _);
        Assert.False(c.IsActive);
        Assert.False(_overlay.Visible);
    }

    [Fact]
    public void StartWithWindows_IsForwardedToTheRegistration()
    {
        using var c = Create(null, out _);

        c.Update(s => s with { StartWithWindows = true });

        Assert.True(_startup.Calls.Last());
    }

    [Fact]
    public void Update_ClampsValues_AndRaisesChanged()
    {
        using var c = Create(null, out _);
        var raised = 0;
        c.Changed += (_, _) => raised++;

        c.Update(s => s with { Dim = 500 });

        Assert.Equal(100, c.Settings.Dim);
        Assert.Equal(1, raised);
    }

    [Fact]
    public void NoOpUpdate_DoesNotRaiseChanged()
    {
        using var c = Create(null, out _);
        var raised = 0;
        c.Changed += (_, _) => raised++;

        c.Update(s => s);

        Assert.Equal(0, raised);
    }

    [Fact]
    public void ApplyPreset_UpdatesSettings()
    {
        using var c = Create(null, out _);
        var night = DimmerPresets.All.First(p => p.Id == "night");

        c.ApplyPreset(night);

        Assert.True(night.Matches(c.Settings));
    }

    [Fact]
    public async Task RapidChanges_AreSavedOnceWithTheFinalValue()
    {
        using var c = Create(null, out var store);

        c.Update(s => s with { Dim = 10 });
        c.Update(s => s with { Dim = 20 });
        c.Update(s => s with { Dim = 30 });
        await Task.Delay(400);

        Assert.Single(store.Saved);
        Assert.Equal(30, store.Saved[0].Settings.Dim);
    }

    [Fact]
    public void Dispose_FlushesSaveHidesOverlayAndRestoresPanel()
    {
        var c = Create(null, out var store);
        c.Start();
        c.Update(s => s with { Dim = 66 });

        c.Dispose();

        Assert.False(_overlay.Visible);
        Assert.True(_panel.Restores > 0);
        Assert.Equal(66, store.Saved.Last().Settings.Dim);
        Assert.True(store.Saved.Last().Active);
    }
}
