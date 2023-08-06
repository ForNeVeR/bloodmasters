namespace Bloodmasters.Tests.Paths;

[Collection(TestCollections.Paths)]
public sealed class TempDirPathTests
{
    [Fact(DisplayName = "Temporary directory should contain suffix 'Bloodmasters'")]
    public void TemporaryDirectoryShouldContainSuffixBloodmasters()
    {
        // Arrange
        var dirName = Path.GetFileName(CodeImp.Bloodmasters.Paths.TempDirPath);

        // Assert
        Assert.StartsWith("Bloodmasters", dirName);
    }
}
