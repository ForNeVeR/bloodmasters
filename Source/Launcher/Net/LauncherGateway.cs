using Bloodmasters.Net;

namespace Bloodmasters.Launcher.Net;

public class LauncherGateway : Gateway
{
    public LauncherGateway(int port, int simping, int simloss) : base(port, simping, simloss)
    {
    }

    protected override void WriteLine(string text)
    {
    }
}
