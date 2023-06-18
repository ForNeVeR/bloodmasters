using Vortice.Mathematics;
using Color = System.Drawing.Color;

namespace CodeImp.Bloodmasters.Client.Graphics;

internal static class ColorOperator
{
    public static int Scale(int colorValue, float scale)
    {
        var color = Color.FromArgb(colorValue);
        var scaled = new Color4(color.R, color.G, color.B, color.A) * scale;
        scaled.ToRgba(out var r, out var g, out var b, out var a);
        return General.ARGB(a, r, g, b);
    }
}
