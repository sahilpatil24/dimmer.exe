using ScreenDimmer.Core;

namespace ScreenDimmer.Core.Tests;

public class SettingsAndPresetsTests
{
    [Fact]
    public void Sanitized_ClampsEverything()
    {
        var s = new DimmerSettings { Dim = 250, Tint = -500, PanelLimit = -3 }.Sanitized();
        Assert.Equal(100, s.Dim);
        Assert.Equal(-100, s.Tint);
        Assert.Equal(0, s.PanelLimit);
    }

    [Fact]
    public void ApplyingPreset_KeepsStartupAndResumeFlags()
    {
        var baseSettings = new DimmerSettings { StartWithWindows = true, ResumeOnLaunch = false };
        foreach (var preset in DimmerPresets.All)
        {
            var applied = preset.ApplyTo(baseSettings);
            Assert.True(applied.StartWithWindows, preset.Id);
            Assert.False(applied.ResumeOnLaunch, preset.Id);
            Assert.True(preset.Matches(applied), preset.Id);
        }
    }

    [Fact]
    public void PresetIds_AreUnique()
    {
        var ids = DimmerPresets.All.Select(p => p.Id).ToHashSet();
        Assert.Equal(DimmerPresets.All.Count, ids.Count);
    }

    [Fact]
    public void Presets_NeverExceedTheDarknessCap()
    {
        foreach (var preset in DimmerPresets.All)
        {
            var alpha = OverlayMath.EffectiveAlpha(preset.ApplyTo(new DimmerSettings()));
            Assert.True(alpha < 0.85, preset.Id);
        }
    }
}
