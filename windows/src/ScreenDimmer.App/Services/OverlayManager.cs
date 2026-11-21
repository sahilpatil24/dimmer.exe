using System;
using System.Collections.Generic;
using System.Windows.Threading;
using Microsoft.Win32;
using ScreenDimmer.Core;

namespace ScreenDimmer.App.Services;

/// <summary>Creates one <see cref="OverlayWindow"/> per monitor and keeps them current.</summary>
public sealed class OverlayManager : IOverlay, IDisposable
{
    private readonly List<OverlayWindow> _windows = new();
    private readonly DispatcherTimer _keepOnTop;
    private OverlayColor _color = OverlayColor.Transparent;
    private bool _visible;

    public OverlayManager()
    {
        _keepOnTop = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
        _keepOnTop.Tick += (_, _) =>
        {
            foreach (var window in _windows) window.BringToTopmost();
        };

        SystemEvents.DisplaySettingsChanged += OnDisplaySettingsChanged;
    }

    public void Show(OverlayColor color)
    {
        _color = color;
        _visible = true;

        EnsureWindows();
        foreach (var window in _windows) window.SetColor(color);
        _keepOnTop.Start();
    }

    public void Hide()
    {
        _visible = false;
        _keepOnTop.Stop();
        CloseAll();
    }

    private void EnsureWindows()
    {
        if (_windows.Count > 0) return;

        foreach (var screen in System.Windows.Forms.Screen.AllScreens)
        {
            var window = new OverlayWindow(screen.Bounds);
            window.SetColor(_color);
            window.Show();
            _windows.Add(window);
        }
    }

    private void CloseAll()
    {
        foreach (var window in _windows) window.Close();
        _windows.Clear();
    }

    private void OnDisplaySettingsChanged(object? sender, EventArgs e)
    {
        // Raised on a background thread: hop to the UI thread and rebuild.
        System.Windows.Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
        {
            if (!_visible) return;
            CloseAll();
            EnsureWindows();
        }));
    }

    public void Dispose()
    {
        SystemEvents.DisplaySettingsChanged -= OnDisplaySettingsChanged;
        Hide();
    }
}
