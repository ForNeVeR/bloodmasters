/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using System.Drawing;
using System.Globalization;
using System.Collections;
using CodeImp.Bloodmasters;
using CodeImp;

#if CLIENT
using CodeImp.Bloodmasters.Client;
#endif

namespace CodeImp.Bloodmasters.Server
{
	[WeaponInfo(WEAPON.PLASMA, RefireDelay=100, Description="Plasma Cannon",
				AmmoType=AMMO.PLASMA, InitialAmmo=50, UseAmmo=1)]
	public class WPlasmaCannon : Weapon
	{
		#region ================== Constants
		
		private const float PROJECTILE_VELOCITY = 1.5f;
		private const float PROJECTILE_OFFSET = 4f;
		private const float PROJECTILE_Z = 7f;
		
		#endregion
		
		#region ================== Variables
		
		#endregion
		
		#region ================== Constructor / Destructor
		
		// Constructor
		public WPlasmaCannon(Client client) : base(client)
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
			
			// Move projectil somewhat forward
			Vector3D pos = client.State.pos + Vector3D.FromActorAngle(client.AimAngle, client.AimAngleZ, PROJECTILE_OFFSET);
			
			// Spawn projectile
			new PlasmaBall(pos + new Vector3D(0f, 0f, PROJECTILE_Z), vel, client);
		}
		
		#endregion
	}
}
