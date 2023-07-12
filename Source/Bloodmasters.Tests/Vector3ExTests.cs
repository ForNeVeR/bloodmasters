using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace Bloodmasters.Tests.Graphics;

public class Vector3ExTests
{
    private static Matrix GetEmptyMatrix() => new Matrix();
    private static Matrix GetMatrixWithNormalColumns1()
    {
        var matrix = new Matrix
        {
            M11 = 4f, M21 = 3f, M31 = 2f, M41 = 6f,
            M12 = 4f, M22 = 5f, M32 = 2f, M42 = 7f,
            M13 = 3f, M23 = 1f, M33 = 2f, M43 = 6f,
            M14 = 2f, M24 = 6f, M34 = 1f, M44 = 9f
        };

        return matrix;
    }
    private static Matrix GetMatrixWithNormalColumns2()
    {
        var matrix = new Matrix
        {
            M11 = 6f, M21 = 2f, M31 = 1f, M41 = 3f,
            M12 = 4f, M22 = 3f, M32 = 3f, M42 = 4f,
            M13 = 5f, M23 = 3f, M33 = 1f, M43 = 4f,
            M14 = 2f, M24 = 3f, M34 = 8f, M44 = 6f
        };

        return matrix;
    }
    private static Matrix GetMatrixWithNormalColumns3()
    {
        var matrix = new Matrix
        {
            M11 = 7f, M21 = 5f, M31 = 3f, M41 = 6f,
            M12 = 3f, M22 = 4f, M32 = 5f, M42 = 9f,
            M13 = 2f, M23 = 1f, M33 = 2f, M43 = 3f,
            M14 = 4f, M24 = 5f, M34 = 6f, M44 = 7f
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
            MaxZ = 9f,
            MinZ = 1f
        };

        return viewport;
    }

    [Fact]
    public void AllDataZero_Success()
    {
        _ = Vector3.Project(new Vector3(0f, 0f, 0f), new Viewport(), GetEmptyMatrix(), GetEmptyMatrix(), GetEmptyMatrix());
    }

    [Fact]
    public void VectorIsZero_True()
    {
        //Act
        var result = Vector3.Project(new Vector3(0f, 0f, 0f), new Viewport(), GetMatrixWithNormalColumns3(),
            GetMatrixWithNormalColumns2(), GetMatrixWithNormalColumns1());

        //Assert
        Assert.Equal(new Vector3(), result);
    }

    [Fact]
    public void VectorIsZero_False()
    {
        //Act
        var result = Vector3.Project(new Vector3(0f, 0f, 0f), GetNormalViewport(), GetEmptyMatrix(), GetEmptyMatrix(), GetEmptyMatrix());

        //Assert
        Assert.NotEqual(new Vector3(), result);
    }

    [Fact]
    public void IsNormalizedWithEmptyData_False()
    {
        //Act
        var result = Vector3.Project(new Vector3(0f, 0f, 0f), new Viewport(), new Matrix(), new Matrix(), new Matrix());

        //Assert
        Assert.Equal(1f, result.Length());
    }

    [Fact]
    public void IsNormalizedWithNotEmptyData_False()
    {
        //Act
        var result = Vector3.Project(new Vector3(3f, 3f, 3f), GetNormalViewport(), GetMatrixWithNormalColumns3(),
            GetMatrixWithNormalColumns2(), GetMatrixWithNormalColumns1());

        //Assert
        Assert.NotEqual(1f, result.Length());
    }

    [Fact]
    public void NormalWithData_Success()
    {
        _ = Vector3.Project(new Vector3(4f, 4f, 4f), GetNormalViewport(), GetMatrixWithNormalColumns2(), GetMatrixWithNormalColumns1(), GetMatrixWithNormalColumns3());
    }

    [Fact]
    public void CorrectCalculationsWithNormalData_Success()
    {
        const float tolerance = 0.000001f;

        var result = Vector3.Project(new Vector3(4f, 4f, 4f), GetNormalViewport(), GetMatrixWithNormalColumns1(),
            GetMatrixWithNormalColumns2(), GetMatrixWithNormalColumns3());

        Assert.Equal(6.634325f, result.X, tolerance);
        Assert.Equal(6.068153f, result.Y, tolerance);
        Assert.Equal(6.3136373f, result.Z, tolerance);
    }
}
