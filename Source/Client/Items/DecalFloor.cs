/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using Bloodmasters.Client.Graphics;
using Bloodmasters.Client.LevelMap;
using Bloodmasters.LevelMap;

namespace Bloodmasters.Client.Items;

[ClientItem(7003, Temporary=true)]
public class DecalFloor : Item
{
    #region ================== Constants

    #endregion

    #region ================== Variables

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public DecalFloor(Thing t) : base(t)
    {
        // Create the decal
        FloorDecal.Spawn((ClientSector)t.Sector, t.X, t.Y, FloorDecal.blooddecals, true, false, false);
    }

    #endregion
}
