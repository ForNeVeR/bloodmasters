using System;
using CodeImp.Bloodmasters.Server;

namespace CodeImp.Bloodmasters.Client;

public class ClientGlobal : IGlobal
{
    public Random Random => General.random;

    public bool LogToFile => General.logtofile;
    public string LogFileName => General.logfilename;
    public string TempPath => General.temppath;

    public int RealTime => SharedGeneral.realtime;

    public GameServer Server => General.server;

    public void CatchLag() => General.CatchLag();

    public void OutputError(Exception error) => General.OutputError(error);
    public void WriteErrorLine(Exception error) => General.WriteErrorLine(error);
}
