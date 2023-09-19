using System.IO;
using CodeImp.Bloodmasters.Client.Graphics;
using CodeImp.Bloodmasters.Map;

namespace CodeImp.Bloodmasters.Client;

public class ClientSidedef : Sidedef
{
    // Visual Sidedef
    private VisualSidedef vissidedef = null;

    public VisualSidedef VisualSidedef { get { return vissidedef; } set { vissidedef = value; } }

    public ClientSidedef(BinaryReader data, Sector[] sectors, int index) : base(data, sectors, index)
    {
    }
}
