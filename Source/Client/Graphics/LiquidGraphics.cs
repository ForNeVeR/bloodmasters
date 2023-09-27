/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using CodeImp.Bloodmasters.Client.Resources;
using SharpDX;
using SharpDX.Direct3D9;

namespace CodeImp.Bloodmasters.Client.Graphics;

public class LiquidGraphics : ITextureResource
{
    #region ================== Constants

    private const int TEXTURE_SIZE = 256;
    private const int GRID_RESOLUTION = 4;
    private const float ROTATE_LENGTH = 16f;
    private const float CELL_SIZE_PIXELS = (float)TEXTURE_SIZE / (float)GRID_RESOLUTION;
    private const float CELL_SIZE = CELL_SIZE_PIXELS / (float)TEXTURE_SIZE;
    private const int NUM_V = GRID_RESOLUTION + 3;
    private const int NUM_C = GRID_RESOLUTION + 2;
    private const int NUM_VERTICES = NUM_V * NUM_V;
    private const int NUM_CELLS = NUM_C * NUM_C;

    #endregion

    #region ================== Variables

    private TextureResource basetexture;
    private Texture texture;
    private TLVertex[] vertices1;
    private TLVertex[] vertices2;
    private short[] indices;
    private ImageInformation info;
    private Viewport viewport;
    private readonly float rotatespeed;

    #endregion

    #region ================== Properties

    public ImageInformation Info { get { return info; } }
    public Texture Texture { get { return texture; } }

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public LiquidGraphics(string texfile, float speed)
    {
        // Load base texture
        string texarch = ArchiveManager.FindFileArchive(texfile);
        if(texarch != "")
        {
            // Load the floor texture
            string tempfile = ArchiveManager.ExtractFile(texarch + "/" + texfile);
            basetexture = Direct3D.LoadTexture(tempfile, true, !Direct3D.hightextures);
        }

        // Set the speed
        rotatespeed = speed * 0.001f;

        // Make texture info
        info.Format = (Format)Direct3D.DisplayFormat;
        info.Depth = 1;
        info.Width = TEXTURE_SIZE;
        info.Height = TEXTURE_SIZE;
        info.MipLevels = 1;
        info.ImageFileFormat = ImageFileFormat.Bmp;
        info.ResourceType = ResourceType.Texture;

        // Make up viewport
        viewport = new Viewport();
        viewport.X = 0;
        viewport.Y = 0;
        viewport.Width = TEXTURE_SIZE;
        viewport.Height = TEXTURE_SIZE;
        viewport.MinDepth = 0f;
        viewport.MaxDepth = 10f;

        // Make rendertarget
        CreateRendertarget();

        // Make initial geometry
        MakeGeometry();
    }

    // Dispose
    public void Dispose()
    {
        // Clean up
        DestroyRendertarget();
        basetexture = null;
        GC.SuppressFinalize(this);
    }

    #endregion

    #region ================== Resource Management

    // This unloads all unstable resources
    public void UnloadResources()
    {
        // Clean up
        DestroyRendertarget();
    }

    // This rebuilds unstable resources
    public void ReloadResources()
    {
        // Make rendertarget
        CreateRendertarget();
    }

    #endregion

    #region ================== Rendertarget

    // This creates the render target
    private void CreateRendertarget()
    {
        // Make sure it is destroyed first
        DestroyRendertarget();

        // Create render target
        texture = new Texture(Direct3D.d3dd, TEXTURE_SIZE, TEXTURE_SIZE, 1,
            Usage.RenderTarget, (Format)Direct3D.DisplayFormat, Pool.Default);
    }

    // This destroys the render target
    private void DestroyRendertarget()
    {
        // Clean up
        if(texture != null) texture.Dispose();
        texture = null;
    }

    #endregion

    #region ================== Geometry

    // Makes the initial geometry
    private void MakeGeometry()
    {
        TLVertex v = new TLVertex();
        int vi = 0, ii = 0;
        float x, y;

        // Make vertices and indices arrays
        vertices1 = new TLVertex[NUM_VERTICES];
        vertices2 = new TLVertex[NUM_VERTICES];
        indices = new short[NUM_CELLS * 6];

        // Start here
        x = -CELL_SIZE;
        y = -CELL_SIZE;

        // Go for all grid points in y
        for(int gpy = 0; gpy < NUM_V; gpy++)
        {
            // Go for all grid points in x
            for(int gpx = 0; gpx < NUM_V; gpx++)
            {
                // Make vertex
                v.color = -1;
                v.rhw = 1f;
                v.tu = x;
                v.tv = y;
                v.z = 0.5f;

                // Add vertex
                vertices1[vi] = v;

                // Different texture coordinates for second set
                v.tu = -x;
                v.tv = -y;

                // Add vertex
                vertices2[vi] = v;

                // Move on
                x += CELL_SIZE;
                vi++;
            }

            // Next row
            y += CELL_SIZE;
            x = -CELL_SIZE;
        }

        // Now make indices per quad

        // Go for all rows
        for(int r = 0; r < NUM_C; r++)
        {
            // Go for all cols
            for(int c = 0; c < NUM_C; c++)
            {
                // Make first triangle
                indices[ii++] = (short)((c + 0) + ((r + 0) * NUM_V));
                indices[ii++] = (short)((c + 1) + ((r + 0) * NUM_V));
                indices[ii++] = (short)((c + 0) + ((r + 1) * NUM_V));

                // Make second triangle
                indices[ii++] = (short)((c + 0) + ((r + 1) * NUM_V));
                indices[ii++] = (short)((c + 1) + ((r + 0) * NUM_V));
                indices[ii++] = (short)((c + 1) + ((r + 1) * NUM_V));
            }
        }
    }

