using SharpDX;

namespace CodeImp.Bloodmasters.Client.Graphics;

internal static class RectangleEx
{
    public static RectangleF Inflate(this RectangleF rect, float x, float y)
    {
        rect.Inflate(x, y);
        return rect;
    }

    public static RectangleF ToSharpDx(this System.Drawing.RectangleF rect)
    {
        return new(rect.X, rect.Y, rect.Width, rect.Height);
    }

    public static System.Drawing.RectangleF ToSystemDrawing(this RectangleF rect)
    {
        return new(rect.X, rect.Y, rect.Width, rect.Height);
    }
}
