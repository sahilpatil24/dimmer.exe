namespace ScreenDimmer.Core;

/// <summary>Draws the dimming layer on every monitor.</summary>
public interface IOverlay
{
    /// <summary>Shows the overlay, or updates its color if already showing.</summary>
    void Show(OverlayColor color);

    void Hide();
}

/// <summary>Controls the built-in (laptop) panel brightness.</summary>
public interface IPanelBrightness
{
    bool IsSupported { get; }

    /// <summary>Remembers the current brightness the first time, then sets <paramref name="percent"/>.</summary>
    void Limit(int percent);

    /// <summary>Puts the brightness back to what it was. Safe to call repeatedly.</summary>
    void Restore();
}

/// <summary>Registers / unregisters the app to launch with Windows.</summary>
public interface IStartupRegistration
{
    void SetEnabled(bool enabled);
}

/// <summary>Loads and saves <see cref="PersistedState"/>.</summary>
public interface ISettingsStore
{
    PersistedState Load();
    void Save(PersistedState state);
}
