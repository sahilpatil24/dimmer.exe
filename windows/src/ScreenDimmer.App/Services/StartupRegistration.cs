using System;
using System.Diagnostics;
using Microsoft.Win32;
using ScreenDimmer.Core;

namespace ScreenDimmer.App.Services;

/// <summary>Adds/removes the app from HKCU\...\Run so it launches (to the tray) with Windows.</summary>
public sealed class StartupRegistration : IStartupRegistration
{
    private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "ScreenDimmer";

    public void SetEnabled(bool enabled)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKey, writable: true)
                            ?? Registry.CurrentUser.CreateSubKey(RunKey);

            if (enabled)
            {
                var exe = Environment.ProcessPath;
                if (!string.IsNullOrEmpty(exe))
                {
                    key.SetValue(ValueName, $"\"{exe}\" --minimized");
                }
            }
            else
            {
                key.DeleteValue(ValueName, throwOnMissingValue: false);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Startup registration failed: {ex.Message}");
        }
    }
}
