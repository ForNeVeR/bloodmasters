/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/


using SharpDX.Direct3D9;

namespace CodeImp.Bloodmasters.Launcher.Interface;

public struct DisplayAdapterItem
{
    public int ordinal;
    public string description;

    // Constructor
    public DisplayAdapterItem(int adapterIndex, AdapterDetails ai)
    {
        ordinal = adapterIndex;
        description = ai.Description;
    }

    // String representation
    public override string ToString()
    {
        return description;
    }
}
