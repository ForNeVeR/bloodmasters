/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using Bloodmasters.LevelMap;

namespace Bloodmasters.Server;

[ServerItem(4001, RespawnTime=0)]
public class BlueFlag : Flag
{
    #region ================== Constants

    #endregion

    #region ================== Variables

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public BlueFlag(Thing t) : base(t)
    {
        // Set teams
        this.thisteam = TEAM.BLUE;
        this.otherteam = TEAM.RED;
    }

    #endregion
}
