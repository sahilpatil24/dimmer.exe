using System;
using System.Drawing;
using System.Windows.Forms;
using ScreenDimmer.Core;

namespace ScreenDimmer.App.Tray;

/// <summary>Notification-area icon with a menu: open, toggle, presets, exit.</summary>
public sealed class TrayService : IDisposable
{
    private readonly DimmerController _controller;
    private readonly NotifyIcon _notifyIcon;
    private readonly ToolStripMenuItem _toggleItem;
    private readonly Icon _onIcon = TrayIcons.Create(true);
    private readonly Icon _offIcon = TrayIcons.Create(false);

    public TrayService(DimmerController controller, Action showWindow, Action exit)
    {
        _controller = controller;

        var menu = new ContextMenuStrip();
        menu.Items.Add("Open Screen Dimmer", null, (_, _) => showWindow());

        _toggleItem = new ToolStripMenuItem("Dimmer on");
        _toggleItem.Click += (_, _) => _controller.Toggle();
        menu.Items.Add(_toggleItem);

        var presets = new ToolStripMenuItem("Presets");
        foreach (var preset in DimmerPresets.All)
        {
            var captured = preset;
            presets.DropDownItems.Add(captured.Label, null, (_, _) =>
            {
                _controller.ApplyPreset(captured);
                _controller.Start();
            });
        }

        menu.Items.Add(presets);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Exit", null, (_, _) => exit());

        _notifyIcon = new NotifyIcon
        {
            Icon = _offIcon,
            Text = "Screen Dimmer",
            ContextMenuStrip = menu,
            Visible = true,
        };
        _notifyIcon.MouseClick += (_, e) =>
        {
            if (e.Button == MouseButtons.Left) showWindow();
        };

        _controller.Changed += OnChanged;
        Refresh();
    }

    private void OnChanged(object? sender, EventArgs e) => Refresh();

    private void Refresh()
    {
        _toggleItem.Checked = _controller.IsActive;
        _notifyIcon.Icon = _controller.IsActive ? _onIcon : _offIcon;
        _notifyIcon.Text = _controller.IsActive ? "Screen Dimmer: on" : "Screen Dimmer: off";
    }

    public void Dispose()
    {
        _controller.Changed -= OnChanged;
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _onIcon.Dispose();
        _offIcon.Dispose();
    }
}
