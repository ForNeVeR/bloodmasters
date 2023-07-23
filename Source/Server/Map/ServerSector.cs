#if CLIENT
using CodeImp.Bloodmasters.Client;
#endif

namespace CodeImp.Bloodmasters.Server.Map;

public class ServerSector : Sector
{
    public ServerSector(BinaryReader data, int index, Bloodmasters.Map map) : base(data, index, map)
    {
    }

    protected override void DropPlayers()
    {
        // Go for all clients
        foreach(Client c in Global.Instance.Server.clients)
        {
            // Client in this sector and on the floor?
            if((c != null) && c.IsAlive && (c.HighestSector == this) && c.IsOnFloor)
            {
                // Drop on to highest sector
                c.DropImmediately();
            }
        }
    }

    protected override void UpdateClientSounds()
    {
    }

    protected override void UpdateLightmaps()
    {
    }

    protected override void UpdateClientLighting()
    {
    }

    protected override void PlaySounds(bool playstopsound)
    {
    }
}
