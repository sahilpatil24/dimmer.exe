# Screen Dimmer for Windows

A tray app that dims your screen below the normal minimum and filters blue light.
C# / .NET 8 / WPF. Windows 10/11.

## Features

- **Dim level** slider with a hard cap: very dark, never fully black.
- **Presets**: Reading, Cinema, Evening, Night, Deep night, Maximum.
- **Blue light filter**: one slider from cool/blue through neutral to warm/yellow.
- **Extra dim** (laptops): also caps the panel's own brightness via WMI, restored on exit.
- **Multi-monitor**: every display is covered.
- **Tray icon** with on/off and presets, **global hotkeys**, **start with Windows**, single instance.

## Architecture

```
src/ScreenDimmer.Core      (net8.0, no Windows dependencies, unit tested)
  Settings/    DimmerSettings, DimmerPresets
  Rendering/   OverlayMath         settings -> one ARGB color
  Abstractions IOverlay, IPanelBrightness, IStartupRegistration, ISettingsStore
  Controller/  DimmerController    owns state, drives the abstractions
  Persistence/ JsonSettingsStore   %AppData%\ScreenDimmer\settings.json

src/ScreenDimmer.App       (net8.0-windows, WPF)
  Native/      NativeMethods       the few Win32 calls
  Services/    OverlayWindow/Manager, PanelBrightnessService (WMI),
               StartupRegistration (registry), HotkeyService
  Tray/        TrayService, TrayIcons
  ViewModels/  MainViewModel (thin binding layer), RelayCommand
  Views/       Theme.xaml, MainWindow
  AppHost.cs   composition root

tests/ScreenDimmer.Core.Tests   xunit, runs on any OS
```

Design notes:

- The overlay is a borderless WPF window per monitor with the extended styles
  `WS_EX_TRANSPARENT | WS_EX_LAYERED | WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW`, so clicks go straight
  through, it never takes focus, and it stays out of Alt+Tab. A 2-second timer re-asserts topmost.
- Dim and tint are composed into a single ARGB color (`OverlayMath`). Caps: dim 78%, tint 28%,
  combined maximum about 84% opacity.
- All logic lives in `Core` behind interfaces, so the controller is tested with fakes and the WPF
  layer stays thin.
- WMI is slow, so panel brightness changes run on one background worker that always applies the
  latest value; the original brightness is captured first and restored on off/exit.

## Setup

See [`setup.md`](setup.md). Commit guide: [`docs/COMMIT_PLAN.md`](docs/COMMIT_PLAN.md).

## Limitations

- Can't draw over elevated windows, UAC prompts, or exclusive-fullscreen games.
- Overlay dimming is a software effect, so screenshots may include the tint.
- Panel brightness control needs a laptop/internal display that exposes WMI brightness.
