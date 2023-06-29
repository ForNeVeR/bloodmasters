/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using CodeImp.Bloodmasters.Client.Graphics;

namespace CodeImp.Bloodmasters.Client
{
	public class FireEffect
	{
		#region ================== Constants

		private const int FLAME_INTERVAL = 500;
		private const int SMOKE_INTERVAL = 30;
		private const float FADE_IN = 0.02f;
		private const float LIGHT_FLUX = 0.1f;

		#endregion

		#region ================== Variables

		private DynamicLight light;
		private Actor actor;
		private bool disposed = false;
		private int spawntime = 0;
		private bool spawnfront = false;
		private Vector3D lightoffset = new Vector3D(0f, 0f, 8f);
		private int smoketime = 0;
		private int lightcolor = 0;
		private float startalpha = 0f;
		private int intensity = 2000;
		private ISound sound;
		private int fluxoffset;

		#endregion

		#region ================== Properties

		public int Intensity { get { return intensity; } set { intensity = value; } }

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public FireEffect(Actor actor)
		{
			// Set members
			this.actor = actor;

			// Make dynamic light
			lightcolor = General.ARGB(1f, 1f, 0.9f, 0.5f);
			light = new DynamicLight(actor.Position, 15f, 0, 3);

			// Create sound
			sound = DirectSound.GetSound("playerfire.wav", true);
			sound.Position = this.actor.Position;
			sound.Play(0f, true);

			// Random flux offset
			fluxoffset = General.random.Next(1000);

			// Process once
			this.Process();
		}

		// Disposer
		public void Dispose()
		{
			// Clean up
			if(sound != null) sound.Dispose();
			if(light != null) light.Dispose();
			sound = null;
			light = null;
			actor = null;
			disposed = true;
			GC.SuppressFinalize(this);
		}

		#endregion

		#region ================== Methods

		// Processing
		public void Process()
		{
			float lightalpha;

			// Not disposed?
			if(!disposed)
			{
				// Determine intensity
				if(intensity > 1000)
				{
					// Normal linear increase
					if(startalpha < 1f) startalpha += FADE_IN;
					if(startalpha > 1f) startalpha = 1f;
				}
				else
				{
					// Fade out by intensity
					startalpha = (float)intensity / 1000f;
					if(startalpha <= 0f) startalpha = 0f;
				}

				// Move light to match actor position and change intensity
				lightalpha = startalpha + (float)Math.Sin((float)(General.currenttime + fluxoffset) / 50f) * LIGHT_FLUX;
				if(lightalpha > 1f) lightalpha = 1f; else if(lightalpha < 0f) lightalpha = 0f;
				light.Color = ColorOperator.Scale(lightcolor, lightalpha);
				light.Position = actor.Position + lightoffset;

				// Mouse sound to match actor position
				sound.Volume = startalpha;
				sound.Position = actor.Position;

				// Time to spawn smoke?
				if(General.currenttime >= smoketime)
				{
					// Spawn a smoke particle
					General.arena.p_trail.Add(actor.Position + Vector3D.Random(General.random, 3f, 3f, 4f), Vector3D.Random(General.random, 0.01f, 0.01f, 0.15f), General.ARGB(1f, 0.3f, 0.3f, 0.3f));

					// Next smoke time
					smoketime = SharedGeneral.currenttime + SMOKE_INTERVAL;
				}

				// Only spawn flames when not fading out
				if(intensity > 1000)
				{
					// Time to spawn a flame?
					if(General.currenttime >= spawntime)
					{
						// Spawn a flame now
						new FireFlame(actor, spawnfront);
						spawnfront = !spawnfront;

						// Next spawn time
						spawntime = SharedGeneral.currenttime + FLAME_INTERVAL;
					}
				}
			}
		}

		#endregion
	}
}
