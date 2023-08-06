namespace Bloodmasters.Tests.Paths;

[Collection(TestCollections.Paths)]
public class DownloadedResourceDirTests
{
    [Fact(DisplayName = "DownloadedResourceDir should be named 'Bloodmasters'")]
    public void DownloadedResourceDirShouldBeNamedBloodmasters()
    {
        // Arrange
        var dirName = Path.GetFileName(CodeImp.Bloodmasters.Paths.DownloadedResourceDir);

        // Assert
        Assert.Equal("Bloodmasters", dirName);
    }
}
