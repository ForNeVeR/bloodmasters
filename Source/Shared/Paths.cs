using System.Diagnostics;
using System.Reflection;

namespace CodeImp.Bloodmasters;

public static class Paths
{
    private const string DevModeMarkerFileName = ".bloodmasters.dev.marker";

    /// <summary>Directory of the entry binary.</summary>
    /// <remarks>In dev mode, it looks like <c>Source/Client/bin/Debug/net7.0-windows/Bloodmasters.exe</c>.</remarks>
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

    private const string LauncherExecutableFileName = "BMLauncher.exe";
    public static readonly string LauncherPath =
        IsDevModeBuild
            ? Path.Combine(SolutionRootPath!, "Build", LauncherExecutableFileName)
            : Path.Combine(AppBaseDir, LauncherExecutableFileName);
}
