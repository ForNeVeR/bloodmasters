using CodeImp.Bloodmasters.Server.Map;

namespace CodeImp.Bloodmasters.Server;

internal class ServerMap : Bloodmasters.Map
{
    public ServerMap(string mapname, bool infoonly, string temppath) : base(mapname, infoonly, temppath)
    {
    }

    protected override Sector CreateSector(BinaryReader data, int i)
    {
        return new ServerSector(data, i, this);
    }

    protected override Sidedef CreateSidedef(BinaryReader data, Sector[] sectors, int index) =>
        new(data, sectors, index);
}
