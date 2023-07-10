using System;
using System.IO;
#if CLIENT
using CodeImp.Bloodmasters.Client;
#endif

namespace CodeImp.Bloodmasters.Server.Net;

public class ServerGateway : Gateway
{
    public ServerGateway(int port, int simping, int simloss) : base(port, simping, simloss)
    {
    }

    protected override void WriteLine(string text)
    {
        Console.WriteLine(text);

        // Write to log file as well?
        if(General.logtofile)
        {
            // Append text to the file
            StreamWriter logf = File.AppendText(General.logfilename);
            logf.WriteLine(General.StripColorCodes(text));
            logf.Flush();
            logf.Close();
        }
    }
}
