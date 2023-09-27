/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using System.Collections.Generic;
using CodeImp.Bloodmasters.Client.LevelMap;
using CodeImp.Bloodmasters.Client.Resources;
using CodeImp.Bloodmasters.LevelMap;
using SharpDX;
using SharpDX.Direct3D9;

namespace CodeImp.Bloodmasters.Client.Graphics;

public class WallDecal : Decal
{
    #region ================== Constants

    private const int BLOOD_TEXTURES = 4;
    private const int BULLET_TEXTURES = 1;
    private const int PLASMA_TEXTURES = 4;
    private const int EXPLODE_TEXTURES = 4;
    private const float SCALE_XY = 0.16f; //0.08f;
    private const float SCALE_Z = 0.32f; //0.26f;
    private const float Z_BIAS = 0.1f;

    #endregion

    #region ================== Variables

    // Wall decal textures
    public static TextureResource[] blooddecals = new TextureResource[BLOOD_TEXTURES];
    public static TextureResource[] bulletdecals = new TextureResource[BULLET_TEXTURES];
    public static TextureResource[] plasmadecals = new TextureResource[PLASMA_TEXTURES];
    public static TextureResource[] explodedecals = new TextureResource[EXPLODE_TEXTURES];

    // Decal texture
    private TextureResource texture;

    // Sidedef and sector where this decal is on
    private readonly VisualSidedef sidedef;

    // Geometry
    private static VertexBuffer vertices = null;
    private Matrix decalmatrix;
    private Matrix lightmapmatrix;

    #endregion

    #region ================== Properties

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public WallDecal(Linedef line, float u, float height, VisualSidedef vside,
        TextureResource texture, bool permanent) : base(permanent)
    {
        float decalanglez, decalangley;

        // Keep texture reference
        this.texture = texture;

        // Get vertices
        Vector2D v1 = General.map.Vertices[line.v1];
        Vector2D v2 = General.map.Vertices[line.v2];

        // Determine coordinates
        x = v1.x + u * (v2.x - v1.x);
        y = v1.y + u * (v2.y - v1.y);
        z = height;

        // Move coordinates slightly by camera vector
        // to stay in front of the wall
        x += General.arena.CameraVector.X * Z_BIAS;
        y += General.arena.CameraVector.Y * Z_BIAS;
        z += General.arena.CameraVector.Z * Z_BIAS;

        // Keep reference to VisualSidedef
        sidedef = vside;

        // Get reference to VisualSector
        sector = ((ClientSector)sidedef.Sidedef.Sector).VisualSector;

        // Add decal to list
        General.arena.AddDecal(this);

        // Determine angle
        decalanglez = sidedef.Sidedef.Angle;
        decalangley = (float)General.random.NextDouble() * (float)Math.PI * 2f;

        // Create decal matrix
        float w = texture.info.Width * SCALE_XY;
        float h = texture.info.Height * SCALE_Z;
        decalmatrix = Matrix.Identity;
        decalmatrix *= Matrix.RotationY(decalangley);
        decalmatrix *= Matrix.Scaling(w, 1f, h);
        decalmatrix *= Matrix.RotationZ(decalanglez);
        decalmatrix *= Matrix.Translation(x, y, z);

        // Create lightmap matrix
        float lx = sector.LightmapScaledX(x);
        float ly = sector.LightmapScaledY(y);
        lightmapmatrix = Matrix.Identity;
        lightmapmatrix *= Matrix.RotationZ(decalangley);
        lightmapmatrix *= Matrix.Scaling(w * sector.LightmapScaleX, 0f, 1f);
        lightmapmatrix *= Matrix.RotationZ(decalanglez);
        lightmapmatrix *= Matrix.Scaling(1f, sector.LightmapAspect, 1f);
        lightmapmatrix.M31 = lx;
        lightmapmatrix.M32 = ly;
    }

