/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using Bloodmasters.Client.Graphics;
using Bloodmasters.Client.Items;
using Bloodmasters.Client.LevelMap;
using Bloodmasters.Client.Lights;
using Bloodmasters.Client.Resources;
using SharpDX.Direct3D9;
using Direct3D = Bloodmasters.Client.Graphics.Direct3D;
using Graphics_Sprite = Bloodmasters.Client.Graphics.Sprite;
using Sprite = Bloodmasters.Client.Graphics.Sprite;

namespace Bloodmasters.Client.Projectiles;

[ProjectileInfo(PROJECTILE.PLASMABALL)]
public class PlasmaBall : Projectile
{
    #region ================== Constants

    private const float SPRITE_SIZE = 3f;

    #endregion

    #region ================== Variables

    // Static components
    public static TextureResource plasmaball;

    // Members
    private Graphics_Sprite sprite;
    private ISound flying;
    private readonly DynamicLight light;

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public PlasmaBall(string id, Vector3D start, Vector3D vel) : base(id, start, vel)
    {
        // Copy properties
        state.pos = start;
        state.vel = vel;

        // Make the ball sprite
        sprite = new Graphics_Sprite(start, SPRITE_SIZE, false, true);
        UpdateSprite();

        // Make the light
        light = new DynamicLight(start, 10f, General.ARGB(0.3f, 0.4f, 0.8f, 1f), 3);

        // Create flying sound
        flying = SoundSystem.GetSound("plasmafly.wav", true);
        flying.Position = start;
        flying.Play(true);
    }

    // Dispose
    public override void Dispose()
    {
        // Clean up
        flying.Dispose();
        sprite = null;
        flying = null;
        light.Dispose();

        // Dispose base
        base.Dispose();
    }

    #endregion

    #region ================== Methods

    // This updates the sprite for the velocity
    private void UpdateSprite()
    {
        Vector2D normal;
        float rotangle;

        // Calculate sprite rotation angle
        normal = state.vel;
        normal.Normalize();
        rotangle = (float)Math.Atan2(-normal.y, normal.x) + (float)Math.PI * 0.25f;
        sprite.Rotation = rotangle;

        // Update sprite
        sprite.Update();
    }

    // When teleported
    public override void TeleportTo(Vector3D oldpos, Vector3D newpos, Vector3D newvel)
    {
        // Teleport base class
        base.TeleportTo(oldpos, newpos, newvel);

        // Update sprites
        UpdateSprite();
    }

    // When updated
    public override void Update(Vector3D newpos, Vector3D newvel)
    {
        // Update base class
        base.Update(newpos, newvel);

        // Update sprites
        UpdateSprite();
    }

