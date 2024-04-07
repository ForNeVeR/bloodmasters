using System.IO;
using Bloodmasters.Client.Graphics;
using Bloodmasters.LevelMap;

namespace Bloodmasters.Client.LevelMap;

public class ClientSidedef : Sidedef
{
    // Visual Sidedef
    private VisualSidedef vissidedef = null;

    public VisualSidedef VisualSidedef { get { return vissidedef; } set { vissidedef = value; } }

    public ClientSidedef(BinaryReader data, Sector[] sectors, int index) : base(data, sectors, index)
    {
    }
}
