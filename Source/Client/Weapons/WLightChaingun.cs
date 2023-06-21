/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using SharpDX.Direct3D9;

namespace CodeImp.Bloodmasters.Client
{
	[WeaponInfo(WEAPON.SMG, RefireDelay=100, Description="SMG",
				Sound="chain1fire.wav", AmmoType=AMMO.BULLETS, UseAmmo=1)]
	public class WLightChaingun : Weapon
	{
		#region ================== Constants

		// Fire flare
		private const float FLARE_ALPHA_START = 1f;
		private const float FLARE_ALPHA_CHANGE = -0.1f;
		private const float FLARE_SIZE_START = 6f;
		private const float FLARE_SIZE_CHANGE = -0.1f;
		private const float FLARE_OFFSET_X = 1f;
		private const float FLARE_OFFSET_Y = -1f;
		private const float FLARE_OFFSET_Z = 10f;
		private const float FLARE_DELTA_ANGLE = 0.43f;
		private const float FLARE_DISTANCE = 3.8f;
		private const float BULLET_SPREAD = 6f;

		#endregion

		#region ================== Variables

		// Fire flare
		public static TextureResource flaretex;
		private Sprite flare;
		private float flarealpha = 0f;

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public WLightChaingun(Client client) : base(client)
		{
			// Make fire flare sprite
			flare = new Sprite(new Vector3D(), FLARE_SIZE_START, false, true);
		}

		// Disposer
		public override void Dispose()
		{
			// Clean up
			flare = null;

			// Dispose base
			base.Dispose();
		}

		#endregion

		#region ================== Methods

		// This is called when the weapon is shooting
		protected override void ShootOnce()
		{
			// Play the shooting sound
			if(client.Actor.Sector.VisualSector.InScreen)
				DirectSound.PlaySound(sound, client.Actor.Position);

			// Make the actor play the shooting animation
			client.Actor.PlayShootingAnimation(2, 0);

			// Set fire flare
			flarealpha = FLARE_ALPHA_START;
			flare.Size = FLARE_SIZE_START;
			flare.Rotation = (float)General.random.NextDouble() * 2f * (float)Math.PI;

			// Spawn a bullet
			new Bullet(client.Actor, BULLET_SPREAD);
		}

		// This processes the weapon
		public override void Process()
		{
			// Process base class
			base.Process();

			// Process the fire flare
			if(flarealpha > 0f)
			{
				// Position flare
				flare.Position = Weapon.GetFlarePosition(client.Actor);

				// Decrease alpha and size
				flare.Size += FLARE_SIZE_CHANGE;
				flarealpha += FLARE_ALPHA_CHANGE;
				if(flarealpha < 0f) flarealpha = 0f;

				// Update flare
				flare.Update();

				// Update light
				light.Visible = true;
				light.Color = General.ARGB(flarealpha * 0.4f, 1f, 1f, 1f);
			}
			else
			{
				// No light
				light.Visible = false;
			}
		}

		// This renders the weapon
		public override void Render()
		{
			// Render the fire flare
			if(flarealpha > 0f)
			{
				// Set render mode
				Direct3D.SetDrawMode(DRAWMODE.NADDITIVEALPHA);
				Direct3D.d3dd.SetRenderState(RenderState.TextureFactor, General.ARGB(flarealpha, 1f, 1f, 1f));

				// Set the sprite texture
				Direct3D.d3dd.SetTexture(0, flaretex.texture);
				Direct3D.d3dd.SetTexture(1, null);

				// Render
				flare.Render();
			}
		}

		#endregion
	}
}
