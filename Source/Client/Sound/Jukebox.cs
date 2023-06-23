/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System.Collections;
using System.IO;

namespace CodeImp.Bloodmasters.Client
{
	public class Jukebox
	{
		#region ================== Constants

		#endregion

		#region ================== Variables

		// Tracks to play
		private string[] playlist;

		// Music settings
		private float volume;

		// Current track
		private int currentitem = 0;
		private Track currenttrack = null;

		#endregion

		#region ================== Properties

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public Jukebox()
		{
			// Determine volume
			volume = (float)General.config.ReadSetting("musicvolume", 50) / 100f;

			// Make playlist from directory
			string musicdir = Path.Combine(General.apppath, "Music");
			playlist = Directory.GetFiles(musicdir, "*.mp3");

			// Randomize playlist?
			if(General.config.ReadSetting("musicrandom", true))
			{
				// Do this many times
				for(int i = 0; i < playlist.Length * 10; i++)
				{
					// Go for all items
					for(int a = 0; a < playlist.Length; a++)
					{
						// Pick a random entry b
						int b = General.random.Next(playlist.Length);

						// Swap items
						string temp = playlist[a];
						playlist[a] = playlist[b];
						playlist[b] = temp;
					}
				}
			}
			else
			{
				// Sort list alphabetically
				ArrayList sorter = new ArrayList(playlist);
				sorter.Sort();
				playlist = (string[])sorter.ToArray(typeof(string));
			}

			// Start playing the first track
			if(playlist.Length > 0)
			{
				// Load the MP3 and play it
				currenttrack =  new Track(playlist[currentitem], playlist[currentitem]);
				currenttrack.Play(volume, 0f, false);
			}
		}

		// Disposer
		public void Dispose()
		{
			// Dispose current track
			if(currenttrack != null) currenttrack.Dispose();

			// Clean up
			playlist = null;
		}

		#endregion

		#region ================== Processing

		// This processes the jukebox
		public void Process()
		{
			// Anything in the jukebox?
			if(playlist.Length > 0)
			{
				// Track ended?
				if(currenttrack.Ended)
				{
					// Dispose current track
					currenttrack.Dispose();

					// Go to next track
					currentitem++;
					if(currentitem == playlist.Length) currentitem = 0;
					currenttrack = new Track(playlist[currentitem], playlist[currentitem]);
					currenttrack.Play(volume, 0f, false);
				}
			}
		}

		#endregion
	}
}
