/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using CodeImp.Bloodmasters.Client.Graphics;
using SharpDX;
using SharpDX.Direct3D9;

namespace CodeImp.Bloodmasters.Client;

public class Shadow
{
    #region ================== Constants

    // Amount to bias the Z
    private const float Z_BIAS = 0.04f;

    // Max height from the floor when still rendering a shadow
    private const float MAX_HEIGHT = 20f;
    private const float MAX_HEIGHT_MUL = 1f / MAX_HEIGHT;

    #endregion

    #region ================== Variables

    // Texture
    public static TextureResource texture;

    // Vertices
    public static VertexBuffer vertices;

    #endregion

    #region ================== Geometry

    // Create the geometry
    public static unsafe void CreateGeometry()
    {
        // Create vertex buffer
        vertices = new VertexBuffer(Direct3D.d3dd, sizeof(MVertex) * 4,
            Usage.WriteOnly, MVertex.Format, Pool.Default);

        // Lock vertex buffer
        var verts = vertices.Lock<MVertex>(0, 4);

        // Lefttop
        verts[0].x = -0.5f;
        verts[0].y = -0.5f;
        verts[0].z = 0f;
        verts[0].t1u = 1f / (float)texture.info.Width;
        verts[0].t1v = 1f / (float)texture.info.Height;
        verts[0].color = -1;

        // Righttop
        verts[1].x = 0.5f;
        verts[1].y = -0.5f;
        verts[1].z = 0f;
        verts[1].t1u = 1f - 1f / (float)texture.info.Width;
        verts[1].t1v = 1f / (float)texture.info.Height;
        verts[1].color = -1;

        // Leftbottom
        verts[2].x = -0.5f;
        verts[2].y = 0.5f;
        verts[2].z = 0f;
        verts[2].t1u = 1f / (float)texture.info.Width;
        verts[2].t1v = 1f - 1f / (float)texture.info.Height;
        verts[2].color = -1;

        // Rightbottom
        verts[3].x = 0.5f;
        verts[3].y = 0.5f;
        verts[3].z = 0f;
        verts[3].t1u = 1f - 1f / (float)texture.info.Width;
        verts[3].t1v = 1f - 1f / (float)texture.info.Height;
        verts[3].color = -1;

        // Done filling the vertex buffer
        vertices.Unlock();
    }

    // Destroy the geometry
    public static void DestroyGeometry()
    {
        if(vertices != null)
        {
            vertices.Dispose();
            vertices = null;
        }
    }

    #endregion

    #region ================== Methods

    // This calculates the amount of alpha for the
    // given height difference from the floor
    public static float AlphaAtHeight(float floorheight, float objheight)
    {
        return AlphaAtHeight(objheight - floorheight);
    }

    // This calculates the amount of alpha for the
    // given height difference from the floor
    public static float AlphaAtHeight(float deltaheight)
    {
        float a = 1f - deltaheight * MAX_HEIGHT_MUL;
        if(a < 0f) a = 0f;
        else if(a > 1f) a = 1f;
        return a;
    }

    #endregion

    #region ================== Rendering

    // This renders a shadow
    public static void RenderAt(float x, float y, float z, float size, float alpha)
    {
        // Drawing settings
        Direct3D.d3dd.SetRenderState(RenderState.TextureFactor, General.ARGB(alpha, 1f, 1f, 1f));

        // World matrix
        Matrix scale = Matrix.Scaling(size, size, 1f);
        Matrix position = Matrix.Translation(x, y, z + Z_BIAS);
        Direct3D.d3dd.SetTransform(TransformState.World, Matrix.Multiply(scale, position));

        // Render shadow
        Direct3D.d3dd.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
    }

    #endregion
}
