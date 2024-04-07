/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using Bloodmasters.LevelMap;

namespace Bloodmasters.Server;

public class Platform : DynamicSector
{
    #region ================== Constants

    // Timing and speed
    private const int MOVE_BACK_DELAY = 1000;
    private const float SPEED = 0.6f;

    #endregion

    #region ================== Variables

    // Default height
    private float idleheight;

    // Platrform status
    private bool moving;

    // Time when to move the platform back
    // This is 0 when platform is not to be moved back
    private int movetime;

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public Platform(Sector s, bool lowplatform) : base(s)
    {
        // Determine idle height
        if(lowplatform)
            this.idleheight = s.LowestFloor;
        else
            this.idleheight = s.HeightFloor;
    }

    // Disposer
    public override void Dispose()
    {
        // Dispose base class
        base.Dispose();
    }

    #endregion

    #region ================== Methods

    // This processes te platform
    public override void Process()
    {
        bool lower = false;
        float lowerpos = float.MaxValue;

        // Process the sector movement
        sector.Process();

        // Go for all clients to check if anyone
        // is in this sector or a proximity sector
        foreach(Client c in Host.Instance.Server.clients)
        {
            // Client in the game?
            if((c != null) && !c.Loading && !c.Spectator && c.IsAlive)
            {
                // Client touching this floor?
                if(c.State.pos.z <= c.Sector.CurrentFloor + Consts.FLOOR_TOUCH_TOLERANCE)
                {
                    // Check if the sector has the same tag and field effect
                    if((sector.Tag == c.Sector.Tag) &&
                       (c.Sector.Effect == SECTOREFFECT.PLATFORMFIELD))
                    {
                        // Someone is in the platform proximity
                        // so lower to this height (and prefer lower heights)
                        lower = true;
                        if(c.Sector.CurrentFloor < lowerpos) lowerpos = c.Sector.CurrentFloor;
                        break;
                    }
                }
            }
        }

        // If the platform is moving and the sector reached
        // its target height then the platform is now stopped
        if(moving && (sector.CurrentFloor == sector.TargetFloor))
        {
            // Platform reached its target
            moving = false;
            SendSectorUpdate = true;
        }

        // Anyone who wants to use the platform?
        if(lower)
        {
            // Not already moving to this height?
            if(sector.TargetFloor != lowerpos)
            {
                // Move to this height now
                sector.MoveTo(lowerpos, SPEED);
                moving = true;
                SendSectorUpdate = true;
            }

            // Do not go back yet!
            movetime = 0;
        }
        else
        {
            // Stopped in a non-idle position?
            if((!moving) && (sector.CurrentFloor != idleheight))
            {
                // Timer to return to idle not set?
                if(movetime == 0)
                {
                    // Set timer to return to idle position
                    movetime = SharedGeneral.currenttime + MOVE_BACK_DELAY;
                }
                // Time to move?
                else if(movetime < SharedGeneral.currenttime)
                {
                    // Move back to idle position
                    sector.MoveTo(idleheight, SPEED);
                    moving = true;
                    SendSectorUpdate = true;
                }
            }
        }

        // Process base class
        base.Process();
    }

    #endregion
}
