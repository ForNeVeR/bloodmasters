/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using System.Drawing;
using System.Globalization;
using System.Collections;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using CodeImp.Bloodmasters;
using CodeImp;

namespace CodeImp.Bloodmasters.Client
{
	public class Animation
	{
		#region ================== Variables
		
		// Static variables
		private static Hashtable animations = new Hashtable();
		
		// Instance stuff
		private int frametime;
		private int origframetime;
		private TextureResource[] frames;
		private string filename;
		private int startframe;
		
		// Animating
		private int curframe = 0;
		private int nextframetime = 0;
		private bool repeat = false;
		private bool ended = false;
		
		#endregion
		
		#region ================== Properties
		
		public TextureResource CurrentFrame { get { return frames[curframe]; } }
		public int NumFrames { get { return frames.Length; } }
		public int OrigFrameTime { get { return origframetime; } }
		public bool Ended { get { return ended; } }
		public string Filename { get { return filename; } }
		
		public int FrameTime
		{
			get { return frametime; }
			set { frametime = value; if(frametime < 1) frametime = 1; }
		}
		
		public int CurrentFrameIndex
		{
			get { return curframe; }
			set
			{
				curframe = value;
				if(curframe < 0) curframe = 0;
				else if(curframe > frames.Length - 1) curframe = frames.Length - 1;
				nextframetime = General.currenttime + frametime;
			}
		}
		
		#endregion
		
		#region ================== Constructor / Destructor
		
		// Constructor
		private Animation()
		{
			GC.SuppressFinalize(this);
		}
		
		#endregion
		
		#region ================== Loading / Unloading
		
		// This loads an animation
		// Filename must include archive name
		public static void Load(string anifile)
		{
			int numframes;
			Archive archive;
			string tempfile;
			string spritefile;
			string prefix;
			
			// Find the archive
			archive = ArchiveManager.GetFileArchive(anifile);
			
			// Does the file exist in this archive?
			if(!ArchiveManager.FilePathNameExists(anifile))
			{
				// No such file in archive
				General.console.AddMessage("Unable to load animation \"" + anifile + "\"", false);
				return;
			}
			
			// Make animation
			Animation newani = new Animation();
			newani.filename = anifile;
			
			// Check if this is a configuration file
			if(anifile.ToLower().EndsWith(".cfg"))
			{
				// Read the configuration
				tempfile = ArchiveManager.ExtractFile(anifile);
				Configuration cfg = new Configuration(tempfile);
				newani.frametime = cfg.ReadSetting("frametime", 50);
				newani.startframe = cfg.ReadSetting("startframe", 1) - 1;
				newani.repeat = cfg.ReadSetting("repeat", false);
				prefix = cfg.ReadSetting("prefix", "unknownprefix");
				numframes = cfg.ReadSetting("frames", 1);
				
				// Reserve memory for frames
				newani.frames = new TextureResource[numframes];
				
				// Go load all frames
				for(int i = 0; i < numframes; i++)
				{
					// Determine frame file number
					float filenum = i + 1;
					
					// Extract the sprite
					spritefile = prefix + "_" + filenum.ToString("0000", CultureInfo.InvariantCulture) + ".tga";
					tempfile = ArchiveManager.ExtractFile(archive.Title + "/" + spritefile);
					
					// Load sprite
					newani.frames[i] = Direct3D.LoadTexture(tempfile, true);
				}
			}
			else
			{
				// Make single frame animation
				newani.frametime = int.MaxValue;
				
				// Reserve memory for frame
				newani.frames = new TextureResource[1];
				
				// Load the frame
				tempfile = ArchiveManager.ExtractFile(anifile);
				newani.frames[0] = Direct3D.LoadTexture(tempfile, true);
			}
			
			// Store the new animation
			Animation.animations.Add(anifile.ToLower(), newani);
		}
		
		// This tests if the given animation is loaded
		public static bool IsLoaded(string anifile)
		{
			// Check if the specified animation is not loaded yet
			return Animation.animations.Contains(anifile.ToLower());
		}
		
		// This returns a new instance of an animation
		public static Animation CreateFrom(string anifile)
		{
			// Get the loaded animation
			Animation ani = (Animation)Animation.animations[anifile.ToLower()];
			
			// Make a new animation with same properties
			Animation newani = new Animation();
			newani.curframe = ani.startframe;
			newani.frametime = ani.frametime;
			newani.origframetime = ani.frametime;
			newani.frames = ani.frames;
			newani.repeat = ani.repeat;
			newani.filename = ani.filename;
			newani.nextframetime = General.currenttime + ani.frametime;
			
			// Return result
			return newani;
		}
		
		// This unloads all animations
		public static void UnloadAll()
		{
			// Clear animations
			Animation.animations.Clear();
			Animation.animations = new Hashtable();
		}
		
		#endregion
		
		#region ================== Processing
		
		// This processes the animation
		public void Process()
		{
			// Animation not single frame?
			if(frametime < int.MaxValue)
			{
				// Time for next frame?
				while(nextframetime <= General.currenttime)
				{
					// Next frame
					curframe++;
					
					// Check if repeating
					if(repeat)
					{
						// Go back to first frame when at the end
						if(curframe > frames.Length - 1) curframe = 0;
					}
					else
					{
						// Stay at the last frame when at the end
						if(curframe > frames.Length - 1)
						{
							// Animation ended, stay at the last frame
							curframe = frames.Length - 1;
							ended = true;
						}
					}
					
					// Set the next frame time
					nextframetime += frametime;
				}
			}
		}
		
		#endregion
	}
}
