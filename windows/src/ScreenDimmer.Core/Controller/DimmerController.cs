namespace ScreenDimmer.Core;

/// <summary>
/// The brain of the app: owns settings and on/off state and pushes them to the
/// overlay, panel brightness and startup registration. UI, tray and hotkeys
/// all talk to this one class. Call it from a single (UI) thread.
/// </summary>
public sealed class DimmerController : IDisposable
{
    private static readonly TimeSpan DefaultSaveDelay = TimeSpan.FromMilliseconds(400);

    private readonly ISettingsStore _store;
    private readonly IOverlay _overlay;
    private readonly IPanelBrightness _panel;
    private readonly IStartupRegistration _startup;
    private readonly TimeSpan _saveDelay;
    private readonly object _saveGate = new();
    private Timer? _saveTimer;
    private bool _disposed;

    public DimmerController(
        ISettingsStore store,
        IOverlay overlay,
        IPanelBrightness panel,
        IStartupRegistration startup,
        TimeSpan? saveDelay = null)
    {
        _store = store;
        _overlay = overlay;
        _panel = panel;
        _startup = startup;
        _saveDelay = saveDelay ?? DefaultSaveDelay;

        var state = store.Load();
        Settings = state.Settings.Sanitized();
        IsActive = state.Active && Settings.ResumeOnLaunch;

        _startup.SetEnabled(Settings.StartWithWindows);
        Apply();
    }

    public DimmerSettings Settings { get; private set; }

    public bool IsActive { get; private set; }

    /// <summary>Raised after any change to <see cref="Settings"/> or <see cref="IsActive"/>.</summary>
    public event EventHandler? Changed;

    public void Start() => SetActive(true);

    public void Stop() => SetActive(false);

    public void Toggle() => SetActive(!IsActive);

    public void ApplyPreset(DimmerPreset preset) => Update(preset.ApplyTo);

    /// <summary>Applies a change to the settings; out-of-range values are clamped.</summary>
    public void Update(Func<DimmerSettings, DimmerSettings> change)
    {
        var next = change(Settings).Sanitized();
        if (next == Settings) return;

        var startupChanged = next.StartWithWindows != Settings.StartWithWindows;
        Settings = next;

        if (startupChanged) _startup.SetEnabled(next.StartWithWindows);
        Apply();
        Changed?.Invoke(this, EventArgs.Empty);
        ScheduleSave();
    }

    private void SetActive(bool active)
    {
        if (IsActive == active) return;
        IsActive = active;
        Apply();
        Changed?.Invoke(this, EventArgs.Empty);
        ScheduleSave();
    }

    private void Apply()
    {
        if (IsActive) _overlay.Show(OverlayMath.ComputeColor(Settings));
        else _overlay.Hide();

        var limit = IsActive && _panel.IsSupported ? OverlayMath.PanelBrightness(Settings) : null;
        if (limit.HasValue) _panel.Limit(limit.Value);
        else _panel.Restore();
    }

    private void ScheduleSave()
    {
        lock (_saveGate)
        {
            if (_disposed) return;
            _saveTimer ??= new Timer(_ => SaveNow(), null, Timeout.Infinite, Timeout.Infinite);
            _saveTimer.Change(_saveDelay, Timeout.InfiniteTimeSpan);
        }
    }

    private void SaveNow()
    {
        try
        {
            _store.Save(new PersistedState { Settings = Settings, Active = IsActive });
        }
        catch (Exception)
        {
            // Saving is best-effort; never take the app down over it.
        }
    }

    public void Dispose()
    {
        lock (_saveGate)
        {
            if (_disposed) return;
            _disposed = true;
            _saveTimer?.Dispose();
        }

        SaveNow();
        _overlay.Hide();
        _panel.Restore();
    }
}
