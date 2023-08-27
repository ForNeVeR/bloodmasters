/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

namespace CodeImp.Bloodmasters.Client
{
	public class Track
	{
		#region ================== Variables

		// Variables
		private ISound snd;

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

		#region ================== Public Methods

		// Play sound
		public void Play()
		{
			snd.Stop();
			snd.Play();
		}

		#endregion
	}
}
