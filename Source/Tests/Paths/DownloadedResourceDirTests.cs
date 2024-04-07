namespace Bloodmasters.Tests.Paths;

public class DownloadedResourceDirTests
{
    [Fact(DisplayName = "DownloadedResourceDir should be named 'Downloads'")]
    public void DownloadedResourceDirShouldBeNamedBloodmasters()
    {
        // Arrange
        var dirName = Path.GetFileName(Bloodmasters.Paths.Instance.DownloadedResourceDir);

        // Assert
        Assert.Equal("Downloads", dirName);
    }

    [Fact(DisplayName = "DownloadedResourceDir should be subdirectory of the 'Bloodmasters' directory")]
    public void DownloadedResourceDirShouldBeSubdirectoryOfBloodmasters()
    {
        // Arrange
        var dirPath = Bloodmasters.Paths.Instance.DownloadedResourceDir;
        var bloodmastersDirPath = Path.GetDirectoryName(dirPath);

        // Assert
        Assert.Equal("Bloodmasters", Path.GetFileName(bloodmastersDirPath));
    }
}
