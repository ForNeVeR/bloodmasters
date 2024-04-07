/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using Bloodmasters.LevelMap;

namespace Bloodmasters.Server.Items;

[ServerItem(4003, RespawnTime=0)]
public class WhiteScavengerItem : ScavengerItem
{
    #region ================== Constants

    #endregion

    #region ================== Variables

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public WhiteScavengerItem(Thing t) : base(t)
    {
        // Set teams
        this.thisteam = TEAM.NONE;
        this.otherteam = TEAM.NONE;

        // If this is not a Scavenger game, remove the item
        if(Host.Instance.Server.GameType != GAMETYPE.SC) this.Temporary = true;
    }

    #endregion
}
