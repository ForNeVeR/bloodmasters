/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using Bloodmasters.Client.Effects;
using Bloodmasters.Client.Graphics;
using Bloodmasters.Client.Items;
using Bloodmasters.Client.LevelMap;
using Bloodmasters.Client.Resources;
using Bloodmasters.Client.Sound;
using SharpDX.Direct3D9;
using Direct3D = Bloodmasters.Client.Graphics.Direct3D;

namespace Bloodmasters.Client.Projectiles;

[ProjectileInfo(PROJECTILE.GRENADE)]
public class Grenade : Projectile
{
    #region ================== Constants

    private const float SPRITE_BODY_SIZE = 3f;
    private const int SMOKE_INTERVAL = 30;

    #endregion

    #region ================== Variables

    // Static components
    public static TextureResource texbody;

    // Members
    private Graphics.Sprite spritebody;
    private int smoketime;
    private float rotation;
    private ClientSector sector;

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public Grenade(string id, Vector3D start, Vector3D vel) : base(id, start, vel)
    {
        // Copy properties
        state.pos = start;
        state.vel = vel;

        // Set initial smoke time
        smoketime = SharedGeneral.currenttime - 1;

        // Make the rocket sprites
        spritebody = new Graphics.Sprite(start, SPRITE_BODY_SIZE, true, true);
        UpdateSprites();
    }

    // Dispose
    public override void Dispose()
    {
        // Clean up
        spritebody = null;

        // Dispose base
        base.Dispose();
    }

    #endregion

    #region ================== Methods

    // This updates the sprites for the velocity
    private void UpdateSprites()
    {
        // Rotate sprite
        rotation += state.vel.Length() * 0.3f;
        if (rotation > ((float)Math.PI * 2f)) rotation -= ((float)Math.PI * 2f);
        spritebody.Rotation = rotation;

        // Update sprites
        spritebody.Update();
    }

    // When updated
    public override void Update(Vector3D newpos, Vector3D newvel)
    {
        // Update base class
        base.Update(newpos, newvel);

        // Make bounce sound
        if ((sector != null) && sector.VisualSector.InScreen)
            SoundSystem.PlaySound("grenadebounce.wav", newpos);
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
                    // Create particles
                    for (int i = 0; i < 5; i++)
                        General.arena.p_blood.Add(atpos, state.vel * 0.04f, General.ARGB(1f, 1f, 0.0f, 0.0f));

                    // Floor decal
                    if ((sector != null) && (sector.Material != (int)SECTORMATERIAL.LIQUID))
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
                if (((decalpos.z - sector.CurrentFloor) < 2f) &&
                   ((decalpos.z - sector.CurrentFloor) > -2f))
                {
                    // Spawn mark on the floor
                    if ((sector != null) && (sector.Material != (int)SECTORMATERIAL.LIQUID))
                        FloorDecal.Spawn(sector, decalpos.x, decalpos.y, FloorDecal.explodedecals, false, false, false);
                }
                else
                {
                    // Spawn mark on the wall
                    WallDecal.Spawn(decalpos.x, decalpos.y, decalpos.z, 2f, WallDecal.explodedecals, false);
                }
            }

            // Make hit sound
            if (sector.VisualSector.InScreen)
                SoundSystem.PlaySound("rockethit.wav", atpos);

            // Spawn explosion effect
            new RocketExplodeEffect(decalpos);
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
                FloodedSector.SpawnWaterParticles(atpos, new Vector3D(0f, 0f, 0.5f), 10);
                break;
            case LIQUID.LAVA:
                FloodedSector.SpawnLavaParticles(atpos, new Vector3D(0f, 0f, 0.5f), 10);
                break;
        }
    }

    // Process the projectile
    public override void Process()
    {
        // Process base object
        base.Process();

        // Where are we now?
        sector = (ClientSector)General.map.GetSubSectorAt(state.pos.x, state.pos.y).Sector;

        // Process physics
        if (state.pos.z > (sector.CurrentFloor + 0.2f))
        {
            state.vel.z -= Consts.GRENADE_GRAVITY;
            state.vel.x /= 1f + Consts.GRENADE_DECELERATE_AIR;
            state.vel.y /= 1f + Consts.GRENADE_DECELERATE_AIR;
        }
        else
        {
            if (state.vel.z < -0.00000001f) state.vel.z = 0f;
            state.vel.x /= 1f + Consts.GRENADE_DECELERATE_FLOOR;
            state.vel.y /= 1f + Consts.GRENADE_DECELERATE_FLOOR;
        }

        // Position sprite
        spritebody.Position = state.pos + new Vector3D(0f, 0f, 1f);
        UpdateSprites();

        // Time to spawn smoke?
        if ((smoketime < SharedGeneral.currenttime) && (state.vel.Length() > 0.5f) && sector.VisualSector.InScreen)
        {
            // Make smoke
            Vector3D smokepos = state.pos + new Vector3D(0f, 0f, 0.1f);
            Vector3D smokevel = state.vel * 0.1f + Vector3D.Random(General.random, 0.02f, 0.02f, 0f);
            General.arena.p_trail.Add(smokepos, smokevel, General.ARGB(1f, 0.5f, 0.5f, 0.5f), 1, 200);
            smoketime += SMOKE_INTERVAL;
        }
    }

    // This renders the shadow
    public override void RenderShadow()
    {
        // Check if in screen
        if (!this.InScreen)
            return;

        // Render the shadow
        Shadow.RenderAt(pos.x, pos.y, sector.CurrentFloor, 1.6f,
            Shadow.AlphaAtHeight(sector.CurrentFloor, pos.z) * 0.5f);
    }

    // Render the projectile
    public override void Render()
    {
        // Check if in screen
        if (!this.InScreen)
            return;

        // Set render mode
        Direct3D.SetDrawMode(DRAWMODE.NLIGHTMAPALPHA);
        Direct3D.d3dd.SetRenderState(RenderState.TextureFactor, -1);
        Direct3D.d3dd.SetRenderState(RenderState.ZEnable, true);

        // Set lightmap
        Direct3D.d3dd.SetTexture(1, sector.VisualSector.Lightmap);

        // Texture
        Direct3D.d3dd.SetTexture(0, Grenade.texbody.texture);

        // Render body
        spritebody.Render();
    }

    #endregion
}
