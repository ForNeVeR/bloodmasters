/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using Bloodmasters.Client.Lights;
using Bloodmasters.LevelMap;

namespace Bloodmasters.Client.Items;

[ClientItem(6001, Temporary=true, OnFloor=false)]
public class Light : Item
{
    #region ================== Constants

    #endregion

    #region ================== Variables

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public Light(Thing t) : base(t)
    {
        // Create the light
        new StaticLight(t, General.arena.VisualSectors, true);
    }

    #endregion
}
