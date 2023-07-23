using CodeImp.Bloodmasters.Client.Graphics;
using SharpDX;

namespace Bloodmasters.Tests.Graphics;

public class VectorExTests
{
    private static Matrix GetEmptyMatrix() => new Matrix();
    private static Matrix GetMatrixWithNormalColumns1()
    {
        var matrix = new Matrix
        {
            Column1 = new Vector4(4f, 3f, 2f, 6f),
            Column2 = new Vector4(4f, 5f, 2f, 7f),
            Column3 = new Vector4(3f, 1f, 2f, 6f),
            Column4 = new Vector4(2f, 6f, 1f, 9f)
        };

        return matrix;
    }
    private static Matrix GetMatrixWithNormalColumns2()
    {
        var matrix = new Matrix
        {
            Column1 = new Vector4(6f, 2f, 1f, 3f),
            Column2 = new Vector4(4f, 3f, 3f, 4f),
            Column3 = new Vector4(5f, 3f, 1f, 4f),
            Column4 = new Vector4(2f, 3f, 8f, 6f)
        };

        return matrix;
    }
    private static Matrix GetMatrixWithNormalColumns3()
    {
        var matrix = new Matrix
        {
            Column1 = new Vector4(7f, 5f, 3f, 6f),
            Column2 = new Vector4(3f, 4f, 5f, 9f),
            Column3 = new Vector4(2f, 1f, 2f, 3f),
            Column4 = new Vector4(4f, 5f, 6f, 7f)
        };

        return matrix;
    }

    private static Viewport GetNormalViewport()
    {
        var viewport = new Viewport
        {
            Height = 7,
            Width = 4,
            X = 3,
            Y = 6,
            MaxDepth = 9f,
            MinDepth = 1f
        };

        return viewport;
    }

    [Fact]
    public void AllDataZero_Success()
    {
        _ = new Vector3(0f).Project(new Viewport(), GetEmptyMatrix(), GetEmptyMatrix(), GetEmptyMatrix());
    }

    [Fact]
    public void VectorIsZero_True()
    {
        //Act
        var result = new Vector3(0f).Project(new Viewport(), GetMatrixWithNormalColumns3(),
            GetMatrixWithNormalColumns2(), GetMatrixWithNormalColumns1());

        //Assert
        Assert.True(result.IsZero);
    }

    [Fact]
    public void VectorIsZero_False()
    {
        //Act
        var result = new Vector3(0f).Project(GetNormalViewport(), GetEmptyMatrix(), GetEmptyMatrix(), GetEmptyMatrix());

        //Assert
        Assert.False(result.IsZero);
    }

    [Fact]
    public void IsNormalizedWithEmptyData_False()
    {
        //Act
        var result = new Vector3(0f).Project(new Viewport(), new Matrix(), new Matrix(), new Matrix());

        //Assert
        Assert.False(result.IsNormalized);
    }

    [Fact]
    public void IsNormalizedWithNotEmptyData_False()
    {
        //Act
        var result = new Vector3(3f).Project(GetNormalViewport(), GetMatrixWithNormalColumns3(),
            GetMatrixWithNormalColumns2(), GetMatrixWithNormalColumns1());

        //Assert
        Assert.False(result.IsNormalized);
    }

    [Fact]
    public void NormalWithData_Success()
    {
        _ = new Vector3(4f).Project(GetNormalViewport(), GetMatrixWithNormalColumns2(), GetMatrixWithNormalColumns1(), GetMatrixWithNormalColumns3());
    }

    [Fact]
    public void CorrectCalculationsWithNormalData_Success()
    {
        const float tolerance = 0.000001f;

        var result = new Vector3(4f).Project(GetNormalViewport(), GetMatrixWithNormalColumns1(),
            GetMatrixWithNormalColumns2(), GetMatrixWithNormalColumns3());

        Assert.Equal(6.634325f, result.X, tolerance);
        Assert.Equal(6.068153f, result.Y, tolerance);
        Assert.Equal(6.3136373f, result.Z, tolerance);
    }
}
