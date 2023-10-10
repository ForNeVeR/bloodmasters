namespace CodeImp.Bloodmasters.Tests.Paths;

public class ConfigDirPathTests
{
    [Fact(DisplayName = "ConfigDirPath should be named 'Config' in usual mode")]
    public void ConfigDirPathShouldBeNamedConfigInUsualMode()
    {
        // Arrange
        var sut = CodeImp.Bloodmasters.Paths.Instance;
        var dirName = Path.GetFileName(sut.ConfigDirPath);

        // Assert
        Assert.Equal("Config", dirName);
    }

    [Fact(DisplayName = "ConfigDirPath should be named 'Debug' in dev mode",
        Skip = "Dev mode requires .bloodmasters.dev.marker file")]
    public void ConfigDirPathShouldBeNamedDebugInDevMode()
    {
        // Arrange
        var sut = CodeImp.Bloodmasters.Paths.Create(StartupMode.Dev);
        var dirName = Path.GetFileName(sut.ConfigDirPath);

        // Assert
        Assert.Equal("Debug", dirName);
    }

    [Fact(DisplayName = "ConfigDirPath should be subdirectory of the 'Bloodmasters' directory in dev mode",
        Skip = "Dev mode requires .bloodmasters.dev.marker file")]
    public void ConfigDirPathShouldBeSubdirectoryOfBloodmastersInDevMode()
    {
        // Arrange
        var sut = CodeImp.Bloodmasters.Paths.Create(StartupMode.Dev);
        var dirPath = sut.ConfigDirPath;
        var bloodmastersDirPath = Path.GetDirectoryName(dirPath);

        // Assert
        Assert.Equal("Bloodmasters", Path.GetFileName(bloodmastersDirPath));
    }
}
