/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System.IO;

namespace CodeImp.Bloodmasters.Client
{
	[ClientItem(9000)]
	public class AmbientSound : Item
	{
		#region ================== Variables

		// The positional sound
		private ISound sound;
        #endregion

        #region ================== Constructor / Destructor

        // Constructor
        public AmbientSound(Thing t) : base(t)
		{
			// Get arguments
			int index = t.Arg[0];
			int volume = t.Arg[1];

			// Find the sound filename
			string filename = General.map.GetSoundFilename(index);

			// Check if the sound must be loaded
			if(!SoundSystem.SoundExists(filename))
			{
				// Find out where this sound file is
				string archive = ArchiveManager.FindFileArchive(filename);
				if(archive != "")
				{
                    // Extract and load the file
                    SoundSystem.CreateSound(filename, ArchiveManager.ExtractFile(archive + "/" + filename));
                }
                else
				{
					// Problem!
					throw(new FileNotFoundException("Unable to load the sound file \"" + filename + "\".", filename));
				}
			}

			// Make the sound
			sound = SoundSystem.GetSound(filename, true);
			sound.Position = this.pos;
			sound.Volume = (float)volume / 255f;
			sound.Play(true);

			// Change to random offset
			sound.SetRandomOffset();
		}

        // When disposed
        public override void Dispose()
        {
        	// Clean up
        	sound.Dispose();
        	sound = null;

        	// Dispose base
        	base.Dispose();
        }

        #endregion

        #region ================== Methods


        // Invisible item
        public override void Render()
		{
			// Do nothing
		}

		// Invisible item
		public override void RenderShadow()
		{
			// Do nothing
		}

		#endregion
	}
}
