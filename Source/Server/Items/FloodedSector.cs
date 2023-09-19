/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using CodeImp.Bloodmasters.Map;

namespace CodeImp.Bloodmasters.Server;

[ServerItem(9003, OnFloor=false)]
public class FloodedSector : Item
{
    #region ================== Constants

    private const int TICK_INTERVAL = 100;

    #endregion

    #region ================== Variables

    private int ticktime;

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public FloodedSector(Thing t) : base(t)
    {
        // Apply liquid settings to sector
        t.Sector.LiquidType = (LIQUID)t.Arg[0];
        t.Sector.LiquidHeight = this.Position.z;

        // Set timer
        ticktime = SharedGeneral.currenttime;
    }

    #endregion

    #region ================== Processing

    // Processing
    public override void Process()
    {
        // Time to check for players?
        if(ticktime < SharedGeneral.currenttime)
        {
            // Go for all clients
            foreach(Client c in Host.Instance.Server.clients)
            {
                // Client playing, alive, in this sector and below this height?
                if((c != null) && c.IsAlive)
                {
                    // Client in this sector and below me?
                    if((c.HighestSector == this.Sector) && (c.State.pos.z <= this.Position.z))
                    {
                        // Liquid effect should apply to this player
                        switch(Sector.LiquidType)
                        {
                            // Water
                            case LIQUID.WATER:

                                // Player on fire? Then kill the fire.
                                if(c.FireIntensity > 0) c.KillFire();
                                break;

                            // Lava
                            case LIQUID.LAVA:

                                // No fire yet?
                                if(c.FireIntensity < 1000)
                                {
                                    // Create big fire
                                    c.AddFireIntensity(2000, null);
                                }
                                else
                                {
                                    // Add to fire intensity
                                    c.AddFireIntensity(200, null);
                                }
                                break;
                        }
                    }
                }
            }

            // Increase timer
            ticktime += TICK_INTERVAL;
        }

        // Pass control to base class
        base.Process();
    }

    #endregion
}
