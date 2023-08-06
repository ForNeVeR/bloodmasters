namespace Bloodmasters.Tests.Paths;

[Collection(TestCollections.Paths)]
public class ConfigDirPathTests
{
    [Fact(DisplayName = "ConfigDirPath should be named 'Bloodmasters'")]
    public void ConfigDirPathShouldBeNamedBloodmasters()
    {
        // Arrange
        var dirName = Path.GetFileName(CodeImp.Bloodmasters.Paths.ConfigDirPath);

        // Assert
        Assert.Equal("Bloodmasters", dirName);
    }
}
