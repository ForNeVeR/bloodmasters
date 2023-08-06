using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace CodeImp.Bloodmasters;

public static class Paths
{
    private const string DevModeMarkerFileName = ".bloodmasters.dev.marker";
    private const string DevSolutionRootMarkerFileName = ".bloodmasters.solution-root.marker";
    private const string TargetFrameworkForExecutables = "net7.0-windows";
    private const string BuildConfiguration =
#if DEBUG
        "Debug";
#else
        "Release";
#endif

    /// <summary>Directory of the entry binary.</summary>
    /// <remarks>In dev mode, it looks like <c>Source/Client/bin/Debug/net7.0-windows</c>.</remarks>
    private static readonly string AppBaseDir = GetAppBaseDir();
    private static string GetAppBaseDir()
    {
        var modulePath = Assembly.GetEntryAssembly()?.Location ?? Process.GetCurrentProcess().MainModule?.FileName;
        if (modulePath == null) return Environment.CurrentDirectory;
        return Path.GetDirectoryName(modulePath) ?? Environment.CurrentDirectory;
    }

    private static readonly bool IsDevModeBuild =
        File.Exists(Path.Combine(AppBaseDir, DevModeMarkerFileName));

    private static readonly string? SolutionRootPath =
        IsDevModeBuild
            ? FindSolutionRootRelativelyTo(AppBaseDir)
            : null;
    private static string? FindSolutionRootRelativelyTo(string appBaseDir)
    {
        var currentDir = appBaseDir;
        while (currentDir != null && !File.Exists(Path.Combine(currentDir, DevSolutionRootMarkerFileName)))
        {
            currentDir = Path.GetDirectoryName(currentDir);
        }

        return currentDir;
    }

    private const string ClientExecutableFileName = "Bloodmasters.exe";
    public static readonly string ClientExecutablePath =
        IsDevModeBuild
            ? Path.Combine(
                SolutionRootPath!,
                "Source",
                "Client",
                "bin",
                BuildConfiguration,
                TargetFrameworkForExecutables,
                ClientExecutableFileName)
            : Path.Combine(AppBaseDir, ClientExecutableFileName);

    private static readonly string ContentDirPath =
        IsDevModeBuild
            ? Path.Combine(SolutionRootPath!, "Source", "Content")
            : Path.Combine(AppBaseDir);

    private const string LauncherExecutableFileName = "BMLauncher.exe";
    public static readonly string LauncherExecutablePath =
        IsDevModeBuild
            ? Path.Combine(
                SolutionRootPath!,
                "Source",
                "Launcher",
                "bin",
                BuildConfiguration,
                TargetFrameworkForExecutables,
                LauncherExecutableFileName)
            : Path.Combine(AppBaseDir, LauncherExecutableFileName);

    /// <summary>Directory with the resources distributed alongside tha game. Read-only access.</summary>
    public static readonly string BundledResourceDir = ContentDirPath;

    /// <summary>Directory for the downloaded resources. Read + write access.</summary>
    public static readonly string DownloadedResourceDir = Directory.CreateDirectory(
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Bloodmasters"
        )).FullName;

    /// <summary>Directory with the game configuration files. Read + write access.</summary>
    public static readonly string ConfigDirPath =
        IsDevModeBuild
            ? Path.Combine(SolutionRootPath!, "Source", "Config", "Debug")
            : Path.Combine(AppBaseDir); // TODO[#93]: Better path for r+w files in release, like AppData

    /// <summary>Directory for the log files. Write access.</summary>
    // TODO[#93]: Logs in production should be moved to another place
    public static readonly string LogDirPath = AppBaseDir;

    /// <summary>Directory for screenshots. Write access.</summary>
    // TODO[#93]: Write screenshots to the user documents folder
    public static readonly string ScreenshotsDirPath = Path.Combine(AppBaseDir, "Screenshots");

    /// <summary>Directory for temporary data. Read + write access.</summary>
    public static readonly string TempDirPath = Directory.CreateTempSubdirectory(prefix: "Bloodmasters").FullName;
}
