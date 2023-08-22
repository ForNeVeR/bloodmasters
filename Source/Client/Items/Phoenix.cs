/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

namespace CodeImp.Bloodmasters.Client;

[ClientItem(1007, Sprite="phoenix.tga",
    Bob = true,
    Description="Phoenix",
    SpriteOffset=-0.6f,
    Sound="weaponpickup.wav")]
public class Phoenix : Item
{
    #region ================== Constants

    #endregion

    #region ================== Variables

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public Phoenix(Thing t) : base(t)
    {
    }

    #endregion

    // When picked up / taken
    public override void Take(Client clnt)
    {
        // Taken by me?
        if(General.localclient == clnt)
        {
            // Display item description
            General.hud.ShowItemMessage(this.Description);

            // Lock current weapon when automatically switching
            if(General.autoswitchweapon && !General.localclient.IsShooting)
                clnt.RequestSwitchWeaponTo(WEAPON.PHOENIX, false);
        }
    }
}
