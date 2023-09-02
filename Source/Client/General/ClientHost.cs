using System;
using CodeImp.Bloodmasters.Server;

namespace CodeImp.Bloodmasters.Client;

public class ClientHost : IHost
{
    public string HostKindName => "Client";
    public bool IsServer => false;

    public Random Random => General.random;

    public bool LogToFile => General.logtofile;
    public string LogFileName => General.logfilename;

    public int RealTime => SharedGeneral.realtime;

    public GameServer Server => General.server;

    public void CatchLag() => General.CatchLag();

    public bool IsConsoleVisible => General.serverwindow != null;

    public void WriteMessage(string markup, bool showWhenClient)
    {
        // Output to server window
        if(General.serverwindow != null)
            General.serverwindow.Write(Markup.StripColorCodes(markup));
        else if(General.console != null)
            if(showWhenClient) General.console.AddMessage(Markup.StripColorCodes(markup.Trim()), true);
    }

    public void OutputError(Exception error) => General.OutputError(error);
    public void WriteErrorLine(Exception error) => General.WriteErrorLine(error);
}
