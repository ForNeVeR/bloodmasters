/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using NAudio.Wave;

namespace CodeImp.Bloodmasters.Client;

public static class SoundSystem
{
    #region ================== Constants

    // Positional settings
    public const float PAN_CENTER_RANGE = 20f;
    public const float PAN_ROLLOFF_SCALE = 50f;
    public const float VOL_CENTER_RANGE = 20f;
    public const float VOL_ROLLOFF_SCALE = 40f;

    // Sounds update interval
    public const int UPDATE_INTERVAL = 100;

    // Log table accuracy
    public const float LOG_TABLE_MUL = 10000f;

    #endregion

    #region ================== Variables

    // Log table
    private static float[] logtable;

    // Devices
    private static NAudioPlaybackEngine? _playbackEngine;

    // Resources
    private static Dictionary<string, ISound> sounds = new();

    // Settings
    private static bool playeffects;
    private static bool playmusic;
    private static float effectsvolume;
    private static float musicvolume;

    // 3D Sound
    private static Vector2D listenpos;
    private static List<ISound> playingsounds = new();

    #endregion

    #region ================== Initialization, Reset and Termination

    // Terminates the sound system
    public static void Terminate()
    {
        // Trash all sounds
        DestroyAllResources();

        // Kill it
        _playbackEngine?.Dispose();
        _playbackEngine = null;
    }

    // Initializes the sound system
    public static bool Initialize(Form target)
    {
        int soundfreq;
        int soundbits;

        // Init log table
        BuildLog10Table();

        // Get settings from configuration
        playeffects = General.config.ReadSetting("sounds", true);
        playmusic = General.config.ReadSetting("music", true);
        effectsvolume = CalcVolumeScale(General.config.ReadSetting("soundsvolume", 100) / 100f);
        musicvolume = CalcVolumeScale(General.config.ReadSetting("musicvolume", 50) / 100f);
        soundfreq = General.config.ReadSetting("soundfrequency", 0);
        soundbits = General.config.ReadSetting("soundbits", 0);

        if (playeffects || playmusic)
        {
            var waveFormat = soundfreq > 0 && soundbits > 0
                ? new WaveFormat(soundfreq, soundbits, 2)
                : WaveFormat.CreateIeeeFloatWaveFormat(44100, 2);

            _playbackEngine = new NAudioPlaybackEngine(waveFormat);
        }

        // Playing sounds?
        if(SoundSystem.playeffects)
        {
            // Go for all files in the sounds archive
            Archive soundsrar = ArchiveManager.GetArchive("sounds.rar");
            foreach(string filename in soundsrar.FileNames)
            {
                // Load this sound
                CreateSound(filename, ArchiveManager.ExtractFile("sounds.rar/" + filename), SoundType.Sound);
            }
        }

        // No problems
        return true;
    }

    #endregion

    #region ================== Sounds

    // This removes a sound from sounds collection
    public static void RemovePlayingSound(ISound snd)
    {
        // Remove if exists
        int index = playingsounds.IndexOf(snd);
        if(index > -1) playingsounds.RemoveAt(index);
    }

    // This adds a sound to sounds collection
    public static void AddPlayingSound(ISound snd)
    {
        // Remove if exists
        playingsounds.Add(snd);
    }

    // This sets the volume and panning for the given position
    public static void GetPositionalEffect(Vector2D soundpos, out float volume, out float pan)
    {
        float deltalen, deltax;

        // Get the delta vector
        Vector2D delta = soundpos - listenpos;
        deltalen = delta.Length();
        deltax = delta.y + delta.x;
        if(float.IsNaN(deltalen)) deltalen = 0f;
        if(float.IsNaN(deltax)) deltax = 0f;

        // Object within center range?
        if((deltax > -PAN_CENTER_RANGE) &&
           (deltax < PAN_CENTER_RANGE))
        {
            // No panning
            pan = 0;
        }
        // Panning to the left or right?
        else if(deltax < 0f)
        {
            // Calculate panning to the left
            pan = (int)((deltax + PAN_CENTER_RANGE) * PAN_ROLLOFF_SCALE);
        }
        else
        {
            // Calculate panning to the right
            pan = (int)((deltax - PAN_CENTER_RANGE) * PAN_ROLLOFF_SCALE);
        }

        // Object within center range?
        if(deltalen < VOL_CENTER_RANGE)
        {
            // Normal volume
            volume = 0;
        }
        else
        {
            // Calculate volume by distance
            volume = (int)((deltalen - VOL_CENTER_RANGE) * VOL_ROLLOFF_SCALE);
        }
    }

    // This sets the coordinates of the listener
    public static void SetListenCoordinates(Vector2D pos)
    {
        // Set new coordinates
        listenpos = pos;
    }

    // This resets all positional sounds
    public static void ResetPositionalSounds()
    {
        // Go for all positional sounds
        foreach(ISound snd in playingsounds)
        {
            // Reset volume/pan settings
            snd.ResetSettings();
        }
    }