    // Dispose
    public override void Dispose()
    {
        // Release references
        texture = null;

        // Remove decal from list
        General.arena.RemoveDecal(this);
        GC.SuppressFinalize(this);
    }

    #endregion

    #region ================== Static Methods

    // This makes a wall decal when it collides with a
    // wall at the given coordinates
    public static WallDecal Spawn(float nx, float ny, float nz, float size,
        TextureResource[] textureset, bool permanent)
    {
        ClientSidedef sd = null;

        // Dont make a decal when not using decals
        if(!Decal.showdecals) return null;

        // Get all lines near rectangle
        List<Linedef> lines = General.map.BlockMap.GetCollisionLines(nx, ny, size);
        if(lines.Count > 0)
        {
            // Get the nearest line
            Linedef line = General.map.GetNearestLine(nx, ny, lines);

            // Get distance to the line
            float d = line.DistanceToLine(nx, ny);
            if(d < size)
            {
                // Determine side of line
                float side = line.SideOfLine(nx, ny);
                if(side < 0) sd = (ClientSidedef)line.Front; else sd = (ClientSidedef)line.Back;
                if((sd != null) && (sd.OtherSide != null) && (sd.VisualSidedef != null))
                {
                    // Check if within wall section
                    if((nz < (sd.OtherSide.Sector.CurrentFloor + 0.2f)) ||
                       (sd.OtherSide.Sector.HasCeiling &&
                        (nz > sd.OtherSide.Sector.HeightCeil) &&
                        (nz < sd.OtherSide.Sector.FakeHeightCeil + 0.2f)))
                    {
                        // Choose a random decal
                        int decal = General.random.Next(textureset.Length);

                        // Determine percentage of line needed
                        float un = ((float)textureset[decal].info.Width * SCALE_XY * 0.5f) / line.Length;

                        // Find coordinates on the line
                        float u = line.NearestOnLine(nx, ny);

                        // Check if enough space on the line
                        if((u >= un) && (u <= 1f - un))
                        {
                            // Make the decal!
                            return new WallDecal(line, u, nz, sd.VisualSidedef, textureset[decal], permanent);
                        }
                    }
                }
            }
        }

        // No decal to make
        return null;
    }

    // This makes a wall decal
    public static WallDecal Spawn(ClientSidedef sd, float uline, float z, TextureResource[] textureset, bool permanent)
    {
        // Dont make a decal when not using decals
        if(!Decal.showdecals) return null;
        if(sd == null) return null;

        // Sides available?
        if((sd.OtherSide != null) && (sd.VisualSidedef != null))
        {
            // Check if within wall section
            if(z < (sd.OtherSide.Sector.CurrentFloor + 0.2f))
            {
                // Choose a random decal
                int decal = General.random.Next(textureset.Length);

                // Determine percentage of line needed
                float un = ((float)textureset[decal].info.Width * SCALE_XY * 0.5f) / sd.Linedef.Length;

                // Check if enough space on the line
                if((uline >= un) && (uline <= 1f - un))
                {
                    // Make the decal!
                    return new WallDecal(sd.Linedef, uline, z, sd.VisualSidedef, textureset[decal], permanent);
                }
            }
        }

        // No decal to make
        return null;
    }

