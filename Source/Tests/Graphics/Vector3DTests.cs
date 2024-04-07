namespace Bloodmasters.Tests.Graphics;

public class Vector3DTests
{
    [Fact(DisplayName = "Vector3D copy constructor should correctly copy data from source Vector3D")]
    public void CopyConstructorShouldCorrectlyCopyDataFromSourceVector3D()
    {
        // Arrange
        var source = new Vector3D(1f, 2f, 3f);

        // Act
        var result = new Vector3D(source);

        // Assert
        Assert.Equal(source.x, result.x);
        Assert.Equal(source.y, result.y);
        Assert.Equal(source.z, result.z);
    }

    [Fact(DisplayName = "Vector3D copy constructor should correctly copy data from source Vector2D")]
    public void CopyConstructorShouldCorrectlyCopyDataFromSourceVector2D()
    {
        // Arrange
        var source = new Vector3D(1f, 2f, 3f);

        // Act
        var result = new Vector3D((Vector2D)source);

        // Assert
        Assert.Equal(source.x, result.x);
        Assert.Equal(source.y, result.y);
        Assert.Equal(0, result.z);
    }
}
