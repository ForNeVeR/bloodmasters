using System.Diagnostics;
using System.Reflection;

namespace CodeImp.Bloodmasters;

internal enum StartupMode
{
    Production,
    Dev
}

public abstract class Paths
{
    public const string GameName = "Bloodmasters";
    public const string DefaultConfigFileName = "Bloodmasters.cfg";

    protected const string ClientExecutableFileName = "Bloodmasters.exe";
    protected const string LauncherExecutableFileName = "BMLauncher.exe";

    private static readonly Lazy<Paths> _instance
        = new(() => Create(StartupMode.Production));

    public static readonly Paths Instance
        = _instance.Value;

    internal static Paths Create(StartupMode startupMode)
        => startupMode switch
        {
            StartupMode.Production => new ProductionPaths(),
            StartupMode.Dev => new DevPaths(),
            _ => throw new NotSupportedException()
        };

    /// <summary>Directory of the entry binary.</summary>
    /// <remarks>In dev mode, it looks like <c>Source/Client/bin/Debug/net7.0-windows</c>.</remarks>
    protected static readonly string AppBaseDir = EvaluateAppBaseDir();

    private static string EvaluateAppBaseDir()
    {
        var modulePath = Assembly.GetEntryAssembly()?.Location ?? Process.GetCurrentProcess().MainModule?.FileName;

        if (modulePath is null)
        {
            return Environment.CurrentDirectory;
        }

        return Path.GetDirectoryName(modulePath) ?? Environment.CurrentDirectory;
    }

    protected static string EvaluateDir(Environment.SpecialFolder specialFolder, params string[] additionalPaths)
    {
        var targetPath = Path.Combine(
            additionalPaths
                .Prepend(Environment.GetFolderPath(specialFolder))
                .ToArray());

        return Directory.CreateDirectory(targetPath).FullName;
    }

    public abstract string ClientExecutablePath { get; }

    public abstract string ContentDirPath { get; }

    public abstract string LauncherExecutablePath { get; }

    public abstract string ConfigDirPath { get; }

    /// <summary>Directory with the resources distributed alongside tha game. Read-only access.</summary>
    public string BundledResourceDir
        => ContentDirPath;

    /// <summary>Directory for the log files. Write access.</summary>
    // TODO[#93]: Logs in production should be moved to another place
    public string LogDirPath
        => AppBaseDir;

    /// <summary>Directory for screenshots. Write access.</summary>
    public string ScreenshotsDir { get; } =
        EvaluateDir(
            Environment.SpecialFolder.MyPictures,
            GameName);

    /// <summary>Directory for the downloaded resources. Read + write access.</summary>
    public string DownloadedResourceDir { get; } =
        EvaluateDir(
            Environment.SpecialFolder.LocalApplicationData,
            GameName,
            "Downloads");

    /// <summary>Directory for temporary data. Read + write access.</summary>
    public string TempDir { get; } =
        Directory.CreateTempSubdirectory(GameName).FullName;
}

file sealed class ProductionPaths : Paths
{
    /// <inheritdoc />
    public override string ClientExecutablePath
        => Path.Combine(AppBaseDir, ClientExecutableFileName);

    /// <inheritdoc />
    public override string ContentDirPath
        => Path.Combine(AppBaseDir);

    /// <inheritdoc />
    public override string LauncherExecutablePath
        => Path.Combine(AppBaseDir, LauncherExecutableFileName);

    /// <inheritdoc />
    public override string ConfigDirPath { get; } =
        EvaluateDir(
            Environment.SpecialFolder.ApplicationData,
            GameName,
            "Config");
}

file sealed class DevPaths : Paths
{
    private const string DevModeMarkerFileName = ".bloodmasters.dev.marker";
    private const string DevSolutionRootMarkerFileName = ".bloodmasters.solution-root.marker";
    private const string ExecutablesTargetFramework = "net7.0-windows";
    private const string BuildConfiguration =
#if DEBUG
        "Debug";
#else
        "Release";
#endif

    private static readonly string SolutionRootPath =
        FindSolutionRootRelativelyTo(AppBaseDir)
            ?? throw new InvalidOperationException("Unable to find solution root");

    private static string? FindSolutionRootRelativelyTo(string appBaseDir)
    {
        var currentDir = appBaseDir;

        while (currentDir != null && !File.Exists(Path.Combine(currentDir, DevSolutionRootMarkerFileName)))
        {
            currentDir = Path.GetDirectoryName(currentDir);
        }

        return currentDir;
    }

    public DevPaths()
    {
        if (!File.Exists(Path.Combine(AppBaseDir, DevModeMarkerFileName)))
        {
            throw new InvalidOperationException(
                $"{nameof(DevPaths)} can only be used if the file \"{DevModeMarkerFileName}\" exists");
        }
    }

    /// <inheritdoc />
    public override string ClientExecutablePath
        => Path.Combine(
            SolutionRootPath,
            "Source",
            "Client",
            "bin",
            BuildConfiguration,
            ExecutablesTargetFramework,
            ClientExecutableFileName);

    /// <inheritdoc />
    public override string ContentDirPath
        => Path.Combine(SolutionRootPath, "Source", "Content");

    /// <inheritdoc />
    public override string LauncherExecutablePath
        => Path.Combine(
            SolutionRootPath,
            "Source",
            "Launcher",
            "bin",
            BuildConfiguration,
            ExecutablesTargetFramework,
            LauncherExecutableFileName);

    /// <inheritdoc />
    public override string ConfigDirPath
        => Path.Combine(SolutionRootPath, "Source", "Config", "Debug");
}
