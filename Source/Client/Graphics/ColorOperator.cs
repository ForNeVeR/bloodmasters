using SharpDX;

namespace CodeImp.Bloodmasters.Client.Graphics;

internal static class ColorOperator
{
    public static Color FromArgb(int argb)
    {
        var sdColor = System.Drawing.Color.FromArgb(argb);
        return new Color(red: sdColor.R, green: sdColor.G, blue: sdColor.B, alpha: sdColor.A);
    }

    public static int ToArgb(this Color color)
    {
        return System.Drawing.Color.FromArgb(alpha: color.A, red: color.R, green: color.G, blue: color.B).ToArgb();
    }

    public static int Scale(int argbColor, float scale)
    {
        var scaled = Color.Scale(FromArgb(argbColor), scale);
        return scaled.ToArgb();
    }

    public static int AdjustSaturation(int argbColor, float saturation)
    {
        var color = FromArgb(argbColor);
        var adjusted = Color.AdjustSaturation(color, saturation);
        return adjusted.ToArgb();
    }

    public static int AdjustContrast(int argbColor, float contrast)
    {
        var color = FromArgb(argbColor);
        var adjusted = Color.AdjustContrast(color, contrast);
        return adjusted.ToArgb();
    }

    public static int Modulate(int leftArgbColor, int rightArgbColor)
    {
        var left = FromArgb(leftArgbColor);
        var right= FromArgb(rightArgbColor);
        var modulated = Color.Modulate(left, right);
        return modulated.ToArgb();
    }
}