    // This loads all wall decal textures
    public static void LoadTextures()
    {
        // Go for all blood textures
        for(int i = 0; i < BLOOD_TEXTURES; i++)
        {
            // Load the sprite
            string filename = "sprites/wallblood" + i + ".tga";
            string tempfile = ArchiveManager.ExtractFile(filename);
            blooddecals[i] = null;
            blooddecals[i] = Direct3D.LoadTexture(tempfile, true);
            if(blooddecals[i] == null) throw(new Exception("Cannot load decal '" + filename + "'"));
        }

        // Go for all bullet textures
        for(int i = 0; i < BULLET_TEXTURES; i++)
        {
            // Load the sprite
            string filename = "sprites/wallbullet" + i + ".tga";
            string tempfile = ArchiveManager.ExtractFile(filename);
            bulletdecals[i] = null;
            bulletdecals[i] = Direct3D.LoadTexture(tempfile, true);
            if(bulletdecals[i] == null) throw(new Exception("Cannot load decal '" + filename + "'"));
        }

        // Go for all plasma textures
        for(int i = 0; i < PLASMA_TEXTURES; i++)
        {
            // Load the sprite
            string filename = "sprites/wallplasma" + i + ".tga";
            string tempfile = ArchiveManager.ExtractFile(filename);
            plasmadecals[i] = null;
            plasmadecals[i] = Direct3D.LoadTexture(tempfile, true);
            if(plasmadecals[i] == null) throw(new Exception("Cannot load decal '" + filename + "'"));
        }

        // Go for all explode textures
        for(int i = 0; i < EXPLODE_TEXTURES; i++)
        {
            // Load the sprite
            string filename = "sprites/wallexplode" + i + ".tga";
            string tempfile = ArchiveManager.ExtractFile(filename);
            explodedecals[i] = null;
            explodedecals[i] = Direct3D.LoadTexture(tempfile, true);
            if(explodedecals[i] == null) throw(new Exception("Cannot load decal '" + filename + "'"));
        }
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
        verts[0].z = 0.5f;
        verts[0].t1u = 0f;
        verts[0].t1v = 0f;
        verts[0].color = -1;
        verts[0].t2u = -0.5f;
        verts[0].t2v = -0.5f;

        // Righttop
        verts[1].x = 0.5f;
        verts[1].y = 0f;
        verts[1].z = 0.5f;
        verts[1].t1u = 1f;
        verts[1].t1v = 0f;
        verts[1].color = -1;
        verts[1].t2u = 0.5f;
        verts[1].t2v = -0.5f;

        // Leftbottom
        verts[2].x = -0.5f;
        verts[2].y = 0f;
        verts[2].z = -0.5f;
        verts[2].t1u = 0f;
        verts[2].t1v = 1f;
        verts[2].color = -1;
        verts[2].t2u = -0.5f;
        verts[2].t2v = 0.5f;

        // Rightbottom
        verts[3].x = 0.5f;
        verts[3].y = 0f;
        verts[3].z = -0.5f;
        verts[3].t1u = 1f;
        verts[3].t1v = 1f;
        verts[3].color = -1;
        verts[3].t2u = 0.5f;
        verts[3].t2v = 0.5f;

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

    #region ================== Rendering

    // Render decal
    public override void Render()
    {
        // Check if visible
        if((sector.InScreen) && (texture != null))
        {
            // Get back sector
            Sector sc = null;
            if(sidedef.Sidedef.OtherSide != null) sc = sidedef.Sidedef.OtherSide.Sector;

            // Back sector dynamic?
            if((sc != null) && sc.Dynamic)
            {
                // Make world matrix
                Matrix m = Matrix.Translation(0f, 0f, sc.CurrentFloor - sc.HeightFloor);
                Direct3D.d3dd.SetTransform(TransformState.World, decalmatrix * m);
            }
            else
            {
                // Reset world matrix
                Direct3D.d3dd.SetTransform(TransformState.World, decalmatrix);
            }

            // Apply lightmap matrix
            Direct3D.d3dd.SetTransform(TransformState.Texture1, lightmapmatrix);

            // Set the texture and vertices stream
            Direct3D.d3dd.SetTexture(0, texture.texture);
            Direct3D.d3dd.SetStreamSource(0, WallDecal.vertices, 0, MVertex.Stride);

            // Set the lightmap from visual sector
            Direct3D.d3dd.SetTexture(1, sector.Lightmap);

            // Set transparency
            Direct3D.d3dd.SetRenderState(RenderState.TextureFactor, base.fadecolor);

            // Render it!
            Direct3D.d3dd.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
        }
    }

    #endregion
}
