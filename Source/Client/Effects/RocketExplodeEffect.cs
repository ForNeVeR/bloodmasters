/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using Bloodmasters.Client.Graphics;
using Bloodmasters.Client.LevelMap;
using Bloodmasters.Client.Lights;
using SharpDX.Direct3D9;
using Direct3D = Bloodmasters.Client.Graphics.Direct3D;
using Graphics_Sprite = Bloodmasters.Client.Graphics.Sprite;
using Sprite = Bloodmasters.Client.Graphics.Sprite;

namespace Bloodmasters.Client.Effects;

public class RocketExplodeEffect : VisualObject
{
    #region ================== Constants

    #endregion

    #region ================== Variables

    private Graphics_Sprite sprite;
    private Animation ani;
    private readonly ClientSector sector;
    private bool disposed;

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public RocketExplodeEffect(Vector3D spawnpos)
    {
        // Position
        this.pos = spawnpos;
        this.renderbias = 50f;
        this.renderpass = 2;

        // Determine current sector
        sector = (ClientSector)General.map.GetSubSectorAt(pos.x, pos.y).Sector;

        // Spawn the light
        if(DynamicLight.dynamiclights)
            new RocketExplodeLight(spawnpos);

        // Only when in the screen
        if(sector.VisualSector.InScreen)
        {
            // Spawn particles
            for(int i = 0; i < 12; i++)
                General.arena.p_magic.Add(spawnpos + Vector3D.Random(General.random, 4f, 4f, 2f), Vector3D.Random(General.random, 0.2f, 0.2f, 0.2f), General.ARGB(1f, 1f, 1f, 0.2f));
            for(int i = 0; i < 12; i++)
                General.arena.p_magic.Add(spawnpos + Vector3D.Random(General.random, 4f, 4f, 2f), Vector3D.Random(General.random, 0.2f, 0.2f, 0.2f), General.ARGB(1f, 1f, 0.6f, 0.2f));
            for(int i = 0; i < 30; i++)
                General.arena.p_smoke.Add(spawnpos + Vector3D.Random(General.random, 7f, 7f, 5f), Vector3D.Random(General.random, 0.04f, 0.04f, 0.1f), General.ARGB(1f, 0.5f, 0.5f, 0.5f));
        }

        // Make effect
        sprite = new Graphics_Sprite(spawnpos + new Vector3D(2f, -2f, 15f), 10f, false, true);
        ani = Animation.CreateFrom("sprites/rocketexplode.cfg");
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
                SharpDX.Direct3D9.Direct3D.SetDrawMode(DRAWMODE.NADDITIVEALPHA);
                SharpDX.Direct3D9.Direct3D.d3dd.SetRenderState(RenderState.TextureFactor, -1);
                //Direct3D.d3dd.SetRenderState(RenderState.ZEnable, false);

                // No lightmap
                SharpDX.Direct3D9.Direct3D.d3dd.SetTexture(1, null);

                // Set animation frame
                SharpDX.Direct3D9.Direct3D.d3dd.SetTexture(0, ani.CurrentFrame.texture);

                // Render sprite
                sprite.Render();

                // Restore Z buffer
                //Direct3D.d3dd.SetRenderState(RenderState.ZEnable, true);
            }
        }
    }

    #endregion
}