    // This returns a sound object by filename
    public static ISound GetSound(string filename, bool positional)
    {
        // Not playing sounds?
        if(!SoundSystem.playeffects) return new NullSound();

        if (!sounds.TryGetValue(filename, out var snd))
        {
            // Error, sound not loaded
            if(General.console != null) General.console.AddMessage("Sound file \"" + filename + "\" is not loaded.", true);
            return new NullSound();
        }

        return snd switch
        {
            NullSound ns => ns,
            Sound s => new Sound(s, positional),
            _ => throw new NotSupportedException(
                $"Sound type {snd.GetType()} of sound \"{filename}\" is not supported for clone.")
        };
    }

    // Plays a sound
    public static void PlaySound(string filename)
    {
        // Get the sound object and play it
        ISound snd = GetSound(filename, false);
        snd.AutoDispose = true;
        snd.Play();
    }

    // Plays a sound at a fixed location
    public static void PlaySound(string filename, Vector2D pos)
    {
        // Get the sound object and play it
        ISound snd = GetSound(filename, true);
        snd.AutoDispose = true;
        snd.Position = pos;
        snd.Play();
    }

    // Plays a sound at a fixed location with specified volume
    public static void PlaySound(string filename, Vector2D pos, float volume)
    {
        // Get the sound object and play it
        ISound snd = GetSound(filename, true);
        snd.AutoDispose = true;
        snd.Position = pos;
        snd.Play(volume, false);
    }

    #endregion

    #region ================== Resources

    // This checks if a sound exists
    public static bool SoundExists(string filename)
    {
        return sounds.ContainsKey(filename);
    }

    // This creates a new sound
    public static ISound CreateSound(string filename, string fullfilename, SoundType soundType)
    {
        ISound s;

        // Check if not already exists
        if (sounds.ContainsKey(filename) == false)
        {
            s = soundType switch
            {
                // Not playing sounds?
                SoundType.Sound when !playeffects => new NullSound(),
                SoundType.Music when !playmusic => new NullSound(),
                // Load the sound
                _ => new Sound(_playbackEngine!, filename, fullfilename, soundType)
            };

            // Add to collection
            if (soundType != SoundType.Music)
            {
                sounds.Add(filename, s);
            }

            return s;
        }
        else
        {
            // Sound already created
            throw(new Exception("Sound resource '" + filename + "' already exists."));
        }
    }

    // This destroys a sound
    public static void DestroySound(string filename)
    {
        // Remove from collection if sound exists
        if(sounds.Remove(filename, out ISound s))
        {
            // Dispose it
            s.Dispose();
        }
    }

    // This destroys all resources
    public static void DestroyAllResources()
    {
        // Go for all playing sounds
        for(int i = playingsounds.Count - 1; i >= 0; i--)
        {
            // Get the sound
            ISound s = playingsounds[i];

            // Dispose it
            s.Dispose();
        }
        playingsounds.Clear();

        // Go for all sounds
        foreach(ISound s in sounds.Values)
        {
            // Dispose it
            s.Dispose();
        }
        sounds.Clear();
    }

    #endregion

    #region ================== Processing

    // This processes sounds
    public static void Process()
    {
        // Go for all playing sounds
        for(int i = playingsounds.Count - 1; i >= 0; i--)
        {
            // Get the sound
            ISound s = playingsounds[i];

            // Update sound
            s.Update();

            // Auto Dispose?
            if(s.AutoDispose)
            {
                // Dispose when done playing
                if(!s.Playing) s.Dispose();
            }
        }
    }

    #endregion

    #region ================== Tools

    // This builds the log table
    public static void BuildLog10Table()
    {
        logtable = new float[(int)LOG_TABLE_MUL + 1];
        for(int i = 0; i < ((int)LOG_TABLE_MUL + 1); i++)
        {
            if(i == 0)
                logtable[i] = -4f;
            else
                logtable[i] = (float)Math.Log10((float)i / LOG_TABLE_MUL);
        }
    }

    // This looks up a log value in a table
    public static float Log10Table(float v)
    {
        return logtable[(int)(v * LOG_TABLE_MUL)];
    }

    // This converts a linear value from 0f to 1f into
    // a logarithmic value for sound volume.
    public static int CalcVolumeScale(float scale)
    {
        float db;

        // Ensure scale is within acceptable range
        if(scale >= 1f) return 0; else if(scale <= 0.0001f) return -10000;

        // Calculate logarithmic value for given scale
        //db = 20f * (float)Math.Log10(scale);
        db = 20f * Log10Table(scale);
        return (int)(100f * db);
    }

    // This converts a linear value from 0f to 1f into
    // a logarithmic value for sound pan.
    public static int CalcPanningScale(float scale)
    {
        float db;

        // Maximum left or right?
        if(Math.Abs(scale) >= 1f)
        {
            // Maximum db on that side
            db = -100f;
        }
        // Otherwise
        else
        {
            // Calculate db for given scale
            //db = 20f * (float)Math.Log10(1f - Math.Abs(scale));
            db = 20f * Log10Table(1f - Math.Abs(scale));
        }

        // Return panning
        if(scale > 0f) return -(int)(db * 100f); else return (int)(db * 100f);
    }

    public static float GetVolume(SoundType soundType)
    {
        return soundType switch
        {
            SoundType.Sound => effectsvolume,
            SoundType.Music => musicvolume,
            _ => throw new SwitchExpressionException(soundType)
        };
    }

    #endregion
}
