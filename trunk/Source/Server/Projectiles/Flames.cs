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
	[ProjectileInfo(PROJECTILE.FLAMES)]
	public class Flames : Projectile
	{
		#region ================== Constants
		
		private const float FADEOUT_SPEED = 0.009f;
		private const int FADEOUT_DELAY = 800;
		private const int FIRE_INTENSITY = 2000;
		private const float FIRE_DAMAGE = 20f;
		private const int DAMAGE_INTERVAL = 100;
		private const int DAMAGE_MAX_INTERVAL = 200;
		private const float SPLASH_Z_SCALE = 0.2f;
		private const float RANGE = 5f;
		
		#endregion
		
		#region ================== Variables
		
		// Damage amount
		private float intensity = 1f;
		
		// Timing
		private int fadeouttime;
		private int damagetime;
		
		#endregion
		
		#region ================== Constructor / Destructor
		
		// Constructor
		public Flames(Vector3D start, Vector3D vel, Client source) : base(start, vel, source)
		{
			// Unable to go through a teleport
			teleportable = false;
			
			// Set timing
			fadeouttime = General.currenttime + FADEOUT_DELAY;
			damagetime = General.currenttime + DAMAGE_INTERVAL;
			
			// Starting underneath a floor?
			if(start.z < this.sector.CurrentFloor) this.Destroy(false, null);
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
			Vector3D cpos;
			float amp = 1f;
			float damagedistance;
			float firedistance;
			
			// Process projectile
			base.Process();
			
			// Decelerate
			state.vel /= 1f + Consts.FLAMES_DECELERATE;
			
			// Stay above floor
			if(state.pos.z < (sector.CurrentFloor + 0.1f)) state.pos.z = sector.CurrentFloor + 0.1f;
			
			// fadeout time?
			if(fadeouttime < General.currenttime)
			{
				// Decrease intensity
				intensity -= FADEOUT_SPEED;
				
				// Projectile at smallest?
				if(intensity <= 0f)
				{
					// Destroy it
					intensity = 0f;
					this.Destroy(false, null);
				}
			}
			
			// Leave when no more source
			if(this.Source == null) return;
			
			// Time to do damage?
			if(damagetime <= General.currenttime)
			{
				// Determine damage and fire distances
				damagedistance = RANGE * 2f;
				firedistance = RANGE + Consts.PLAYER_DIAMETER;
				
				// Go for all playing clients
				foreach(Client c in General.server.clients)
				{
					// Client alive?
					if((c != null) && (!c.Loading) && (c.IsAlive) &&
					   ((c != Source) || (travellength > FREE_TRAVEL_LENGTH)))
					{
						// Time to do damage to this client?
						if((c.LastFlameTime + DAMAGE_MAX_INTERVAL) < General.currenttime)
						{
							// Determine client position
							cpos = c.State.pos + new Vector3D(0f, 0f, 7f);
							
							// Calculate distance to fire
							Vector3D delta = cpos - state.pos;
							delta.z *= SPLASH_Z_SCALE;
							float distance = delta.Length();
							
							// Within splash range?
							if(distance < damagedistance)
							{
								amp = intensity;
								
								// Check if something is blocking in between client and fire
								if(General.server.map.FindRayMapCollision(state.pos, cpos))
								{
									// Inside strong range?
									if(distance < RANGE)
									{
										// Half the damage only
										amp = intensity * 0.5f;
									}
									else
									{
										// No damage
										amp = 0f;
									}
								}
								
								// Calculate damage
								float damage = ((1f - (distance / damagedistance)) * FIRE_DAMAGE) * amp;
								
								// Doing any damage?
								if(damage >= 2f)
								{
									// Set the last frame time on client
									c.LastFlameTime = General.currenttime;
									
									// Hurt the player
									c.Hurt(this.Source, Client.DEATH_FIRE_SOURCE, (int)damage, DEATHMETHOD.NORMAL_NOGIB, state.pos);
								}
								
								// Lighting on fire?
								if(distance < firedistance)
								{
									// Set the last frame time on client
									c.LastFlameTime = General.currenttime;
									
									// Not a team game or on other team, but always myself
									if((c.Team != this.Source.Team) || !General.server.IsTeamGame || (c == this.Source))
									{
										// Create fire if no shields
										if(c.Powerup != POWERUP.SHIELDS)
										{
											if(c.FireIntensity < 1000)
												c.AddFireIntensity(2000, this.Source);
											else
												c.AddFireIntensity(FIRE_INTENSITY, this.Source);
										}
									}
								}
							}
						}
					}
				}
				
				// Next damage time
				damagetime = General.currenttime + DAMAGE_INTERVAL;
			}
		}
		
		#endregion
	}
}
