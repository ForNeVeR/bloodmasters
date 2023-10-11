/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

namespace CodeImp.Bloodmasters.Server;

[WeaponInfo(WEAPON.GRENADE_LAUNCHER, RefireDelay=500, Description="Grenade Launcher",
    AmmoType=AMMO.GRENADES, InitialAmmo=10, UseAmmo=1)]
public class WGrenadeLauncher : Weapon
{
    #region ================== Constants

    private const float PROJECTILE_VELOCITY = 0.8f;
    private const float PROJECTILE_OFFSET = 4f;
    private const float PROJECTILE_Z = 7f;

    #endregion

    #region ================== Variables

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public WGrenadeLauncher(Client client) : base(client)
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

    // This is called when the weapon is shooting
    protected override void ShootOnce()
    {
        // Determine projectile velocity
        Vector3D vel = Vector3D.FromActorAngle(client.AimAngle, client.AimAngleZ, PROJECTILE_VELOCITY);

        // Extra Z boost
        vel.z += 0.5f;

        // Spawn projectile
        new Grenade(GetProjectileInitialPosition(client), vel, client);
    }

    #endregion
}
