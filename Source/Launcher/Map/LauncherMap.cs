using System;
using System.IO;

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
}
