namespace CodeImp.Bloodmasters.Tests.Paths;

public sealed class TempDirPathTests
{
    [Fact(DisplayName = "Temporary directory should contain suffix 'Bloodmasters'")]
    public void TemporaryDirectoryShouldContainSuffixBloodmasters()
    {
        // Arrange
        var dirName = Path.GetFileName(CodeImp.Bloodmasters.Paths.Instance.TempDir);

        // Assert
        Assert.StartsWith("Bloodmasters", dirName);
    }
}
