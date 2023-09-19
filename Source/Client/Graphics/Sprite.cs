/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using CodeImp.Bloodmasters.Map;
using SharpDX;
using SharpDX.Direct3D9;

namespace CodeImp.Bloodmasters.Client.Graphics;

public class Sprite
{
    #region ================== Constants

    // Billboard angles
    private const float SPRITE_ANGLE_X = (float)Math.PI * 0.25f;
    private const float SPRITE_ANGLE_Z = (float)Math.PI * -0.25f;

    #endregion

    #region ================== Variables

    // Sprite vertices
    private static VertexBuffer vertices = null;

    // Sprite matrix
    private Matrix matsprite = Matrix.Identity;
    private Matrix matlightmap = Matrix.Identity;
    private Matrix matdynlightmap = Matrix.Identity;

    // Sprite properties
    private Vector3D position;
    private bool lightmapped;
    private float scale;
    private float rotate;
    private Vector3D prevposition;
    private float prevscale;
    private float prevrotate;
    private float prevrotatex;
    private float offsetz;
    private float rotatex;
    private ClientSector sector;

    #endregion

    #region ================== Properties

    public static VertexBuffer Vertices { get { return vertices; } }
    public Vector3D Position { get { return position; } set { position = value; } }
    public float Size { get { return scale; } set { scale = value; } }
    public bool Mapped { get { return lightmapped; } }
    public float Rotation { get { return rotate; } set { rotate = value; } }
    public float RotateX { get { return rotatex; } set { rotatex = value; } }
    public Sector Sector { get { return sector; } }

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public Sprite(Vector3D pos, float size, bool mapped, bool centered)
    {
        // Copy properties
        position = pos;
        lightmapped = mapped;
        scale = size;
        if(centered) offsetz = -0.5f; else offsetz = 0f;
        rotatex = SPRITE_ANGLE_Z;

        // Update matrices
        Update();

        GC.SuppressFinalize(this);
    }

    #endregion

    #region ================== Geometry

    // This creates the generic item vertices
    public static unsafe void CreateGeometry()
    {
        // Create vertex buffer
        vertices = new VertexBuffer(Direct3D.d3dd, sizeof(MVertex) * 4,
            Usage.WriteOnly, MVertex.Format, Pool.Default);

        // Lock vertex buffer
        var verts = vertices.Lock<MVertex>(0, 4);

        // Lefttop
        verts[0].x = -0.5f;
        verts[0].y = 0f;
        verts[0].z = 1f;
        verts[0].t1u = 0f;
        verts[0].t1v = 0f;
        verts[0].color = -1;
        verts[0].t2u = -0.5f;
        verts[0].t2v = -0.5f;
        verts[0].t3u = 0f;
        verts[0].t3v = 0f;

        // Righttop
        verts[1].x = 0.5f;
        verts[1].y = 0f;
        verts[1].z = 1f;
        verts[1].t1u = 1f;
        verts[1].t1v = 0f;
        verts[1].color = -1;
        verts[1].t2u = 0.5f;
        verts[1].t2v = 0.5f;
        verts[1].t3u = 0f;
        verts[1].t3v = 0f;

        // Leftbottom
        verts[2].x = -0.5f;
        verts[2].y = 0f;
        verts[2].z = 0f;
        verts[2].t1u = 0f;
        verts[2].t1v = 1f;
        verts[2].color = -1;
        verts[2].t2u = -0.5f;
        verts[2].t2v = -0.5f;
        verts[2].t3u = 0f;
        verts[2].t3v = 0f;

        // Rightbottom
        verts[3].x = 0.5f;
        verts[3].y = 0f;
        verts[3].z = 0f;
        verts[3].t1u = 1f;
        verts[3].t1v = 1f;
        verts[3].color = -1;
        verts[3].t2u = 0.5f;
        verts[3].t2v = 0.5f;
        verts[3].t3u = 0f;
        verts[3].t3v = 0f;

        // Done filling the vertex buffer
        vertices.Unlock();
    }

    // This destroys the vertices
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

    // This updates matrices if needed
    public void Update()
    {
        // Check if lightmap matrix update is needed
        if(((position != prevposition) || (scale != prevscale)) && lightmapped)
        {
            // Find the sector
            sector = (ClientSector)General.map.GetSubSectorAt(position.x, position.y).Sector;

            // Get positions on lightmap
            float lx = sector.VisualSector.LightmapScaledX(position.x);
            float ly = sector.VisualSector.LightmapScaledY(position.y);

            // Make the lightmap matrix
            matlightmap = Matrix.Identity;
            matlightmap *= Matrix.Scaling(scale * sector.VisualSector.LightmapScaleX, 0f, 1f);
            matlightmap *= Matrix.RotationZ(SPRITE_ANGLE_Z);
            matlightmap *= Matrix.Scaling(1f, sector.VisualSector.LightmapAspect, 1f);
            matlightmap *= Direct3D.MatrixTranslateTx(lx, ly);

            // Make dynamuc lightmap matrix
            matdynlightmap = Direct3D.MatrixTranslateTx(position.x, position.y);
        }

        // Check if sprite matrix update is needed
        if((scale != prevscale) || (rotate != prevrotate) || (rotatex != prevrotatex))
        {
            // Scale sprite
            Matrix mscale = Matrix.Scaling(scale, 1f, scale);

            // Rotate sprite
            Matrix mrot0 = Matrix.RotationY(rotate);
            Matrix mrot1 = Matrix.RotationX(rotatex);
            Matrix mrot2 = Matrix.RotationZ(SPRITE_ANGLE_X);
            Matrix mrotate = Matrix.Multiply(Matrix.Multiply(mrot0, mrot1), mrot2);

            // Combine scale and rotation
            matsprite = Matrix.Translation(0f, 0f, offsetz);
            matsprite *= Matrix.Multiply(mscale, mrotate);
        }

        // Copy current properties
        prevposition = position;
        prevscale = scale;
        prevrotate = rotate;
        prevrotatex = rotatex;
    }

    // This renders the sprite
    public void Render()
    {
        // Apply lightmap matrix
        if(lightmapped)
        {
            Direct3D.d3dd.SetTransform(TransformState.Texture1, matlightmap);
            Direct3D.d3dd.SetTransform(TransformState.Texture2, matdynlightmap * General.arena.LightmapMatrix);
        }

        // Set vertices stream
        Direct3D.d3dd.SetStreamSource(0, Sprite.Vertices, 0, MVertex.Stride);

        // Position the sprite
        Matrix apos = Matrix.Translation(position.x, position.y, position.z);
        Direct3D.d3dd.SetTransform(TransformState.World, Matrix.Multiply(matsprite, apos));

        // Render the sprite
        Direct3D.d3dd.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
    }

    #endregion
}
