using System.IO;

namespace CodeImp.Bloodmasters.Client;

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
