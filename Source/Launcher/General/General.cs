/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using Vortice.Direct3D9;

namespace CodeImp.Bloodmasters.Launcher
{
	public class General
	{
		// API declarations
		[DllImport("user32.dll")] public static extern int LockWindowUpdate(IntPtr hwnd);

		#region ================== Variables

		// Paths and names
		public static string apppath = "";
		public static string appname = "";
		public static string temppath = "";

		// Configuration
		public static Configuration config;

		// Filenames
		private static string configfilename = "Bloodmasters.cfg";
		private static string ip2countryfilename = "ip-to-country.csv";
		private static string logfilename = "";

		// IP-To-Country database
		public static IP2Country ip2country;

		// Windows
		public static FormMain mainwindow = null;

		// Settings
		public static string playername;

		// Randomizer
		public static Random random = new Random();

		// Processes
		private static Process bmproc = null;
		private static string serverfile;

		#endregion

		#region ================== Startup / Exit

		// This is the very first initialize of the engine
		private static bool Initialize()
		{
			// Enable OS visual styles
			Application.EnableVisualStyles();
			Application.DoEvents();		// This must be here to work around a .NET bug

			// Setup application path
			Uri localpath = new Uri(Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase), true);
			apppath = localpath.AbsolutePath;

			// Setup application name
			appname = Assembly.GetExecutingAssembly().GetName().Name;

			// Temporary directory (in system temporary directory)
			do { temppath = Path.Combine(Path.GetTempPath(), RandomString(8)); }
			while(Directory.Exists(temppath) || File.Exists(temppath));

			// Make temporary directory
			Directory.CreateDirectory(temppath);

			// Setup filenames
			configfilename = Path.Combine(General.apppath, configfilename);
			logfilename = Path.Combine(apppath, appname + ".log");
			ip2countryfilename = Path.Combine(General.apppath, ip2countryfilename);

			// Initialize DirectX
			try { Direct3D.InitDX(); }
			catch(Exception)
			{
				// DirectX not installed?
				MessageBox.Show(null, "Unable to initialize DirectX. Please ensure that you have the latest version of DirectX installed.", "Bloodmasters", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}

			// Return success
			return true;
		}

		// Main program entry
		// This is where the fun begins
		[STAThread] private static void Main(string[] args)
		{
			// This will keep a thrown exception
			Exception ex = null;

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
					WriteErrorLine(e);

					// Keep the exception
					ex = e;
				}
			}

			// Show exception if an exception was thrown
			if(ex != null) ShowException(ex);

