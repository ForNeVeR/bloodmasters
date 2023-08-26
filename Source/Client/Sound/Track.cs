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
		#region ================== Variables

		// Variables
		private ISound snd;
		private float pan;

		#endregion

		#region ================== Properties

        public bool Ended => !snd.Playing || snd.CurrentPosition == snd.Length;

		#endregion

		#region ================== Constructor / Destructor / Dispose

		// Constructor
		public Track(string filename, string fullfilename)
		{
			snd = SoundSystem.CreateSound(filename, fullfilename, SoundType.Music);
		}

		// Dispose
		public void Dispose()
		{
			snd.Stop();
			snd.Dispose();
            snd = null;
        }

		#endregion

		#region ================== Private Methods

		// Apply volume and pan
		private void ApplySettings()
		{
			float db;

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
			// TODO: if(pan > 0f) snd.Balance = -(int)(db * 100f); else snd.Balance = (int)(db * 100f);
		}

		#endregion

		#region ================== Public Methods

		// Play sound
		public void Play(float pan)
		{
			snd.Stop();
			this.pan = pan;
			ApplySettings();
			snd.Play();
		}

		#endregion
	}
}
