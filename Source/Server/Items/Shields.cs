/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

namespace CodeImp.Bloodmasters.Server;

[ServerItem(3003, RespawnTime=90000)]
public class Shields : Item
{
    #region ================== Constants

    #endregion

    #region ================== Variables

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public Shields(Thing t) : base(t)
    {
    }

    #endregion

    #region ================== Control

    // This is calledwhen the item is being touched by a player
    public override void Pickup(Client c)
    {
        // Do what you have to do
        base.Pickup(c);

        // Take the item
        this.Take(c);

        // Give powerup to player
        c.GivePowerup(POWERUP.SHIELDS, Consts.POWERUP_SHIELD_COUNT);
    }

    #endregion
}
