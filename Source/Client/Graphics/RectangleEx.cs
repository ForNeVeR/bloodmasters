using SharpDX;

namespace CodeImp.Bloodmasters.Client.Graphics;

internal static class RectangleEx
{
    public static RectangleF Inflate(RectangleF rect, float x, float y)
    {
        rect.Inflate(x, y);
        return rect;
    }
}
