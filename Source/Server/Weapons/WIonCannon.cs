/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

namespace CodeImp.Bloodmasters.Server;

[WeaponInfo(WEAPON.IONCANNON, RefireDelay=500, Description="Ion Cannon",
    AmmoType=AMMO.PLASMA, InitialAmmo=60, UseAmmo=20)]
public class WIonConnon : Weapon
{
    #region ================== Constants

    private const float PROJECTILE_VELOCITY = 0.6f;
    private const float PROJECTILE_OFFSET = 4f;
    private const float PROJECTILE_Z = 7f;
    private const int LOAD_DELAY = 1000;
    private const int SPINDOWN_DELAY = 1000;

    #endregion

    #region ================== Variables

    // States
    private CANNONSTATE state = CANNONSTATE.IDLE;
    private int statechangetime = 0;

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public WIonConnon(Client client) : base(client)
    {
    }

    // Disposer
    public override void Dispose()
    {
        // Dispose base
        base.Dispose();
    }

    #endregion

    #region ================== Methods

    // This is called when the trigger is pulled
    public override bool Trigger()
    {
        // Check if gun is idle
        if(this.IsIdle())
        {
            // Go to loading state
            state = CANNONSTATE.LOADING;
            statechangetime = SharedGeneral.currenttime + LOAD_DELAY;
            return false;
        }

        // Check if done loading
        if((state == CANNONSTATE.LOADING) && (SharedGeneral.currenttime > statechangetime))
        {
            // Fire weapon and back to idle
            state = CANNONSTATE.IDLE;
            return base.Trigger();
        }

        // Not firing
        return false;
    }

    // This is called when the trigger is released
    public override void Released()
    {
        // Check if the weapon is loading
        if(state == CANNONSTATE.LOADING)
        {
            // Stop laoding now
            state = CANNONSTATE.IDLE;
        }

        // Base class stuff
        base.Released();
    }

    // This is called when the weapon is shooting
    protected override void ShootOnce()
    {
        // Determine projectile velocity
        Vector3D vel = Vector3D.FromActorAngle(client.AimAngle, client.AimAngleZ, PROJECTILE_VELOCITY);

        // Move projectil somewhat forward
        Vector3D pos = client.State.pos + Vector3D.FromActorAngle(client.AimAngle, client.AimAngleZ, PROJECTILE_OFFSET);

        // Spawn projectile
        new IonBall(pos + new Vector3D(0f, 0f, PROJECTILE_Z), vel, client);
    }

    // This is called to check if the weapon is ready
    public override bool IsIdle()
    {
        // Return if the weapon is idle
        return (state == CANNONSTATE.IDLE) && (refiretime < SharedGeneral.currenttime);
    }

    #endregion
}

// Cannon states
public enum CANNONSTATE
{
    IDLE = 0,
    LOADING = 1,
}
