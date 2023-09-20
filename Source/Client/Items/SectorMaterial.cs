/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using CodeImp.Bloodmasters.LevelMap;

namespace CodeImp.Bloodmasters.Client.Items;

[ClientItem(9001, Temporary=true)]
public class SectorMaterial : Item
{
    #region ================== Constants

    #endregion

    #region ================== Variables

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public SectorMaterial(Thing t) : base(t)
    {
        // Apply sector material
        t.Sector.SetSurfaceMaterial(t.Arg[0]);
    }

    #endregion
}
