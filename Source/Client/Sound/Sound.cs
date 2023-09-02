/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using JetBrains.Lifetimes;
using NAudio.Wave;

namespace CodeImp.Bloodmasters.Client;

internal class Sound : ISound
{
    #region ================== Variables

    // Variables
    private readonly NAudioPlaybackEngine _playbackEngine;

    // This stores two sound sample providers: the controlled one is used to change the sound parameters, while the
    // output gets actually passed to the playback engine. In a simple case, the playback is the same as the
    // controlled one, but it may be different if we need to resample it to another frequency.
    private readonly AudioSampleProvider _controlSample;
    private readonly ISampleProvider _outputSample;

    private readonly SoundType _soundType;
    private bool repeat = false;
    private bool autodispose = false;
    private string filename;
    private string fullfilename;
    private float volume = 1f;
    private float newvolume = 1f;
    private int absvolume = 0;
    private bool positional;
    private bool disposed;
    private Vector2D pos;
    private bool update = true;
    private int nextupdatetime = 0;

    #endregion

    #region ================== Properties

    public bool Repeat { get { return repeat; } }
    public bool AutoDispose { get { return autodispose; } set { autodispose = value; } }
    public string Filename { get { return filename; } }
    public float Volume { get { return volume; } set { newvolume = value; update = true; } }
    public bool Playing => _controlSample.State == SoundState.Playing;
    public bool Positional { get { return positional; } }
    public Vector2D Position { get { return pos; } set { pos = value; update = true; } }
    public bool Disposed { get { return disposed; } }
    public int Length => _controlSample.Length;
    public int CurrentPosition => _controlSample.CurrentPosition;

    #endregion

    #region ================== Constructor / Destructor / Dispose

    // Constructor
    public Sound(NAudioPlaybackEngine playbackEngine, string filename, string fullfilename, SoundType soundType)
    {
        _playbackEngine = playbackEngine;
        _soundType = soundType;

        // Keep the filename
        this.filename = filename;
        this.fullfilename = fullfilename;

        // Load the sound
        _controlSample = AudioSampleProvider.ReadFromFile(fullfilename);
        _outputSample = playbackEngine.ConvertToRightChannelCount(_controlSample);

        // Done
    }

    // Clone constructor for positional sound
    public Sound(Sound clonesnd, bool positional)
    {
        _playbackEngine = clonesnd._playbackEngine;

        // Keep the filename
        this.filename = clonesnd.Filename;

        // Clone the sound
        _controlSample = clonesnd._controlSample.Clone();
        _outputSample = _playbackEngine.ConvertToRightChannelCount(_controlSample);
        _soundType = clonesnd._soundType;

        // Add to sounds collection
        SoundSystem.AddPlayingSound(this);

        // Position
        this.positional = positional;
    }

    // Dispose
    public void Dispose()
    {
        if (!disposed)
        {
            // Remove from collection
            SoundSystem.RemovePlayingSound(this);

            // Dispose sound
            if (_controlSample != null)
            {
                _controlSample.Stop();
            }

            disposed = true;
            GC.SuppressFinalize(this);
        }
    }

    #endregion

    #region ================== Methods

    // This resets volume to silent
    public void ResetSettings()
    {
        // Leave when disposed
        if(disposed) return;

        // Reset volume/pan
        _controlSample.VolumeHundredthsOfDb = AudioSampleProvider.MaxVolumeHundredthsOfDb;
        // TODO[#113]: Set pan to 0
    }

    // Called when its time to apply changes
    public void Update()
    {
        int pospan, posvol;
        int vol, pan;

        // Update needed?
        if ((update || positional) && (General.realtime > nextupdatetime))
        {
            // Leave when disposed
            if (disposed) return;

            // Volume changed?
            if (newvolume != volume)
            {
                // Recalculate volume
                volume = newvolume;
                absvolume = SoundSystem.CalcVolumeScale(volume);
            }

            // Positional?
            if (positional)
            {
                // Get positional settings
                SoundSystem.GetPositionalEffect(pos, out posvol, out pospan);

                // Calculate and clip final volume
                pan = pospan;
                vol = SoundSystem.GetVolume(_soundType) - posvol + absvolume;
                if (vol > 0) vol = 0;
                else if (vol < -10000) vol = -10000;
                if (pan > 10000) pan = 10000;
                else if (pan < -10000) pan = -10000;

                // Apply final volume
                _controlSample.VolumeHundredthsOfDb = vol;
                // TODO[#113]: Pan
                // _soundSample.Pan = pan;
            }
            else
            {
                // Apply volume
                _controlSample.VolumeHundredthsOfDb = SoundSystem.GetVolume(_soundType) + absvolume;
            }

            // Set next update time
            nextupdatetime = General.realtime + SoundSystem.UPDATE_INTERVAL;
            // Stop updating until something changes
            update = false;
        }
    }

    // This sets the sound in a random playing position
    public void SetRandomOffset()
    {
        // Seek to a random position
        if (_controlSample != null) _controlSample.CurrentPosition = General.random.Next(_controlSample.Length);
    }

    // Play sound
    private readonly SequentialLifetimes _playbackLifetimes = new(Lifetime.Eternal);
    public void Play() { Play(1f, false); }
    public void Play(bool repeat) { Play(1f, repeat); }

    public void Play(float volume, bool repeat)
    {
        // Leave when disposed
        if (disposed) return;

        _controlSample.State = SoundState.Playing;

        // Repeat?
        _controlSample.ShouldRepeat = repeat;
        // TODO[#116] this will overwrite position even if it was set to random offset via SetRandomOffset()
        _controlSample.CurrentPosition = 0;

        // Apply new settings
        this.newvolume = volume;
        this.repeat = repeat;
        this.Update();

        // Play the sound
        var nextPlaybackLifetime = _playbackLifetimes.Next();
        _playbackEngine.PlaySound(nextPlaybackLifetime, _outputSample);
    }

    // Stops all instances
    public void Stop()
    {
        _controlSample.Stop();
        _playbackLifetimes.TerminateCurrent();
    }

    #endregion
}
