/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

// The General class has general purpose functions and main routines.
// It also has some public objects that are often referenced by many
// other classes in the engine.

using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using Bloodmasters.DedicatedServer;

namespace CodeImp.Bloodmasters.Server
{
	internal sealed class General : SharedGeneral
	{
		#region ================== Variables

		// Application paths and name
		public static string appname = "";

        // Filenames
		public static string logfilename = "";
		public static bool logtofile = false;

		// Game stuff
		public static GameServer server;

		// Clock
		public static int lagwarningtime;

		// Randomizer
		public static Random random = new Random();

		#endregion

		#region ================== Initialization / Terminate

		// This is the very first initialize of the engine
		private static bool Initialize()
		{
			// Setup application name
			appname = Assembly.GetExecutingAssembly().GetName().Name;

            // Open all archives with archivemanager
			ArchiveManager.Initialize(Paths.BundledResourceDir);

			// Setup filenames
			logfilename = Path.Combine(Paths.LogDirPath, appname + ".log");

			// Setup clock
			SharedGeneral.previoustime = SharedGeneral.GetCurrentTime();
			SharedGeneral.realtime = SharedGeneral.GetCurrentTime() + 1;

			// Return success
			return true;
		}

		// This unloads all resources and the game
		private static void Terminate()
		{
			// Terminate server stuff
			if(server != null) server.Dispose();
			server = null;
		}

		// This termines the entire application
		public static void Exit()
		{
			// Close archives
			ArchiveManager.Dispose();

			// Delete the temporary directory
			if(!string.IsNullOrEmpty(Paths.TempDir)) Directory.Delete(Paths.TempDir, true);

			// End of program
			//Application.Exit();
		}

		#endregion

		#region ================== Configuration

		// This finds and reads the server configuration
		public static string LoadServerConfiguration(string[] args)
		{
			// Determine config file
			string configfile = Path.Combine(Paths.ConfigDirPath, "bmserver.cfg");
			if(args.Length > 0) configfile = args[args.Length - 1];

			// Load the config file
			Configuration cfg = new Configuration(configfile, true);

			// Get log filename
			string logfile = cfg.ReadSetting("logfile", "");
			if(logfile != null)
			{
				// Write everything to log file as well
				logtofile = true;

				// Check if full path given
				if(Path.IsPathRooted(logfile))
				{
					// Full path to log file
					logfilename = logfile;
				}
				else
				{
					// Make path relative to app path
					logfilename = Path.Combine(Paths.LogDirPath, logfile);
				}
			}

			// Set lag warning time
			lagwarningtime = cfg.ReadSetting("lagwarningtime", 100);

			// Return the data
			return cfg.OutputConfiguration("", false);
		}

		#endregion

		#region ================== Logging

		// DEBUG:
		public static void DisplayAndLog(string text)
		{
			Console.WriteLine(text);
			WriteLogLine(text);
		}

		// This writes text to the log file
		public static void WriteLogLine(string text)
		{
			// Open or create the log file
			StreamWriter log = File.AppendText(logfilename);

			// Write the text to the file
			log.WriteLine(text);

			// Close the file
			log.Close();
			log = null;
		}

		// This writes an exception to the log file
		public static void WriteErrorLine(Exception error)
		{
			// Open or create the log file
			StreamWriter log = File.AppendText(logfilename);

			// Write the error to the file
			log.WriteLine();
			log.WriteLine(error.Source + " throws " + error.GetType().Name + ":");
			log.WriteLine(error.Message);
			log.WriteLine(error.StackTrace);
			log.WriteLine();

			// Close the file
			log.Close();
			log = null;
		}

		// This writes an exception to the console
		public static void OutputError(Exception error)
		{
			// Write the error to the console
			Console.WriteLine();
			Console.WriteLine(error.Source + " throws " + error.GetType().Name + ":");
			Console.WriteLine(error.Message);
			Console.WriteLine(error.StackTrace);
			Console.WriteLine();
		}

		#endregion

		#region ================== Processing

		// This is the main game loop
		private static void GeneralLoop()
		{
			// Continue while a client or server is running
			while(server != null)
			{
				// Process and render a single frame
				DoOneFrame();
			}
		}

		// This processes and renders a single frame
		public static void DoOneFrame()
		{
			int deltatime;

			// Calculate the frame time
			realtime = GetCurrentTime();
			deltatime = realtime - previoustime;
			previoustime = realtime;
			accumulator += deltatime;

			// Always process networking
			server.gateway.Process();

			// Check for server lag
			if(deltatime >= lagwarningtime)
			{
				// Warn the server admin
				Console.WriteLine("Warning: server processing lag detected (" + deltatime + " mspf)");
			}

			// Enough delta time for processing?
			while(accumulator >= Consts.TIMESTEP)
			{
				// Advance time
				currenttime += Consts.TIMESTEP;

				// Process now
				server.Process();

				// Time processed
				accumulator -= Consts.TIMESTEP;
			}

			// Wait a bit
			Thread.Sleep(4);
		}

