namespace Bloodmasters.Tests.Paths;

public class ScreenshotsDirTests
{
    [Fact(DisplayName = "ScreenshotsDir should be named 'Bloodmasters'")]
    public void ScreenshotsDirShouldBeNamedBloodmasters()
    {
        // Arrange
        var dirName = Path.GetFileName(CodeImp.Bloodmasters.Paths.Instance.ScreenshotsDir);

        // Assert
        Assert.Equal("Bloodmasters", dirName);
    }
}
