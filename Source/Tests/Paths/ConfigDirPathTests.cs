namespace Bloodmasters.Tests.Paths;

public class ConfigDirPathTests
{
    [Fact(DisplayName = "ConfigDirPath should be named 'Config' in release mode")]
    public void ConfigDirPathShouldBeNamedDebugInDevModeAndConfigOtherwise()
    {
        var paths = new CodeImp.Bloodmasters.Paths();
        var dirName = Path.GetFileName(paths.ConfigDirPath);

        // Assert
        Assert.Equal("Config",dirName);
    }

    [Fact(DisplayName = "ConfigDirPath should be named 'Debug' in dev mode")]
    public void ConfigDirPathShouldBeNamedDebugInDevMode()
    {
        // Arrange
        var paths = new CodeImp.Bloodmasters.Paths(isDev: true);
        var dirName = Path.GetFileName(paths.ConfigDirPath);

        // Assert
        Assert.Equal("Debug", dirName);
    }

    [Fact(DisplayName = "ConfigDirPath should be subdirectory of the 'Bloodmasters' directory in dev mode")]
    public void DownloadedResourceDirShouldBeSubdirectoryOfBloodmasters()
    {
        if (CodeImp.Bloodmasters.Paths.Instance.IsDevModeBuild) return;

        // Arrange
        var dirPath = CodeImp.Bloodmasters.Paths.Instance.ConfigDirPath;
        var bloodmastersDirPath = Path.GetDirectoryName(dirPath);

        // Assert
        Assert.Equal("Bloodmasters", Path.GetFileName(bloodmastersDirPath));
    }
}
