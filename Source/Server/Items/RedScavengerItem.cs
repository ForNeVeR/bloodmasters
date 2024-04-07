/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using Bloodmasters.LevelMap;

namespace Bloodmasters.Server.Items;

[ServerItem(4004, RespawnTime=0)]
public class RedScavengerItem : ScavengerItem
{
    #region ================== Constants

    #endregion

    #region ================== Variables

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public RedScavengerItem(Thing t) : base(t)
    {
        // Set teams
        this.thisteam = TEAM.RED;
        this.otherteam = TEAM.BLUE;

        // For normal Scavenger game, place a White item instead
        if(Host.Instance.Server.GameType == GAMETYPE.SC)
        {
            // Make white item
            Item white = new WhiteScavengerItem(t);
            Host.Instance.Server.items.Add(white.Key, white);
        }

        // If this is not a Team Scavenger game, remove the item
        if(Host.Instance.Server.GameType != GAMETYPE.TSC) this.Temporary = true;
    }

    #endregion
}
