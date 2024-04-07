using Bloodmasters;

namespace Bloodmasters.Tests.Paths;

public class ConfigDirPathTests
{
    [Fact(DisplayName = "ConfigDirPath should be named 'Config' in production mode")]
    public void ConfigDirPathShouldBeNamedConfigInProductionMode()
    {
        // Arrange
        var sut = Bloodmasters.Paths.Create(StartupMode.Production);
        var dirName = Path.GetFileName(sut.ConfigDirPath);

        // Assert
        Assert.Equal("Config", dirName);
    }

    [Fact(DisplayName = "ConfigDirPath should be named 'Debug' in dev mode")]
    public void ConfigDirPathShouldBeNamedDebugInDevMode()
    {
        // Arrange
        var sut = Bloodmasters.Paths.Create(StartupMode.Dev);
        var dirName = Path.GetFileName(sut.ConfigDirPath);

        // Assert
        Assert.Equal("Debug", dirName);
    }

    [Fact(DisplayName = "ConfigDirPath should be subdirectory of the 'Bloodmasters' directory in production mode")]
    public void ConfigDirPathShouldBeSubdirectoryOfBloodmastersInProductionMode()
    {
        // Arrange
        var sut = Bloodmasters.Paths.Create(StartupMode.Production);
        var bloodmastersDirPath = Path.GetDirectoryName(sut.ConfigDirPath);

        // Assert
        Assert.Equal("Bloodmasters", Path.GetFileName(bloodmastersDirPath));
    }

    [Fact(DisplayName = "ConfigDirPath should be subdirectory of the 'Bloodmasters' directory in dev mode")]
    public void ConfigDirPathShouldBeSubdirectoryOfBloodmastersInDevMode()
    {
        // Arrange
        var sut = Bloodmasters.Paths.Create(StartupMode.Dev);

        // Assert
        Assert.Contains("bloodmasters", sut.ConfigDirPath.Split(Path.DirectorySeparatorChar));
    }
}
