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
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using CodeImp.Bloodmasters;
using CodeImp;

namespace CodeImp.Bloodmasters.Client
{
	[WeaponInfo(WEAPON.IONCANNON, RefireDelay=500, Description="Ion Cannon",
				Sound="ioncannon_fire.wav", AmmoType=AMMO.PLASMA, UseAmmo=20)]
	public class WIonCannon : Weapon
	{
		#region ================== Constants
		
		// Fire flare
		private const float FLARE_ALPHA_START = 1f;
		private const float FLARE_ALPHA_CHANGE = -0.1f;
		private const float FLARE_SIZE_START = 8f;
		private const float FLARE_SIZE_CHANGE = -0.06f;
		private const int LOAD_DELAY = 1000;
		
		#endregion
		
		#region ================== Variables
		
		// Fire flare
		public static TextureResource flaretex;
		private Sprite flare;
		private float flarealpha = 0f;
		
		// States
		private CANNONSTATE state = CANNONSTATE.IDLE;
		private int statechangetime = 0;
		
		// Sounds
		private ISound loader = null;
		
		#endregion
		
		#region ================== Constructor / Destructor
		
		// Constructor
		public WIonCannon(Client client) : base(client)
		{
			// Make fire flare sprite
			flare = new Sprite(new Vector3D(), FLARE_SIZE_START, false, true);
		}
		
		// Disposer
		public override void Dispose()
		{
			// Clean up
			if(loader != null) loader.Dispose();
			loader = null;
			flare = null;
			
			// Dispose base
			base.Dispose();
		}
		
		#endregion
		
		#region ================== Methods
		
		// This is called when the trigger is pulled
		public override void Trigger()
		{
			// Check if gun is idle
			if(this.IsIdle())
			{
				// Go to loading state
				state = CANNONSTATE.LOADING;
				statechangetime = General.currenttime + LOAD_DELAY;
				
				// Dispose loader sound, if any
				if(loader != null) loader.Dispose();
				
				// Change the sound
				loader = DirectSound.GetSound("ioncannon_load.wav", true);
				if(client.Actor != null) loader.Position = client.Actor.Position;
				loader.Play(false);
				return;
			}
			
			// Time to fire?
			if((state == CANNONSTATE.LOADING) && (General.currenttime > statechangetime))
			{
				// Dispose loader sound, if any
				if(loader != null) loader.Dispose();
				
				// FIRE!
				base.Trigger();
				
				// Return to idle state
				state = CANNONSTATE.IDLE;
			}
		}
		
		// This is called when the trigger is released
		public override void Released()
		{
			// Check if the weapon is loading
			if(state == CANNONSTATE.LOADING)
			{
				// Dispose loader sound, if any
				if(loader != null) loader.Dispose();
				
				// Stop loading
				state = CANNONSTATE.IDLE;
			}
			
			// Base class stuff
			base.Released();
		}
		
		// This is called when the weapon is shooting
		protected override void ShootOnce()
		{
			// Play the shooting sound
			if(client.Actor.Sector.VisualSector.InScreen)
				DirectSound.PlaySound(sound, client.Actor.Position);
			
			// Make the actor play the shooting animation
			client.Actor.PlayShootingAnimation(1, 0);
			
			// Set fire flare
			flarealpha = FLARE_ALPHA_START;
			flare.Size = FLARE_SIZE_START;
			flare.Rotation = (float)General.random.NextDouble() * 2f * (float)Math.PI;
		}
		
		// This processes the weapon
		public override void Process()
		{
			// Process base class
			base.Process();
			
			// Loading sound playing?
			if((loader != null) && !loader.Disposed)
			{
				// Loading sound stopped?
				if(!loader.Playing)
				{
					// Remove loader sound
					loader.Dispose();
					loader = null;
				}
				else
				{
					// Client actor avilable?
					if(client.Actor != null)
					{
						// Reposition loader sound
						loader.Position = client.Actor.Position;
					}
				}
			}
			
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
				light.Color = General.ARGB(flarealpha * 0.5f, 1f, 1f, 1f);
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
				Direct3D.d3dd.RenderState.TextureFactor = General.ARGB(flarealpha, 1f, 1f, 1f);
				
				// Set the sprite texture
				Direct3D.d3dd.SetTexture(0, flaretex.texture);
				Direct3D.d3dd.SetTexture(1, null);
				
				// Render
				flare.Render();
			}
		}
		
		// This is called to check if the weapon is ready
		public override bool IsIdle()
		{
			// Return if the weapon is idle
			return (state == CANNONSTATE.IDLE) && (refiretime < General.currenttime);
		}
		
		#endregion
	}
	
	// Cannon states
	public enum CANNONSTATE
	{
		IDLE = 0,
		LOADING = 1,
	}
}
