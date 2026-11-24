# 🌙 Dimmer (`dimmer.exe`)

A lightweight, high-performance tray application for **Windows 10 & 11** that dims your screen below standard hardware minimums and filters blue light to reduce eye strain.

Built natively with **C# / .NET 8 / WPF**.

---

## 📥 Download

Ready to use — no installation required!

[![Download dimmer.exe](https://img.shields.io/badge/Download-dimmer.exe-blue?style=for-the-badge&logo=windows)](https://github.com/sahilpatil24/dimmer.exe/raw/main/dimmer.exe)

* **[⬇️ Direct Download `dimmer.exe`](https://github.com/sahilpatil24/dimmer.exe/raw/main/dimmer.exe)** *(Single-file portable executable, ~530 KB)*
* **[📦 GitHub Releases](https://github.com/sahilpatil24/dimmer.exe/releases)** *(All release builds & changelogs)*

### How to Run:
1. Download **`dimmer.exe`** from the link above.
2. Double-click **`dimmer.exe`** to launch.
3. The app will open and place a moon icon in your system notification tray (near the clock).
4. Click **Turn on** or use `Ctrl + Alt + D` to toggle dimming instantly!

> **Note on Windows SmartScreen**: Because this is an unsigned open-source binary, Windows SmartScreen may show a prompt on first launch. Click **More info** ➔ **Run anyway**.

---

## ✨ Features

- **Ultra-Low Dim Level**: Dims displays below standard hardware limits.
- **Fail-Safe Brightness Math**: Designed with hard opacity caps (maximum combined opacity ~84%), mathematically guaranteeing that the screen is **never completely blacked out**.
- **Blue Light Filter**: Smooth color temperature slider ranging from cool blue to warm candle amber.
- **One-Click Presets**: Quick profiles for any environment:
  - 📖 **Reading** (mild dim, comfortable tint)
  - 🎬 **Cinema** (focused dark overlay)
  - 🌆 **Evening** (warm tint, moderate dim)
  - 🌙 **Night** (standard night working profile)
  - 🌌 **Deep Night** (extra dark, deep amber)
  - ⚡ **Maximum** (maximum safe dim level)
- **Extra Dim (Laptop Panel Brightness)**: Communicates directly with ACPI/WMI on laptops to lower the actual backlight below the hardware slider, and automatically restores your original brightness upon exit.
- **Multi-Monitor Support**: Covers all connected monitors automatically and adapts immediately to resolution, DPI, or display arrangement changes.
- **Click-Through & Non-Intrusive**: Overlays use native Win32 extended styles (`WS_EX_TRANSPARENT | WS_EX_LAYERED | WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW`). Mouse clicks and typing pass straight through to underlying applications, without stealing window focus or cluttering `Alt+Tab`.
- **System Tray Lifecycle**: Runs quietly in the taskbar notification tray. Closing the window minimizes to tray; exit anytime via the tray context menu.
- **Start with Windows**: Toggleable option in settings to launch silently on Windows boot.

---

## ⌨️ Global Hotkeys

Control brightness and color temperature anytime without leaving your current app or game:

| Shortcut | Action |
|---|---|
| `Ctrl + Alt + D` | Toggle dimmer on / off |
| `Ctrl + Alt + Up` | Increase dim level (+5%) |
| `Ctrl + Alt + Down` | Decrease dim level (-5%) |
| `Ctrl + Alt + Right` | Warmer color temperature (+5%) |
| `Ctrl + Alt + Left` | Cooler color temperature (-5%) |

---

## 🏗️ Project Architecture

```
ScreenDimmerWin/
├── dimmer.exe                 # Pre-built portable Windows executable
├── README.md                  # Main project overview & download portal
├── Android/                   # Android companion architecture & plans
└── Windows/                   # Complete Windows implementation source
    ├── ScreenDimmer.sln       # Visual Studio solution
    ├── README.md              # Detailed technical Windows guide
    ├── src/
    │   ├── ScreenDimmer.Core/ # Net8.0 portable core logic, math, controller & state
    │   └── ScreenDimmer.App/  # WPF UI, multi-monitor overlays, WMI, tray & hotkeys
    └── tests/
        └── ScreenDimmer.Core.Tests/ # xUnit test suite (100% logic coverage)
```

---

## 🛠️ Building from Source

If you prefer building the binary yourself, you only need the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0):

```powershell
# Clone the repository
git clone https://github.com/sahilpatil24/dimmer.exe.git
cd dimmer.exe

# Build and run the WPF application
dotnet run --project Windows/src/ScreenDimmer.App -c Release

# Or publish a single-file portable executable
dotnet publish Windows/src/ScreenDimmer.App/ScreenDimmer.App.csproj -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o publish
```

---

## 📄 License

This project is open-source. Feel free to use, modify, and distribute.
