/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using System.Globalization;

namespace CodeImp.Bloodmasters.Client
{
	public class FleshDebris : Debris
	{
		#region ================== Constants

		private const float FLOOR_DECELERATION = 0.9f;
		private const int SOUND_VARIATIONS = 4;
		private const int PARTICLE_MIN_TIME = 50;
		private const int PARTICLE_RANDOM_TIME = 200;
		public const int GIB_LIMBS = 8;

		#endregion

		#region ================== Variables

		// Debris texture
		public static TextureResource[] textures = new TextureResource[GIB_LIMBS];

		// Particles
		private int particletime = 0;

		#endregion

		#region ================== Constructor / Disposer

		// Constructor
		public FleshDebris(Vector3D pos, Vector3D vel, int limpindex) : base(pos, vel)
		{
			// Set the texture
			SetTexture(textures[limpindex].texture);

			// Next particle time
			particletime = SharedGeneral.currenttime + General.random.Next(PARTICLE_RANDOM_TIME);
		}

		#endregion

		#region ================== Methods

		// This returns a random flesh gib number
		public static int RandomFlesh()
		{
			// First 3 are leg, arm and head
			return General.random.Next(FleshDebris.GIB_LIMBS - 3) + 3;
		}

		// This loads all limb
		public static void LoadGibLimps()
		{
			// Go for all gib limb
			for(int i = 1; i <= GIB_LIMBS; i++)
			{
				// Load gib sprites
				textures[i-1] = Direct3D.LoadTexture(ArchiveManager.ExtractFile("sprites/limb" + i.ToString(CultureInfo.InvariantCulture) + "_0_0001.tga"), true);
			}
		}

		// This makes a random sound
		private void MakeCollideSound()
		{
			// Check if in screen
			if(sector.VisualSector.InScreen)
			{
				// Make sound
				int var = General.random.Next(SOUND_VARIATIONS) + 1;
				DirectSound.PlaySound("bloodsplat" + var.ToString(CultureInfo.InvariantCulture) + ".wav", pos);
			}
		}

		// This spawns a bunch of particles
		private void SplashParticles()
		{
			// Number of particles
			for(int i = 0; i < 6; i++)
			{
				// Spawn particle
				General.arena.p_blood.Add(pos + new Vector3D(0f, 0f, 0.1f), Vector3D.Random(General.random, 0.06f, 0.06f, 0.3f), General.ARGB(1f, 0.8f, 0f, 0f));
			}
		}

		// When being processed
		public override void Process()
		{
			// Process base
			base.Process();

			// Not disposed?
			if(!Disposed)
			{
				// Time to spawn particle?
				if((SharedGeneral.currenttime > particletime) && !Stopped && sector.VisualSector.InScreen)
				{
					// Spawn particle
					Vector3D particlevel = vel * 0.5f + Vector3D.Random(General.random, 0.02f, 0.02f, 0f);
					General.arena.p_blood.Add(pos, particlevel, General.ARGB(1f, 0.8f, 0f, 0f));

					// Extend time
					particletime += PARTICLE_MIN_TIME + General.random.Next(PARTICLE_RANDOM_TIME);
				}
			}
		}

		// When colliding with something
		public override void Collide(object hitobj)
		{
			Sidedef sd;
			Sector s;
			bool onfloor;

			// Colliding with a wall?
			if(hitobj is Sidedef)
			{
				// More than just Z velocity?
				if((Math.Abs(vel.x) > 0.001f) ||
				   (Math.Abs(vel.y) > 0.001f))
				{
					// Get the sidedef
					sd = (Sidedef)hitobj;

					// Particle splash
					if(sector.VisualSector.InScreen) SplashParticles();

					// Stop all movement
					vel = new Vector3D(0f, 0f, 0f);
					this.collisions = false;

					// Stop rotating
					this.StopRotating();

					// Bloodsplat!
					if(sector.VisualSector.InScreen) MakeCollideSound();

					// Spawn wall blood here
					WallDecal.Spawn(pos.x, pos.y, pos.z, 3f, WallDecal.blooddecals, false);
				}
				else
				{
					// Stop X/Y movement
					vel = new Vector3D(0f, 0f, vel.z);
				}
			}
			// Colliding with a floor/ceiling?
			else if(hitobj is Sector)
			{
				// Get the sector
				s = (Sector)hitobj;

				// Hitting the floor?
				if(s.CurrentFloor > (pos.z - 1f))
				{
					// Destroy when deep in floor
					if(s.CurrentFloor > (pos.z + 2f))
					{
						// Destroy now
						this.Dispose();
					}
					// Destroy silently when on F_SKY1
					else if(s.TextureFloor == Sector.NO_FLAT)
					{
						// Destroy silently
						this.Dispose();
					}
					else
					{
						// On fake ceiling?
						if(s.FakeHeightCeil < (pos.z + 1f))
						{
							// Position on fake ceiling
							pos.z = s.FakeHeightCeil;
							onfloor = false;
						}
						else
						{
							// Position on the floor
							pos.z = s.CurrentFloor;
							onfloor = true;
						}

						// Falling on floor in liquid sector?
						if((SECTORMATERIAL)sector.Material == SECTORMATERIAL.LIQUID)
						{
							// Check if on screen
							if(sector.VisualSector.InScreen)
							{
								// Make splash sound
								DirectSound.PlaySound("dropwater.wav", pos, 0.5f);

								// Determine type of splash to make
								switch(sector.LiquidType)
								{
									case LIQUID.WATER: FloodedSector.SpawnWaterParticles(pos, new Vector3D(0f, 0f, 0.5f), 10); break;
									case LIQUID.LAVA: FloodedSector.SpawnLavaParticles(pos, new Vector3D(0f, 0f, 0.5f), 10); break;
								}

								// Also splash blood
								SplashParticles();
							}

							// Dispose debris
							this.Dispose();
						}
						else
						{
							// Lots of downward movement?
							if(vel.z < -0.1f)
							{
								// Particle splash
								if(sector.VisualSector.InScreen) SplashParticles();

								// Bloodsplat!
								if(sector.VisualSector.InScreen) MakeCollideSound();
								if(onfloor) FloorDecal.Spawn(s, pos.x, pos.y, FloorDecal.blooddecals, false, false, true);

								// Fade out
								this.FadeOut();
							}

							// Stop here
							this.StopRotating();

							// Stop Z movement and decelerate
							vel = new Vector3D(vel.x * FLOOR_DECELERATION,
												vel.y * FLOOR_DECELERATION, 0f);

							// Done moving?
							if((Math.Abs(vel.x) < 0.001f) &&
							(Math.Abs(vel.y) < 0.001f))
							{
								// Bloodsplat!
								if(onfloor) FloorDecal.Spawn(s, pos.x, pos.y, FloorDecal.blooddecals, false, true, false);

								// Stopped
								this.StopMoving();
							}
						}
					}
				}
				// Ceiling?
				else if(s.HeightCeil < (pos.z + 1f))
				{
					// Stop velocity
					vel = new Vector3D(0f, 0f, 0f);
				}
				else
				{
					// No clue what this could be, but destroy it
					this.Dispose();
				}
			}
			// Colliding with a player?
			else if(hitobj is Client)
			{
				// Dont do anything
				//this.Dispose();
			}
			else
			{
				// Destroy silently
				this.Dispose();
			}
		}

		#endregion
	}
}
