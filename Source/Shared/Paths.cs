using System.Diagnostics;
using System.Reflection;

namespace CodeImp.Bloodmasters;

internal enum StartupMode
{
    AutoDetect,
    Production,
    Dev
}

public abstract class Paths
{
    public const string GameName = "Bloodmasters";
    public const string DefaultConfigFileName = "Bloodmasters.cfg";

    protected const string ClientExecutableFileName = "Bloodmasters.exe";
    protected const string LauncherExecutableFileName = "BMLauncher.exe";

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

    internal static Paths Create(StartupMode startupMode)
        => startupMode switch
        {
            StartupMode.AutoDetect => DevPaths.HasDevModeMarker ? new DevPaths() : new ProductionPaths(),
            StartupMode.Production => new ProductionPaths(),
            StartupMode.Dev => new DevPaths(),
            _ => throw new NotSupportedException()
        };

    private static readonly Lazy<Paths> _instance
        = new(() => Create(StartupMode.AutoDetect));

    public static readonly Paths Instance
        = _instance.Value;

    protected static string EvaluateDir(Environment.SpecialFolder specialFolder, params string[] additionalPaths)
    {
        var targetPath = Path.Combine(
            additionalPaths
                .Prepend(Environment.GetFolderPath(specialFolder))
                .ToArray());

        return Directory.CreateDirectory(targetPath).FullName;
    }

    internal abstract StartupMode CurrentMode { get; }

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
    internal override StartupMode CurrentMode
        => StartupMode.Production;

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

    public static readonly bool HasDevModeMarker =
        File.Exists(Path.Combine(AppBaseDir, DevModeMarkerFileName));

    private static string? FindSolutionRootRelativelyTo(string appBaseDir)
    {
        var currentDir = appBaseDir;

        while (currentDir != null && !File.Exists(Path.Combine(currentDir, DevSolutionRootMarkerFileName)))
        {
            currentDir = Path.GetDirectoryName(currentDir);
        }

        return currentDir;
    }

    private readonly string _solutionRootPath;

    public DevPaths()
    {
        if (!HasDevModeMarker)
        {
            throw new InvalidOperationException(
                $"{nameof(DevPaths)} can only be used if the file \"{DevModeMarkerFileName}\" exists");
        }

        _solutionRootPath =
            FindSolutionRootRelativelyTo(AppBaseDir)
                ?? throw new InvalidOperationException("Unable to find solution root");
    }

    /// <inheritdoc />
    internal override StartupMode CurrentMode
        => StartupMode.Dev;

    /// <inheritdoc />
    public override string ClientExecutablePath
        => Path.Combine(
            _solutionRootPath,
            "Source",
            "Client",
            "bin",
            BuildConfiguration,
            ExecutablesTargetFramework,
            ClientExecutableFileName);

    /// <inheritdoc />
    public override string ContentDirPath
        => Path.Combine(_solutionRootPath, "Source", "Content");

    /// <inheritdoc />
    public override string LauncherExecutablePath
        => Path.Combine(
            _solutionRootPath,
            "Source",
            "Launcher",
            "bin",
            BuildConfiguration,
            ExecutablesTargetFramework,
            LauncherExecutableFileName);

    /// <inheritdoc />
    public override string ConfigDirPath
        => Path.Combine(_solutionRootPath, "Source", "Config", "Debug");
}
