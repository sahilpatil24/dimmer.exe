namespace ScreenDimmer.Core;

/// <summary>A named combination of dim level, color filter and optional panel cap.</summary>
public sealed record DimmerPreset(
    string Id,
    string Label,
    int Dim,
    bool FilterEnabled,
    int Tint,
    int? PanelLimit = null)
{
    /// <summary>Applies the preset but keeps unrelated settings (startup, resume).</summary>
    public DimmerSettings ApplyTo(DimmerSettings current) => current with
    {
        Dim = Dim,
        FilterEnabled = FilterEnabled,
        Tint = Tint,
        PanelLimitEnabled = PanelLimit.HasValue,
        PanelLimit = PanelLimit ?? current.PanelLimit,
    };

    public bool Matches(DimmerSettings s) =>
        s.Dim == Dim &&
        s.FilterEnabled == FilterEnabled &&
        (!FilterEnabled || s.Tint == Tint) &&
        s.PanelLimitEnabled == PanelLimit.HasValue &&
        (!PanelLimit.HasValue || s.PanelLimit == PanelLimit.Value);
}

public static class DimmerPresets
{
    public static IReadOnlyList<DimmerPreset> All { get; } = new[]
    {
        new DimmerPreset("reading", "Reading", 15, true, 25),
        new DimmerPreset("cinema", "Cinema", 45, false, 0),
        new DimmerPreset("evening", "Evening", 30, true, 40),
        new DimmerPreset("night", "Night", 55, true, 60),
        new DimmerPreset("deep_night", "Deep night", 75, true, 75, 20),
        new DimmerPreset("maximum", "Maximum", 100, true, 80, 0),
    };
}
