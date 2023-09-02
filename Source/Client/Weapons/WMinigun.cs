/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using SharpDX.Direct3D9;

namespace CodeImp.Bloodmasters.Client;

[WeaponInfo(WEAPON.MINIGUN, RefireDelay=50, Description="Minigun",
    Sound="chain2fire.wav", AmmoType=AMMO.BULLETS, UseAmmo=1)]
public class WMinigun : Weapon
{
    #region ================== Constants

    // Fire flare
    private const float FLARE_ALPHA_START = 1f;
    private const float FLARE_ALPHA_CHANGE = -0.1f;
    private const float FLARE_SIZE_START = 6f;
    private const float FLARE_SIZE_CHANGE = -0.1f;
    private const float BULLET_SPREAD = 10f;
    private const int SPINUP_DELAY = 1000;
    private const int SPINDOWN_DELAY = 1000;

    #endregion

    #region ================== Variables

    // Fire flare
    public static TextureResource flaretex;
    private Sprite flare;
    private float flarealpha = 0f;

    // States
    private MINIGUNSTATE state = MINIGUNSTATE.IDLE;
    private int statechangetime = 0;

    // Sounds
    private ISound rotor = null;

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public WMinigun(Client client) : base(client)
    {
        // Make fire flare sprite
        flare = new Sprite(new Vector3D(), FLARE_SIZE_START, false, true);
    }

    // Disposer
    public override void Dispose()
    {
        // Clean up
        if(rotor != null) rotor.Dispose();
        rotor = null;
        flare = null;

        // Dispose base
        base.Dispose();
    }

    #endregion

    #region ================== Methods

    // This changes the rotor sound
    private void ChangeRotorSound(string snd, bool repeat)
    {
        // Determine filename
        string filename = "minigun_" + snd + ".wav";

        // Change the sound?
        if ((rotor == null) || (string.Compare(rotor.Filename, filename, true) != 0))
        {
            // Dispose old sound, if any
            if (rotor != null) rotor.Dispose();

            // Change the sound
            rotor = SoundSystem.GetSound(filename, true);
            if (client.Actor != null) rotor.Position = client.Actor.Position;
            rotor.Play(repeat);
        }
    }

    // This is called when the trigger is pulled
    public override void Trigger()
    {
        // Check if gun is idle
        if((state == MINIGUNSTATE.IDLE) || ((state == MINIGUNSTATE.SPINDOWN) && (statechangetime < SharedGeneral.currenttime)))
        {
            // Go to spin up state
            state = MINIGUNSTATE.SPINUP;
            statechangetime = SharedGeneral.currenttime + SPINUP_DELAY;
            ChangeRotorSound("s", false);
            //client.Actor.PlayShootingAnimation(2, 1);
            return;
        }

        // Check if gun is firing
        if(((state == MINIGUNSTATE.SPINUP) && (statechangetime < SharedGeneral.currenttime)) ||
           (state == MINIGUNSTATE.FIRING))
        {
            // Fire weapon
            state = MINIGUNSTATE.FIRING;
            ChangeRotorSound("r", true);
            base.Trigger();
        }
    }

    // This is called when the trigger is released
    public override void Released()
    {
        // Check if the weapon is spinning
        if((state == MINIGUNSTATE.SPINUP) || (state == MINIGUNSTATE.FIRING))
        {
            // Spin down now
            state = MINIGUNSTATE.SPINDOWN;
            statechangetime = SharedGeneral.currenttime + SPINDOWN_DELAY;
            //client.Actor.PlayShootingAnimation(2, -1);
            ChangeRotorSound("e", false);
        }

        // Check if spinned down
        if((state == MINIGUNSTATE.SPINDOWN) && (statechangetime < SharedGeneral.currenttime))
        {
            // Now idle
            state = MINIGUNSTATE.IDLE;
        }

        // Base class stuff
        base.Released();
    }

    // This is called when the weapon is shooting
    protected override void ShootOnce()
    {
        // Play the shooting sound
        if(client.Actor.Sector.VisualSector.InScreen)
            SoundSystem.PlaySound(sound, client.Actor.Position);

        // Make the actor play the shooting animation
        client.Actor.PlayShootingAnimation(2, -1);

        // Set fire flare
        flarealpha = FLARE_ALPHA_START;
        flare.Size = FLARE_SIZE_START;
        flare.Rotation = (float)General.random.NextDouble() * 2f * (float)Math.PI;

        // Create flash light
        //new FlashLight(GetFlarePosition());

        // Spawn a bullet
        new Bullet(client.Actor, BULLET_SPREAD);
    }

    // This processes the weapon
    public override void Process()
    {
        // Process base class
        base.Process();

        // Rotor sound playing?
        if((rotor != null) && !rotor.Disposed)
        {
            // Rotor sound stopped?
            if(!rotor.Playing)
            {
                // Remove rotor sound
                rotor.Dispose();
                rotor = null;
            }
            else
            {
                // Client actor avilable?
                if(client.Actor != null)
                {
                    // Reposition rotor sound
                    rotor.Position = client.Actor.Position;
                }
            }
        }

        // Process the fire flare
        if(flarealpha > 0f)
        {
            // Position flare
            flare.Position = Weapon.GetFlarePosition(client.Actor);

            // Decrease alpha and size
            flare.Size += FLARE_SIZE_CHANGE;
            flarealpha += FLARE_ALPHA_CHANGE;
            if(flarealpha < 0f) flarealpha = 0f;

            // Update flare
            flare.Update();

            // Update light
            light.Visible = true;
            light.Color = General.ARGB(flarealpha * 0.5f, 1f, 1f, 1f);
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
            Direct3D.d3dd.SetTexture(0, flaretex.texture);
            Direct3D.d3dd.SetTexture(1, null);

            // Render
            flare.Render();
        }
    }

    // This is called to check if the weapon is ready
    public override bool IsIdle()
    {
        // Return if the weapon is idle
        return (state == MINIGUNSTATE.IDLE);
    }

    #endregion
}

// Minigun states
public enum MINIGUNSTATE
{
    IDLE = 0,
    SPINUP = 1,
    FIRING = 2,
    SPINDOWN = 3,
}
