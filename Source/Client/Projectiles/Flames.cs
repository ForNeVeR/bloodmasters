/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;

namespace CodeImp.Bloodmasters.Client
{
	[ProjectileInfo(PROJECTILE.FLAMES)]
	public class Flames : Projectile
	{
		#region ================== Constants

		private const float RANGE = 8f;
		private const float FADEOUT_SPEED = 0.01f;
		private const int FADEOUT_DELAY = 800;
		private const int FLAME_INTERVAL = 500;
		private const int SMOKE_INTERVAL = 100;
		private const float SOUND_VOLUME = 0.4f;
		private const float SOUND_FADEIN = 0.002f;
		private const float LIGHT_FLUX = 0.1f;

		#endregion

		#region ================== Variables

		// Members
		private int fadeouttime;
		private int spawntime = 0;
		private float intensity = 1f;
		private Sound firesound = null;
		private DynamicLight light;
		private int smoketime = 0;
		private int fluxoffset;

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public Flames(string id, Vector3D start, Vector3D vel) : base(id, start, vel)
		{
			// Copy properties
			state.pos = start;
			state.vel = vel;

			// Set fade out time
			fadeouttime = SharedGeneral.currenttime + FADEOUT_DELAY;

			// Make the light
			light = new DynamicLight(start, 12f, 0, 2);
			light.Color = General.ARGB(0.2f + intensity * 0.8f, 0.6f, 0.5f, 0.3f);

			// Spawn a flame now
			new PhoenixFlame(state.pos + Vector3D.Random(General.random, 0f, 0f, -4f), state.vel);

			// Next spawn time
			spawntime = SharedGeneral.currenttime + (FLAME_INTERVAL / 2);
			smoketime = SharedGeneral.currenttime + General.random.Next(SMOKE_INTERVAL);

			// Random flux offset
			fluxoffset = General.random.Next(1000);

			// Create sound
			//firesound = DirectSound.GetSound("playerfire.wav", true);
			//firesound.Position = start;
			//firesound.Volume = 0f;
			//firesound.SetRandomOffset();
			//firesound.Play(true);
		}

		// Dispose
		public override void Dispose()
		{
			// Clean up
			if(firesound != null) firesound.Dispose();
			light.Dispose();

			// Dispose base
			base.Dispose();
		}

		#endregion

		#region ================== Methods

		// Process the projectile
		public override void Process()
		{
			float lightalpha;

			// Process base object
			base.Process();

			// Decelerate
			state.vel /= 1f + Consts.FLAMES_DECELERATE;

			// Stay above floor
			//if(state.pos.z < (sector.CurrentFloor + 0.1f)) state.pos.z = sector.CurrentFloor + 0.1f;

			// Reposition sound
			//firesound.Position = state.pos;

			// fadeout time?
			if(fadeouttime < SharedGeneral.currenttime)
			{
				// Decrease range
				intensity -= FADEOUT_SPEED;
				if(intensity < 0f) intensity = 0f;

				// Decrease volume
				//firesound.Volume = (range / RANGE_MAX) * SOUND_VOLUME;
			}

			// Update the light
			lightalpha = (intensity * 0.8f) + (float)Math.Sin((float)(SharedGeneral.currenttime + fluxoffset) / 50f) * LIGHT_FLUX;
			if(lightalpha > 1f) lightalpha = 1f; else if(lightalpha < 0f) lightalpha = 0f;
			light.Position = this.state.pos;
			light.Color = General.ARGB(lightalpha, 0.6f, 0.5f, 0.3f);

			// Only spawn stuff when not decreased
			if(fadeouttime > SharedGeneral.currenttime)
			{
				// Time to spawn a flame?
				if(SharedGeneral.currenttime >= spawntime)
				{
					// Spawn a flame now
					new PhoenixFlame(state.pos + Vector3D.Random(General.random, intensity * RANGE, intensity * RANGE, 6f),
									state.vel);

					// Next spawn time
					spawntime = SharedGeneral.currenttime + FLAME_INTERVAL;
				}

				// Time to spawn smoke?
				if((SharedGeneral.currenttime >= smoketime) && this.InScreen)
				{
					// Spawn a smoke particle
					General.arena.p_smoke.Add(this.Position + Vector3D.Random(General.random, intensity * RANGE, intensity * RANGE, 6f), Vector3D.Random(General.random, 0.01f, 0.01f, 0.15f), General.ARGB(1f, 0.6f, 0.6f, 0.6f));

					// Next smoke time
					smoketime = SharedGeneral.currenttime + SMOKE_INTERVAL;
				}
			}
		}

		#endregion
	}
}
