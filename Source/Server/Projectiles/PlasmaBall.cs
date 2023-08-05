/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

namespace CodeImp.Bloodmasters.Server
{
	[ProjectileInfo(PROJECTILE.PLASMABALL)]
	public class PlasmaBall : Projectile
	{
		#region ================== Constants

		private const int HIT_DAMAGE = 8;
		private const float HIT_PUSH = 0.1f;
		private const int HIT_DRAIN_SHIELD = 2000;

		#endregion

		#region ================== Variables

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public PlasmaBall(Vector3D start, Vector3D vel, Client source) : base(start, vel, source)
		{
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

				// Drain shields if any
				if(c.Powerup == POWERUP.SHIELDS) c.DecreasePowerupCount(this.Source, HIT_DRAIN_SHIELD);

				// Push and damage the player
				c.Push(pushvec);
				c.Hurt(this.Source, Client.DEATH_PLASMA, HIT_DAMAGE, DEATHMETHOD.NORMAL, state.pos);

				// Destroy here
				this.Destroy(false, c);
			}
			else
			{
				// Destroy silently
				this.Destroy(true, null);
			}
		}

		#endregion
	}
}
