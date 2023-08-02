namespace Bloodmasters.Tests.Paths;

public sealed class TempDirPathTests : IClassFixture<TempDirPathTestsFixture>
{
    private readonly string _tempDir;

    public TempDirPathTests(TempDirPathTestsFixture fixture)
    {
        _tempDir = fixture.TempDir;
    }

    [Fact(DisplayName = "Temporary directory should not be empty")]
    public void TemporaryDirectoryShouldNotBeEmpty()
    {
        Assert.NotEmpty(_tempDir);
    }

    [Fact(DisplayName = "Temporary directory should exist in the file system")]
    public void TemporaryDirectoryShouldExistInTheFileSystem()
    {
        Assert.True(Directory.Exists(_tempDir));
    }

    [Fact(DisplayName = "Temporary directory should only be created once")]
    public void TemporaryDirectoryShouldOnlyBeCreatedOnce()
    {
        // Arrange
        var tempDirPath2 = CodeImp.Bloodmasters.Paths.TempDirPath;

        // Assert
        Assert.Equal(_tempDir, tempDirPath2);
    }

    [Fact(DisplayName = "Temporary directory should contain suffix 'Bloodmasters'")]
    public void TemporaryDirectoryShouldContainSuffixBloodmasters()
    {
        // Arrange
        var dirName = Path.GetFileName(_tempDir);

        // Assert
        Assert.StartsWith("Bloodmasters", dirName);
    }
}

public sealed class TempDirPathTestsFixture : IDisposable
{
    internal readonly string TempDir;

    public TempDirPathTestsFixture()
    {
        TempDir = CodeImp.Bloodmasters.Paths.TempDirPath;
    }

    public void Dispose()
    {
        Directory.Delete(TempDir);
    }
}