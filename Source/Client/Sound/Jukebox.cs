/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using System.IO;

namespace CodeImp.Bloodmasters.Client;

public class Jukebox
{
    #region ================== Variables

    // Tracks to play
    private string[] playlist;

    // Current track
    private int currentitem = 0;
    private Track currenttrack = null;

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public Jukebox()
    {
        // Make playlist from directory
        string musicdir = Path.Combine(Paths.BundledResourceDir, "Music");
        playlist = Directory.GetFiles(musicdir, "*.mp3");

        // Randomize playlist?
        if (General.config.ReadSetting("musicrandom", true))
        {
            // Do this many times
            for (int i = 0; i < playlist.Length * 10; i++)
            {
                // Go for all items
                for (int a = 0; a < playlist.Length; a++)
                {
                    // Pick a random entry b
                    int b = General.random.Next(playlist.Length);

                    // Swap items
                    (playlist[a], playlist[b]) = (playlist[b], playlist[a]);
                }
            }
        }
        else
        {
            // Sort list alphabetically
            Array.Sort(playlist);
        }

        // Start playing the first track
        if (playlist.Length > 0)
        {
            // Load the MP3 and play it
            // TODO[#112]: Load this asynchronously
            currenttrack = new Track(playlist[currentitem], playlist[currentitem]);
            currenttrack.Play();
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
        if (playlist.Length > 0)
        {
            // Track ended?
            if (currenttrack.Ended)
            {
                // Dispose current track
                currenttrack.Dispose();

                // Go to next track
                currentitem++;
                if (currentitem == playlist.Length) currentitem = 0;
                // TODO[#112]: Load this asynchronously
                currenttrack = new Track(playlist[currentitem], playlist[currentitem]);
                currenttrack.Play();
            }
        }
    }

    #endregion
}