			// End of program
			Exit();
		}

		// This load the application
		private static void _Main(string[] args)
		{
			string deviceerror = null;

			// Initialize
			if(Initialize())
			{
				// Load configuration
				if(LoadConfiguration())
				{
					// Load main window
					mainwindow = new FormMain();

					// Show window
					mainwindow.Show();
					mainwindow.Update();

					// Open all archives with archivemanager
					mainwindow.ShowStatus("Loading data archives...");
					ArchiveManager.Initialize(General.apppath, General.temppath);

					// Refrehs maps in list
					mainwindow.RefreshMapsLists();

					// Select adapter
					mainwindow.ShowStatus("Initializing video adapter...");
					try { Direct3D.SelectAdapter(General.config.ReadSetting("displaydriver", 0)); }
					catch(Exception)
					{
						// DirectX not installed?
						MessageBox.Show(null, "Unable to initialize DirectX. Please ensure that you have the latest version of DirectX installed.", "Bloodmasters", MessageBoxButtons.OK, MessageBoxIcon.Error);
						return;
					}

					// Validate adapter and, if not valid, select a valid adapter
					deviceerror = Direct3D.SelectValidAdapter();
					if(deviceerror == null)
					{
						// Try loading the flags?
						if(File.Exists(ip2countryfilename))
						{
							// Load flag images
							if(mainwindow.LoadFlagImages())
							{
								// Load ip-to-country database
								mainwindow.ShowStatus("Loading IP country database...");
								ip2country = new IP2Country(ip2countryfilename);
								mainwindow.ShowStatus("Ready.");
							}
							else
							{
								// Empty lookup table
								ip2country = new IP2Country();
							}
						}
						else
						{
							// Empty lookup table
							ip2country = new IP2Country();
							mainwindow.LoadNoFlagImages();
						}

						// Refresh list if preferred
						if(General.config.ReadSetting("startuprefresh", true))
							mainwindow.RefreshGamesList();

						// New player?
						if(General.playername.ToLower().Trim() == "newbie")
						{
							// Ask to change the name
							MessageBox.Show(mainwindow, "Welcome and thank you for trying Bloodmasters!\nPlease change your player name and configure your settings.", "Bloodmasters", MessageBoxButtons.OK, MessageBoxIcon.Information);
							mainwindow.btnOptions_Click(null, EventArgs.Empty);
						}

						// Run from main window
						mainwindow.tmrUpdateList.Enabled = true;
						Application.Run(mainwindow);

						// Save configuration
						General.SaveConfiguration();
					}
					else
					{
						// No valid adapter exists
						MessageBox.Show(null, "You do not have a valid video device that meets the requirements for this game.\nProblem description: " + deviceerror + "\n\nPlease ensure you have the latest video drivers for your video card (see manufacturer website) and that Microsoft DirectX 9 is properly installed.", "Bloodmasters", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				}
			}
		}

		// This termines the entire application
		public static void Exit()
		{
			// Close launcher
			if(mainwindow != null)
			{
				mainwindow.Close();
				mainwindow.Dispose();
			}

			// Close archives
			ArchiveManager.Dispose();

			// Delete the temporary directory
			if(General.temppath != "")
				try { Directory.Delete(General.temppath, true); } catch(Exception) { }

			// End of program
			Application.Exit();
		}

		#endregion

		#region ================== Configuration

		// Loads the settings configuration
		private static bool LoadConfiguration()
		{
			IDictionary weaponorder;

			// Check if settings configuration file exists
			if(File.Exists(configfilename))
			{
				// Load configuration
				config = new Configuration(configfilename, true);

				// Check for errors
				if(config.ErrorResult != 0)
				{
					// Error in configuration
					MessageBox.Show("Error in configuration file " + Path.GetFileName(configfilename) + " on line " + config.ErrorLine + ":\n" + config.ErrorDescription,
									Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
					return false;
				}

				// Apply configuration settings
				Direct3D.DisplayWidth = config.ReadSetting("displaywidth", 800);
				Direct3D.DisplayHeight = config.ReadSetting("displayheight", 600);
				Direct3D.DisplayFormat = config.ReadSetting("displayformat", (int)Format.X8R8G8B8);
				Direct3D.DisplayRefreshRate = config.ReadSetting("displayrate", 70);
				Direct3D.DisplayWindowed = config.ReadSetting("displaywindowed", false);
				Direct3D.DisplaySyncRefresh = config.ReadSetting("displaysync", true);
				Direct3D.DisplayFSAA = config.ReadSetting("displayfsaa", 0);
				Direct3D.DisplayGamma = config.ReadSetting("displaygamma", 0);
				General.playername = config.ReadSetting("playername", "Newbie");

				// Check if a weaponorder structure exists
				weaponorder = config.ReadSetting("weaponorder", new Hashtable());
				if(weaponorder.Count == 0)
				{
					// Make the default weapon order
					weaponorder = new ListDictionary();
					weaponorder.Add("2", "");
					weaponorder.Add("3", "");
					weaponorder.Add("1", "");
					weaponorder.Add("0", "");
					weaponorder.Add("5", "");
					weaponorder.Add("4", "");
					weaponorder.Add("6", "");

					// Save setting
					config.WriteSetting("weaponorder", weaponorder);
				}

				// Success
				return true;
			}
			else
			{
				// Cannot find configuration
				MessageBox.Show("Unable to load the configuration file " + Path.GetFileName(configfilename) + ".", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}
		}

		// Save settings to configuration
		public static void SaveConfiguration()
		{
			// Write settings that could have changed during the game
			config.WriteSetting("displaywidth", Direct3D.DisplayWidth);
			config.WriteSetting("displayheight", Direct3D.DisplayHeight);
			config.WriteSetting("displayformat", Direct3D.DisplayFormat);
			config.WriteSetting("displayrate", Direct3D.DisplayRefreshRate);
			config.WriteSetting("displaywindowed", Direct3D.DisplayWindowed);
			config.WriteSetting("displaysync", Direct3D.DisplaySyncRefresh);
			config.WriteSetting("displayfsaa", Direct3D.DisplayFSAA);
			config.WriteSetting("playername", General.playername);
			config.WriteSetting("displaygamma", Direct3D.DisplayGamma);

			// Save current configuration to file
			config.SaveConfiguration(configfilename);
		}

		#endregion

		#region ================== Logging

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
			log.WriteLine(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString());
			log.WriteLine(error.Source + " throws " + error.GetType().Name + ":");
			log.WriteLine(error.Message);
			log.WriteLine(error.StackTrace);
			log.WriteLine();
			log.WriteLine();

			// Close the file
			log.Close();
			log = null;
		}

		#endregion

		#region ================== Launcher

		// This launches bloodmasters
		public static void LaunchBloodmasters(Configuration cmdargs, string servfile)
		{
			// Show status
			mainwindow.ShowStatus("Launching game...");

			// Determine launch filename
			string launchfile = MakeUniqueFilename(apppath, "launch_", ".cfg");
			serverfile = servfile;

			// Write launch file
			cmdargs.SaveConfiguration(launchfile);

			// Make the process
			bmproc = new Process();
			bmproc.StartInfo.WorkingDirectory = General.apppath;
			bmproc.StartInfo.FileName = "Bloodmasters.exe";
			bmproc.StartInfo.Arguments = launchfile;
			bmproc.StartInfo.CreateNoWindow = false;
			bmproc.StartInfo.ErrorDialog = false;
			bmproc.StartInfo.UseShellExecute = false;
			bmproc.StartInfo.RedirectStandardError = true;
			bmproc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

			// Start the process
			if(bmproc.Start())
			{
				// Start waiting thread
				Thread bmt = new Thread(new ThreadStart(WaitForBloodmasters));
				bmt.Name = "Waiter:" + launchfile;
				bmt.Start();
				mainwindow.ShowStatus("Ready.");
			}
			else
			{
				// Show message, unable to launch
				mainwindow.ShowStatus("Ready.");
				MessageBox.Show(mainwindow, "Unable to launch Bloodmasters.",
						"Bloodmasters", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}

		// This waits for bloodmasters to finish
		public static void WaitForBloodmasters()
		{
			// Copy the required variables
			Process bmp = bmproc;
			string servfile = serverfile;

			// Wait for bmp to finish
			bmp.WaitForExit();

			// Remove launch file
			try { File.Delete(bmp.StartInfo.Arguments); }
			catch(Exception) { }

			// Remove server file
			if(servfile != "")
			{
				try { File.Delete(servfile); }
				catch(Exception) { }
			}

			// Check for errors
			Configuration errstr = new Configuration();
			errstr.InputConfiguration(bmp.StandardError.ReadToEnd());
			if(errstr.ReadSetting("message", "") != "")
			{
				// Advanced details available?
				if(errstr.ReadSetting("details", "") != "")
				{
					// Load the exception dialog
					FormError errdialog = new FormError();

					// Make title and message
					errdialog.lblTitle.Text += errstr.ReadSetting("exception", "") + ":";
					errdialog.lblMessage.Text = errstr.ReadSetting("message", "");

					// Make error report info
					errdialog.txtCallStack.Text = errstr.ReadSetting("details", "");

					// Show the dialog
					errdialog.txtCallStack.SelectionStart = 0;
					errdialog.ShowDialog(mainwindow);

					// Unload the dialog
					errdialog.Close();
					errdialog.Dispose();
					errdialog = null;
				}
				else
				{
					// Show messagebox only
					MessageBox.Show(mainwindow, errstr.ReadSetting("message", ""), "Bloodmaster", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}

			// Clean up
			bmp.Dispose();
		}

		#endregion

		#region ================== Misc Functions

		// This makes a unique filename
		public static string MakeUniqueFilename(string path, string prefix, string ext)
		{
			string filename;

			do
			{
				// Make random filename
				filename = Path.Combine(path, prefix + RandomString(6) + ext);
			}
			// Continue trying until a unique filename is found
			while(File.Exists(filename));

			// Return filename
			return filename;
		}

		// This validates a player name and returns
		// the problem as a description
		public static string ValidatePlayerName(string name)
		{
			string strippedname = General.StripColorCodes(name);

			// Check length
			if(strippedname.Length < 1)
			{
				// Name too long
				return "Please enter a valid player name.";
			}

			// Check actual length
			if(name.Length > Consts.MAX_PLAYER_NAME_STR_LEN)
			{
				// Name too long
				return "Player name, including color codes, may not be longer than " + Consts.MAX_PLAYER_NAME_STR_LEN + " characters.";
			}

			// Check length
			if(strippedname.Length > Consts.MAX_PLAYER_NAME_LEN)
			{
				// Name too long
				return "Player name may not be longer than " + Consts.MAX_PLAYER_NAME_LEN + " characters.";
			}

			// Check first character
			if(strippedname.StartsWith("#"))
			{
				// May not start with #
				return "Player name may not start with #.";
			}

			// Check if name contains any of the required characters
			if(strippedname.IndexOfAny(Consts.REQ_PLAYER_CHARS.ToCharArray()) == -1)
			{
				// Must contain an alphanumeric character
				return "Player name must contain at least one alphanumeric character.";
			}

			// Check for invalid characters
			if(name.IndexOf(',') != -1) return "Player name may not contain a comma.";
			if(name.IndexOf('\"') != -1) return "Player name may not contain quotes.";
			if(name.IndexOf('\'') != -1) return "Player name may not contain quotes.";

			// No problems
			return null;
		}

		// This returns the time in milliseconds
		public static int GetCurrentTime()
		{
			// Use standard clock
			return Environment.TickCount;
		}

		// This gets a description for a game type
		public static string GameTypeDescription(GAMETYPE g)
		{
			// Determine game type description
			switch(g)
			{
				case GAMETYPE.DM: return "Deathmatch";
				case GAMETYPE.CTF: return "Capture The Flag";
				case GAMETYPE.SC: return "Scavenger";
				case GAMETYPE.TDM: return "Team Deathmatch";
				case GAMETYPE.TSC: return "Team Scavenger";
				default: return "UNKNOWN GAME NAME";
			}
		}

		// This returns the next power of 2
		public static int NextPowerOf2(int v)
		{
			int p = 0;

			// Continue increasing until higher than v
			while(Math.Pow(2, p) < v) p++;

			// Return power
			return (int)Math.Pow(2, p);
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

		// This opens a URL in the default browser
		public static void OpenWebsite(string url)
		{
			RegistryKey key = null;
			Process p = null;
			string browser;

			try
			{
				// Get the registry key where default browser is stored
				key = Registry.ClassesRoot.OpenSubKey(@"HTTP\shell\open\command", false);

				// Trim off quotes
				browser = key.GetValue(null).ToString().ToLower().Replace("\"", "");

				// String doesnt end in EXE?
				if(!browser.EndsWith("exe"))
				{
					// Get rid of everything after the ".exe"
					browser = browser.Substring(0, browser.LastIndexOf(".exe") + 4);
				}
			}
			finally
			{
				// Clean up
				if(key != null) key.Close();
			}

			try
			{
				// Fork a process
				p = new Process();
				p.StartInfo.FileName = browser;
				p.StartInfo.Arguments = url;
				p.Start();
			}
			catch(Exception) { }

			// Clean up
			if(p != null) p.Dispose();
		}

		// This shows the exception dialog
		private static void ShowException(Exception ex)
		{
			// Load the exception dialog
			FormError errdialog = new FormError();

			// Make title and message
			errdialog.lblTitle.Text += ex.GetType().Name + ":";
			errdialog.lblMessage.Text = ex.Message;

			// Make error report info
			errdialog.txtCallStack.Text = errdialog.lblTitle.Text + "\r\n" +
										ex.Message + "\r\n" + ex.StackTrace;

			// Show the dialog
			errdialog.txtCallStack.SelectionStart = 0;
			errdialog.ShowDialog();

			// Unload the dialog
			errdialog.Close();
			errdialog.Dispose();
			errdialog = null;
		}

		#endregion
	}
}
