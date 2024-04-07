/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

namespace Bloodmasters.Server;

[WeaponInfo(WEAPON.SMG, RefireDelay=100, Description="SMG",
    AmmoType=AMMO.BULLETS, InitialAmmo=100, UseAmmo=1)]
public class WLightChaingun : Weapon
{
    #region ================== Constants

    private const float BULLET_SPREAD = 6f;
    private const int BULLET_DAMAGE = 5;
    private const float BULLET_PUSH = 0.02f;

    #endregion

    #region ================== Variables

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public WLightChaingun(Client client) : base(client)
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
        // Fire a bullet
        new Bullet(this.client, BULLET_SPREAD, Client.DEATH_SMG, BULLET_DAMAGE, BULLET_PUSH);
    }

    #endregion
}
