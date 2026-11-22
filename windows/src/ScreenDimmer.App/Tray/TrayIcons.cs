using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using ScreenDimmer.App.Native;

namespace ScreenDimmer.App.Tray;

/// <summary>Draws the tray icons in code so no binary asset is needed.</summary>
internal static class TrayIcons
{
    public static Icon Create(bool on)
    {
        var color = on ? Color.FromArgb(255, 181, 71) : Color.FromArgb(150, 150, 158);

        using var bitmap = new Bitmap(32, 32);
        using (var g = Graphics.FromImage(bitmap))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Transparent);
            using var pen = new Pen(color, 3f);
            using var brush = new SolidBrush(color);
            g.DrawEllipse(pen, 4, 4, 23, 23);
            g.FillPie(brush, 4, 4, 23, 23, 90, 180); // left half filled = "dimmed"
        }

        var handle = bitmap.GetHicon();
        try
        {
            using var temp = Icon.FromHandle(handle);
            return (Icon)temp.Clone();
        }
        finally
        {
            NativeMethods.DestroyIcon(handle);
        }
    }
}
