using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using ScreenDimmer.App.Native;
using ScreenDimmer.Core;

namespace ScreenDimmer.App.Services;

/// <summary>
/// One borderless, click-through, always-on-top window covering a single monitor.
/// It never takes focus and does not appear in Alt+Tab.
/// </summary>
public sealed class OverlayWindow : Window
{
    private readonly SolidColorBrush _brush = new(Colors.Transparent);
    private readonly System.Drawing.Rectangle _bounds;
    private IntPtr _hwnd = IntPtr.Zero;

    public OverlayWindow(System.Drawing.Rectangle bounds)
    {
        _bounds = bounds;

        WindowStyle = WindowStyle.None;
        ResizeMode = ResizeMode.NoResize;
        AllowsTransparency = true;
        Background = _brush;
        ShowInTaskbar = false;
        ShowActivated = false;
        Topmost = true;
        Focusable = false;
        IsHitTestVisible = false;
        WindowStartupLocation = WindowStartupLocation.Manual;

        // Rough placement; the exact pixel rectangle is applied in SourceInitialized.
        Left = bounds.Left;
        Top = bounds.Top;
        Width = bounds.Width;
        Height = bounds.Height;

        SourceInitialized += OnSourceInitialized;
    }

    public void SetColor(OverlayColor color) =>
        _brush.Color = Color.FromArgb(color.A, color.R, color.G, color.B);

    /// <summary>Re-asserts topmost + exact monitor bounds (DPI changes, taskbar, other topmost apps).</summary>
    public void BringToTopmost()
    {
        if (_hwnd == IntPtr.Zero) return;
        NativeMethods.SetWindowPos(
            _hwnd,
            NativeMethods.HWND_TOPMOST,
            _bounds.Left,
            _bounds.Top,
            _bounds.Width,
            _bounds.Height,
            NativeMethods.SWP_NOACTIVATE | NativeMethods.SWP_SHOWWINDOW);
    }

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        _hwnd = new WindowInteropHelper(this).Handle;

        var exStyle = NativeMethods.GetWindowLong(_hwnd, NativeMethods.GWL_EXSTYLE);
        exStyle |= NativeMethods.WS_EX_TRANSPARENT
                 | NativeMethods.WS_EX_LAYERED
                 | NativeMethods.WS_EX_NOACTIVATE
                 | NativeMethods.WS_EX_TOOLWINDOW;
        NativeMethods.SetWindowLong(_hwnd, NativeMethods.GWL_EXSTYLE, exStyle);

        BringToTopmost();
    }
}
