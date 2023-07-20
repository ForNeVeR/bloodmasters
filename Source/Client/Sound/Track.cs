/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;

namespace CodeImp.Bloodmasters.Client
{
	public class Track
	{
		#region ================== Constants

		private const float DEFAULT_FADE_SPEED = 0.1f;

		#endregion

		#region ================== Variables

		// Variables
        // TODO[#16]: Restore this functionality
		// private Audio snd;
		private bool repeat = false;
		private bool paused = false;
		private bool playing = false;
		private bool playafterpauze = false;
		private string filename;
		private float targetvolume = 0;
		private float fadespeed = 0;
		private float volume;
		private float pan;

		#endregion

		#region ================== Properties

		public bool Repeat { get { return repeat; } }
		public bool Playing { get { return playing; } }
		public bool PlayAfterPauze { get { return playafterpauze; } }
		public string Filename { get { return filename; } }
		public float Volume { get { return volume; } set { volume = value; ApplySettings(); } }
		public float Pan { get { return pan; } set { pan = value; ApplySettings(); } }
		// public double Duration { get { return snd.Duration; } }
		// public double Position { get { return snd.CurrentPosition; } }
        // TODO[#16]: Restore this functionality
        public bool Ended => false;
		// public bool Ended { get { return (snd.State == StateFlags.Stopped) || (snd.CurrentPosition == snd.Duration); } }

		public int Instances
		{
			get { return 1; }
			set { throw(new Exception("Multiple instances not supported for sounds of AudioSound type.")); }
		}

		#endregion

		#region ================== Constructor / Destructor / Dispose

		// Constructor
		public Track(string filename, string fullfilename)
		{
			// Keep the filename
			this.filename = filename;

			// TODO[#16]: Load the sound
			// snd = new Audio(fullfilename, false);
		}

		// Dispose
		public void Dispose()
		{
			// TODO[#16]: Dispose the sound
			// snd.Stop();
			// snd.Dispose();
			// snd = null;
		}

		#endregion

		#region ================== Private Methods

		// Apply volume and pan
		private void ApplySettings()
		{
			float db;

			// Not silent?
			if(volume > 0.00001f)
			{
				// Calculate dB
				db = 20f * (float)Math.Log10(volume);
				// TODO[#16]: snd.Volume = (int)(100f * db);
			}
			else
			{
				// Silent
				// TODO[#16]: snd.Volume = -10000;
			}

			// Completely left or right?
			if(Math.Abs(pan) > 0.9999f)
			{
				// Maximum
				db = -100f;
			}
			else
			{
				// Calculate dB
				db = 20f * (float)Math.Log10(1f - Math.Abs(pan));
			}
			// TODO[#16]: if(pan > 0f) snd.Balance = -(int)(db * 100f); else snd.Balance = (int)(db * 100f);
		}

		#endregion

		#region ================== Public Methods

		// Pauze
		public void Pauze(bool playafterpauze)
		{
			// NOTE: Dont set the pause variable,
			// it is only for the source control!

			// Pauze the track
			// TODO[#16]: snd.Pause();
			this.playafterpauze = playafterpauze;
		}

		// Resume
		public void Resume()
		{
			// Pauze the track
			// TOOD: if(!paused) snd.Play();
			this.playafterpauze = false;
		}

		// Fade
		public void FadeVolume(float targetvolume) { FadeVolume(targetvolume, DEFAULT_FADE_SPEED); }
		public void FadeVolume(float targetvolume, float fadespeed)
		{
			// Set the fade settings
			this.targetvolume = targetvolume;
			this.fadespeed = fadespeed;
		}

		// Play sound
		public void Play() { Play(1f, 0f, false); }
		public void Play(float volume, float pan, bool repeat)
		{
			// TODO[#16]: snd.Stop();
			this.targetvolume = volume;
			this.volume = volume;
			this.pan = pan;
			this.repeat = repeat;
			this.paused = false;
			this.playing = true;
			ApplySettings();
			// TODO[#16]: snd.Play();
		}

		// Stops
		public void Stop()
		{
			// Stop now
			this.repeat = false;
			this.paused = false;
			this.playing = false;
			// TODO[#16]: snd.Stop();
		}

		#endregion

		#region ================== Processing

		// Process track
		public void Process()
		{
			// TODO: Did the track end?
			//if((snd.State == StateFlags.Stopped) ||
			//   (snd.CurrentPosition == snd.Duration))
			//{
			//	// Repeat the track?
			//	if(repeat)
			//	{
			//		// Restart
			//		snd.Stop();
			//		snd.Play();
			//	}
			//}

			// Fade?
			if(volume != targetvolume)
			{
				// Fade in or out?
				if(volume < targetvolume)
				{
					// Fade in to target
					volume += fadespeed * SharedGeneral.currenttime;
					if(volume > targetvolume) volume = targetvolume;
				}
				else
				{
					// Fade out to target
					volume -= fadespeed * SharedGeneral.currenttime;
					if(volume < targetvolume) volume = targetvolume;
				}

				// Apply new volume
				ApplySettings();
			}
		}

		#endregion
	}
}