    // This updates vertex coordinates
    private void UpdateGeometry()
    {
        TLVertex v = new TLVertex();
        float x, y;
        int vi = 0;
        int ox, oy;
        float rx, ry;

        // Start here
        x = -CELL_SIZE_PIXELS;
        y = -CELL_SIZE_PIXELS;

        // Go for all grid points in y
        for(int gpy = 0; gpy < NUM_V; gpy++)
        {
            // Go for all grid points in x
            for(int gpx = 0; gpx < NUM_V; gpx++)
            {
                // Determine rotate offset by number of pixels from center
                ox = (int)Math.Abs(x - (TEXTURE_SIZE * 0.5f)) * 200 + (int)Math.Abs(y - (TEXTURE_SIZE * 0.5f)) * 50;
                oy = (int)Math.Abs(y - (TEXTURE_SIZE * 0.5f)) * 100 + (int)Math.Abs(x - (TEXTURE_SIZE * 0.5f)) * 300;

                // Determine rotate speed by number of pixels from center
                rx = Math.Abs(x - (TEXTURE_SIZE * 0.5f)) * 0.002f * rotatespeed;
                ry = Math.Abs(y - (TEXTURE_SIZE * 0.5f)) * 0.002f * rotatespeed;

                // Determine vertex coordinates
                vertices1[vi].x = x + (float)Math.Sin((float)(SharedGeneral.currenttime + ox) * rx) * ROTATE_LENGTH;
                vertices1[vi].y = y + (float)Math.Cos((float)(SharedGeneral.currenttime + oy) * ry) * ROTATE_LENGTH;

                // Determine rotate offset by number of pixels from center
                ox = (int)Math.Abs(x - (TEXTURE_SIZE * 0.5f)) * 150 + (int)Math.Abs(y - (TEXTURE_SIZE * 0.5f)) * 200;
                oy = (int)Math.Abs(y - (TEXTURE_SIZE * 0.5f)) * 220 + (int)Math.Abs(x - (TEXTURE_SIZE * 0.5f)) * 100;

                // Determine rotate speed by number of pixels from center
                rx = ((TEXTURE_SIZE * 0.5f) - Math.Abs(x - (TEXTURE_SIZE * 0.5f))) * 0.002f * rotatespeed;
                ry = ((TEXTURE_SIZE * 0.5f) - Math.Abs(y - (TEXTURE_SIZE * 0.5f))) * 0.002f * rotatespeed;

                // Determine vertex coordinates
                vertices2[vi].x = x - (float)Math.Sin((float)(SharedGeneral.currenttime + ox) * rx) * ROTATE_LENGTH;
                vertices2[vi].y = y - (float)Math.Cos((float)(SharedGeneral.currenttime + oy) * ry) * ROTATE_LENGTH;

                // Move on
                x += CELL_SIZE_PIXELS;
                vi++;
            }

            // Next row
            y += CELL_SIZE_PIXELS;
            x = -CELL_SIZE_PIXELS;
        }
    }

    #endregion

    #region ================== Processing

    // Process this
    public void Process()
    {
        // Update geometry
        UpdateGeometry();
    }

    #endregion

    #region ================== Rendering

    // Render
    public void Render()
    {
        Surface texsurface;

        // Set rendering target
        texsurface = texture.GetSurfaceLevel(0);
        Direct3D.d3dd.DepthStencilSurface = null;
        Direct3D.d3dd.SetRenderTarget(0, texsurface);
        //Direct3D.d3dd.Viewport = viewport;

        // Begin of rendering routine
        Direct3D.d3dd.BeginScene();

        // Set drawing mode
        Direct3D.SetDrawMode(DRAWMODE.TLMODALPHA);
        Direct3D.d3dd.SetTexture(0, basetexture.texture);
        Direct3D.d3dd.SetRenderState(RenderState.TextureFactor, -1);

        // Render the liquid
        Direct3D.d3dd.DrawIndexedUserPrimitives(PrimitiveType.TriangleList, 0,
            vertices1.Length, NUM_CELLS * 2, indices, Format.Index16, vertices1);

        // Blend with second set
        Direct3D.d3dd.SetRenderState(RenderState.TextureFactor, General.ARGB(0.5f, 1f, 1f, 1f));

        // Render the liquid
        Direct3D.d3dd.DrawIndexedUserPrimitives(PrimitiveType.TriangleList, 0,
            vertices2.Length, NUM_CELLS * 2, indices, Format.Index16, vertices2);

        // Done rendering
        Direct3D.d3dd.EndScene();

        // Clean up
        //Direct3D.d3dd.SetRenderTarget(0, null);
        //Direct3D.d3dd.DepthStencilSurface = null;
        texsurface.Dispose();
        texsurface = null;
    }

    #endregion
}
