using System.Drawing;
using Vortice.Direct3D9;

namespace CodeImp.Bloodmasters.Client.Graphics;

internal static class ColorEx
{
    public static Colorvalue ToColorValue(this Color color)
    {
        return new Colorvalue
        {
            A = color.A / 255f,
            B = color.B / 255f,
            G = color.G / 255f,
            R = color.R / 255f
        };
    }
}