    // When destroyed
    public override void Destroy(Vector3D atpos, bool silent, Client hitplayer)
    {
        Vector3D decalpos = atpos;

        // Where are we now?
        ClientSector sector = (ClientSector)General.map.GetSubSectorAt(state.pos.x, state.pos.y).Sector;

        // Not silent?
        if ((silent == false) && (sector != null))
        {
            // Hitting a player?
            if (hitplayer != null)
            {
                // Player is not carrying a shield?
                if (hitplayer.Powerup != POWERUP.SHIELDS)
                {
                    // Check if on screen
                    if (sector.VisualSector.InScreen)
                    {
                        // Create particles
                        for (int i = 0; i < 2; i++)
                            General.arena.p_blood.Add(atpos, state.vel * 0.04f, General.ARGB(1f, 1f, 0.0f, 0.0f));
                    }

                    // Floor decal
                    if ((sector != null) && (sector.Material != (int)SECTORMATERIAL.LIQUID) && (General.random.Next(100) < 30))
                        FloorDecal.Spawn(sector, state.pos.x, state.pos.y, FloorDecal.blooddecals, false, true, false);

                    // Create wall decal
                    if (General.random.Next(100) < 50)
                        WallDecal.Spawn(state.pos.x, state.pos.y, state.pos.z + (float)General.random.NextDouble() * 10f - 6f, Consts.PLAYER_DIAMETER, WallDecal.blooddecals, false);
                }
            }
            else
            {
                // Track back a little
                decalpos = atpos - this.state.vel;

                // Near the floor?
                if (((decalpos.z - sector.CurrentFloor) < 2f) &&
                   ((decalpos.z - sector.CurrentFloor) > -2f))
                {
                    // Spawn mark on the floor
                    if ((sector != null) && (sector.Material != (int)SECTORMATERIAL.LIQUID))
                        FloorDecal.Spawn(sector, decalpos.x, decalpos.y, FloorDecal.plasmadecals, false, false, false);
                }
                else
                {
                    // Spawn mark on the wall
                    WallDecal.Spawn(decalpos.x, decalpos.y, decalpos.z, 2f, WallDecal.plasmadecals, false);
                }
            }

            // Kill flying sound
            flying.Stop();

            // Make hit sound
            if (sector.VisualSector.InScreen)
                SoundSystem.PlaySound("plasmahit.wav", atpos);

            // Check if on screen
            if (sector.VisualSector.InScreen)
            {
                // Spawn particles
                for (int i = 0; i < 3; i++)
                    General.arena.p_magic.Add(decalpos + Vector3D.Random(General.random, 1f, 1f, 1f),
                        Vector3D.Random(General.random, 0.1f, 0.1f, 0.2f), General.ARGB(1f, 0f, 0.6f, 1f));
                for (int i = 0; i < 2; i++)
                    General.arena.p_magic.Add(decalpos + Vector3D.Random(General.random, 1f, 1f, 1f),
                        Vector3D.Random(General.random, 0.1f, 0.1f, 0.2f), -1);
            }
        }
        // Silent destroy
        else if (sector != null)
        {
            HandleSilentDestroy(atpos, sector);
        }

        // Destroy base
        base.Destroy(atpos, silent, hitplayer);
    }

    private static void HandleSilentDestroy(Vector3D atpos, ClientSector sector)
    {
        // In a liquid sector?
        if ((SECTORMATERIAL)sector.Material != SECTORMATERIAL.LIQUID)
            return;

        // Make splash sound
        if (sector.VisualSector.InScreen)
            SoundSystem.PlaySound("dropwater.wav", atpos);

        // Check if on screen
        if (!sector.VisualSector.InScreen)
            return;

        // Determine type of splash to make
        switch (sector.LiquidType)
        {
            case LIQUID.WATER:
                FloodedSector.SpawnWaterParticles(atpos, new Vector3D(0f, 0f, 0.5f), 3);
                break;
            case LIQUID.LAVA:
                FloodedSector.SpawnLavaParticles(atpos, new Vector3D(0f, 0f, 0.5f), 3);
                break;
        }
    }

    // Process the projectile
    public override void Process()
    {
        // Process base object
        base.Process();

        // Position sprite
        sprite.Position = this.state.pos;

        // Position light
        light.Position = this.state.pos;

        // Update sound coordinates
        flying.Position = state.pos;
    }

    // Render the projectile
    public override void Render()
    {
        // Check if in screen
        if (!this.InScreen)
            return;

        // Set render mode
        SharpDX.Direct3D9.Direct3D.SetDrawMode(DRAWMODE.NADDITIVEALPHA);
        SharpDX.Direct3D9.Direct3D.d3dd.SetRenderState(RenderState.TextureFactor, -1);
        SharpDX.Direct3D9.Direct3D.d3dd.SetRenderState(RenderState.ZEnable, true);

        // No lightmap
        SharpDX.Direct3D9.Direct3D.d3dd.SetTexture(1, null);

        // Texture
        SharpDX.Direct3D9.Direct3D.d3dd.SetTexture(0, PlasmaBall.plasmaball.texture);

        // Render sprite
        sprite.Render();
    }

    #endregion
}
