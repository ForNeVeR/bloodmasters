/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using Bloodmasters.Client.Graphics;
using SharpDX.Direct3D9;
using Direct3D = Bloodmasters.Client.Graphics.Direct3D;

namespace Bloodmasters.Client.Weapons;

[WeaponInfo(WEAPON.PHOENIX, RefireDelay=50, Description="Phoenix",
    AmmoType=AMMO.FUEL, UseAmmo=1)]
public class WPhoenix : Weapon
{
    #region ================== Constants

    // Fire flare
    private const float FLARE_ALPHA_START = 1f;
    private const float FLARE_ALPHA_CHANGE = -0.06f;
    private const float FLARE_SIZE_START = 2f;
    private const float FLARE_SIZE_CHANGE = 0.2f;
    private const float FLARE_SIZE_END = 7f;
    private const int FIRE_SOUND_CHANGE_DELAY = 800;

    #endregion

    #region ================== Variables

    // Fire flare
    private Graphics.Sprite flare;
    private readonly Animation flareani;
    private float flarealpha = 0f;

    // States
    private bool firing = false;
    private int firechangetime = 0;

    // Sounds
    private ISound firesound = null;

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public WPhoenix(Client client) : base(client)
    {
        // Make fire flare sprite
        flare = new Graphics.Sprite(new Vector3D(), FLARE_SIZE_START, false, false);
        flareani = Animation.CreateFrom("sprites/phoenixflare.cfg");
    }

    // Disposer
    public override void Dispose()
    {
        // Clean up
        if(firesound != null) firesound.Dispose();
        firesound = null;
        flare = null;

        // Dispose base
        base.Dispose();
    }

    #endregion

    #region ================== Methods

    // This changes the fire sound
    private void ChangeFireSound(string snd, bool repeat)
    {
        // Determine filename
        string filename = "phoenix_" + snd + ".wav";

        // Change the sound?
        if((firesound == null) || (string.Compare(firesound.Filename, filename, true) != 0))
        {
            // Dispose old sound, if any
            if(firesound != null) firesound.Dispose();

            // Change the sound
            firesound = SoundSystem.GetSound(filename, true);
            if(client.Actor != null) firesound.Position = client.Actor.Position;
            firesound.Play(repeat);
        }
    }

    // This is called when the trigger is pulled
    public override void Trigger()
    {
        // Check if gun is idle
        if(!firing)
        {
            // Fire now
            firing = true;
            firechangetime = SharedGeneral.currenttime + FIRE_SOUND_CHANGE_DELAY;
            ChangeFireSound("s", false);
            return;
        }
        else
        {
            // Time to change fire sound?
            if(firechangetime < SharedGeneral.currenttime)
            {
                // Change fire sound
                ChangeFireSound("r", true);
            }
        }

        // Trigger base
        base.Trigger();
    }

    // This is called when the trigger is released
    public override void Released()
    {
        // Done shooting
        if(firing) ChangeFireSound("e", false);
        firing = false;

        // Base class stuff
        base.Released();
    }

    // This is called when the weapon is shooting
    protected override void ShootOnce()
    {
        // Make the actor play the shooting animation
        client.Actor.PlayShootingAnimation(2, -1);
    }

    // This processes the weapon
    public override void Process()
    {
        // Process base class
        base.Process();

        // Fire sound set?
        if(firesound != null)
        {
            // Fire sound stopped?
            if(!firesound.Playing)
            {
                // Remove fire sound
                firesound.Dispose();
                firesound = null;
            }
            else
            {
                // Client actor avilable?
                if(client.Actor != null)
                {
                    // Reposition fire sound
                    firesound.Position = client.Actor.Position;
                }
            }
        }

        // Firing?
        if(firing)
        {
            // Increase the flare
            flarealpha = FLARE_ALPHA_START;
            if(flare.Size < FLARE_SIZE_END) flare.Size += FLARE_SIZE_CHANGE;
            if(flare.Size < FLARE_SIZE_START) flare.Size = FLARE_SIZE_START;
        }
        else if(flarealpha > 0f)
        {
            // Decrease the flare
            flarealpha = 0f;
            flare.Size = FLARE_SIZE_START;
        }

        // Process the fire flare
        if(flarealpha > 0f)
        {
            // Position flare
            flare.Rotation = -client.Actor.AimAngle + (float)Math.PI * 0.75f;
            flare.Position = Weapon.GetFlarePosition(client.Actor);

            // Update flare
            flare.Update();
            flareani.Process();

            // Update light
            light.Visible = true;
            light.Color = General.ARGB(flarealpha * 0.3f, 0.7f, 0.8f, 1f);
        }
        else
        {
            // No light
            light.Visible = false;
        }
    }

    // This renders the weapon
    public override void Render()
    {
        // Render the fire flare
        if(flarealpha > 0f)
        {
            // Set render mode
            Direct3D.SetDrawMode(DRAWMODE.NADDITIVEALPHA);
            Direct3D.d3dd.SetRenderState(RenderState.TextureFactor, General.ARGB(flarealpha, 1f, 1f, 1f));

            // Set the sprite texture
            Direct3D.d3dd.SetTexture(0, flareani.CurrentFrame.texture);
            Direct3D.d3dd.SetTexture(1, null);

            // Render
            flare.Render();
        }
    }

    #endregion
}
