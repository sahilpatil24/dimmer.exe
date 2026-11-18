namespace ScreenDimmer.Core;

/// <summary>Allowed ranges for the user-facing percentages.</summary>
public static class DimmerLimits
{
    public const int MinDim = 0;
    public const int MaxDim = 100;
    public const int MinTint = -100; // cool / blue
    public const int MaxTint = 100;  // warm / yellow
    public const int MinPanel = 0;
    public const int MaxPanel = 100;
}

/// <summary>Everything the user can tweak. Plain data, serialized as JSON.</summary>
public sealed record DimmerSettings
{
    /// <summary>Dim strength 0..100. 100 is the darkest allowed, never full black.</summary>
    public int Dim { get; init; } = 40;

    /// <summary>Whether the warm/cool color filter is applied.</summary>
    public bool FilterEnabled { get; init; }

    /// <summary>-100 (cool/blue) .. 0 (neutral) .. 100 (warm/yellow).</summary>
    public int Tint { get; init; } = 30;

    /// <summary>Also cap the laptop panel's own brightness while dimming.</summary>
    public bool PanelLimitEnabled { get; init; }

    /// <summary>Panel brightness cap, 0 (lowest) .. 100.</summary>
    public int PanelLimit { get; init; } = 10;

    /// <summary>Launch with Windows (to the tray).</summary>
    public bool StartWithWindows { get; init; }

    /// <summary>Turn dimming back on at launch if it was on when the app closed.</summary>
    public bool ResumeOnLaunch { get; init; } = true;

    public DimmerSettings Sanitized() => this with
    {
        Dim = Math.Clamp(Dim, DimmerLimits.MinDim, DimmerLimits.MaxDim),
        Tint = Math.Clamp(Tint, DimmerLimits.MinTint, DimmerLimits.MaxTint),
        PanelLimit = Math.Clamp(PanelLimit, DimmerLimits.MinPanel, DimmerLimits.MaxPanel),
    };
}

/// <summary>What gets written to disk: the settings plus whether dimming was on.</summary>
public sealed record PersistedState
{
    public DimmerSettings Settings { get; init; } = new();
    public bool Active { get; init; }
}
