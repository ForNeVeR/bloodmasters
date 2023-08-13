/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using SharpDX.DirectSound;

namespace CodeImp.Bloodmasters.Client
{
	internal class Sound : ISound
	{
		#region ================== Variables

		// Variables
        private readonly NAudioPlaybackEngine _playbackEngine;
        private readonly SimpleSampleProvider _soundSample;
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
		private bool update = false;
		private int nextupdatetime = 0;

		#endregion

		#region ================== Properties

		public bool Repeat { get { return repeat; } }
		public bool AutoDispose { get { return autodispose; } set { autodispose = value; } }
		public string Filename { get { return filename; } }
		public float Volume { get { return volume; } set { newvolume = value; update = true; } }
        public bool Playing => _soundSample.IsPlaying;
		public bool Positional { get { return positional; } }
		public Vector2D Position { get { return pos; } set { pos = value; update = true; } }
		public bool Disposed { get { return disposed; } }

		#endregion

		#region ================== Constructor / Destructor / Dispose

		// Constructor
		public Sound(NAudioPlaybackEngine playbackEngine, string filename, string fullfilename)
        {
            _playbackEngine = playbackEngine;

			// Keep the filename
			this.filename = filename;
            this.fullfilename = fullfilename;

			// Load the sound
            _soundSample = SimpleSampleProvider.ReadFromFile(fullfilename);

			// Done
		}

		// Clone constructor for positional sound
		public Sound(ISound clonesnd, bool positional)
		{
			// Keep the filename
			this.filename = clonesnd.Filename;

			// Clone the sound
			_soundSample = ((Sound)clonesnd)._soundSample.Clone();

			// Add to sounds collection
			SoundSystem.AddPlayingSound(this);

			// Position
			this.positional = positional;
		}

		// Dispose
		public void Dispose()
		{
			if(!disposed)
			{
				// Remove from collection
				SoundSystem.RemovePlayingSound(this);

				// Dispose sound
				if(_soundSample != null)
				{
					_soundSample.Stop();
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
            // TODO: Volume control
			// _soundSample.Volume = 0;
			// _soundSample.Volume = -10000;
		}

		// Called when its time to apply changes
		public void Update()
		{
			int pospan, posvol;
			int vol, pan;

			// Update needed?
			if((update || positional) && (General.realtime > nextupdatetime))
			{
				// Leave when disposed
				if(disposed) return;

				// Volume changed?
				if(newvolume != volume)
				{
					// Recalculate volume
					volume = newvolume;
					absvolume = SoundSystem.CalcVolumeScale(volume);
				}

				// Positional?
				if(positional)
				{
					// Get positional settings
					SoundSystem.GetPositionalEffect(pos, out posvol, out pospan);

					// Calculate and clip final volume
					pan = pospan;
					vol = SoundSystem.effectsvolume - posvol + absvolume;
					if(vol > 0) vol = 0; else if(vol < -10000) vol = -10000;
					if(pan > 10000) pan = 10000; else if(pan < -10000) pan = -10000;

					// Apply final volume
                    // TODO: Volume, pan
					// _soundSample.Volume = vol;
					// _soundSample.Pan = pan;
				}
				else
				{
					// Apply volume
                    // TODO: Volume, pan
					//_soundSample.Volume = DirectSound.effectsvolume + absvolume;
				}

				// Set next update time
				nextupdatetime = General.realtime + SoundSystem.UPDATE_INTERVAL;
			}
		}

		// This sets the sound in a random playing position
		public void SetRandomOffset()
		{
			// Seek to a random position
			if(_soundSample != null) _soundSample.CurrentPosition = General.random.Next(_soundSample.Length);
		}

		// Play sound
		public void Play() { Play(1f, false); }
		public void Play(bool repeat) { Play(1f, repeat); }
		public void Play(float volume, bool repeat)
		{
			PlayFlags flags = 0;

			// Leave when disposed
			if(disposed) return;

            if (Playing) Stop();

			// Repeat?
            _soundSample.ShouldRepeat = repeat;
			_soundSample.CurrentPosition = 0;

			// Apply new settings
			this.newvolume = volume;
			this.repeat = repeat;
			this.Update();

			// Play the sound
            _playbackEngine.PlaySound(_soundSample);
		}

		// Stops all instances
		public void Stop()
        {
            _soundSample.Stop();
            _playbackEngine.StopSound(_soundSample);
        }

		#endregion
	}
}
