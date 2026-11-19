namespace ScreenDimmer.Core;

/// <summary>A straight (non-premultiplied) ARGB color for the overlay.</summary>
public readonly record struct OverlayColor(byte A, byte R, byte G, byte B)
{
    public static OverlayColor Transparent => new(0, 0, 0, 0);
    public bool IsTransparent => A == 0;
}

/// <summary>
/// Turns <see cref="DimmerSettings"/> into the single color drawn by the overlay.
/// A tint layer and a black layer are composed into one ARGB value.
/// The caps guarantee the screen is never fully black: the darkest possible
/// result is about 84% opaque (0.78 + 0.28 * (1 - 0.78)).
/// </summary>
public static class OverlayMath
{
    public const double MaxDimAlpha = 0.78;
    public const double MaxTintAlpha = 0.28;

    private static readonly (int R, int G, int B) Warm = (255, 138, 30);
    private static readonly (int R, int G, int B) Cool = (70, 130, 255);

    /// <summary>Effective on-screen opacity of the combined overlay (0..~0.84).</summary>
    public static double EffectiveAlpha(DimmerSettings settings)
    {
        var s = settings.Sanitized();
        var d = DimAlpha(s);
        var t = TintAlpha(s);
        return d + t * (1.0 - d);
    }

    public static OverlayColor ComputeColor(DimmerSettings settings)
    {
        var s = settings.Sanitized();
        var d = DimAlpha(s);
        var t = TintAlpha(s);
        var total = d + t * (1.0 - d);
        if (total <= 0.0005) return OverlayColor.Transparent;

        var rgb = s.Tint >= 0 ? Warm : Cool;
        // Black adds no color, so only the tint layer is weighted.
        var tintWeight = t * (1.0 - d) / total;
        return new OverlayColor(
            (byte)Math.Round(total * 255.0),
            (byte)Math.Round(rgb.R * tintWeight),
            (byte)Math.Round(rgb.G * tintWeight),
            (byte)Math.Round(rgb.B * tintWeight));
    }

    /// <summary>Panel brightness cap 0..100, or null when the cap is off.</summary>
    public static int? PanelBrightness(DimmerSettings settings)
    {
        var s = settings.Sanitized();
        return s.PanelLimitEnabled ? s.PanelLimit : null;
    }

    private static double DimAlpha(DimmerSettings s) => s.Dim / 100.0 * MaxDimAlpha;

    private static double TintAlpha(DimmerSettings s) =>
        s.FilterEnabled ? Math.Abs(s.Tint) / 100.0 * MaxTintAlpha : 0.0;
}
