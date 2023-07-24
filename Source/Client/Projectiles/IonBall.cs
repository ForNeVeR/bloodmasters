/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using System.Collections;
using FireAndForgetAudioSample;
using SharpDX.Direct3D9;

namespace CodeImp.Bloodmasters.Client
{
	[ProjectileInfo(PROJECTILE.IONBALL)]
	public class IonBall : Projectile, ILightningNode
	{
		#region ================== Constants

		private const float SPRITE_SIZE = 5f;
		private const int PARTICLE_INTERVAL = 10;

		#endregion

		#region ================== Variables

		// Static components
		public static TextureResource plasmaball;

		// Members
		private Sprite sprite;
		private ISound flying;
		private DynamicLight light;
		private ArrayList lightnings = new ArrayList();
		private int particletime;

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public IonBall(string id, Vector3D start, Vector3D vel) : base(id, start, vel)
		{
			// Copy properties
			state.pos = start;
			state.vel = vel;

			// Set initial particle time
			particletime = General.currenttime - 1;

			// Make the ball sprite
			sprite = new Sprite(start, SPRITE_SIZE, false, true);
			UpdateSprite();

			// Make the light
			light = new DynamicLight(start, 16f, General.ARGB(1f, 0.4f, 0.5f, 1f), 3);

            // Create flying sound
            //flying = DirectSound.GetSound("plasmafly.wav", true);
            //flying = new NullSound();
            //flying.Position = start;
            //flying.Play(true);
            string snd = DirectSound.GetSound("plasmafly.wav", false);
            var сachedSound = new CachedSound(snd);
            AudioPlaybackEngine.Instance.PlaySound(сachedSound);
        }

		// Dispose
		public override void Dispose()
		{
			// Clean up
			RemoveAllLightnings();
			flying.Dispose();
			sprite = null;
			flying = null;
			light.Dispose();

			// Dispose base
			base.Dispose();
		}

		#endregion

		#region ================== Methods

		// This updates the sprite for the velocity
		private void UpdateSprite()
		{
			Vector2D normal;
			float rotangle;

			// Calculate sprite rotation angle
			normal = state.vel;
			normal.Normalize();
			rotangle = (float)Math.Atan2(-normal.y, normal.x) + (float)Math.PI * 0.25f;
			sprite.Rotation = rotangle;

			// Update sprite
			sprite.Update();
		}

		// When teleported
		public override void TeleportTo(Vector3D oldpos, Vector3D newpos, Vector3D newvel)
		{
			// Teleport base class
			base.TeleportTo(oldpos, newpos, newvel);

			// Remove lightnings
			RemoveAllLightnings();

			// Update sprites
			UpdateSprite();
		}

		// When updated
		public override void Update(Vector3D newpos, Vector3D newvel)
		{
			// Update base class
			base.Update(newpos, newvel);

			// Update sprites
			UpdateSprite();
		}

		// When destroyed
		public override void Destroy(Vector3D atpos, bool silent, Client hitplayer)
		{
			Vector3D decalpos = atpos;

			// Where are we now?
			var sector = (ClientSector)General.map.GetSubSectorAt(state.pos.x, state.pos.y).Sector;

			// Not silent?
			if((silent == false) && (sector != null))
			{
				// Hitting a player?
				if(hitplayer != null)
				{
					// Player is not carrying a shield?
					if(hitplayer.Powerup != POWERUP.SHIELDS)
					{
						// Create particles
						for(int i = 0; i < 2; i++)
							General.arena.p_blood.Add(atpos, state.vel * 0.04f, General.ARGB(1f, 1f, 0.0f, 0.0f));

						// Floor decal
						if((sector != null) && (sector.Material != (int)SECTORMATERIAL.LIQUID))
							FloorDecal.Spawn(sector, state.pos.x, state.pos.y, FloorDecal.blooddecals, false, true, false);

						// Create wall decal
						if(General.random.Next(100) < 50)
							WallDecal.Spawn(state.pos.x, state.pos.y, state.pos.z + (float)General.random.NextDouble() * 10f - 6f, Consts.PLAYER_DIAMETER, WallDecal.blooddecals, false);
					}
				}
				else
				{
					// Track back a little
					decalpos = atpos - this.state.vel;

					// Near the floor?
					if(((decalpos.z - sector.CurrentFloor) < 2f) &&
					   ((decalpos.z - sector.CurrentFloor) > -2f))
					{
						// Spawn mark on the floor
						if((sector != null) && (sector.Material != (int)SECTORMATERIAL.LIQUID))
							FloorDecal.Spawn(sector, decalpos.x, decalpos.y, FloorDecal.explodedecals, false, false, false);
					}
					else
					{
						// Spawn mark on the wall
						WallDecal.Spawn(decalpos.x, decalpos.y, decalpos.z, 2f, WallDecal.explodedecals, false);
					}
				}

				// Kill flying sound
				flying.Stop();

                string snd = DirectSound.GetSound("playerfire.wav", false);
                var сachedSound = new CachedSound(snd);

                // Make hit sound
                if (sector.VisualSector.InScreen)
                    AudioPlaybackEngine.Instance.PlaySound(сachedSound);//DirectSound.PlaySound("ionexplode.wav", atpos);

                // Spawn explosion effect
                new IonExplodeEffect(decalpos, SourceID, Team);
			}
			// Silent destroy
			else if(sector != null)
			{
				// In a liquid sector?
				if((SECTORMATERIAL)sector.Material == SECTORMATERIAL.LIQUID)
				{
					// Make splash sound
					if(sector.VisualSector.InScreen)
						DirectSound.PlaySound("dropwater.wav", atpos);

					// Check if on screen
					if(sector.VisualSector.InScreen)
					{
						// Determine type of splash to make
						switch(sector.LiquidType)
						{
							case LIQUID.WATER: FloodedSector.SpawnWaterParticles(atpos, new Vector3D(0f, 0f, 0.5f), 10); break;
							case LIQUID.LAVA: FloodedSector.SpawnLavaParticles(atpos, new Vector3D(0f, 0f, 0.5f), 10); break;
						}
					}
				}
			}

			// Destroy base
			base.Destroy(atpos, silent, hitplayer);
		}

