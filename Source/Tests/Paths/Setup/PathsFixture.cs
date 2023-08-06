namespace Bloodmasters.Tests.Paths.Setup;

public class PathsFixture : IDisposable
{
    public static IEnumerable<object[]> GetAllDirPaths()
    {
        yield return new object[] { CodeImp.Bloodmasters.Paths.TempDirPath };
        yield return new object[] { CodeImp.Bloodmasters.Paths.DownloadedResourceDir };
        yield return new object[] { CodeImp.Bloodmasters.Paths.ConfigDirPath };
    }

    public void Dispose()
    {
        foreach (string dir in GetAllDirPaths().SelectMany(x => x))
        {
            Directory.Delete(dir);
        }
    }
}
