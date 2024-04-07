/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

namespace Bloodmasters.Server.Projectiles;

[ProjectileInfo(PROJECTILE.NUKEDETONATION)]
public class NukeDetonation : Projectile
{
    #region ================== Constants

    private const float HARD_RANGE = 20f;
    private const float SOFT_RANGE = 60f;
    private const int HARD_DAMAGE = 600;
    private const float SPLASH_Z_SCALE = 0.1f;
    private const float FIRE_RANGE = 30f;
    private const int FIRE_PROJECTILES = 60;
    private const int FIRE_DELAY = 600;
    private const int FIRE_INTENSITY = 2000;
    private const float PROJECTILE_Z = 7f;
    private const int HURT_DELAY = 500;

    #endregion

    #region ================== Variables

    // Timing
    private int firespawntime;
    private int hurttime;

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public NukeDetonation(Vector3D pos, Client source) : base(pos, new Vector3D(0f, 0f, 0f), source)
    {
        // Set timing
        firespawntime = SharedGeneral.currenttime + FIRE_DELAY;
        hurttime = SharedGeneral.currenttime + HURT_DELAY;

        // Kill the source player immediately
        // I use the falling death method here so that the actor
        // completely disappears without sound.
        source.Hurt(source, Client.DEATH_SELFNUKE, 1000, DEATHMETHOD.QUIET, true);
    }

    // Dispose
    public override void Dispose()
    {
        // Dispose base
        base.Dispose();
    }

    #endregion

    #region ================== Methods

    // When processed
    public override void Process()
    {
        float angle, mx, my, distance;
        Vector3D firepos;
        bool spawnfire;
        float amp;

        // Process projectile
        base.Process();

        // Time to hurt the players?
        if((SharedGeneral.currenttime > hurttime) && (hurttime > 0))
        {
            // Go for all playing clients
            foreach(Client c in Host.Instance.Server.clients)
            {
                // Client alive?
                if((c != null) && (!c.Loading) && (c.IsAlive))
                {
                    // Calculate distance to fire
                    Vector3D delta = c.State.pos - state.pos;
                    delta.z *= SPLASH_Z_SCALE;
                    distance = delta.Length();

                    // Make push vector
                    Vector3D pushvec = delta;
                    pushvec.MakeLength(SOFT_RANGE - delta.Length());
                    pushvec.Scale(0.03f);

                    // Lighting on fire?
                    if(distance < FIRE_RANGE)
                    {
                        // Create fire if no shields
                        if(c.Powerup != POWERUP.SHIELDS)
                            c.AddFireIntensity(FIRE_INTENSITY, this.Source);
                    }

                    // Within hard range?
                    if(distance < HARD_RANGE)
                    {
                        // Die now!
                        c.Push(pushvec);
                        c.Hurt(this.Source, Client.DEATH_NUKE, HARD_DAMAGE, DEATHMETHOD.NORMAL, true);
                    }
                    // Within splash range?
                    else if(distance < SOFT_RANGE)
                    {
                        // Check if something is blocking in between client and explosion
                        if(Host.Instance.Server.map.FindRayMapCollision(state.pos, c.State.pos))
                        {
                            // Half the damage only
                            amp = 0.5f;
                        }
                        else
                        {
                            // Take full damage
                            amp = 1f;
                        }

                        // Calculate damage
                        float damage = ((1f - (distance / SOFT_RANGE)) * HARD_DAMAGE) * amp;

                        // Doing any damage?
                        if(damage >= 2f)
                        {
                            // Hurt the player
                            c.Push(pushvec);
                            c.Hurt(this.Source, Client.DEATH_NUKE, (int)damage, DEATHMETHOD.NORMAL, state.pos);
                            if(c.Powerup != POWERUP.SHIELDS) c.AddFireIntensity(5000, this.Source);
                        }
                    }
                }
            }

            // Done hurting players
            hurttime = 0;
        }

        // Time to spawn the fire?
        if(SharedGeneral.currenttime > firespawntime)
        {
            // Spawn fire projectiles
            for(int i = 0; i < FIRE_PROJECTILES; i++)
            {
                // Make a random direction
                angle = (float)(Host.Instance.Random.NextDouble() * Math.PI * 2D);
                mx = (float)Math.Sin(angle);
                my = (float)Math.Cos(angle);

                // Make random distance
                do
                {
                    distance = (float)Host.Instance.Random.NextDouble();
                }
                while((distance < (float)Host.Instance.Random.NextDouble()) &&
                      (distance < (float)Host.Instance.Random.NextDouble()) &&
                      (distance < (float)Host.Instance.Random.NextDouble()) &&
                      (distance < (float)Host.Instance.Random.NextDouble()));

                // Make final position
                firepos = new Vector3D(state.pos.x + (mx * distance * FIRE_RANGE), state.pos.y + (my * distance * FIRE_RANGE), state.pos.z + PROJECTILE_Z);

                // Within hard range?
                if((distance * FIRE_RANGE) < HARD_RANGE)
                {
                    // Always spawn fire within hard range
                    spawnfire = true;
                }
                else
                {
                    // Check if explosion reaches here
                    spawnfire = !Host.Instance.Server.map.FindRayMapCollision(state.pos, firepos);
                }

                // Spawn the fire
                if(spawnfire) new Flames(firepos, new Vector3D(0f, 0f, 0f), this.Source);
            }

            // This was the last action, destroy projectile
            this.Destroy(false, null);
        }
    }

    #endregion
}
