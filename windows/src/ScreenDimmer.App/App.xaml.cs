using System;
using System.Linq;
using System.Threading;
using System.Windows;

namespace ScreenDimmer.App;

public partial class App : Application
{
    private const string MutexName = "ScreenDimmer.SingleInstance";
    private const string ShowEventName = "ScreenDimmer.ShowWindow";

    private Mutex? _mutex;
    private EventWaitHandle? _showEvent;
    private RegisteredWaitHandle? _wait;
    private AppHost? _host;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _mutex = new Mutex(initiallyOwned: true, MutexName, out var isFirstInstance);
        _showEvent = new EventWaitHandle(false, EventResetMode.AutoReset, ShowEventName);

        if (!isFirstInstance)
        {
            // Already running: ask that instance to show its window, then leave.
            _showEvent.Set();
            Shutdown();
            return;
        }

        _wait = ThreadPool.RegisterWaitForSingleObject(
            _showEvent,
            (_, _) => Dispatcher.BeginInvoke(new Action(() => _host?.ShowWindow())),
            null,
            Timeout.Infinite,
            executeOnlyOnce: false);

        var startHidden = e.Args.Contains("--minimized");
        _host = new AppHost(startHidden);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _wait?.Unregister(null);
        _host?.Dispose();
        _showEvent?.Dispose();
        _mutex?.Dispose();
        base.OnExit(e);
    }
}
