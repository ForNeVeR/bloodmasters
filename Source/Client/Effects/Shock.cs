/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using System.Collections.Generic;
using Bloodmasters.Client.Graphics;
using Bloodmasters.Client.Resources;
using SharpDX;
using SharpDX.Direct3D9;
using Direct3D = Bloodmasters.Client.Graphics.Direct3D;

namespace Bloodmasters.Client.Effects;

public class Shock : VisualObject
{
    #region ================== Constants

    private const float CORNER_DISTANCE = 2f;
    private const float CORNER_RND_OFFSET = 14f;
    private const float CORNER_RND_WIDTH = 6f;
    private const float CORNER_MIN_WIDTH = 10f;
    private const int CORNERS_REMOVE = 1;
    private const int CORNERS_RND_ADD = 3;
    private const float FADE_ALPHA_START = 3f;

    #endregion

    #region ================== Variables

    public static TextureResource texture;
    private float fade = FADE_ALPHA_START;
    private readonly float fadechange;
    private MVertex[] verts;
    private readonly int faces;
    private bool disposed;

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public Shock(Vector3D from, Vector3D to, float fadechange)
    {
        List<MVertex> v = new List<MVertex>();

        // Set fade speed
        this.fadechange = fadechange;

        // Determine coordinates to use for VisualObject position
        if(VisualObject.Compare(from, 0f, to, 0f) < 0)
            this.pos = from;
        else
            this.pos = to;

        // Render bias
        this.renderbias = -10f;

        // Make the shock
        AddShockVertices(v, from, to);

        // Make vertices array from list
        verts = v.ToArray();
        faces = verts.Length / 3;
    }

    // This adds shock vertices over a trajectory
    private void AddShockVertices(List<MVertex> v, Vector3D from, Vector3D to)
    {
        int corners, segments;
        float rnd_offset, rnd_width, min_width;
        float deltalen, segstart, seglen, segend, soffset, swidth;
        Vector3D delta, from2d, to2d, delta2d, vs, ve;
        Vector3D v1, v2, v3, v4, p3, p4, trjnorm;

        // Determine scales
        rnd_offset = CORNER_RND_OFFSET * ((float)Direct3D.DisplayWidth / 640f);
        rnd_width = CORNER_RND_WIDTH * ((float)Direct3D.DisplayWidth / 640f);
        min_width = CORNER_MIN_WIDTH * ((float)Direct3D.DisplayWidth / 640f);

        // Determine coordinates
        delta = to - from;
        deltalen = delta.Length();

        // Determine number of corners
        corners = (int)(deltalen / CORNER_DISTANCE);
        corners = (corners - CORNERS_REMOVE) + General.random.Next(CORNERS_RND_ADD);
        if(corners < 0) corners = 0;
        segments = corners + 1;

        // Project the trajectory coordinates
        from2d = General.arena.Projected(from.ToDx()).FromDx();
        to2d = General.arena.Projected(to.ToDx()).FromDx();
        delta2d = to2d - from2d;

        // Calculate segment length scalar in 2D
        seglen = 1f / (float)segments;

        // Determine trajectory normal
        trjnorm = new Vector3D(-delta2d.y, delta2d.x, 0f);
        trjnorm.Normalize();

        // Initialize first segment start
        segstart = 0f;
        v1 = from;
        v2 = from;

        // Go for all segments to build them
        for(int s = 0; s < segments; s++)
        {
            // Determine segment end
            segend = segstart + seglen;

            // Determine segments start/end vectors (2D)
            vs = from2d + (delta2d * segstart);
            ve = from2d + (delta2d * segend);

            // Make random width and offset for next corner
            soffset = ((float)General.random.NextDouble() - 0.5f) * rnd_offset;
            swidth = min_width + (float)General.random.NextDouble() * rnd_width;

            // Last segment?
            if(s == (segments - 1))
            {
                // End vertices on trajectory end
                v3 = to;
                v4 = to;
            }
            else
            {
                // Determine segments end vertices (2D)
                p3 = (ve + (trjnorm * soffset)) + (trjnorm * swidth);
                p4 = (ve + (trjnorm * soffset)) - (trjnorm * swidth);

                // Unproject vertices to 3D space
                v3 = General.arena.Unprojected(p3.ToDx()).FromDx();
                v4 = General.arena.Unprojected(p4.ToDx()).FromDx();
            }

            // Make real vertices
            v.AddRange(Direct3D.MQuadList(v1, 0f, 0f, v2, 0f, 1f, v3, 1f, 0f, v4, 1f, 1f));

            // Copy values for next segment
            segstart = segend;
            v1 = v3;
            v2 = v4;
        }
    }

    // Disposer
    public override void Dispose()
    {
        // Clean up
        verts = null;
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
            // Change fade
            fade += fadechange;

            // Dispose me when disappeared
            if(fade <= 0f) this.Dispose();
        }
    }

    #endregion

    #region ================== Rendering

    // Rendering
    public override void Render()
    {
        // Set render mode
        Direct3D.SetDrawMode(DRAWMODE.NADDITIVEALPHA);
        Direct3D.d3dd.SetRenderState(RenderState.ZWriteEnable, false);
        if(fade > 1f)
            Direct3D.d3dd.SetRenderState(RenderState.TextureFactor, General.ARGB(1f, 1f, 1f, 1f));
        else
            Direct3D.d3dd.SetRenderState(RenderState.TextureFactor, General.ARGB(fade, 1f, 1f, 1f));

        // Set textures
        Direct3D.d3dd.SetTexture(0, texture.texture);
        Direct3D.d3dd.SetTexture(1, null);

        // Set matrices
        Direct3D.d3dd.SetTransform(TransformState.World, Matrix.Identity);
        Direct3D.d3dd.SetTransform(TransformState.Texture0, Matrix.Identity);

        // Render the shock
        Direct3D.d3dd.DrawUserPrimitives(PrimitiveType.TriangleList, faces, verts);
    }

    #endregion
}
