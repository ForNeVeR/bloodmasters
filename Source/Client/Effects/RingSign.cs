/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using Bloodmasters.Client.Resources;
using SharpDX;
using SharpDX.Direct3D9;

namespace Bloodmasters.Client.Effects;

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
        float size = SIZE * (float)Math.Sin((float)SharedGeneral.currenttime * SPEED);

        // World matrix
        Matrix scale = Matrix.Scaling(size, size, 1f);
        Matrix position = Matrix.Translation(x, y, z + Z_BIAS);
        Matrix rotate = Matrix.RotationZ((float)SharedGeneral.currenttime * 0.004f);
        Graphics.Direct3D.d3dd.SetTransform(TransformState.World, Matrix.Multiply(Matrix.Multiply(rotate, scale), position));

        // Render shadow
        Graphics.Direct3D.d3dd.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
    }

    #endregion
}
