/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using Bloodmasters.LevelMap;

namespace Bloodmasters.Server.Projectiles;

[ProjectileInfo(PROJECTILE.IONBALL)]
public class IonBall : Projectile
{
    #region ================== Constants

    private const int HIT_DAMAGE = 20;
    private const float HIT_PUSH = 0.5f;
    private const int HIT_DRAIN_SHIELD = 10000;
    private const int FLYBY_DRAIN_SHIELD = 3000;
    private const int FLYBY_DAMAGE = 15;
    private const int FLYBY_INTERVAL = 100;
    private const int EXPLODE_DAMAGE = 40;
    private const float SPLASH_Z_SCALE = 0.2f;

    #endregion

    #region ================== Variables

    private int damagetime;

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public IonBall(Vector3D start, Vector3D vel, Client source) : base(start, vel, source)
    {
        // Set damage time
        damagetime = SharedGeneral.currenttime;
    }

    // Dispose
    public override void Dispose()
    {
        // Dispose base
        base.Dispose();
    }

    #endregion

    #region ================== Methods

    // When colliding
    protected override void Collide(object hitobj)
    {
        // Colliding with a wall?
        if(hitobj is Sidedef)
        {
            // Destroy silently when on a single sided wall
            this.Destroy(((((Sidedef)hitobj).Linedef.Flags & LINEFLAG.DOUBLESIDED) == 0), null);
        }
        // Colliding with a floor/ceiling?
        else if(hitobj is Sector)
        {
            // Floor or ceiling?
            if(sector.CurrentFloor >= (state.pos.z - 1f))
            {
                // Destroy silently when on F_SKY1
                this.Destroy((sector.TextureFloor == Sector.NO_FLAT) ||
                             ((SECTORMATERIAL)sector.Material == SECTORMATERIAL.LIQUID), null);
            }
            else if(sector.HeightCeil < (state.pos.z + 1f))
            {
                // Destroy silently when on F_SKY1
                this.Destroy((sector.TextureCeil == Sector.NO_FLAT), null);
            }
            else
            {
                // WTF? Whatever, destroy silently
                this.Destroy(true, null);
            }
        }
        // Colliding with a player?
        else if(hitobj is Client)
        {
            Client c = (Client)hitobj;

            // Make push vector
            Vector3D pushvec = this.Vel;
            pushvec.MakeLength(HIT_PUSH);

            // Push and damage the player
            c.Push(pushvec);
            c.Hurt(this.Source, Client.DEATH_PLASMA, HIT_DAMAGE, DEATHMETHOD.NORMAL_NOGIB, state.pos);

            // Destroy here
            this.Destroy(false, c);
        }
        else
        {
            // Destroy silently
            this.Destroy(true, null);
        }
    }

    // When destroyed
    public override void Destroy(bool silent, Client hitplayer)
    {
        Vector3D cpos;

        // Go for all playing clients
        foreach(Client c in Host.Instance.Server.clients)
        {
            // Client alive?
            if((c != null) && (!c.Loading) && (c.IsAlive) && (c != this.Source))
            {
                // No team game or on other team?
                if(!Host.Instance.Server.IsTeamGame || (c.Team != this.Source.Team))
                {
                    // Determine client position
                    cpos = c.State.pos + new Vector3D(0f, 0f, 7f);

                    // Calculate distance to fire
                    Vector3D delta = cpos - state.pos;
                    delta.z *= SPLASH_Z_SCALE;
                    float distance = delta.Length();

                    // Within splash range?
                    if(distance < Consts.ION_EXPLODE_RANGE)
                    {
                        // Check if nothing is blocking in between
                        if(!Host.Instance.Server.map.FindRayMapCollision(state.pos, cpos))
                        {
                            // Hurt the player
                            c.Hurt(this.Source, Client.DEATH_STATIC, EXPLODE_DAMAGE, DEATHMETHOD.NORMAL_NOGIB, state.pos);

                            // Drain shields if any
                            if(c.Powerup == POWERUP.SHIELDS) c.DecreasePowerupCount(this.Source, HIT_DRAIN_SHIELD);
                        }
                    }
                }
            }
        }

        // Process base class
        base.Destroy(silent, hitplayer);
    }

    // When processed
    public override void Process()
    {
        Vector3D cpos;

        // Process base class
        base.Process();

        // Time to do damage?
        if((damagetime <= SharedGeneral.currenttime) && (this.Source != null))
        {
            // Go for all playing clients
            foreach(Client c in Host.Instance.Server.clients)
            {
                // Client alive?
                if((c != null) && (!c.Loading) && (c.IsAlive) && (c != this.Source))
                {
                    // No team game or on other team?
                    if(!Host.Instance.Server.IsTeamGame || (c.Team != this.Source.Team))
                    {
                        // Determine client position
                        cpos = c.State.pos + new Vector3D(0f, 0f, 7f);

                        // Calculate distance to fire
                        Vector3D delta = cpos - state.pos;
                        delta.z *= SPLASH_Z_SCALE;
                        float distance = delta.Length();

                        // Within splash range?
                        if(distance < Consts.ION_FLYBY_RANGE)
                        {
                            // Check if nothing is blocking in between
                            if(!Host.Instance.Server.map.FindRayMapCollision(state.pos, cpos))
                            {
                                // Hurt the player
                                c.Hurt(this.Source, Client.DEATH_STATIC, FLYBY_DAMAGE, DEATHMETHOD.NORMAL_NOGIB, state.pos);

                                // Drain shields if any
                                if(c.Powerup == POWERUP.SHIELDS) c.DecreasePowerupCount(this.Source, FLYBY_DRAIN_SHIELD);
                            }
                        }
                    }
                }
            }

            // Next damage time
            damagetime = SharedGeneral.currenttime + FLYBY_INTERVAL;
        }
    }

    #endregion
}
