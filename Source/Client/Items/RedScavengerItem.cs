/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

namespace CodeImp.Bloodmasters.Client;

[ClientItem(4004, Sprite="sc_red.tga",
    Bob = true,
    Description="Scavenger Item",
    Sound="pickuphealth.wav")]
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
        // Set team
        SetTeam(TEAM.RED);

        // For normal Scavenger game, place a White item instead
        if(General.gametype == GAMETYPE.SC)
        {
            // Make white item
            Item white = new WhiteScavengerItem(t);
            General.arena.Items.Add(white.Key, white);
        }

        // If this is not a Team Scavenger game, remove the item
        if(General.gametype != GAMETYPE.TSC) this.Temporary = true;
    }

    #endregion
}
