namespace Bloodmasters.Tests.Paths;

[Collection(TestCollections.Paths)]
public class ConfigDirPathTests
{
    [Fact(DisplayName = "ConfigDirPath should be named 'Debug' in dev mode and 'Bloodmasters' otherwise")]
    public void ConfigDirPathShouldBeNamedDebugInDevModeAndBloodmastersOtherwise()
    {
        // Arrange
        var dirName = Path.GetFileName(CodeImp.Bloodmasters.Paths.ConfigDirPath);

        // Assert
        Assert.Equal(
            CodeImp.Bloodmasters.Paths.IsDevModeBuild ? "Debug" : "Bloodmasters",
            dirName);
    }
}
