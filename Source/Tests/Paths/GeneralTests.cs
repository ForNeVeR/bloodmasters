namespace Bloodmasters.Tests.Paths;

public class GeneralTests
{
    [Theory(DisplayName = "Directory should not be empty")]
    [MemberData(nameof(GetAllDirPaths))]
    public void TemporaryDirectoryShouldNotBeEmpty(string dirPath)
    {
        Assert.NotEmpty(dirPath);
    }

    [Theory(DisplayName = "Directory should exist in the file system")]
    [MemberData(nameof(GetAllDirPaths))]
    public void TemporaryDirectoryShouldExistInTheFileSystem(string dirPath)
    {
        Assert.True(Directory.Exists(dirPath));
    }

    public static IEnumerable<object[]> GetAllDirPaths()
    {
        yield return new object[] { Bloodmasters.Paths.Instance.TempDir };
        yield return new object[] { Bloodmasters.Paths.Instance.DownloadedResourceDir };
        yield return new object[] { Bloodmasters.Paths.Instance.ConfigDirPath };
        yield return new object[] { Bloodmasters.Paths.Instance.ScreenshotsDir };
    }
}
