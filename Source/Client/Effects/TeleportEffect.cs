/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using System.Drawing;
using CodeImp.Bloodmasters.Client.Graphics;
using CodeImp.Bloodmasters.Client.LevelMap;
using CodeImp.Bloodmasters.Client.Lights;
using SharpDX.Direct3D9;
using Direct3D = CodeImp.Bloodmasters.Client.Graphics.Direct3D;
using Sprite = CodeImp.Bloodmasters.Client.Graphics.Sprite;

namespace CodeImp.Bloodmasters.Client.Effects;

public class TeleportEffect : VisualObject
{
    #region ================== Constants

    #endregion

    #region ================== Variables

    private Sprite sprite;
    private Animation ani;
    private ClientSector sector;
    private bool disposed;
    private int teamcolor;

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public TeleportEffect(Vector3D spawnpos, TEAM team, bool small)
    {
        int smokeamount;
        float spritescale;
        float animationscale;
        float smokerange;

        // Small version?
        if(small)
        {
            // Small version
            smokeamount = 2;
            smokerange = 1f;
            spritescale = 4f;
            animationscale = 0.2f;
        }
        else
        {
            // Large version
            smokeamount = 10;
            smokerange = 5f;
            spritescale = 9f;
            animationscale = 1f;
        }

        // Position
        this.pos = spawnpos;
        this.renderbias = 50f;
        this.renderpass = 2;

        // Determine current sector
        sector = (ClientSector)General.map.GetSubSectorAt(pos.x, pos.y).Sector;

        // Spawn the light
        if(small)
        {
            // Little dynamic light
            new TeleportSmallLight(spawnpos);
        }
        else
        {
            // Big static light
            new TeleportLight(spawnpos);
        }

        // Determines team color
        switch(team)
        {
            case TEAM.NONE: teamcolor = General.ARGB(1f, 1f, 1f, 1f); break;
            case TEAM.RED: teamcolor = General.ARGB(1f, 1f, 0.4f, 0.4f); break;
            case TEAM.BLUE: teamcolor = General.ARGB(1f, 0.4f, 0.5f, 1f); break;
            default: teamcolor = Color.White.ToArgb(); break;
        }

        // Spawn smoke particles
        for(int i = 0; i < smokeamount; i++)
            General.arena.p_smoke.Add(spawnpos + Vector3D.Random(General.random, smokerange, smokerange, smokerange),
                Vector3D.Random(General.random, 0.02f, 0.02f, 0.01f), General.ARGB(1f, 0.6f, 0.6f, 0.6f));

        // Make effect
        sprite = new Sprite(spawnpos + new Vector3D(0f, 0f, 3f), spritescale, false, true);
        ani = Animation.CreateFrom("sprites/teleport.cfg");
        ani.FrameTime = (int)((float)ani.FrameTime * animationscale);
    }

    // Disposer
    public override void Dispose()
    {
        // Clean up
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
            // Process animation
            ani.Process();

            // Dispose me when animation has ended
            if(ani.Ended) this.Dispose();
        }
    }

    #endregion

    #region ================== Rendering

    // Rendering
    public override void Render()
    {
        // Within the map and not disposed?
        if((sector != null) && !disposed)
        {
            // Check if in screen
            if(sector.VisualSector.InScreen)
            {
                // Set render mode
                Direct3D.SetDrawMode(DRAWMODE.NADDITIVEALPHA);
                Direct3D.d3dd.SetRenderState(RenderState.TextureFactor, teamcolor);
                Direct3D.d3dd.SetRenderState(RenderState.ZEnable, false);

                // No lightmap
                Direct3D.d3dd.SetTexture(1, null);

                // Set animation frame
                Direct3D.d3dd.SetTexture(0, ani.CurrentFrame.texture);

                // Render sprite
                sprite.Render();

                // Restore Z buffer
                Direct3D.d3dd.SetRenderState(RenderState.ZEnable, true);
            }
        }
    }

    #endregion
}
