using CodeImp.Bloodmasters.Map;

namespace CodeImp.Bloodmasters.Server.Map;

internal class ServerMap : CodeImp.Bloodmasters.Map.Map
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
