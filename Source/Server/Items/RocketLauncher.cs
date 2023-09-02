/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

namespace CodeImp.Bloodmasters.Server;

[ServerItem(1004, RespawnTime=5000)]
public class RocketLauncher : Item
{
    #region ================== Constants

    private const WEAPON weaponid = WEAPON.ROCKET_LAUNCHER;

    #endregion

    #region ================== Variables

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public RocketLauncher(Thing t) : base(t)
    {
    }

    #endregion

    #region ================== Control

    // This is called when the item is being touched by a player
    public override void Pickup(Client c)
    {
        // Check if the client does not have this weapon yet
        if(!c.HasWeapon(weaponid))
        {
            // Do what you have to do
            base.Pickup(c);

            // Take the item
            this.Take(c);

            // Give the weapon
            c.GiveWeapon(weaponid);
        }
    }

    #endregion
}
