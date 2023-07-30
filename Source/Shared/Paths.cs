using System.Diagnostics;
using System.Reflection;

namespace CodeImp.Bloodmasters;

public static class Paths
{
    private const string DevModeMarkerFileName = ".bloodmasters.dev.marker";
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
            ? Path.Combine(AppBaseDir, "../../../../../")
            : null;

    private const string ClientExecutableFileName = "Bloodmasters.exe";
    public static readonly string ClientExecutablePath =
        IsDevModeBuild
            ? Path.Combine(
                SolutionRootPath!,
                "Source",
                "Bloodmasters",
                "bin",
                BuildConfiguration,
                TargetFrameworkForExecutables,
                ClientExecutableFileName)
            : Path.Combine(AppBaseDir, ClientExecutableFileName);

    // TODO: Get rid of its usage.
    private static string AllPurposeDirPath =
        IsDevModeBuild
            ? Path.Combine(SolutionRootPath!, "Build")
            : Path.Combine(AppBaseDir);

    private const string LauncherExecutableFileName = "BMLauncher.exe";
    public static readonly string LauncherExecutablePath =
        IsDevModeBuild
            ? Path.Combine(SolutionRootPath!, "Build", LauncherExecutableFileName)
            : Path.Combine(AppBaseDir, LauncherExecutableFileName);

    /// <summary>Directory with the resources distributed alongside tha game. Read-only access.</summary>
    public static readonly string BundledResourceDir = AllPurposeDirPath;

    /// <summary>Directory for the downloaded resources. Read + write access.</summary>
    public static readonly string DownloadedResourceDir = AllPurposeDirPath;

    /// <summary>Directory with the game configuration files. Read + write access.</summary>
    public static readonly string ConfigDirPath = AllPurposeDirPath;

    /// <summary>Directory for the log files. Write access.</summary>
    public static readonly string LogDirPath = AllPurposeDirPath;
}
