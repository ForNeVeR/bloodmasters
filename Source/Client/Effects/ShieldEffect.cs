/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using Bloodmasters.Client.Graphics;
using Bloodmasters.Client.Lights;
using Bloodmasters.Client.Resources;
using SharpDX.Direct3D9;
using Direct3D = Bloodmasters.Client.Graphics.Direct3D;

namespace Bloodmasters.Client.Effects;

public class ShieldEffect : VisualObject
{
    #region ================== Constants

    private const float ALPHA_START = 2f;
    private const float LIGHT_DISTANCE = 2f;

    #endregion

    #region ================== Variables

    // Shield image
    public static TextureResource shieldimage;

    // Members
    private Vector3D offset = new Vector3D(1f, -1f, 12f);
    private DynamicLight light;
    private Actor actor;
    private Graphics.Sprite sprite;
    private bool disposed = false;
    private float alpha;
    private readonly float angle;
    private readonly float fadeout;
    private readonly int lightcolor;

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public ShieldEffect(Actor actor, float angle, float fadeout)
    {
        // Set members
        this.actor = actor;
        this.angle = angle;
        this.renderbias = 3f;
        this.pos = actor.Position;
        this.alpha = ALPHA_START;
        this.fadeout = fadeout;
        this.lightcolor = General.ARGB(1f, 0.2f, 0.5f, 0.1f);

        // Play the shield hit sound when in screen
        if (actor.Sector.VisualSector.InScreen)
            SoundSystem.PlaySound("shieldhit.wav", actor.Position);

        // Make dynamic light
        light = new DynamicLight(this.pos, 16f, lightcolor, 3);

        // Make the sprite
        sprite = new Graphics.Sprite(this.pos + offset, 6f, false, true);
        sprite.RotateX = (float)Math.PI * 0.7f;
        sprite.Rotation = angle - (float)Math.PI * 0.35f;
        sprite.Update();

        // Process once
        this.Process();
    }

    // Disposer
    public override void Dispose()
    {
        // Clean up
        light.Dispose();
        light = null;
        actor = null;
        sprite = null;
        disposed = true;
        base.Dispose();
        GC.SuppressFinalize(this);
    }

    #endregion

    #region ================== Processing

    // Processing
    public override void Process()
    {
        Vector3D lightpos;

        // Not disposed?
        if(!disposed)
        {
            // Actor dead or disposed?
            if(actor.IsDead || actor.Disposed)
            {
                // Dispose me as well
                this.Dispose();
            }
            else
            {
                // Move object to match actor position
                this.pos = actor.Position;
                sprite.Position = actor.Position + offset;

                // Move light to match actor position
                lightpos = this.pos + Vector3D.FromMapAngle(angle + (float)Math.PI * 0.5f, LIGHT_DISTANCE);
                light.Position = lightpos;
                if(alpha > 1f)
                    light.Color = lightcolor;
                else
                    light.Color = ColorOperator.Scale(lightcolor, alpha);

                // Fade the alpha
                alpha -= fadeout;
                if(alpha <= 0f) this.Dispose();
            }
        }
    }

    #endregion

    #region ================== Rendering

    // Rendering
    public override void Render()
    {
        // Check if in screen
        if(actor.Sector.VisualSector.InScreen && !disposed)
        {
            // Set render mode
            Direct3D.SetDrawMode(DRAWMODE.NADDITIVEALPHA);
            if(alpha > 1f)
                Direct3D.d3dd.SetRenderState(RenderState.TextureFactor, -1);
            else
                Direct3D.d3dd.SetRenderState(RenderState.TextureFactor, General.ARGB(alpha, 1f, 1f, 1f));

            // No lightmap
            Direct3D.d3dd.SetTexture(1, null);

            // Set shield texture
            Direct3D.d3dd.SetTexture(0, shieldimage.texture);

            // Render sprite
            sprite.Render();
        }
    }

    #endregion
}
