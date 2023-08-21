using System.Diagnostics;
using System.Reflection;

namespace CodeImp.Bloodmasters;

public class Paths
{
    private const string DevModeMarkerFileName = ".bloodmasters.dev.marker";
    private const string DevSolutionRootMarkerFileName = ".bloodmasters.solution-root.marker";
    private const string TargetFrameworkForExecutables = "net7.0-windows";
    private readonly string _buildConfiguration =
#if DEBUG
        "Debug";
#else
        "Release";
#endif

    public Paths(bool isDev = false)
    {
        if (isDev)
            _buildConfiguration = "Debug";
    }

    /// <summary>Directory of the entry binary.</summary>
    /// <remarks>In dev mode, it looks like <c>Source/Client/bin/Debug/net7.0-windows</c>.</remarks>
    private string AppBaseDir => GetAppBaseDir();
    private string GetAppBaseDir()
    {
        var modulePath = Assembly.GetEntryAssembly()?.Location ?? Process.GetCurrentProcess().MainModule?.FileName;
        if (modulePath == null) return Environment.CurrentDirectory;
        return Path.GetDirectoryName(modulePath) ?? Environment.CurrentDirectory;
    }

    internal bool IsDevModeBuild =>
        File.Exists(Path.Combine(AppBaseDir, DevModeMarkerFileName));

    private string? SolutionRootPath =>
        IsDevModeBuild
            ? FindSolutionRootRelativelyTo(AppBaseDir)
            : null;

    private string? FindSolutionRootRelativelyTo(string appBaseDir)
    {
        var currentDir = appBaseDir;
        while (currentDir != null && !File.Exists(Path.Combine(currentDir, DevSolutionRootMarkerFileName)))
        {
            currentDir = Path.GetDirectoryName(currentDir);
        }

        return currentDir;
    }

    private const string ClientExecutableFileName = "Bloodmasters.exe";
    public string ClientExecutablePath =>
        IsDevModeBuild
            ? Path.Combine(
                SolutionRootPath!,
                "Source",
                "Client",
                "bin",
                _buildConfiguration,
                TargetFrameworkForExecutables,
                ClientExecutableFileName)
            : Path.Combine(AppBaseDir, ClientExecutableFileName);

    private string ContentDirPath =>
        IsDevModeBuild
            ? Path.Combine(SolutionRootPath!, "Source", "Content")
            : Path.Combine(AppBaseDir);

    private const string LauncherExecutableFileName = "BMLauncher.exe";
    public string LauncherExecutablePath =>
        IsDevModeBuild
            ? Path.Combine(
                SolutionRootPath!,
                "Source",
                "Launcher",
                "bin",
                _buildConfiguration,
                TargetFrameworkForExecutables,
                LauncherExecutableFileName)
            : Path.Combine(AppBaseDir, LauncherExecutableFileName);

    /// <summary>Directory with the resources distributed alongside tha game. Read-only access.</summary>
    public string BundledResourceDir => ContentDirPath;

    /// <summary>Directory for the downloaded resources. Read + write access.</summary>
    public readonly string DownloadedResourceDir = Directory.CreateDirectory(
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Bloodmasters",
            "Downloads"
        )).FullName;

    /// <summary>Directory with the game configuration files. Read + write access.</summary>
    public string ConfigDirPath =>
        IsDevModeBuild
            ? Path.Combine(SolutionRootPath!, "Source", "Config", "Debug")
            : Directory.CreateDirectory(
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Bloodmasters",
                    "Config"
                )).FullName;

    /// <summary>Directory for the log files. Write access.</summary>
    // TODO[#93]: Logs in production should be moved to another place
    public string LogDirPath => AppBaseDir;

    /// <summary>Directory for screenshots. Write access.</summary>
    public readonly string ScreenshotsDir = Directory.CreateDirectory(
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
            "Bloodmasters"
        )).FullName;

    /// <summary>Directory for temporary data. Read + write access.</summary>
    public readonly string TempDir = Directory.CreateTempSubdirectory(prefix: "Bloodmasters").FullName;
}
