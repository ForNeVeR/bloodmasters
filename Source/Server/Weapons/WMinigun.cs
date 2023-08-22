/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

namespace CodeImp.Bloodmasters.Server;

[WeaponInfo(WEAPON.MINIGUN, RefireDelay=50, Description="Minigun",
    AmmoType=AMMO.BULLETS, InitialAmmo=50, UseAmmo=1)]
public class WMinigun : Weapon
{
    #region ================== Constants

    private const float BULLET_SPREAD = 12f;
    private const int BULLET_DAMAGE = 5;
    private const float BULLET_PUSH = 0.02f;
    private const int SPINUP_DELAY = 1000;
    private const int SPINDOWN_DELAY = 1000;

    #endregion

    #region ================== Variables

    // States
    private MINIGUNSTATE state = MINIGUNSTATE.IDLE;
    private int statechangetime = 0;

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public WMinigun(Client client) : base(client)
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
        if((state == MINIGUNSTATE.IDLE) || ((state == MINIGUNSTATE.SPINDOWN) && (statechangetime < SharedGeneral.currenttime)))
        {
            // Go to spin up state
            state = MINIGUNSTATE.SPINUP;
            statechangetime = SharedGeneral.currenttime + SPINUP_DELAY;
            return false;
        }

        // Check if gun is firing
        if(((state == MINIGUNSTATE.SPINUP) && (statechangetime < SharedGeneral.currenttime)) ||
           (state == MINIGUNSTATE.FIRING))
        {
            // Fire weapon
            state = MINIGUNSTATE.FIRING;
            return base.Trigger();
        }

        // Not firing
        return false;
    }

    // This is called when the trigger is released
    public override void Released()
    {
        // Check if the weapon is spinning
        if((state == MINIGUNSTATE.SPINUP) || (state == MINIGUNSTATE.FIRING))
        {
            // Spin down now
            state = MINIGUNSTATE.SPINDOWN;
            statechangetime = SharedGeneral.currenttime + SPINDOWN_DELAY;
        }

        // Check if spinned down
        if((state == MINIGUNSTATE.SPINDOWN) && (statechangetime < SharedGeneral.currenttime))
        {
            // Now idle
            state = MINIGUNSTATE.IDLE;
        }

        // Base class stuff
        base.Released();
    }

    // This is called when the weapon is shooting
    protected override void ShootOnce()
    {
        // Fire a bullet
        new Bullet(this.client, BULLET_SPREAD, Client.DEATH_MINIGUN, BULLET_DAMAGE, BULLET_PUSH);
    }

    // This is called to check if the weapon is ready
    public override bool IsIdle()
    {
        // Return if the weapon is idle
        return (state == MINIGUNSTATE.IDLE);
    }

    #endregion
}

// Minigun states
public enum MINIGUNSTATE
{
    IDLE = 0,
    SPINUP = 1,
    FIRING = 2,
    SPINDOWN = 3,
}
