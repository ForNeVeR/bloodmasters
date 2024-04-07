/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using Bloodmasters.Client.Graphics;
using Bloodmasters.Client.Lights;
using SharpDX.Direct3D9;
using Direct3D = Bloodmasters.Client.Graphics.Direct3D;
using Graphics_Sprite = Bloodmasters.Client.Graphics.Sprite;
using Sprite = Bloodmasters.Client.Graphics.Sprite;

namespace Bloodmasters.Client.Effects;

public class RageEffect : VisualObject
{
    #region ================== Constants

    private const float ALPHA = 0.5f;
    private const float OFFSET_Z = 5f;

    #endregion

    #region ================== Variables

    private DynamicLight light;
    private Actor actor;
    private Graphics_Sprite sprite;
    private Animation ani;
    private ISound sound;
    private bool disposed = false;

    #endregion

    #region ================== Properties

    public int LightColor { get { return light.Color; } set { light.Color = value; } }

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public RageEffect(Actor actor)
    {
        // Set members
        this.actor = actor;
        this.renderbias = 0f;

        // Move with actor
        this.pos = actor.Position;

        // Get the sound
        sound = SoundSystem.GetSound("rage.wav", true);
        sound.Position = actor.Position;
        sound.Play(true);

        // Make light
        light = new DynamicLight(actor.Position, 12f, General.ARGB(1f, 1f, 0.2f, 0.1f), 2);

        // Make the sprite
        sprite = new Graphics_Sprite(this.pos, 12f, false, true);
        sprite.Update();

        // Create animation
        ani = Animation.CreateFrom("sprites/rage.cfg");
    }

    // Disposer
    public override void Dispose()
    {
        // Clean up
        if(sound != null) sound.Dispose();
        sound = null;
        if(light != null) light.Dispose();
        light = null;
        actor = null;
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
            // Move object to match actor position
            this.pos = actor.Position + new Vector3D(0f, 0f, OFFSET_Z);
            sprite.Position = this.Position;
            light.Position = this.Position;
            sound.Position = this.Position;

            // Process animation
            ani.Process();

            // Dispose when end of animation
            if(ani.Ended) this.Dispose();
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
            Direct3D.d3dd.SetRenderState(RenderState.TextureFactor, General.ARGB(ALPHA, 1f, 1f, 1f));

            // No lightmap
            Direct3D.d3dd.SetTexture(1, null);

            // Set texture
            Direct3D.d3dd.SetTexture(0, ani.CurrentFrame.texture);

            // Render sprite
            sprite.Render();
        }
    }

    #endregion
}
