using System;
using System.Diagnostics;
using System.Management;
using System.Threading;
using System.Threading.Tasks;
using ScreenDimmer.Core;

namespace ScreenDimmer.App.Services;

/// <summary>
/// Caps the built-in (laptop) panel brightness through WMI. Desktop monitors
/// usually don't expose this, in which case <see cref="IsSupported"/> is false.
/// WMI is slow, so all work runs on one background worker that always applies
/// the latest requested value.
/// </summary>
public sealed class PanelBrightnessService : IPanelBrightness, IDisposable
{
    private const int None = -1;

    private readonly object _gate = new();
    private int _pending = None;
    private bool _restorePending;
    private int _original = None;
    private bool _busy;

    public PanelBrightnessService()
    {
        IsSupported = QuerySupported();
    }

    public bool IsSupported { get; }

    public void Limit(int percent)
    {
        if (!IsSupported) return;
        lock (_gate)
        {
            _pending = Math.Clamp(percent, 0, 100);
            _restorePending = false;
            StartWorkerLocked();
        }
    }

    public void Restore()
    {
        if (!IsSupported) return;
        lock (_gate)
        {
            if (_original == None && _pending == None) return;
            _restorePending = true;
            StartWorkerLocked();
        }
    }

    private void StartWorkerLocked()
    {
        if (_busy) return;
        _busy = true;
        Task.Run(Work);
    }

    private void Work()
    {
        while (true)
        {
            int apply;
            bool restore;
            lock (_gate)
            {
                if (_restorePending)
                {
                    restore = true;
                    apply = None;
                    _restorePending = false;
                    _pending = None;
                }
                else if (_pending != None)
                {
                    restore = false;
                    apply = _pending;
                    _pending = None;
                }
                else
                {
                    _busy = false;
                    return;
                }
            }

            try
            {
                if (restore)
                {
                    int original;
                    lock (_gate)
                    {
                        original = _original;
                        _original = None;
                    }

                    if (original != None) SetBrightness(original);
                }
                else
                {
                    bool needOriginal;
                    lock (_gate) needOriginal = _original == None;

                    if (needOriginal)
                    {
                        var current = ReadBrightness();
                        lock (_gate)
                        {
                            if (_original == None) _original = current;
                        }
                    }

                    SetBrightness(apply);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Panel brightness error: {ex.Message}");
            }
        }
    }

    private static bool QuerySupported()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher(
                @"root\WMI", "SELECT * FROM WmiMonitorBrightnessMethods");
            using var results = searcher.Get();
            return results.Count > 0;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static int ReadBrightness()
    {
        using var searcher = new ManagementObjectSearcher(
            @"root\WMI", "SELECT CurrentBrightness FROM WmiMonitorBrightness");
        foreach (ManagementObject obj in searcher.Get())
        {
            using (obj)
            {
                return Convert.ToInt32(obj["CurrentBrightness"]);
            }
        }

        return None;
    }

    private static void SetBrightness(int percent)
    {
        using var searcher = new ManagementObjectSearcher(
            @"root\WMI", "SELECT * FROM WmiMonitorBrightnessMethods");
        foreach (ManagementObject obj in searcher.Get())
        {
            using (obj)
            {
                obj.InvokeMethod("WmiSetBrightness", new object[] { (uint)1, (byte)percent });
            }
        }
    }

    /// <summary>Restores the original brightness and waits briefly for the worker to finish.</summary>
    public void Dispose()
    {
        Restore();
        var deadline = DateTime.UtcNow.AddSeconds(2);
        while (DateTime.UtcNow < deadline)
        {
            lock (_gate)
            {
                if (!_busy) return;
            }

            Thread.Sleep(20);
        }
    }
}
