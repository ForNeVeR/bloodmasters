/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using Bloodmasters.Client.LevelMap;
using SharpDX;
using SharpDX.Direct3D9;
using Color = System.Drawing.Color;

namespace Bloodmasters.Client.Graphics;

public class Particle //: VisualObject
{
    #region ================== Constants

    // Sprite angles
    private const float ANGLE_FLOOR_X = (float)Math.PI * -0.5f;
    private const float ANGLE_X = (float)Math.PI * -0.25f;
    private const float ANGLE_Z = (float)Math.PI * 0.25f;

    // Test intervals
    private const int SECTOR_TEST_INTERVAL = 500;
    private const int PLAYER_TEST_INTERVAL = 50;

    // Player intersection settings
    private const float PLAYER_VELOCITY_FACTOR = 0.02f;

    // Decay/fadein time/speed
    public float DECAY_SPEED = 0.02f;
    public float FADEIN_SPEED = 0.05f;

    #endregion

    #region ================== Variables

    // Geometry
    private Matrix spritescalerotate;

    // Position/velocity
    readonly PhysicsState state;
    private float previousz = 0f;
    private readonly float gravity;
    private ParticleCollection collection;
    private ClientSector sector = null;
    private int sectortesttime = 0;
    private int playertesttime = 0;

    // Color/size/fade
    private readonly int basecolor;
    private int color;
    private float size;
    private readonly float resize;
    //private float rotation;

    // Fade
    private int decaytime;
    private float fade;
    private readonly bool fadein;
    private bool disposed = false;

    // Lightmap
    private readonly bool lightmapped;
    private Matrix lightmapoffsets = Matrix.Identity;

    #endregion

    #region ================== Properties

    public bool Disposed { get { return disposed; } }

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public Particle(Vector3D pos, Vector3D force, float gravity, int color,
        float size, float resize, int decaytime, ParticleCollection collection,
        bool mapped, bool fadein)
    {
        // Copy properties
        state = new ClientPhysicsState(General.map);
        state.Bounce = true;
        state.Radius = 0.01f;
        state.Friction = 0.5f;
        state.Redirect = true;
        state.StepUp = false;
        state.pos = pos;
        state.vel = force;
        this.basecolor = color;
        this.color = color;
        this.size = size;
        this.gravity = gravity;
        this.collection = collection;
        this.previousz = pos.z;
        this.lightmapped = mapped;
        this.resize = resize;
        this.fadein = fadein;
        //this.rotation = (float)General.random.NextDouble() * (float)Math.PI * 2f;

        // Timing for particle to fade in and decay
        this.decaytime = SharedGeneral.currenttime + decaytime;
        if(!fadein) this.fade = 1f; else this.fade = 0f;

        // Update particle settings
        Update(true);

        // Random test times
        sectortesttime = SharedGeneral.currenttime + General.random.Next(SECTOR_TEST_INTERVAL);
        playertesttime = SharedGeneral.currenttime + General.random.Next(PLAYER_TEST_INTERVAL);

        // Outside screen? Then dispose
        //if((sector != null) && !sector.VisualSector.InScreen) this.Dispose();
    }

    // Disposer
    //public override void Dispose()
    public void Dispose()
    {
        // Clean up
        state.Dispose();
        sector = null;
        collection = null;
        disposed = true;
        //base.Dispose();
        GC.SuppressFinalize(this);
    }

    #endregion

    #region ================== Methods

    // Processes the particle and disposes it when decayed
    //public override void Process()
    public void Process()
    {
        // Not disposed already?
        if(!disposed)
        {
            // Apply velocity
            state.ApplyVelocity(false);

            // Apply gravity
            state.vel.z -= gravity;

            // Resize
            size += resize;

            // Update particle settings
            Update(false);

            // Time to decay?
            if(decaytime < SharedGeneral.currenttime)
            {
                // Fade out now
                fade -= DECAY_SPEED;

                // Dispose when gone
                if(fade <= 0f)
                {
                    // Bubye!
                    Dispose();
                }
                else
                {
                    // Determine new color
                    color = Color.FromArgb((int)(fade * 255f), Color.FromArgb(basecolor)).ToArgb();
                }
            }
            // Fade in?
            else if(fadein && (fade < 1f))
            {
                // Fade in
                fade += FADEIN_SPEED;
                if(fade > 1f) fade = 1f;

                // Determine new color
                color = Color.FromArgb((int)(fade * 255f), Color.FromArgb(basecolor)).ToArgb();
            }
        }
    }

