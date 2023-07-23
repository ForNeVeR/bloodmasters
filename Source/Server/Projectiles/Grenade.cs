/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

namespace CodeImp.Bloodmasters.Server
{
	[ProjectileInfo(PROJECTILE.GRENADE)]
	public class Grenade : Projectile
	{
		#region ================== Constants

		private const float SPLASH_PUSH = 1f;
		private const float SPLASH_DAMAGE = 46f;
		private const float SPLASH_RANGE = 15;
		private const float SPLASH_STRONG_RANGE = 8f;
		private const float SPLASH_Z_SCALE = 0.2f;
		private const float SPLASH_FIRE_RANGE = 10f;
		private const int FIRE_INTENSITY = 2000;
		private const int EXPLODE_TIMEOUT = 2000;

		#endregion

		#region ================== Variables

		// Time to explode
		private int explodetime;

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public Grenade(Vector3D start, Vector3D vel, Client source) : base(start, vel, source)
		{
			// Set explode time
			explodetime = SharedGeneral.currenttime + EXPLODE_TIMEOUT;
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
					if((c != null) && !c.Loading && c.IsAlive)
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
								// Source still available?
								if(this.Source != null)
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
			}

			// Destroy base
			base.Destroy(silent, hitplayer);
		}

		// When colliding
		protected override void Collide(object hitobj)
		{
			Sidedef sd;

			// Colliding with a wall?
			if(hitobj is Sidedef)
			{
				// Get the sidedef
				sd = (Sidedef)hitobj;

				// Bounce away from the wall
				Vector2D wallnormal = new Vector2D(-sd.Linedef.nY, sd.Linedef.nX);
				Vector2D incident = state.vel;
				Vector2D bouncevec = Vector2D.Reflect(incident, wallnormal);
				state.vel.Apply2D(bouncevec * 0.8f);
				state.pos += state.vel * 2f;

				// Update projectile trajectory
				this.Update(state.pos, state.vel);
			}
			// Colliding with a floor/ceiling?
			else if(hitobj is Sector)
			{
				// Hitting the floor or ceiling?
				if((sector.CurrentFloor >= (state.pos.z - 1f)) ||
				   ((sector.FakeHeightCeil >= (state.pos.z - 1f)) &&
				    (sector.HeightCeil < state.pos.z)))
				{
					// Destroy when deep in floor
					if(sector.CurrentFloor > (state.pos.z + 2f))
					{
						// Destroy now
						this.Destroy(false, null);
					}
					// Destroy silently when on F_SKY1 or liquid type
					else if((sector.TextureFloor == Sector.NO_FLAT) ||
							((SECTORMATERIAL)sector.Material == SECTORMATERIAL.LIQUID))
					{
						// Destroy silently
						this.Destroy(true, null);
					}
					else
					{
						// Enough downward velocity?
						if(state.vel.z < -0.1f)
						{
							// Only bounce when enough Z movement
							if(state.vel.z < -(Consts.GRENADE_GRAVITY * 5f))
							{
								// Bounce on ground
								state.vel.z = -state.vel.z * Consts.GRENADE_BOUNCEVEL;
							}
							else
							{
								// Stop Z movement
								state.vel.z = 0f;
							}

							// Update projectile trajectory
							this.Update(state.pos, state.vel);
						}
						else
						{
							// On the floor?
							if((state.pos.z < (sector.CurrentFloor + 0.3f)) ||
							   ((state.pos.z < (sector.FakeHeightCeil + 0.3f)) &&
							    (state.pos.z > sector.HeightCeil)))
							{
								// Stop Z movements
								state.vel.z = 0f;
							}
						}
					}
				}
				else if(sector.HeightCeil < (state.pos.z + 1f))
				{
					// Destroy silently when on F_SKY1
					if(sector.TextureCeil == Sector.NO_FLAT)
					{
						// Destroy silently
						this.Destroy(true, null);
					}
					else
					{
						// Enough upward velocity?
						if(state.vel.z >= 0f)
						{
							// Bounce on ceiling
							state.vel.z = -state.vel.z;

							// Update projectile trajectory
							this.Update(state.pos, state.vel);
						}
					}
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

		// When processed
		public override void Process()
		{
			// Process projectile
			base.Process();

			// Process physics
			if(state.pos.z > (sector.CurrentFloor + 0.3f))
			{
				state.vel.z -= Consts.GRENADE_GRAVITY;
				state.vel.x /= 1f + Consts.GRENADE_DECELERATE_AIR;
				state.vel.y /= 1f + Consts.GRENADE_DECELERATE_AIR;
			}
			else
			{
				state.vel.x /= 1f + Consts.GRENADE_DECELERATE_FLOOR;
				state.vel.y /= 1f + Consts.GRENADE_DECELERATE_FLOOR;
			}

			// Stay above floor
			if(state.pos.z < (sector.CurrentFloor + 0.1f)) state.pos.z = sector.CurrentFloor + 0.1f;

			// Time to explode?
			if(explodetime < SharedGeneral.currenttime)
			{
				// Destroy loudly
				this.Destroy(false, null);
			}
		}


		#endregion
	}
}
