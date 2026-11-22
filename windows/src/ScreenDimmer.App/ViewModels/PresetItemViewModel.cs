using ScreenDimmer.Core;

namespace ScreenDimmer.App.ViewModels;

public sealed class PresetItemViewModel : ObservableObject
{
    public PresetItemViewModel(DimmerPreset preset) => Preset = preset;

    public DimmerPreset Preset { get; }

    public string Label => Preset.Label;

    public bool IsActive { get; private set; }

    public void Refresh(DimmerSettings settings)
    {
        var active = Preset.Matches(settings);
        if (active == IsActive) return;
        IsActive = active;
        OnPropertyChanged(nameof(IsActive));
    }
}
