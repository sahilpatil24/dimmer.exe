using System;
using System.Collections.Generic;
using System.Windows.Interop;
using ScreenDimmer.App.Native;

namespace ScreenDimmer.App.Services;

/// <summary>Global hotkeys via RegisterHotKey on a hidden message-only window.</summary>
public sealed class HotkeyService : IDisposable
{
    public const uint Alt = NativeMethods.MOD_ALT;
    public const uint Control = NativeMethods.MOD_CONTROL;
    public const uint Shift = NativeMethods.MOD_SHIFT;

    private readonly HwndSource _source;
    private readonly Dictionary<int, Action> _actions = new();
    private int _nextId = 1;

    public HotkeyService()
    {
        var parameters = new HwndSourceParameters("ScreenDimmerHotkeys")
        {
            ParentWindow = NativeMethods.HWND_MESSAGE,
        };
        _source = new HwndSource(parameters);
        _source.AddHook(WndProc);
    }

    /// <summary>Returns false if another app already owns this combination.</summary>
    public bool Register(uint modifiers, int virtualKey, Action action)
    {
        var id = _nextId++;
        if (!NativeMethods.RegisterHotKey(
                _source.Handle, id, modifiers | NativeMethods.MOD_NOREPEAT, (uint)virtualKey))
        {
            return false;
        }

        _actions[id] = action;
        return true;
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == NativeMethods.WM_HOTKEY && _actions.TryGetValue(wParam.ToInt32(), out var action))
        {
            action();
            handled = true;
        }

        return IntPtr.Zero;
    }

    public void Dispose()
    {
        foreach (var id in _actions.Keys) NativeMethods.UnregisterHotKey(_source.Handle, id);
        _actions.Clear();
        _source.RemoveHook(WndProc);
        _source.Dispose();
    }
}
