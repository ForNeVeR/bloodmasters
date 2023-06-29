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
	public class PhoenixFlame : VisualObject
	{
		#region ================== Constants

		private const float MAX_OFFSET_H = 1f;
		private const float DIFF_OFFSET_V = 0.5f;
		private const float MIN_OFFSET_Z = 2f;
		private const float RND_OFFSET_Z = 2f;
		private const float FADE_IN = 0.01f;
		private const float MAX_ALPHA = 0.5f;
		private const float SIZE_START = 4f;
		private const float SIZE_END = 7f;
		private const float SIZE_INCREASE = 0.1f;

		#endregion

		#region ================== Variables

		private Sprite sprite;
		private Animation ani;
		private ClientSector sector;
		private bool disposed = false;
		private float alpha = 0f;
		private PhysicsState state;

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public PhoenixFlame(Vector3D position, Vector3D velocity)
		{
			// Set members
			this.renderbias = 2f;

			// Move with actor
			state = new PhysicsState(General.map);
			state.Bounce = false;
			state.Blocking = false;
			state.Diameter = 1f;
			state.Friction = 0f;
			state.Height = 1f;
			state.Redirect = false;
			state.StepUp = false;
			state.pos = position;
			state.vel = velocity;
			this.pos = position;

			// Determine sector
			sector = (ClientSector)General.map.GetSubSectorAt(this.pos.x, this.pos.y).Sector;

			// Make the sprite
			sprite = new Sprite(this.pos, SIZE_START, false, true);
			sprite.Update();

			// Create animation
			ani = Animation.CreateFrom("sprites/phoenixfire.cfg");

			// Slight random speed in animation
			float spd = ani.FrameTime;
			spd *= 0.8f + ((float)General.random.NextDouble() * 0.4f);
			ani.FrameTime = (int)Math.Ceiling(spd);
		}

		// Disposer
		public override void Dispose()
		{
			// Clean up
			sprite = null;
			ani = null;
			disposed = true;
			sector = null;
			base.Dispose();
			GC.SuppressFinalize(this);
		}

		#endregion

		#region ================== Processing

		// Processing
		public override void Process()
		{
			// Not disposed?
			if(!disposed)
			{
				// Decelerate
				state.vel /= 1f + Consts.FLAMES_DECELERATE;

				// Move object
				state.ApplyVelocity(false);
				this.pos = state.pos;
				sprite.Position = this.pos;

				// Increase sprite size?
				if(sprite.Size < SIZE_END)
				{
					// Increase sprite
					sprite.Size += SIZE_INCREASE;
					if(sprite.Size > SIZE_END) sprite.Size = SIZE_END;
					sprite.Update();
				}

				// Determine sector
				sector = (ClientSector)General.map.GetSubSectorAt(this.pos.x, this.pos.y).Sector;

				// Process animation
				ani.Process();

				// Fade in
				if(alpha < MAX_ALPHA) alpha += FADE_IN;
				if(alpha > MAX_ALPHA) alpha = MAX_ALPHA;

				// Dispose when end of animation
				if(ani.Ended) this.Dispose();
			}
		}

		#endregion

		#region ================== Rendering

		// Rendering
		public override void Render()
		{
			// Check if in screen
			if(this.sector.VisualSector.InScreen && !disposed)
			{
				// Set render mode
				Direct3D.SetDrawMode(DRAWMODE.NADDITIVEALPHA);
				Direct3D.d3dd.SetRenderState(RenderState.TextureFactor, General.ARGB(alpha, 1f, 1f, 1f));

				// No lightmap
				Direct3D.d3dd.SetTexture(1, null);

				// Set texture
				Direct3D.d3dd.SetTexture(0, ani.CurrentFrame.texture);

				// Render sprite
				sprite.Render();
			}
		}

		#endregion
	}
}
