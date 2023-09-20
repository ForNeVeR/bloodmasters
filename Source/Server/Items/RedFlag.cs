/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using CodeImp.Bloodmasters.LevelMap;

namespace CodeImp.Bloodmasters.Server;

[ServerItem(4002, RespawnTime=0)]
public class RedFlag : Flag
{
    #region ================== Constants

    #endregion

    #region ================== Variables

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public RedFlag(Thing t) : base(t)
    {
        // Set teams
        this.thisteam = TEAM.RED;
        this.otherteam = TEAM.BLUE;
    }

    #endregion
}
