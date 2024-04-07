/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using Bloodmasters.Client.Lights;

namespace Bloodmasters.Client.Effects;

public class Lightning
{
    #region ================== Constants

    private const float FADE_CHANGE_BIG = -0.3f;
    private const float FADE_CHANGE_SMALL = -0.5f;
    private const int SHOCK_SPAWN_DELAY = 50;
    private const string SND_FILE_START = "lightning_s.wav";
    private const string SND_FILE_RUN = "lightning_r.wav";
    private const string SND_FILE_END = "lightning_e.wav";
    private const int MAX_LIGHTS = 6;
    private const float LIGHT_DISTANCE = 5f;

    #endregion

    #region ================== Variables

    // Variables
    private ILightningNode source;
    private ILightningNode target;
    private readonly float sourceheight;
    private readonly float targetheight;
    private ISound snd;
    private readonly float fadechange;
    private int shocktime;
    private DynamicLight[] lights = new DynamicLight[MAX_LIGHTS];

    #endregion

    #region ================== Properties

    public ILightningNode Source { get { return source; } }
    public ILightningNode Target { get { return target; } }

    #endregion

    #region ================== Constructor / Dispose

    // Constructor
    public Lightning(ILightningNode source, float sourceheight, ILightningNode target, float targetheight, bool heavy, bool blastsound)
    {
        // Keep the object references
        this.source = source;
        this.target = target;
        this.sourceheight = sourceheight;
        this.targetheight = targetheight;

        // Add this lightning to both objects
        source.AddLightning(this);
        target.AddLightning(this);

        // Set shock time
        shocktime = SharedGeneral.currenttime - 1;

        // Heavy?
        if (heavy)
        {
            // Set up heavy settings
            fadechange = FADE_CHANGE_BIG;

            // Play blast sound
            if (blastsound) SoundSystem.PlaySound(SND_FILE_START, MakeMiddlePosition());
        }
        else
        {
            // Set up weak settings
            fadechange = FADE_CHANGE_SMALL;
        }

        // Make running sound
        snd = SoundSystem.GetSound(SND_FILE_RUN, true);
        snd.Position = MakeMiddlePosition();

        // Play it
        snd.Play(true);
    }

    // Dispose
    public void Dispose()
    {
        // Play the ending sound
        SoundSystem.PlaySound(SND_FILE_END, MakeMiddlePosition());

        // Remove from both objects
        source.RemoveLightning(this);
        target.RemoveLightning(this);

        // Dispose lights
        foreach (DynamicLight d in lights)
            if (d != null)
                d.Dispose();

        // Dispose sound
        snd.Dispose();
        snd = null;
        source = null;
        target = null;
        lights = null;
        GC.SuppressFinalize(this);
    }

    #endregion

    #region ================== Methods

    // This makes a position in between objects
    public Vector3D MakeMiddlePosition()
    {
        // Position in between objects
        return source.Position + ((target.Position - source.Position) * 0.5f);
    }

    // This creates or sets a light
    private void SetLight(int i, Vector3D lightpos)
    {
        // Check if this is a valid light
        if(i < MAX_LIGHTS)
        {
            // No light created yet?
            if(lights[i] == null)
            {
                // Create the light
                lights[i] = new DynamicLight(lightpos, 16f, General.ARGB(0.2f, 0.2f, 0.6f, 1f), 2);
            }
            else
            {
                // Move the light
                lights[i].Position = lightpos;
            }
        }
    }

    // This rearranges the lights
    private void ArrangeLights(Vector3D from, Vector3D to)
    {
        Vector3D lightpos, delta, flatdelta;
        int numlights;

        // Flat delta coordinates
        delta = to - from;
        flatdelta = delta;
        flatdelta.z = 0f;

        // Determine number of lights
        numlights = (int)(flatdelta.Length() / LIGHT_DISTANCE);
        if(numlights < 1) numlights = 1;

        // Go for all lights
        for(int l = 0; l < numlights; l++)
        {
            // Determine light position
            lightpos = from + (delta / (float)numlights) * (float)(l);

            // Set up the light
            SetLight(l, lightpos);
        }

        // Final light at the end
        SetLight(numlights + 1, to);

        // Discard all other lights
        for(int i = numlights + 2; i < MAX_LIGHTS; i++)
        {
            // Dispose lights
            if(lights[i] != null) lights[i].Dispose();
            lights[i] = null;
        }
    }

    // This processes the lightning
    public void Process()
    {
        Vector3D from, to;

        // Time to spawn another shock?
        if(shocktime < SharedGeneral.currenttime)
        {
            // Determine start and end
            from = source.Position + (source.Velocity * 4f) + new Vector3D(0f, 0f, sourceheight);
            to = target.Position + (target.Velocity * 4f) + new Vector3D(0f, 0f, targetheight);

            // Reposition sound
            snd.Position = MakeMiddlePosition();

            // Rearrange the lights
            ArrangeLights(from, to);

            // Spawn a shock
            new Shock(from, to, fadechange);

            // Next time
            shocktime += SHOCK_SPAWN_DELAY;
        }
    }

    #endregion
}
