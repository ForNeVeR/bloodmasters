/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

namespace CodeImp.Bloodmasters.Client;

[ClientItem(9003, Visible=false, OnFloor=false)]
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
            foreach(Client c in General.clients)
            {
                // Client playing, alive, in this sector and below this height?
                if((c != null) && (c.Actor != null))
                {
                    // Check if on screen
                    if(c.Actor.Sector.VisualSector.InScreen)
                    {
                        // Client in this sector and below me?
                        if((c.Actor.HighestSector == this.Sector) && (c.Actor.State.pos.z <= this.Position.z) && c.Actor.IsOnFloor)
                        {
                            // Liquid effect should apply to this player
                            switch(Sector.LiquidType)
                            {
                                case LIQUID.WATER: SpawnWaterParticles(c.Actor, 3); break;
                                case LIQUID.LAVA: SpawnLavaParticles(c.Actor, 3); break;
                            }
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

    // This makes particles for a player in water
    public static void SpawnWaterParticles(Actor a, int amount)
    {
        // Only when walking
        if(a.State.vel.LengthSq() > 0.0001f)
            SpawnWaterParticles(a.Position, a.Velocity, amount);
    }

    // This makes particles for a position in water
    public static void SpawnWaterParticles(Vector3D pos, Vector3D vel, int amount)
    {
        // Spawn several particles
        for(int i = 0; i < amount; i++)
        {
            General.arena.p_dust.Add(pos + Vector3D.Random(General.random, 2f, 2f, 2f),
                vel * 0.3f + Vector3D.Random(General.random, 0.05f, 0.05f, 0.03f),
                General.ARGB(1f, 0.3f, 0.4f, 1f));
            General.arena.p_dust.Add(pos + Vector3D.Random(General.random, 2f, 2f, 2f),
                vel * 0.3f + Vector3D.Random(General.random, 0.05f, 0.05f, 0.03f),
                General.ARGB(1f, 0.4f, 0.6f, 1f));
            General.arena.p_dust.Add(pos + Vector3D.Random(General.random, 2f, 2f, 2f),
                vel * 0.3f + Vector3D.Random(General.random, 0.05f, 0.05f, 0.03f),
                General.ARGB(1f, 0.5f, 0.9f, 1f));
        }
    }

    // This makes particles for a player in lava
    public static void SpawnLavaParticles(Actor a, int amount)
    {
        // Only when walking
        if(a.State.vel.LengthSq() > 0.0001f)
            SpawnLavaParticles(a.Position, a.Velocity, amount);
    }

    // This makes particles for a position in lava
    public static void SpawnLavaParticles(Vector3D pos, Vector3D vel, int amount)
    {
        // Spawn several particles
        for(int i = 0; i < amount; i++)
        {
            General.arena.p_magic.Add(pos + Vector3D.Random(General.random, 2f, 2f, 2f),
                vel * 0.3f + Vector3D.Random(General.random, 0.05f, 0.05f, 0.03f),
                General.ARGB(1f, 1f, 0.6f, 0.2f));
            General.arena.p_magic.Add(pos + Vector3D.Random(General.random, 2f, 2f, 2f),
                vel * 0.3f + Vector3D.Random(General.random, 0.05f, 0.05f, 0.03f),
                General.ARGB(1f, 1f, 1f, 0.4f));
        }
    }

    #endregion
}
