namespace CodeImp.Bloodmasters.Server;

public interface IHost
{
    /// <summary>Client or server host.</summary>
    public string HostKindName { get; }
    public bool IsServer { get; }

    public Random Random { get; }

    public bool LogToFile { get; }
    public string LogFileName { get; }

    public int RealTime { get; }

    public GameServer Server { get; }

    /// <summary>This makes adjustments to catch up any lag</summary>
    public void CatchLag();

    /// <summary>On client: if the server console is open. On server: always true.</summary>
    public bool IsConsoleVisible { get; }

    /// <summary>This writes an output message.</summary>
    public void WriteMessage(string markup, bool showWhenClient);

    /// <summary>This dumps an exception to the error file</summary>
    public void OutputError(Exception error);

    /// <summary>This writes an exception to the log file</summary>
    public void WriteErrorLine(Exception error);
}

/// <summary>
/// This is a storage for process-global state, such as list of connected clients, current timestamp etc.
/// </summary>
public static class Host
{
    private static IHost _instance = null!;

    /// <remarks>
    /// This is specifically not marked as volatile or anything else for performance. It's the responsibility of whoever
    /// calls the setter to do it in a thread-safe manner, best of all before doing anything else in the program.
    /// </remarks>
    public static IHost Instance
    {
        get => _instance;
        set
        {
#if DEBUG
            if (_instance != null)
                throw new InvalidOperationException(
                    $"Cannot set global instance to {value} because it's already set to {_instance}");
#endif
            _instance = value;
        }
    }
}
