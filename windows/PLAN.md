# Dimmer for Windows (`dimmer.exe`) - Engineering Plan

## 1. Executive Summary
A lightweight, high-performance display dimming application for Windows that allows dimming the screen below hardware limits using a transparent click-through layered window, warm color filtering (blue light cut), system tray controls, and global keyboard shortcuts.

---

## 2. Technical Stack

* **Core Options:**
  1. **Flutter Desktop (Windows):** Native C++ runner, high-performance rendering, seamless system tray (`tray_manager`), native window sizing (`window_manager`), and cross-platform synergy with Android.
  2. **Python / PySide6 + ctypes:** Standalone Win32 script packaged via PyInstaller into `dimmer.exe`.
* **Key Win32 APIs:**
  * `WS_EX_LAYERED (0x80000)`: Enables alpha blending and transparency.
  * `WS_EX_TRANSPARENT (0x20)`: Enables click-through behavior (mouse input passes through to windows below).
  * `WS_EX_TOOLWINDOW (0x80)`: Prevents the overlay from appearing in Alt-Tab or taskbar.
  * `SetLayeredWindowAttributes`: Controls alpha (0–255) and color keying.
  * `GetSystemMetrics(SM_CXVIRTUALSCREEN)` & `GetSystemMetrics(SM_CYVIRTUALSCREEN)`: Spans across multi-monitor setups.
  * `RegisterHotKey`: Listens for global hotkeys when minimized.

---

## 3. Five-Phase Implementation Roadmap

```mermaid
flowchart LR
    P1["Phase 1\nWin32 Overlay Engine"] --> P2["Phase 2\nDimming & Color Tint"]
    P2 --> P3["Phase 3\nSystem Tray & UI"]
    P3 --> P4["Phase 4\nHotkeys & Multi-Screen"]
    P4 --> P5["Phase 5\nRelease dimmer.exe"]
```

### Phase 1: Win32 Overlay Foundation
* Create borderless, topmost, un-focusable window.
* Set `WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW` styles.
* Validate that clicks, typing, and mouse interactions pass completely through to underlying desktop software.

### Phase 2: Dimming Engine & Color Temperature
* Implement smooth alpha adjustment from 0% (off) to 95% (near total blackout).
* Implement warm amber / orange color tinting matrix to filter high-frequency blue light.
* Add presets:
  * **Day:** 0% dim, no tint
  * **Reading:** 25% dim, mild 3400K tint
  * **Night:** 50% dim, warm 2700K tint
  * **Eclipse:** 85% dim, deep 1900K amber tint

### Phase 3: System Tray & UI
* Tray icon in Windows notification area with context menu:
  * Show/Hide controls
  * Preset quick-select
  * Toggle Dimmer On/Off
  * Exit
* Control popup with intuitive sliders for Dim Level and Color Temperature.
* Settings persistence (JSON config saved in `%APPDATA%\Dimmer`).

### Phase 4: Multi-Monitor & Hotkey Daemon
* Dynamically query display bounds on display change (`WM_DISPLAYCHANGE`).
* Cover all active monitors, accommodating different resolutions and DPI scaling.
* Background hotkey listener:
  * `Ctrl + Alt + Down`: Increase Dimming (5% step)
  * `Ctrl + Alt + Up`: Decrease Dimming (5% step)
  * `Ctrl + Alt + D`: Toggle Dimmer On/Off

### Phase 5: Packaging & Release
* Embed high-resolution `.ico` application icon.
* Configure build pipeline to produce clean, single-file or portable `dimmer.exe`.
* Test startup with Windows (`HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run`).

---

## 4. Commit Plan
```text
feat(windows): initialize Windows project workspace and runner
feat(windows): implement Win32 layered transparent window creation
feat(windows): add click-through flag handling (WS_EX_TRANSPARENT)
feat(windows): add alpha blending slider (0-95% opacity)
feat(windows): implement warm color temperature tinting (blue light reduction)
feat(windows): add system tray icon and minimize-to-tray lifecycle
feat(windows): implement floating tray control menu with preset profiles
feat(windows): add persistent configuration store in AppData
feat(windows): implement multi-monitor virtual screen coverage
feat(windows): register global hotkeys for brightness adjustment
feat(windows): package standalone dimmer.exe binary with custom app icon
```