		// This makes adjustments to catch up any lag
		public static void CatchLag()
		{
			// Reset previous time to fix the next delta time
			previoustime = GetCurrentTime();
		}

		#endregion

		#region ================== Startup

		// Main program entry
		// This is where the fun begins
		[STAThread] private static void Main(string[] args)
		{
			// Debugger attached?
			if(Debugger.IsAttached)
			{
				// Run without exception handling
				_Main(args);
			}
			else
			{
				try
				{
					// Run within exception handlers
					_Main(args);
				}

				// Catch any errors
				catch(Exception e)
				{
					// Log the error
					OutputError(e);
					WriteErrorLine(e);
				}
			}

			// End of program
			Exit();
		}

		// This does all the loading and starts up the server
		private static void _Main(string[] args)
        {
            Host.Instance = new ServerHost();

			string serverconfig;
			string versionmsg;

			// Create software information message
			Version v = Assembly.GetExecutingAssembly().GetName().Version;
			versionmsg = "Bloodmasters dedicated server version " + v.ToString(4) + " protocol " + Gateway.PROTOCOL_VERSION;

			// Output software information
			Console.WriteLine(versionmsg);

			// Initialize
			if(Initialize())
			{
				// Output software information to log
				WriteLogLine(versionmsg);

				// Load configuration
				serverconfig = LoadServerConfiguration(args);
				if(serverconfig != "")
				{
					// Create the server
					server = new GameServer();
					server.Initialize(serverconfig);

					// Run the general loop
					GeneralLoop();
				}
			}
		}

		#endregion

		#region ================== Misc Functions

		// This creates a string of random ASCII chars
		public static string RandomString(int len)
		{
			string result = "";

			// ASCII chars to use
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890";

			// Make string
			for(int i = 0; i < len; i++)
			{
				result += chars[random.Next(chars.Length)];
			}

			// Returrn result
			return result;
		}

		// This trims the last color code from a string
		public static string TrimColorCodes(string str)
		{
			// Remove all color code signs from the end of the string
			return str.TrimEnd(Consts.COLOR_CODE_SIGN.ToCharArray());
		}

		// This strips color codes from a string
		public static string StripColorCodes(string str)
		{
			StringBuilder result = new StringBuilder(str.Length);

			// Split the string by color code
			string[] pieces = str.Split(Consts.COLOR_CODE_SIGN.ToCharArray());

			// Go for all pieces and append them
			result.Append(pieces[0]);
			for(int i = 1; i < pieces.Length; i++)
			{
				// Not an empty string?
				if(pieces[i] != "")
				{
					// Append everything except the first character
					result.Append(pieces[i].Substring(1));
				}
			}

			// Return final string
			return result.ToString();
		}

		// Make a color from RGB
		public static int RGB(int r, int g, int b) { return Color.FromArgb(r, g, b).ToArgb(); }

		// Make a random color
		public static int RandomColor()
		{
			return Color.FromArgb((int)(random.NextDouble() * 255f), (int)(random.NextDouble() * 255f), (int)(random.NextDouble() * 255f)).ToArgb();
		}

		// Make a color from RGB
		public static int RGB(float r, float g, float b)
		{
			return Color.FromArgb((int)(r * 255f), (int)(g * 255f), (int)(b * 255f)).ToArgb();
		}

		// Make a color from ARGB
		public static int ARGB(float a, float r, float g, float b)
		{
			return Color.FromArgb((int)(a * 255f), (int)(r * 255f), (int)(g * 255f), (int)(b * 255f)).ToArgb();
		}

		// Distance in 2D
		public static float Distance(float x1, float y1, float x2, float y2)
		{
			// Calculate delta coordinates
			float dx = x1 - x2;
			float dy = y1 - y2;

			// Calculate distance
			return (float)Math.Sqrt(dx * dx + dy * dy);
		}

		// Distance in 3D
		public static float Distance(float x1, float y1, float z1, float x2, float y2, float z2)
		{
			// Calculate delta coordinates
			float dx = x2 - x1;
			float dy = y2 - y1;
			float dz = z2 - z1;

			// Calculate distance
			return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
		}

		// This tests on which side of the line the given coordinates are
		// returns < 0 for front (right) side, > 0 for back (left) side and 0 if on the line
		public static float SideOfLine(float v1x, float v1y, float v2x, float v2y, float x, float y)
		{
			// Calculate and return side information
			return (y - v1y) * (v2x - v1x) - (x - v1x) * (v2y - v1y);
		}

		#endregion
	}
}

