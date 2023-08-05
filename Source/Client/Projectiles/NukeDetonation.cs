/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using FireAndForgetAudioSample;
using SharpDX.Direct3D9;

namespace CodeImp.Bloodmasters.Client
{
	[ProjectileInfo(PROJECTILE.NUKEDETONATION)]
	public class NukeDetonation : Projectile
	{
		#region ================== Constants

		private const float EXPLOSION_SIZE = 80f;
		private const float LIGHT_FADEIN = 0.2f;
		private const float LIGHT_FADEOUT = 0.01f; //0.005f;
		private const int LIGHT_FADEOUT_DELAY = 100; //1500;
		private const int SMOKE_DELAY = 2000;
		private const int SMOKE_PARTICLES = 100;
		private const float SMOKE_RANGE1 = 20f;
		private const float SMOKE_RANGE2 = 30f;
		private const float ROTATE_SPEED = 0.04f;
		private const float ROTATE_FADEOUT = 0.96f;

		#endregion

		#region ================== Variables

		// Members
		private Animation ani;
		private Sprite sprite;
		private DynamicLight light;
		private float lightfade = 0f;
		private int lightfadeouttime;
		private int smoketime;
		private float rotate = 0f;
		private float rotatespeed = ROTATE_SPEED;

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public NukeDetonation(string id, Vector3D pos, Vector3D vel) : base(id, pos, vel)
		{
			// Make the light
			light = new DynamicLight(pos, 150f, 0, 2);
			light.Color = 0;

			// Timing
			lightfadeouttime = SharedGeneral.currenttime + LIGHT_FADEOUT_DELAY;
			smoketime = SharedGeneral.currenttime + SMOKE_DELAY;

			// Make explosion sound
			//DirectSound.PlaySound("nukeexplode.wav", pos, 2f);

            var snd = DirectSound.GetSound("nukeexplode.wav", false);
            var сachedSound = new CachedSound(snd);
            AudioPlaybackEngine.Instance.PlaySound(сachedSound);

            // Rendering options
            this.renderpass = 2;

			// Make the explosion sprite and animation
			sprite = new Sprite(pos, EXPLOSION_SIZE, false, true);
			ani = Animation.CreateFrom("sprites/nukeexplode.cfg");
		}

		// Dispose
		public override void Dispose()
		{
			// Clean up
			light.Dispose();

			// Dispose base
			base.Dispose();
		}

		#endregion

		#region ================== Methods

		// When destroyed by server
		public override void Destroy(Vector3D atpos, bool silent, Client hitplayer)
		{
			// Ignore this. We will destroy this projectile ourself.
		}

		// Process the projectile
		public override void Process()
		{
			// Process base object
			base.Process();

			// Fading in or out?
			if(SharedGeneral.currenttime < lightfadeouttime)
			{
				// Increase light level
				lightfade += LIGHT_FADEIN;
				if(lightfade > 1f) lightfade = 1f;
			}
			else
			{
				// Decrease light level
				lightfade -= LIGHT_FADEOUT;
				if(lightfade < 0f) lightfade = 0f;
			}

			// Apply light level
			light.Color = General.ARGB(lightfade, 1f, 1f, 1f);

			// Rotate sprite
			rotate += rotatespeed;
			rotatespeed *= ROTATE_FADEOUT;
			sprite.Rotation = rotate;
			sprite.Update();

			// Process animation
			ani.Process();

			// Time to spawn lots of smoke?
			if((smoketime < SharedGeneral.currenttime) && (smoketime > 0))
			{
				// Spawn smoke particles
				for(int i = 0; i < SMOKE_PARTICLES; i++)
				{
					// Spawn a smoke particle here
					General.arena.p_smoke.Add(state.pos + Vector3D.Random(General.random, SMOKE_RANGE1, SMOKE_RANGE1, 8f), Vector3D.Random(General.random, 0.01f, 0.01f, 0.1f), General.ARGB(1f, 0.3f, 0.3f, 0.3f));
					General.arena.p_smoke.Add(state.pos + Vector3D.Random(General.random, SMOKE_RANGE2, SMOKE_RANGE2, 8f), Vector3D.Random(General.random, 0.01f, 0.01f, 0.1f), General.ARGB(1f, 0.5f, 0.5f, 0.5f));
				}

				// Done smoking
				smoketime = 0;
			}

			// Done with everything? Then destroy this projectile.
			if(ani.Ended && (lightfade < 0.000001f) && (smoketime == 0))
				base.Destroy(this.pos, true, null);
		}

		// This renders the projectile
		public override void Render()
		{
			// Set render mode
			Direct3D.SetDrawMode(DRAWMODE.NADDITIVEALPHA);
			Direct3D.d3dd.SetRenderState(RenderState.TextureFactor, -1);
			Direct3D.d3dd.SetRenderState(RenderState.ZEnable, false);

			// Texture
			Direct3D.d3dd.SetTexture(0, ani.CurrentFrame.texture);

			// Render the sprite
			sprite.Render();
		}

		#endregion
	}
}
