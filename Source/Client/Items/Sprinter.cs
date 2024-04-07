/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using Bloodmasters.LevelMap;

namespace Bloodmasters.Client.Items;

[ClientItem(3002, Sprite="speed.cfg",
    Bob = true,
    Description="Sprinter",
    Sound="pickuppowerup.wav")]
[PowerupItem(R=0.2f, G=0.2f, B=0.5f)]
public class Sprinter : Powerup
{
    #region ================== Constants

    #endregion

    #region ================== Variables

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public Sprinter(Thing t) : base(t)
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
            clnt.SetPowerupCountdown(Consts.POWERUP_SPEED_COUNT, false);
        }

        // Call the base class
        base.Take(clnt);
    }
}
