using Bloodmasters.LevelMap;

namespace Bloodmasters.Server.LevelMap;

internal class ServerMap : Map
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
