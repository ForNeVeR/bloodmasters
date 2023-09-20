using System;
using System.IO;
using CodeImp.Bloodmasters.LevelMap;

namespace CodeImp.Bloodmasters.Launcher;

public class LauncherMap : Map
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
