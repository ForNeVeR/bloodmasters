/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using CodeImp.Bloodmasters.Client.Effects;
using CodeImp.Bloodmasters.Client.Graphics;
using CodeImp.Bloodmasters.Client.Items;
using CodeImp.Bloodmasters.Client.LevelMap;
using CodeImp.Bloodmasters.Client.Lights;
using CodeImp.Bloodmasters.Client.Resources;
using SharpDX.Direct3D9;
using Direct3D = CodeImp.Bloodmasters.Client.Graphics.Direct3D;
using Sprite = CodeImp.Bloodmasters.Client.Graphics.Sprite;

namespace CodeImp.Bloodmasters.Client.Projectiles;

[ProjectileInfo(PROJECTILE.ROCKET)]
public class Rocket : Projectile
{
    #region ================== Constants

    private const float SPRITE_BODY_SIZE = 2.5f;
    private const float SPRITE_EXHAUST_SIZE = 1.6f;
    private const int SMOKE_INTERVAL = 20;

    #endregion

    #region ================== Variables

    // Static components
    public static TextureResource texbody;
    public static TextureResource texexhaust;

    // Members
    private Sprite spritebody;
    private Sprite spriteexhaust;
    private ISound flying;
    private int smoketime;
    private Vector3D exoffset;
    private DynamicLight light;

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public Rocket(string id, Vector3D start, Vector3D vel) : base(id, start, vel)
    {
        // Copy properties
        state.pos = start;
        state.vel = vel;

        // Set initial smoke time
        smoketime = SharedGeneral.currenttime - 1;

        // Make the rocket sprites
        spritebody = new Sprite(start, SPRITE_BODY_SIZE, false, true);
        spriteexhaust = new Sprite(start, SPRITE_EXHAUST_SIZE, false, true);
        UpdateSprites();

        // Make the light
        light = new DynamicLight(start, 15f, General.ARGB(0.2f, 1f, 0.9f, 0.6f), 3);

        // Create flying sound
        flying = SoundSystem.GetSound("rocketfly.wav", true);
        flying.Position = start;
        flying.Play(true);
    }

    // Dispose
    public override void Dispose()
    {
        // Clean up
        flying.Dispose();
        spritebody = null;
        spriteexhaust = null;
        flying = null;
        light.Dispose();

        // Dispose base
        base.Dispose();
    }

    #endregion

    #region ================== Methods

    // This updates the sprites for the velocity
    private void UpdateSprites()
    {
        Vector2D normal;
        float rotangle;

        // Calculate sprite rotation angle
        normal = state.vel;
        normal.Normalize();
        rotangle = (float)Math.Atan2(-normal.y, normal.x) + (float)Math.PI * 0.25f;
        spritebody.Rotation = rotangle;
        spriteexhaust.Rotation = rotangle;

        // Calculate exhaust offset
        exoffset = (Vector2D)state.vel;
        exoffset.MakeLength(1f);
        exoffset.x -= 0.2f;
        exoffset.y -= -0.2f;
        exoffset.z = -1f;

        // Update sprites
        spritebody.Update();
        spriteexhaust.Update();
    }

    // When teleported
    public override void TeleportTo(Vector3D oldpos, Vector3D newpos, Vector3D newvel)
    {
        // Teleport base class
        base.TeleportTo(oldpos, newpos, newvel);

        // Update sprites
        UpdateSprites();
    }

    // When updated
    public override void Update(Vector3D newpos, Vector3D newvel)
    {
        // Update base class
        base.Update(newpos, newvel);

        // Update sprites
        UpdateSprites();
    }

