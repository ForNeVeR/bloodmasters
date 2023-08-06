namespace Bloodmasters.Tests.Paths.Setup;

public class PathsFixture : IDisposable
{
    public static IEnumerable<object[]> GetAllDirPaths()
    {
        yield return new object[] { CodeImp.Bloodmasters.Paths.TempDir };
        yield return new object[] { CodeImp.Bloodmasters.Paths.DownloadedResourceDir };
        yield return new object[] { CodeImp.Bloodmasters.Paths.ConfigDirPath };
        yield return new object[] { CodeImp.Bloodmasters.Paths.ScreenshotsDir };
    }

    public void Dispose()
    {
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI")))
        {
            foreach (string dir in GetAllDirPaths().SelectMany(x => x))
            {
                Directory.Delete(dir, true);
            }
        }
    }
}
