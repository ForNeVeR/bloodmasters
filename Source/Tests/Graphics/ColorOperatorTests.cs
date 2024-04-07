using Bloodmasters.Client.Graphics;
using SharpDX;

namespace Bloodmasters.Tests.Graphics;

public static class ColorOperatorTests
{
    [Theory]
    [InlineData(0, 0, 0, 0, 0)]
    [InlineData(0xAABBCCDD, 0xAA, 0xBB, 0xCC, 0xDD)]
    public static void FromArgbTest(uint argb, byte a, byte r, byte g, byte b)
    {
        var color = ColorOperator.FromArgb((int)argb);
        Assert.Equal(color.A, a);
        Assert.Equal(color.R, r);
        Assert.Equal(color.G, g);
        Assert.Equal(color.B, b);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0xAABBCCDD)]
    public static void ToArgbTest(uint argb)
    {
        var color = ColorOperator.FromArgb((int)argb);
        Assert.Equal(argb, (uint)color.ToArgb());
    }

    private static void TestOperator(
        uint argb,
        float argument,
        Func<int, float, int> realOperator,
        Func<Color, float, Color> expectedOperator)
    {
        var scaledArgb = realOperator((int)argb, argument);

        var color = ColorOperator.FromArgb((int)argb);
        var expected = expectedOperator(color, argument);

        Assert.Equal(expected.ToArgb(), scaledArgb);
    }

    [Theory]
    [InlineData(0, 0.5f)]
    [InlineData(0xAABBCCDD, 0.75f)]
    public static void ScaleTest(uint argb, float scale) =>
        TestOperator(argb, scale, ColorOperator.Scale, Color.Scale);

    [Theory]
    [InlineData(0, 0.5f)]
    [InlineData(0xAABBCCDD, 0.75f)]
    public static void AdjustSaturation(uint argb, float saturation) =>
        TestOperator(argb, saturation, ColorOperator.AdjustSaturation, Color.AdjustSaturation);

    [Theory]
    [InlineData(0, 0.5f)]
    [InlineData(0xAABBCCDD, 0.75f)]
    public static void AdjustContrast(uint argb, float contrast) =>
        TestOperator(argb, contrast, ColorOperator.AdjustContrast, Color.AdjustContrast);

    [Theory]
    [InlineData(0xAABBCCDD, 0xDDCCBBAA)]
    [InlineData(0xDDCCBBAA, 0xAABBCCDD)]
    public static void Modulate(uint color1, uint color2)
    {
        var result = ColorOperator.Modulate((int)color1, (int)color2);

        Color4 c1 = ColorOperator.FromArgb((int)color1);
        Color4 c2 = ColorOperator.FromArgb((int)color2);
        var expected = Color4.Modulate(c1, c2);
        Assert.Equal(new Color(expected).ToArgb(), result);
    }
}
