/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using System.Drawing;
using System.Collections;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using CodeImp.Bloodmasters;
using CodeImp;

namespace CodeImp.Bloodmasters.Client
{
	public class RingSign
	{
		#region ================== Constants
		
		private const float Z_BIAS = 0.04f;
		private const float SIZE = 8f;
		private const float SPEED = 0.01f;
		
		#endregion
		
		#region ================== Variables
		
		// Texture
		public static TextureResource texture;
		
		#endregion
		
		#region ================== Rendering
		
		// This renders the ring
		public static void RenderAt(float x, float y, float z)
		{
			// Determine size over time
			float size = SIZE * (float)Math.Sin((float)General.currenttime * SPEED);
			
			// World matrix
			Matrix scale = Matrix.Scaling(size, size, 1f);
			Matrix position = Matrix.Translation(x, y, z + Z_BIAS);
			Matrix rotate = Matrix.RotationZ((float)General.currenttime * 0.004f);
			Direct3D.d3dd.Transform.World = Matrix.Multiply(Matrix.Multiply(rotate, scale), position);
			
			// Render shadow
			Direct3D.d3dd.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
		}
		
		#endregion
	}
}