		// Process the projectile
		public override void Process()
		{
			bool haslightning;
			Vector3D cpos;
			int pcolor = -1;

			// Process base object
			base.Process();

			// Go for all actors
			foreach(Actor a in General.arena.Actors)
			{
				// Not myself?
				if(a.ClientID != SourceID)
				{
					// Presume no lightning
					haslightning = false;

					// Actor alive
					if(!a.DeadThreshold)
					{
						// No team game or on other team?
						if(!General.teamgame || (a.Team != Team))
						{
							// Determine client position
							cpos = a.State.pos + new Vector3D(0f, 0f, 6f);

							// Calculate distance to this player
							Vector3D delta = cpos - this.Position;
							delta.z *= Consts.POWERUP_STATIC_Z_SCALE;
							float distance = delta.Length();
							delta.Normalize();

							// Within range?
							if(distance < Consts.ION_FLYBY_RANGE)
							{
								// Check if nothing blocks in between
								if(!General.map.FindRayMapCollision(this.Position, cpos))
								{
									// Check if no lighting to this client yet
									foreach(Lightning l in lightnings) if((l.Source == this) && (l.Target == a)) haslightning = true;

									// Create lighting
									if(!haslightning) new Lightning(this, 1f, a, 8f, false, true);
									haslightning = true;
								}
							}
						}
					}

					// Check if lightning should be found and removed
					if(!haslightning)
					{
						// Go for all lightnings
						foreach(Lightning l in lightnings)
						{
							// This lightning on this target?
							if(l.Target == a)
							{
								// Remove lightning
								l.Dispose();
								break;
							}
						}
					}
				}
			}

			// Process lightnings
			foreach(Lightning l in lightnings) l.Process();

			// Time to spawn particles?
			if((particletime < General.currenttime) && this.InScreen)
			{
				// Random color
				switch(General.random.Next(3))
				{
					case 0: pcolor = General.ARGB(1f, 0.2f, 0.3f, 1f); break;
					case 1: pcolor = General.ARGB(1f, 0.4f, 0.6f, 1f); break;
					case 2: pcolor = General.ARGB(1f, 1f, 1f, 1f); break;
				}

				// Make particles
				Vector3D ppos = state.pos + new Vector3D(0f, 0f, -4f) + Vector3D.Random(General.random, 1f, 1f, 0f);
				Vector3D pvel = state.vel * (0.1f + ((float)General.random.NextDouble() * 0.1f));
				General.arena.p_magic.Add(ppos, pvel, pcolor, 1, 100);
				particletime += PARTICLE_INTERVAL;
			}

			// Position sprite
			sprite.Position = this.state.pos;
			sprite.Rotation = (float)General.currenttime * 0.01f;
			sprite.Update();

			// Position light
			light.Position = this.state.pos;

			// Update sound coodinates
			flying.Position = state.pos;
		}

		// Render the projectile
		public override void Render()
		{
			// Check if in screen
			if(this.InScreen)
			{
				// Set render mode
				Direct3D.SetDrawMode(DRAWMODE.NADDITIVEALPHA);
				Direct3D.d3dd.SetRenderState(RenderState.TextureFactor, -1);
				Direct3D.d3dd.SetRenderState(RenderState.ZEnable, true);

				// No lightmap
				Direct3D.d3dd.SetTexture(1, null);

				// Texture
				Direct3D.d3dd.SetTexture(0, IonBall.plasmaball.texture);

				// Render sprite
				sprite.Render();
			}
		}

		// This removes a lightning
		public void RemoveLightning(Lightning l)
		{
			if(lightnings.Contains(l)) lightnings.Remove(l);
		}

		// This adds a lightning
		public void AddLightning(Lightning l)
		{
			if(!lightnings.Contains(l)) lightnings.Add(l);
		}

		// This removes all lightnings
		private void RemoveAllLightnings()
		{
			// Are there any lightnings?
			if(lightnings.Count > 0)
			{
				// Dispose them all
				for(int i = lightnings.Count - 1; i >= 0; i--)
					((Lightning)lightnings[i]).Dispose();
			}
		}

		#endregion
	}
}
