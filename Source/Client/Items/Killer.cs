/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

namespace CodeImp.Bloodmasters.Client;

[ClientItem(3001, Sprite="skull.cfg",
    Bob = true,
    Description="Killer",
    Sound="pickuppowerup.wav")]
[PowerupItem(R=0.6f, G=0.2f, B=0.2f)]
public class Killer : Powerup
{
    #region ================== Constants

    #endregion

    #region ================== Variables

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public Killer(Thing t) : base(t)
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
            clnt.SetPowerupCountdown(Consts.POWERUP_KILLER_COUNT, false);
        }

        // Call the base class
        base.Take(clnt);
    }
}
