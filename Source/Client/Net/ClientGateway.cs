using Bloodmasters.Net;

namespace Bloodmasters.Client.Net;

public class ClientGateway : Gateway
{
    public ClientGateway(int port, int simping, int simloss) : base(port, simping, simloss)
    {
    }

    protected override void WriteLine(string text)
    {
        if(General.serverwindow != null) General.serverwindow.WriteLine(text);
    }

    public override void WriteStats(string statsmsg)
    {
        // Output stats message
        General.console.AddMessage(statsmsg, true);
    }
}
