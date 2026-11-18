# Screen Dimmer for Windows (`dimmer.exe`) - Engineering Plan

## 1. Executive Summary
A native, high-performance tray application for Windows 10 and 11 that dims displays below standard hardware limits and filters blue light. Built with modern **C# / .NET 8 / WPF**, featuring layered click-through overlays, safety-capped brightness math, hardware panel brightness integration via WMI, global hotkeys, and single-instance tray lifecycle.

---

## 2. Technical Stack & Architecture

* **Framework & Runtime:**
  * **C# / .NET 8 / WPF** (`net8.0-windows` for App, `net8.0` for Core).
  * Unit testing: **xUnit** (`ScreenDimmer.Core.Tests`).
* **Solution Architecture:**
  * `src/ScreenDimmer.Core` (clean logic, zero Windows/WPF dependencies):
    * `Settings/`: `DimmerSettings` (immutable records, sanitization/clamping), `DimmerPresets`.
    * `Rendering/`: `OverlayMath` (ARGB alpha compositing with hard opacity caps preventing total blackout).
    * `Abstractions/`: `IOverlay`, `IPanelBrightness`, `IStartupRegistration`, `ISettingsStore`.
    * `Controller/`: `DimmerController` (central state machine, debounced JSON persistence, coordinator).
    * `Persistence/`: `JsonSettingsStore` (`%AppData%\ScreenDimmer\settings.json`).
  * `src/ScreenDimmer.App` (WPF UI & Windows OS services):
    * `Native/`: `NativeMethods` (`user32.dll` P/Invoke bindings).
    * `Services/`:
      * `OverlayWindow` & `OverlayManager`: Multi-monitor borderless transparent overlays (`WS_EX_TRANSPARENT | WS_EX_LAYERED | WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW`).
      * `PanelBrightnessService`: Laptop internal display backlight control via WMI (`root\WMI`, `WmiMonitorBrightnessMethods`) with automatic brightness restoration on exit.
      * `StartupRegistration`: Windows startup registry entry (`HKCU\Software\Microsoft\Windows\CurrentVersion\Run`).
      * `HotkeyService`: Global shortcut bindings via hidden message window (`RegisterHotKey`).
    * `Tray/`: `TrayService` & `TrayIcons` (Windows notification area icon and context menu).
    * `ViewModels/`: `MainViewModel`, `RelayCommand` (MVVM architecture).
    * `Views/`: `Theme.xaml`, `MainWindow` (dark mode styling, gradient color temperature sliders).
    * `AppHost.cs`: Composition root and lifecycle management.
    * `App.xaml.cs`: Single-instance mutex and named event activation.

---

## 3. Key Win32 & OS Integration Features

* **Click-Through Multi-Monitor Overlays:**
  * Extended window styles `WS_EX_TRANSPARENT | WS_EX_LAYERED | WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW` allow mouse clicks to pass straight through without stealing focus or appearing in Alt+Tab.
  * Multi-monitor support automatically tracks display geometry changes (`SystemEvents.DisplaySettingsChanged`).
* **Color Compositing & Blackout Prevention:**
  * Dim level hard cap: 78% opacity.
  * Tint level hard cap: 28% opacity.
  * Combined maximum opacity is mathematically capped at ~84%, guaranteeing the screen remains visible and readable.
* **WMI Panel Brightness Control:**
  * Communicates with ACPI/WMI brightness methods for laptops.
  * Runs on a background worker thread to prevent UI freezing.
  * Captures initial hardware brightness and guarantees clean restoration on exit.
* **Global Hotkeys:**
  * `Ctrl + Alt + D`: Toggle dimmer on/off.
  * `Ctrl + Alt + Up / Down`: Step dimming by ±5%.
  * `Ctrl + Alt + Right / Left`: Step color temperature (warmer / cooler) by ±5%.

---

## 4. Incremental Implementation Roadmap

1. **Repo Hygiene:** Configure `.gitignore` for .NET 8, WPF, and Windows artifacts.
2. **Core Settings & Presets:** Implement settings models with immutable records, validation, and predefined presets.
3. **Overlay Color Math:** Implement compositing mathematics for dimming + color tinting with safety caps.
4. **Core Logic Unit Tests:** Comprehensive xUnit tests covering clamping, math formulas, and preset bounds.
5. **Controller & Abstractions:** Decouple core logic behind service interfaces (`IOverlay`, `IPanelBrightness`, `ISettingsStore`, `IStartupRegistration`).
6. **Controller Tests:** Verify controller behavior and state transitions using test fakes.
7. **Settings Persistence:** Implement thread-safe JSON settings storage in `%AppData%`.
8. **Multi-Monitor Click-Through Overlay:** WPF borderless layered windows with DPI awareness V2 and display change detection.
9. **Hardware Brightness & Startup Registration:** WMI brightness service and registry startup registration.
10. **System Tray & Global Hotkeys:** Notification tray menu, dynamic tray icon generation, and global hotkey service.
11. **MVVM Layer:** Base ViewModel, RelayCommand, and MainViewModel data bindings.
12. **Main Application Window:** Modern dark-themed WPF UI with gradient sliders and preset pickers.
13. **Composition Root & Single Instance:** Wire all components in `AppHost`, with mutex-based single instance handling.
14. **Solution Configuration:** Visual Studio `.sln` linking all project and test layers.
15. **Documentation & Release:** Comprehensive documentation for setup, architecture, and usage.