    // This updates relevant settings
    private void Update(bool updateall)
    {
        Matrix spritescale, spriterotate;

        // Time to test for sector?
        if((sectortesttime < SharedGeneral.currenttime) || (updateall && lightmapped))
        {
            // Find the new sector
            ClientSector newsector = (ClientSector)General.map.GetSubSectorAt(state.pos.x, state.pos.y).Sector;

            // Current sector known?
            if(sector != null)
            {
                // Accept new sector when floor lower than this one
                if(newsector.CurrentFloor <= sector.CurrentFloor) sector = newsector;
            }
            else
            {
                // Accept current sector
                sector = newsector;
            }

            // New sector test time
            sectortesttime += SECTOR_TEST_INTERVAL;
        }

        // Recalculate lightmap positioin
        if(lightmapped || updateall)
        {
            // Get positions on lightmap
            float lx = sector.VisualSector.LightmapScaledX(state.pos.x);
            float ly = sector.VisualSector.LightmapScaledY(state.pos.y);

            // Make the lightmap matrix
            lightmapoffsets = Matrix.Identity;
            lightmapoffsets *= Matrix.Scaling(size * sector.VisualSector.LightmapScaleX, 0f, 1f);
            lightmapoffsets *= Matrix.RotationZ(ANGLE_Z);
            lightmapoffsets *= Matrix.Scaling(1f, sector.VisualSector.LightmapAspect, 1f);
            lightmapoffsets *= Direct3D.MatrixTranslateTx(lx, ly);
        }

        // Recalculate sprite matrix?
        if((Math.Abs(resize) > 0.000001f) || updateall)
        {
            // Scale sprite
            spritescale = Matrix.Scaling(size, 1f, size);

            // Rotate sprite
            //Matrix rot0 = Matrix.RotationY(rotation);
            Matrix rot1 = Matrix.RotationX(ANGLE_X);
            Matrix rot2 = Matrix.RotationZ(ANGLE_Z);
            //spriterotate = Matrix.Multiply(Matrix.Multiply(rot0, rot1), rot2);
            spriterotate = Matrix.Multiply(rot1, rot2);

            // Combine scale and rotation
            spritescalerotate = Matrix.Multiply(spritescale, spriterotate);
        }

        // Time to test for players?
        if(playertesttime < SharedGeneral.currenttime)
        {
            // Go for all actors
            foreach(Actor a in General.arena.Actors)
            {
                if (state.pos.z < a.Position.z) continue;

                // Check if the actor is near this particle
                Vector3D delta = a.Position - state.pos;
                if(delta.Length() < Consts.PLAYER_DIAMETER)
                {
                    // Apply some velocity of the actor to particle
                    state.vel += (Vector3D)((Vector2D)a.State.vel * PLAYER_VELOCITY_FACTOR);
                }
            }

            // New player test time
            playertesttime += PLAYER_TEST_INTERVAL;
        }

        // Within the map?
        if(sector != null)
        {
            // Dont go below the floor
            if(state.pos.z < sector.CurrentFloor)
            {
                // Stay above the floor, decelerate and fade
                if(previousz >= sector.CurrentFloor) state.pos.z = sector.CurrentFloor;
                state.vel.Scale(0.96f);
                decaytime = 0;
            }
        }

        // Apply new position
        //pos = state.pos;
        previousz = state.pos.z;
    }

    // This renders the particle
    //public override void Render()
    public void Render()
    {
        // Within the map?
        if(sector != null)
        {
            // Check if in screen
            if(sector.VisualSector.InScreen)
            {
                // Set render mode
                Direct3D.SetDrawMode(collection.DrawMode);

                // Using the lightmap?
                if(lightmapped)
                {
                    // Set lightmap
                    Direct3D.d3dd.SetTransform(TransformState.Texture1, lightmapoffsets);
                    Direct3D.d3dd.SetTexture(1, sector.VisualSector.Lightmap);
                }
                else
                {
                    // No lightmap
                    Direct3D.d3dd.SetTexture(1, null);
                }

                // Set the item texture and vertices stream
                Direct3D.d3dd.SetTexture(0, collection.Texture.texture);
                Direct3D.d3dd.SetStreamSource(0, Sprite.Vertices, 0, MVertex.Stride);

                // Draw the particle
                DrawParticle(false);
            }
        }
    }

    // This renders the particle
    public void DrawParticle(bool dochecks)
    {
        // Check visibility?
        if(dochecks)
        {
            // Within the map?
            if(sector == null) return;

            // Check if in screen
            if(!sector.VisualSector.InScreen) return;
        }

        // Using the lightmap?
        if(lightmapped)
        {
            // Set lightmap
            Direct3D.d3dd.SetTransform(TransformState.Texture1, lightmapoffsets);
            Direct3D.d3dd.SetTexture(1, sector.VisualSector.Lightmap);
        }

        // Set particle color
        Direct3D.d3dd.SetRenderState(RenderState.TextureFactor, color);

        // Position the particle
        Matrix apos = Matrix.Translation(state.pos.x, state.pos.y, state.pos.z);

        // Apply world matrix
        Direct3D.d3dd.SetTransform(TransformState.World, Matrix.Multiply(spritescalerotate, apos));

        // Render it!
        Direct3D.d3dd.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
    }

    #endregion
}
