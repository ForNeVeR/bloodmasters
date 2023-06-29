/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using SharpDX;
using SharpDX.Direct3D9;

namespace CodeImp.Bloodmasters.Client
{
	public class NukeSign
	{
		#region ================== Constants

		private const float Z_BIAS = 0.04f;
		private const float SIZE = 6f;

		#endregion

		#region ================== Variables

		// Texture
		public static TextureResource texture;

		#endregion

		#region ================== Rendering

		// This renders the nuke sign
		public static void RenderAt(float x, float y, float z)
		{
			// World matrix
			Matrix scale = Matrix.Scaling(SIZE, SIZE, 1f);
			Matrix position = Matrix.Translation(x, y, z + Z_BIAS);
			Matrix rotate = Matrix.RotationZ((float)SharedGeneral.currenttime * 0.004f);
			Direct3D.d3dd.SetTransform(TransformState.World,  Matrix.Multiply(Matrix.Multiply(rotate, scale), position));

			// Render shadow
			Direct3D.d3dd.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
		}

		#endregion
	}
}