    // When destroyed
    public override void Destroy(Vector3D atpos, bool silent, Client hitplayer)
    {
        Vector3D decalpos = atpos;

        // Where are we now?
        ClientSector sector = (ClientSector)General.map.GetSubSectorAt(state.pos.x, state.pos.y).Sector;

        // Not silent?
        if((silent == false) && (sector != null))
        {
            // Hitting a player?
            if(hitplayer != null)
            {
                // Player is not carrying a shield?
                if(hitplayer.Powerup != POWERUP.SHIELDS)
                {
                    // Check if on screen
                    if(sector.VisualSector.InScreen)
                    {
                        // Create particles
                        for(int i = 0; i < 5; i++)
                            General.arena.p_blood.Add(atpos, state.vel * 0.04f, General.ARGB(1f, 1f, 0.0f, 0.0f));
                    }

                    // Floor decal
                    if((sector != null) && (sector.Material != (int)SECTORMATERIAL.LIQUID))
                        FloorDecal.Spawn(sector, state.pos.x, state.pos.y, FloorDecal.blooddecals, false, true, false);

                    // Create wall decal
                    WallDecal.Spawn(state.pos.x, state.pos.y, state.pos.z + (float)General.random.NextDouble() * 10f - 6f, Consts.PLAYER_DIAMETER, WallDecal.blooddecals, false);
                }
            }
            else
            {
                // Track back a little
                decalpos = atpos - this.state.vel * 2f;

                // Near the floor or ceiling?
                if(((decalpos.z - sector.CurrentFloor) < 2f) &&
                   ((decalpos.z - sector.CurrentFloor) > -2f))
                {
                    // Spawn mark on the floor
                    if((sector != null) && (sector.Material != (int)SECTORMATERIAL.LIQUID))
                        FloorDecal.Spawn(sector, decalpos.x, decalpos.y, FloorDecal.explodedecals, false, false, false);
                }
                else
                {
                    // Spawn mark on the wall
                    WallDecal.Spawn(decalpos.x, decalpos.y, decalpos.z, 2f, WallDecal.explodedecals, false);
                }
            }

            // Kill flying sound
            flying.Stop();

            // Make hit sound
            if(sector.VisualSector.InScreen)
                SoundSystem.PlaySound("rockethit.wav", atpos);

            // Spawn explosion effect
            new RocketExplodeEffect(decalpos);
        }
        // Silent destroy
        else if(sector != null)
        {
            // In a liquid sector?
            if((SECTORMATERIAL)sector.Material == SECTORMATERIAL.LIQUID)
            {
                // Make splash sound
                if(sector.VisualSector.InScreen)
                    SoundSystem.PlaySound("dropwater.wav", atpos);

                // Check if on screen
                if(sector.VisualSector.InScreen)
                {
                    // Determine type of splash to make
                    switch(sector.LiquidType)
                    {
                        case LIQUID.WATER: FloodedSector.SpawnWaterParticles(atpos, new Vector3D(0f, 0f, 0.5f), 10); break;
                        case LIQUID.LAVA: FloodedSector.SpawnLavaParticles(atpos, new Vector3D(0f, 0f, 0.5f), 10); break;
                    }
                }
            }
        }

        // Destroy base
        base.Destroy(atpos, silent, hitplayer);
    }

    // Process the projectile
    public override void Process()
    {
        // Process base object
        base.Process();

        // Position sprites
        spritebody.Position = state.pos;
        spriteexhaust.Position = state.pos - exoffset;

        // Position light
        light.Position = this.state.pos;

        // Time to spawn smoke?
        if((smoketime < SharedGeneral.currenttime) && this.InScreen)
        {
            // Make smoke
            Vector3D smokepos = state.pos + new Vector3D(0f, 0f, -5f);
            Vector3D smokevel = state.vel * 0.2f + Vector3D.Random(General.random, 0.02f, 0.02f, 0f);
            General.arena.p_trail.Add(smokepos, smokevel, General.ARGB(1f, 0.5f, 0.5f, 0.5f), 1, 200);
            smoketime += SMOKE_INTERVAL;
        }

        // Update sound coordinates
        flying.Position = state.pos;
    }

    // Render the projectile
    public override void Render()
    {
        // Check if in screen
        if(this.InScreen)
        {
            // Set render mode
            Direct3D.SetDrawMode(DRAWMODE.NALPHA);
            Direct3D.d3dd.SetRenderState(RenderState.TextureFactor, -1);
            Direct3D.d3dd.SetRenderState(RenderState.ZEnable, true);

            // No lightmap
            Direct3D.d3dd.SetTexture(1, null);

            // Texture
            Direct3D.d3dd.SetTexture(0, Rocket.texbody.texture);

            // Render body
            spritebody.Render();

            // Set render mode
            Direct3D.SetDrawMode(DRAWMODE.NADDITIVEALPHA);
            Direct3D.d3dd.SetRenderState(RenderState.TextureFactor, -1);
            Direct3D.d3dd.SetRenderState(RenderState.ZEnable, true);

            // Texture
            Direct3D.d3dd.SetTexture(0, Rocket.texexhaust.texture);

            // Render exhaust
            spriteexhaust.Render();
        }
    }

    #endregion
}
