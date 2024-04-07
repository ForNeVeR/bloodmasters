using System;
using System.IO;
using Bloodmasters.LevelMap;

namespace Bloodmasters.Launcher.Map;

public class LauncherMap : LevelMap.Map
{
    public LauncherMap(string mapname, bool infoonly, string temppath) : base(mapname, infoonly, temppath)
    {
    }

    protected override Sector CreateSector(BinaryReader data, int i)
    {
        throw new NotSupportedException();
    }

    protected override Sidedef CreateSidedef(BinaryReader data, Sector[] sectors, int index)
    {
        throw new NotSupportedException();
    }
}
