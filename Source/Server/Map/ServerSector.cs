using System.IO;

namespace CodeImp.Bloodmasters.Server.Map;

public class ServerSector : Sector
{
    public ServerSector(BinaryReader data, int index, Bloodmasters.Map map) : base(data, index, map)
    {
    }

    protected override void DropPlayers()
    {
        // Go for all clients
        foreach(Client c in General.server.clients)
        {
            // Client in this sector and on the floor?
            if((c != null) && c.IsAlive && (c.HighestSector == this) && c.IsOnFloor)
            {
                // Drop on to highest sector
                c.DropImmediately();
            }
        }
    }
}
