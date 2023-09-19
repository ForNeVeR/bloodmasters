using System;
using System.IO;
using CodeImp.Bloodmasters.Net;
using CodeImp.Bloodmasters.Server;

namespace CodeImp.Bloodmasters.DedicatedServer;

public class ServerHost : IHost
{
    public string HostKindName => "Dedicated Server";
    public bool IsServer => true;

    public Random Random => General.random;

    public bool LogToFile => General.logtofile;
    public string LogFileName => General.logfilename;

    public int RealTime => SharedGeneral.realtime;

    public GameServer Server => General.server;

    public void CatchLag() => General.CatchLag();

    public bool IsConsoleVisible => true;

    public void WriteMessage(string markup, bool showWhenClient)
    {
        // One message at a time!
        lock(Console.Out)
        {
            // For the server, output to standard console
            Console.Write(Markup.StripColorCodes(markup));

            // Write to log file as well?
            if(LogToFile)
            {
                // Append text to the file
                StreamWriter logf = File.AppendText(Host.Instance.LogFileName);
                logf.Write(Markup.StripColorCodes(markup));
                logf.Flush();
                logf.Close();
            }
        }
    }

    public void OutputError(Exception error) => General.OutputError(error);
    public void WriteErrorLine(Exception error) => General.WriteErrorLine(error);
}
