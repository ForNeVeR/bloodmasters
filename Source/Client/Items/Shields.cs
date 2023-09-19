/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using CodeImp.Bloodmasters.Map;

namespace CodeImp.Bloodmasters.Client.Items;

[ClientItem(3003, Sprite="shield.cfg",
    Bob = true,
    Description="Shields",
    Sound="pickuppowerup.wav")]
[PowerupItem(R=0.2f, G=0.6f, B=0.2f)]
public class Shields : Powerup
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

    // When picked up / taken
    public override void Take(Client clnt)
    {
        // Taken by me?
        if(General.localclient == clnt)
        {
            // Set the powerup countdown
            clnt.SetPowerupCountdown(Consts.POWERUP_SHIELD_COUNT, false);
        }

        // Call the base class
        base.Take(clnt);
    }
}
