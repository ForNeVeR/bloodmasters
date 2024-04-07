using System.IO;
using Bloodmasters.LevelMap;

namespace Bloodmasters.Client.LevelMap;

public class ClientMap : Map
{
    public ClientMap(string mapname, bool infoonly, string temppath) : base(mapname, infoonly, temppath)
    {
    }

    protected override Sector CreateSector(BinaryReader data, int i) =>
        new ClientSector(data, i, this);

    protected override Sidedef CreateSidedef(BinaryReader data, Sector[] sectors, int index) =>
        new ClientSidedef(data, sectors, index);
}
