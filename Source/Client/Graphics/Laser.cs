/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using SharpDX;
using SharpDX.Direct3D9;

namespace CodeImp.Bloodmasters.Client
{
	public class Laser
	{
		#region ================== Constants

		private const float WIDTH = 2f;
		private const float DOTSIZE = 0.6f;
		private const float LASER_OFFSET_X = 1f;
		private const float LASER_OFFSET_Y = -1f;
		private const float LASER_OFFSET_Z = 10f;
		private const float LASER_DELTA_ANGLE = 0.43f;
		private const float LASER_DISTANCE = 3f;
		private static float[] LASER_OPACITY = { 0f, 0.05f, 0.1f, 0.2f, 0.4f };

		#endregion

		#region ================== Variables

		// Appearance
		public static int opacity = 2;
		public static TextureResource texture = null;
		public static TextureResource dottexture = null;
		private static Sprite dot = new Sprite(new Vector3D(0f, 0f, 0f), DOTSIZE, false, true);

		#endregion

		#region ================== Methods

		// This determines the source of the laser for an actor
		public static Vector3D GetSourcePosition(Actor a)
		{
			// Make rounded angle of actor
			//float rangle = Actor.AngleFromDir(Actor.DirFromAngle(a.AimAngle, 0, 16), 0, 16);
			float rangle = a.AimAngle + LASER_DELTA_ANGLE * (float)Math.PI;

			// Position
			return a.Position +
					new Vector3D(LASER_OFFSET_X, LASER_OFFSET_Y, LASER_OFFSET_Z) +
					Vector3D.FromAnimationAngle(rangle, LASER_DISTANCE);
		}

		#endregion

		#region ================== Rendering

		// Rendering
		public static void Render(Vector3D from, Vector3D to)
		{
			Vector3D from2d, to2d, delta2d, trjnorm;
			Vector3D p1, p2, p3, p4;
			Vector3D[] n = new Vector3D[4];
			MVertex[] v = new MVertex[4];
			float width = WIDTH * ((float)Direct3D.DisplayWidth / 640f);

			// Project the coordinates
			from2d = new Vector3D(General.arena.Projected(from));
			to2d = new Vector3D(General.arena.Projected(to));
			delta2d = to2d - from2d;

			// Determine trajectory normal
			trjnorm = new Vector3D(-delta2d.y, delta2d.x, 0f);
			trjnorm.Normalize();

			// Calculate projected vertices
			p1 = from2d - trjnorm * width;
			p2 = from2d + trjnorm * width;
			p3 = to2d - trjnorm * width;
			p4 = to2d + trjnorm * width;

			// Unproject to 3D space
			n[0] = new Vector3D(General.arena.Unprojected(p1));
			n[1] = new Vector3D(General.arena.Unprojected(p2));
			n[2] = new Vector3D(General.arena.Unprojected(p3));
			n[3] = new Vector3D(General.arena.Unprojected(p4));

			// Set vertex properties
			for(int i = 0; i < 4; i++)
			{
				v[i].color = -1;
				v[i].x = n[i].x;
				v[i].y = n[i].y;
				v[i].z = n[i].z;
			}

			// Set texture coordinates
			v[0].t1u = 0f;
			v[0].t1v = 0f;
			v[1].t1u = 0f;
			v[1].t1v = 1f;
			v[2].t1u = 1f;
			v[2].t1v = 0f;
			v[3].t1u = 1f;
			v[3].t1v = 1f;

			// Position the dot
			dot.Position = to + new Vector3D(0f, 0f, 1f);

			// Set render mode
			Direct3D.SetDrawMode(DRAWMODE.NADDITIVEALPHA);
			Direct3D.d3dd.SetRenderState(RenderState.ZWriteEnable, false);
			Direct3D.d3dd.SetRenderState(RenderState.TextureFactor, General.ARGB(LASER_OPACITY[opacity], 1f, 1f, 1f));

			// Set textures
			Direct3D.d3dd.SetTexture(0, texture.texture);
			Direct3D.d3dd.SetTexture(1, null);

			// Set matrices
			Direct3D.d3dd.SetTransform(TransformState.World, Matrix.Identity);
			Direct3D.d3dd.SetTransform(TransformState.Texture0, Matrix.Identity);

			// Render the laser
			Direct3D.d3dd.DrawUserPrimitives(PrimitiveType.TriangleStrip, 2, v);

			// Determine dot intensity
			float dotalpha = LASER_OPACITY[opacity] * 5f;
			if(dotalpha > 1f) dotalpha = 1f;

			// Render the dot
			Direct3D.d3dd.SetRenderState(RenderState.TextureFactor, General.ARGB(dotalpha, 1f, 1f, 1f));
			Direct3D.d3dd.SetTexture(0, dottexture.texture);
			dot.Render();
		}

		#endregion
	}
}
