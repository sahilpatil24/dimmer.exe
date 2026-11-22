using System;
using System.Collections.ObjectModel;
using ScreenDimmer.Core;

namespace ScreenDimmer.App.ViewModels;

/// <summary>Thin binding layer over <see cref="DimmerController"/>; holds no logic of its own.</summary>
public sealed class MainViewModel : ObservableObject
{
    private readonly DimmerController _controller;

    public MainViewModel(DimmerController controller, bool panelSupported)
    {
        _controller = controller;
        PanelSupported = panelSupported;

        foreach (var preset in DimmerPresets.All) Presets.Add(new PresetItemViewModel(preset));

        ToggleCommand = new RelayCommand(_ => _controller.Toggle());
        ApplyPresetCommand = new RelayCommand(p =>
        {
            if (p is PresetItemViewModel item) _controller.ApplyPreset(item.Preset);
        });

        _controller.Changed += (_, _) => Refresh();
        Refresh();
    }

    public ObservableCollection<PresetItemViewModel> Presets { get; } = new();

    public RelayCommand ToggleCommand { get; }

    public RelayCommand ApplyPresetCommand { get; }

    public bool IsActive => _controller.IsActive;

    public string StatusText => IsActive ? "Dimmer is on" : "Dimmer is off";

    public string PowerButtonText => IsActive ? "Turn off" : "Turn on";

    // --- Dimming ---------------------------------------------------------

    public double Dim
    {
        get => _controller.Settings.Dim;
        set => _controller.Update(s => s with { Dim = (int)Math.Round(value) });
    }

    public string DimText => $"{_controller.Settings.Dim}%";

    // --- Color filter ----------------------------------------------------

    public bool FilterEnabled
    {
        get => _controller.Settings.FilterEnabled;
        set => _controller.Update(s => s with { FilterEnabled = value });
    }

    /// <summary>Moving the slider implicitly turns the filter on.</summary>
    public double Tint
    {
        get => _controller.Settings.Tint;
        set => _controller.Update(s => s with { Tint = (int)Math.Round(value), FilterEnabled = true });
    }

    public string TintText
    {
        get
        {
            var tint = _controller.Settings.Tint;
            if (tint == 0) return "Neutral";
            return $"{(tint > 0 ? "Warm" : "Cool")} {Math.Abs(tint)}%";
        }
    }

    // --- Extra dim (laptop panel) ------------------------------------------

    public bool PanelSupported { get; }

    public string PanelNote => PanelSupported
        ? string.Empty
        : "Not available here: this display doesn't allow software brightness control " +
          "(typical for desktop monitors). Overlay dimming still works.";

    public bool PanelLimitEnabled
    {
        get => _controller.Settings.PanelLimitEnabled;
        set => _controller.Update(s => s with { PanelLimitEnabled = value });
    }

    public double PanelLimit
    {
        get => _controller.Settings.PanelLimit;
        set => _controller.Update(s => s with { PanelLimit = (int)Math.Round(value) });
    }

    public string PanelLimitText =>
        _controller.Settings.PanelLimit == 0 ? "Lowest" : $"{_controller.Settings.PanelLimit}%";

    // --- Behavior ----------------------------------------------------------

    public bool StartWithWindows
    {
        get => _controller.Settings.StartWithWindows;
        set => _controller.Update(s => s with { StartWithWindows = value });
    }

    public bool ResumeOnLaunch
    {
        get => _controller.Settings.ResumeOnLaunch;
        set => _controller.Update(s => s with { ResumeOnLaunch = value });
    }

    private void Refresh()
    {
        foreach (var preset in Presets) preset.Refresh(_controller.Settings);
        OnPropertyChanged(string.Empty); // refresh every binding
    }
}
