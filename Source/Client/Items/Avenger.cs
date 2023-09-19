/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using CodeImp.Bloodmasters.Map;

namespace CodeImp.Bloodmasters.Client.Items;

[ClientItem(3006, Sprite="avenger.cfg",
    Bob = true,
    Description="Avenger",
    Sound="pickuppowerup.wav")]
[PowerupItem(R=0.4f, G=0.2f, B=0.4f)]
public class Avenger : Powerup
{
    #region ================== Constants

    #endregion

    #region ================== Variables

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public Avenger(Thing t) : base(t)
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
            clnt.SetPowerupCountdown(Consts.POWERUP_AVENGER_COUNT, false);
        }

        // Call the base class
        base.Take(clnt);
    }
}
