/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using System.Drawing;
using System.Collections;
using CodeImp.Bloodmasters;
using CodeImp;
using SharpDX.Direct3D9;

namespace CodeImp.Bloodmasters.Client
{
	public class FireFlame : VisualObject
	{
		#region ================== Constants

		private const float MAX_OFFSET_H = 1f;
		private const float DIFF_OFFSET_V = 0.5f;
		private const float MIN_OFFSET_Z = 2f;
		private const float RND_OFFSET_Z = 2f;
		private const float FADE_IN = 0.01f;
		private const float MAX_ALPHA = 0.5f;

		#endregion

		#region ================== Variables

		private Actor actor;
		private Sprite sprite;
		private Vector3D offset;
		private Animation ani;
		private bool disposed = false;
		private float alpha = 0f;

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public FireFlame(Actor actor, bool front)
		{
			float offh, offv;

			// Set members
			this.actor = actor;
			if(front) this.renderbias = 2f; else this.renderbias = 0f;

			// Determine offsets
			offh = ((float)General.random.NextDouble() - 0.5f) * MAX_OFFSET_H;
			if(front) offv = DIFF_OFFSET_V; else offv = -DIFF_OFFSET_V;
			offset.x = offh + offv;
			offset.y = offh - offv;
			offset.z = MIN_OFFSET_Z + (float)General.random.NextDouble() * RND_OFFSET_Z;

			// Move with actor
			this.pos = actor.Position + offset;

			// Make the sprite
			sprite = new Sprite(this.pos, 6f, false, true);
			sprite.Update();

			// Create animation
			ani = Animation.CreateFrom("sprites/playerfire.cfg");
		}

		// Disposer
		public override void Dispose()
		{
			// Clean up
			actor = null;
			sprite = null;
			ani = null;
			disposed = true;
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
				// Move object to match actor position
				this.pos = actor.Position + offset;
				sprite.Position = this.Position;

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
			if(actor.Sector.VisualSector.InScreen && !disposed)
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
