namespace Bloodmasters.Tests.Paths;

public class ConfigDirPathTests
{
    [Fact(DisplayName = "ConfigDirPath should be named 'Debug' in dev mode and 'Config' otherwise")]
    public void ConfigDirPathShouldBeNamedDebugInDevModeAndConfigOtherwise()
    {
        // Arrange
        var dirName = Path.GetFileName(CodeImp.Bloodmasters.Paths.ConfigDirPath);

        // Assert
        Assert.Equal(
            CodeImp.Bloodmasters.Paths.IsDevModeBuild ? "Debug" : "Config",
            dirName);
    }

    [Fact(DisplayName = "ConfigDirPath should be subdirectory of the 'Bloodmasters' directory in dev mode")]
    public void DownloadedResourceDirShouldBeSubdirectoryOfBloodmasters()
    {
        if (CodeImp.Bloodmasters.Paths.IsDevModeBuild) return;

        // Arrange
        var dirPath = CodeImp.Bloodmasters.Paths.ConfigDirPath;
        var bloodmastersDirPath = Path.GetDirectoryName(dirPath);

        // Assert
        Assert.Equal("Bloodmasters", Path.GetFileName(bloodmastersDirPath));
    }
}
