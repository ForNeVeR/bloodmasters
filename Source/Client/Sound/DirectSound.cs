/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

// The DirectSound class contains functions for DirectSound related
// functionality which are used throughout this engine. Bla.

using System;
using System.IO;
using System.Threading;
using System.Collections;
using System.Drawing;
using Microsoft.DirectX;
using Microsoft.DirectX.DirectSound;
using System.Windows.Forms;
using CodeImp.Bloodmasters;
using CodeImp;

namespace CodeImp.Bloodmasters.Client
{
	public class DirectSound
	{
		#region ================== Constants
		
		// Special sound files
		public const string SOUND_SILENCE = "silence.wav";
		
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
		public static Device dsd;
		private static Microsoft.DirectX.DirectSound.Buffer dspb;
		
		// Resources
		private static Hashtable sounds = new Hashtable();
		
		// Settings
		public static bool playeffects;
		public static int effectsvolume;
		
		// 3D Sound
		private static Vector2D listenpos;
		private static ArrayList playingsounds = new ArrayList();
		
		#endregion
		
		#region ================== Initialization, Reset and Termination
		
		// Terminates DirectSound
		public static void Terminate()
		{
			// Trash all sounds
			DestroyAllResources();
			
			// Kill it
			try { dspb.Dispose(); } catch(Exception) { }
			try { dsd.Dispose(); } catch(Exception) { }
			dspb = null;
			dsd = null;
		}
		
		// Initializes DirectSound
		public static bool Initialize(Form target)
		{
			Microsoft.DirectX.DirectSound.Buffer dspb;
			BufferDescription bufferdesc;
			WaveFormat bufferformat;
			int soundfreq;
			int soundbits;
			
			// Init log table
			BuildLog10Table();
			
			// Get settings from configuration
			playeffects = General.config.ReadSetting("sounds", true);
			effectsvolume = CalcVolumeScale((float)General.config.ReadSetting("soundsvolume", 100) / 100f);
			soundfreq = General.config.ReadSetting("soundfrequency", 0);
			soundbits = General.config.ReadSetting("soundbits", 0);
			
			// Playing sounds?
			if(DirectSound.playeffects)
			{
				// Create default DirectSound device
				dsd = new Device();
				
				// Set cooperative level
				dsd.SetCooperativeLevel(target, CooperativeLevel.Priority);
				
				// Set the primary buffer format?
				if((soundfreq > 0) && (soundbits > 0))
				{
					// Get the primary buffer
					bufferdesc = new BufferDescription();
					bufferdesc.PrimaryBuffer = true;
					dspb = new Microsoft.DirectX.DirectSound.Buffer(bufferdesc, dsd);
					
					// Make format info
					bufferformat = new WaveFormat();
					bufferformat.FormatTag = WaveFormatTag.Pcm;
					bufferformat.Channels = 2;
					bufferformat.SamplesPerSecond = soundfreq;
					bufferformat.BitsPerSample = (short)soundbits;
					bufferformat.BlockAlign = (short)(2 * soundbits / 8);
					bufferformat.AverageBytesPerSecond = soundfreq * bufferformat.BlockAlign;
					
					// Set the buffer format
					dspb.Format = bufferformat;
					
					// Done
					bufferdesc.Dispose();
				}
				
				// Go for all files in the sounds archive
				Archive soundsrar = ArchiveManager.GetArchive("sounds.rar");
				foreach(string filename in soundsrar.FileNames)
				{
					// Load this sound
					CreateSound(filename, ArchiveManager.ExtractFile("sounds.rar/" + filename));
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
		public static void GetPositionalEffect(Vector2D soundpos, out int volume, out int pan)
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
			if(!DirectSound.playeffects) return new NullSound();
			
			// Return sound object if it exists
			if(!sounds.Contains(filename))
			{
				// Error, sound not loaded
				if(General.console != null) General.console.AddMessage("Sound file \"" + filename + "\" is not loaded.", true);
				return new NullSound();
			}
			
			// Return sound
			ISound newsnd = new Sound((Sound)sounds[filename], positional);
			return newsnd;
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
			return sounds.Contains(filename);
		}
		
		// This creates a new sound
		public static void CreateSound(string fullfilename)
		{
			// Make it so
			CreateSound(Path.GetFileName(fullfilename), fullfilename);
		}
		
		// This creates a new sound
		public static void CreateSound(string filename, string fullfilename)
		{
			ISound s;
			
			// Check if not already exists
			if(sounds.Contains(filename) == false)
			{
				// Not playing sounds?
				if(!DirectSound.playeffects)
				{
					// No sound
					s = new NullSound();
				}
				else
				{
					// Load the sound
					s = new Sound(filename, fullfilename);
				}
				
				// Add to collection
				sounds.Add(filename, s);
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
			// Check if sound exists
			if(sounds.Contains(filename))
			{
				// Dispose it
				ISound s = (ISound)sounds[filename];
				s.Dispose();
				
				// Remove from collection
				sounds.Remove(filename);
			}
		}
		
		// This destroys all resources
		public static void DestroyAllResources()
		{
			// Go for all playing sounds
			for(int i = playingsounds.Count - 1; i >= 0; i--)
			{
				// Get the sound
				ISound s = (ISound)playingsounds[i];
				
				// Dispose it
				s.Dispose();
			}
			playingsounds.Clear();
			
			// Go for all sounds
			foreach(DictionaryEntry de in sounds)
			{
				// Dispose it
				ISound s = (ISound)de.Value;
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
				ISound s = (Sound)playingsounds[i];
				
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

		#endregion
	}
}
