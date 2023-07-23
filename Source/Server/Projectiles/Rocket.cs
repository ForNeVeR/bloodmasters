/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

namespace CodeImp.Bloodmasters.Server
{
	[ProjectileInfo(PROJECTILE.ROCKET)]
	public class Rocket : Projectile
	{
		#region ================== Constants

		private const float SPLASH_PUSH = 0.8f;
		private const float SPLASH_DAMAGE = 46f;
		private const float SPLASH_RANGE = 12;
		private const float SPLASH_STRONG_RANGE = 6f;
		private const float SPLASH_Z_SCALE = 0.2f;
		private const float SPLASH_FIRE_RANGE = 6f;
		private const int FIRE_INTENSITY = 2000;

		#endregion

		#region ================== Variables

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public Rocket(Vector3D start, Vector3D vel, Client source) : base(start, vel, source)
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

		// When destroyed
		public override void Destroy(bool silent, Client hitplayer)
		{
			Vector3D dpos, cpos;
			float amp = 1f;

			// Not silent?
			if(!silent)
			{
				// Destroy position
				dpos = state.pos - state.vel;

				// Go for all playing clients
				foreach(Client c in Host.Instance.Server.clients)
				{
					// Client alive?
					if((c != null) && (!c.Loading) && (c.IsAlive))
					{
						// Determine client position
						cpos = c.State.pos + new Vector3D(0f, 0f, 7f);

						// Calculate distance to explosion
						Vector3D delta = cpos - dpos;
						delta.z *= SPLASH_Z_SCALE;
						float distance = delta.Length();

						// Within splash range?
						if(distance < SPLASH_RANGE)
						{
							amp = 1f;

							// Check if something is blocking in between client and explosion
							if(Host.Instance.Server.map.FindRayMapCollision(dpos, cpos))
							{
								// Inside strong range?
								if(distance < SPLASH_STRONG_RANGE)
								{
									// Half the damage only
									amp = 0.5f;
								}
								else
								{
									// No damage
									amp = 0f;
								}
							}

							// Calculate damage and push velocity
							float damage = ((1f - (distance / SPLASH_RANGE)) * SPLASH_DAMAGE) * amp;
							float pushvel = ((1f - (distance / SPLASH_RANGE)) * SPLASH_PUSH) * amp;

							// Doing any damage?
							if(damage >= 2f)
							{
								// Within fire range?
								if(distance < SPLASH_FIRE_RANGE)
								{
									// Not a team game or on other team, but always myself
									if((c.Team != this.Source.Team) || !Host.Instance.Server.IsTeamGame || (c == this.Source))
									{
										// Create fire if no shields
										if(c.Powerup != POWERUP.SHIELDS) c.AddFireIntensity(FIRE_INTENSITY, this.Source);
									}
								}

								// Make push vector
								Vector3D pushvec = delta;
								pushvec.MakeLength(pushvel);

								// Push and damage the player
								c.Push(pushvec);
								c.Hurt(this.Source, Client.DEATH_EXPLODE, (int)damage, DEATHMETHOD.NORMAL, dpos);
							}
						}
					}
				}
			}

			// Destroy base
			base.Destroy(silent, hitplayer);
		}

		// When colliding
		protected override void Collide(object hitobj)
		{
			// Colliding with a wall?
			if(hitobj is Sidedef)
			{
				// Track back a little
				state.pos -= state.vel;

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
				// Destroy here
				this.Destroy(false, (Client)hitobj);
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
