using ScreenDimmer.Core;

namespace ScreenDimmer.Core.Tests;

public class OverlayMathTests
{
    [Fact]
    public void NoDimNoFilter_IsFullyTransparent()
    {
        var color = OverlayMath.ComputeColor(new DimmerSettings { Dim = 0, FilterEnabled = false });
        Assert.True(color.IsTransparent);
    }

    [Fact]
    public void MaximumSettings_NeverGoFullyBlack()
    {
        var worst = new DimmerSettings { Dim = 100, FilterEnabled = true, Tint = 100 };

        var alpha = OverlayMath.EffectiveAlpha(worst);
        var color = OverlayMath.ComputeColor(worst);

        Assert.True(alpha < 0.85, $"alpha was {alpha}");
        Assert.True(color.A < 255);
    }

    [Fact]
    public void DimLevel_IsMonotonic()
    {
        double previous = -1;
        for (var dim = 0; dim <= 100; dim += 10)
        {
            var alpha = OverlayMath.EffectiveAlpha(new DimmerSettings { Dim = dim });
            Assert.True(alpha > previous || dim == 0);
            previous = alpha;
        }
    }

    [Fact]
    public void WarmTint_IsRedderThanBlue()
    {
        var c = OverlayMath.ComputeColor(new DimmerSettings { Dim = 20, FilterEnabled = true, Tint = 80 });
        Assert.True(c.R > c.B);
    }

    [Fact]
    public void CoolTint_IsBluerThanRed()
    {
        var c = OverlayMath.ComputeColor(new DimmerSettings { Dim = 20, FilterEnabled = true, Tint = -80 });
        Assert.True(c.B > c.R);
    }

    [Fact]
    public void NeutralTint_AddsNoColor()
    {
        var c = OverlayMath.ComputeColor(new DimmerSettings { Dim = 50, FilterEnabled = true, Tint = 0 });
        Assert.Equal(0, c.R);
        Assert.Equal(0, c.G);
        Assert.Equal(0, c.B);
        Assert.True(c.A > 0);
    }

    [Fact]
    public void FilterOnly_StillProducesAVisibleTint()
    {
        var c = OverlayMath.ComputeColor(new DimmerSettings { Dim = 0, FilterEnabled = true, Tint = 60 });
        Assert.True(c.A > 0);
        Assert.True(c.R > 0);
    }

    [Fact]
    public void OutOfRangeValues_AreClamped()
    {
        var c = OverlayMath.ComputeColor(new DimmerSettings { Dim = 900, FilterEnabled = true, Tint = 900 });
        Assert.True(c.A < 255);
    }

    [Fact]
    public void PanelBrightness_OnlyWhenEnabled()
    {
        Assert.Null(OverlayMath.PanelBrightness(new DimmerSettings { PanelLimitEnabled = false, PanelLimit = 30 }));
        Assert.Equal(30, OverlayMath.PanelBrightness(new DimmerSettings { PanelLimitEnabled = true, PanelLimit = 30 }));
    }
}
