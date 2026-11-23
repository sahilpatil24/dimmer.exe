using System;
using System.Windows;
using System.Windows.Input;
using ScreenDimmer.App.Services;
using ScreenDimmer.App.Tray;
using ScreenDimmer.App.ViewModels;
using ScreenDimmer.App.Views;
using ScreenDimmer.Core;

namespace ScreenDimmer.App;

/// <summary>Composition root: builds every service once and wires them together.</summary>
internal sealed class AppHost : IDisposable
{
    private readonly OverlayManager _overlay;
    private readonly PanelBrightnessService _panel;
    private readonly DimmerController _controller;
    private readonly MainWindow _window;
    private readonly TrayService _tray;
    private readonly HotkeyService _hotkeys;

    public AppHost(bool startHidden)
    {
        var store = new JsonSettingsStore(JsonSettingsStore.DefaultPath());
        _overlay = new OverlayManager();
        _panel = new PanelBrightnessService();
        _controller = new DimmerController(store, _overlay, _panel, new StartupRegistration());

        _window = new MainWindow(new MainViewModel(_controller, _panel.IsSupported));
        _tray = new TrayService(_controller, ShowWindow, Exit);

        _hotkeys = new HotkeyService();
        RegisterHotkeys();

        if (!startHidden) ShowWindow();
    }

    public void ShowWindow()
    {
        _window.Show();
        if (_window.WindowState == WindowState.Minimized) _window.WindowState = WindowState.Normal;
        _window.Activate();
    }

    private void Exit()
    {
        _window.AllowClose = true;
        Application.Current.Shutdown();
    }

    private void RegisterHotkeys()
    {
        const uint mods = HotkeyService.Control | HotkeyService.Alt;

        _hotkeys.Register(mods, Vk(Key.D), _controller.Toggle);
        _hotkeys.Register(mods, Vk(Key.Up), () => NudgeDim(+5));
        _hotkeys.Register(mods, Vk(Key.Down), () => NudgeDim(-5));
        _hotkeys.Register(mods, Vk(Key.Right), () => NudgeTint(+5));
        _hotkeys.Register(mods, Vk(Key.Left), () => NudgeTint(-5));
    }

    private void NudgeDim(int delta) =>
        _controller.Update(s => s with { Dim = s.Dim + delta });

    private void NudgeTint(int delta) =>
        _controller.Update(s => s with { Tint = s.Tint + delta, FilterEnabled = true });

    private static int Vk(Key key) => KeyInterop.VirtualKeyFromKey(key);

    public void Dispose()
    {
        _hotkeys.Dispose();
        _tray.Dispose();
        _controller.Dispose(); // saves, hides overlay, restores panel brightness
        _overlay.Dispose();
        _panel.Dispose();
        _window.AllowClose = true;
        _window.Close();
    }
}
