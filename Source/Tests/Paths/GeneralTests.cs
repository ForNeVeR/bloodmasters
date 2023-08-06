using Bloodmasters.Tests.Paths.Setup;

namespace Bloodmasters.Tests.Paths;

[Collection(TestCollections.Paths)]
public class GeneralTests
{
    [Theory(DisplayName = "Directory should not be empty")]
    [MemberData(nameof(PathsFixture.GetAllDirPaths), MemberType = typeof(PathsFixture))]
    public void TemporaryDirectoryShouldNotBeEmpty(string dirPath)
    {
        Assert.NotEmpty(dirPath);
    }

    [Theory(DisplayName = "Directory should exist in the file system")]
    [MemberData(nameof(PathsFixture.GetAllDirPaths), MemberType = typeof(PathsFixture))]
    public void TemporaryDirectoryShouldExistInTheFileSystem(string dirPath)
    {
        Assert.True(Directory.Exists(dirPath));
    }
}
