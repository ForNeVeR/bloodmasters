/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

// The Console shows messages and allows the user to input
// commands as well as processing the commands.

using System;
using System.Text;
using System.IO;
using System.Drawing;
using System.Globalization;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;
using System.Windows.Forms;
using CodeImp.Bloodmasters;
using CodeImp;
using CodeImp.Bloodmasters.Client.Graphics;
using SharpDX.Direct3D9;

namespace CodeImp.Bloodmasters.Client
{
	public class GConsole
	{
		#region ================== Constants

		private const int NUM_SCREEN_LINES = 5;
		private const int NUM_PANEL_LINES = 25;
		private const float LINE_HEIGHT = 0.02f;
		private const float LINES_OFFSET = 0.005f;
		private const float PANEL_BAR_HEIGHT = 0.05f;
		private const float PANEL_TEXTURE_REPEAT = 40f;
		private const string INPUT_PROMPT = ":: ";
		private const string INPUT_CURSOR = "^7_";
		private const string MORE_INDICATOR = "^0<^7more^0>";
		private const int PAGE_SCROLL_LINES = 5;
		private const int BUFFER_SIZE = 2000;

		#endregion

		#region ================== Variables

		// Lines on the screen
		private TextResource[] screenlines;
		private float[] linetimeout;

		// All lines in console
		private StringCollection alllines;

		// Lines in panel
		private TextResource[] panellines;
		private int paneloffset = -NUM_PANEL_LINES + 2;

		// Panel
		private VertexBuffer vertices;
		private bool panelopen = false;

		// Text input
		private TextResource panelinput = null;
		private string inputstr = "";

		// Settings
		public static int line_time = 5000;

		#endregion

		#region ================== Properties

		public bool PanelOpen
		{
			get { return panelopen; }
			set
			{
				// Opening panel?
				if(value)
				{
					// Make the lines
					DestroyPanelLines();
					MakePanelLines();
					panelopen = true;
				}
				else
				{
					// Destroy lines
					DestroyPanelLines();
					panelopen = false;
					inputstr = "";
				}
			}
		}

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public GConsole()
		{
			// Read settings
			line_time = General.config.ReadSetting("conlinetime", 5000);

			// Initialize screen lines
			screenlines = new TextResource[NUM_SCREEN_LINES];
			linetimeout = new float[NUM_SCREEN_LINES];
			for(int i = 0; i < NUM_SCREEN_LINES; i++) screenlines[i] = null;

			// Initialize panel lines
			panellines = new TextResource[NUM_PANEL_LINES];
			for(int i = 0; i < NUM_PANEL_LINES; i++) panellines[i] = null;

			// Initialize input resource
			panelinput = Direct3D.CreateTextResource(General.charset_shaded);
			panelinput.Texture = General.font_shaded.texture;
			panelinput.HorizontalAlign = TextAlignX.Left;
			panelinput.VerticalAlign = TextAlignY.Top;
			panelinput.Viewport = new RectangleF(LINES_OFFSET, LINES_OFFSET + LINE_HEIGHT * (float)NUM_PANEL_LINES, 1f, 0f);
			panelinput.Colors = TextResource.color_brighttext;
			panelinput.Scale = 0.4f;

			// Initialize all lines
			alllines = new StringCollection();
			alllines.Add("");

			// Show version information
			Version v = Assembly.GetExecutingAssembly().GetName().Version;
			alllines.Add("Bloodmasters client version " + v.ToString(4));

			// Initialize geometry
			CreateGeometry();
		}

		// Dispose
		public void Dispose()
		{
			// Dispose screen lines
			for(int i = 0; i < NUM_SCREEN_LINES; i++)
				if(screenlines[i] != null) screenlines[i].Destroy();

			// Clean up
			DestroyGeometry();
			screenlines = null;
			GC.SuppressFinalize(this);
		}

		#endregion

		#region ================== Resource Management

		// This unloads all unstable resources
		public void UnloadResources()
		{
			// Destroy vertices
			DestroyGeometry();
		}

		// This rebuilds unstable resources
		public void ReloadResources()
		{
			// Create vertices
			CreateGeometry();
		}

		#endregion

		#region ================== Geometry

		// This creates the generic item vertices
		public unsafe void CreateGeometry()
		{
			// Bottom coordintes for number of lines
			float linesheight = LINES_OFFSET + LINE_HEIGHT * NUM_PANEL_LINES;

			// Create vertex buffer
			vertices = new VertexBuffer(Direct3D.d3dd, sizeof(TLVertex) * 6,
				Usage.WriteOnly, TLVertex.Format, Pool.Default);

			// Lock vertex buffer
			var verts = vertices.Lock<TLVertex>(0, 6);

			// Lefttop
			verts[0].x = 0f;
			verts[0].y = 0f;
			verts[0].z = 0f;
			verts[0].tu = 0f;
			verts[0].tv = 0.015625f;
			verts[0].color = -1;
			verts[0].rhw = 1f;

			// Righttop
			verts[1].x = Direct3D.DisplayWidth;
			verts[1].y = 0f;
			verts[1].z = 0f;
			verts[1].tu = PANEL_TEXTURE_REPEAT;
			verts[1].tv = 0.015625f;
			verts[1].color = -1;
			verts[1].rhw = 1f;

			// Leftbottom
			verts[2].x = 0f;
			verts[2].y = linesheight * Direct3D.DisplayHeight;
			verts[2].z = 0f;
			verts[2].tu = 0f;
			verts[2].tv = 0.015625f;
			verts[2].color = -1;
			verts[2].rhw = 1f;

			// Rightbottom
			verts[3].x = Direct3D.DisplayWidth;
			verts[3].y = linesheight * Direct3D.DisplayHeight;
			verts[3].z = 0f;
			verts[3].tu = PANEL_TEXTURE_REPEAT;
			verts[3].tv = 0.015625f;
			verts[3].color = -1;
			verts[3].rhw = 1f;

			// Leftbottom
			verts[4].x = 0f;
			verts[4].y = (linesheight + PANEL_BAR_HEIGHT) * Direct3D.DisplayHeight;
			verts[4].z = 0f;
			verts[4].tu = 0f;
			verts[4].tv = 0.984375f;
			verts[4].color = -1;
			verts[4].rhw = 1f;

			// Rightbottom
			verts[5].x = Direct3D.DisplayWidth;
			verts[5].y = (linesheight + PANEL_BAR_HEIGHT) * Direct3D.DisplayHeight;
			verts[5].z = 0f;
			verts[5].tu = PANEL_TEXTURE_REPEAT;
			verts[5].tv = 0.984375f;
			verts[5].color = -1;
			verts[5].rhw = 1f;

			// Done filling the vertex buffer
			vertices.Unlock();
		}

		// This destroys the vertices
		public void DestroyGeometry()
		{
			if(vertices != null)
			{
				vertices.Dispose();
				vertices = null;
			}
		}

		#endregion

		#region ================== Messages

		// This destroys all panel lines
		private void DestroyPanelLines()
		{
			// Go for all lines
			for(int i = 0; i < NUM_PANEL_LINES; i++)
			{
				// Not null?
				if(panellines[i] != null)
				{
					// Destroy it
					panellines[i].Destroy();
					panellines[i] = null;
				}
			}
		}

		// This makes all panel lines
		private void MakePanelLines()
		{
			// Go for all lines
			for(int i = 0; i < NUM_PANEL_LINES; i++)
			{
				// Offset within range?
				if((paneloffset + i >= 0) && (paneloffset + i < alllines.Count))
				{
					// Inidcate more with last line?
					if((i == NUM_PANEL_LINES - 1) &&
					   (paneloffset < alllines.Count - NUM_PANEL_LINES))
					{
						// Indicate more
						panellines[i] = MakeMessage(MORE_INDICATOR, i);
					}
					else
					{
						// Make the line
						panellines[i] = MakeMessage(alllines[paneloffset + i], i);
					}
				}
			}
		}

		// This moves all screen lines in the array 1 up
		// The first item in the array will be lost
		// The last item in the array becomes null
		private void MoveScreenLinesUp()
		{
			// Go for all items except the last
			for(int i = 0; i < NUM_SCREEN_LINES - 1; i++)
			{
				// Destroy old line
				if(screenlines[i] != null) screenlines[i].Destroy();

				// Make new line if next line is not null
				if(screenlines[i + 1] != null)
					screenlines[i] = MakeMessage(screenlines[i + 1].Text, i);
				else
					screenlines[i] = null;

				// Move timeouts also
				linetimeout[i] = linetimeout[i + 1];
			}

			// Reset last item to null
			if(screenlines[NUM_SCREEN_LINES - 1] != null) screenlines[NUM_SCREEN_LINES - 1].Destroy();
			screenlines[NUM_SCREEN_LINES - 1] = null;
		}

		// This finds the first null index in the screenlines list
		// Returns the length of the array when no null items found
		private int FirstNullIndex()
		{
			// Go for all items and return the first null index
			for(int i = 0; i < NUM_SCREEN_LINES; i++)
				if(screenlines[i] == null) return i;

			// No null item found, return length of the array
			return screenlines.Length;
		}

		// This makes a TextResource for a message
		private TextResource MakeMessage(string msg, int pos)
		{
			// Make TextResource for message
			TextResource tr = Direct3D.CreateTextResource(General.charset_shaded);
			tr.Texture = General.font_shaded.texture;
			tr.HorizontalAlign = TextAlignX.Left;
			tr.VerticalAlign = TextAlignY.Top;
			tr.Viewport = new RectangleF(LINES_OFFSET, LINES_OFFSET + LINE_HEIGHT * (float)pos, 1f, 0f);
			tr.Colors = TextResource.color_brighttext;
			tr.Scale = 0.4f;
			tr.Text = msg;
			return tr;
		}

		// This adds a message
		public void AddMessage(string msg) { AddMessage(msg, true); }
		public void AddMessage(string msg, bool onscreen)
		{
			// Add the message to all lines
			alllines.Add(msg);

			// Exceeding buffer size?
			if(alllines.Count > BUFFER_SIZE)
			{
				// Remove a line
				alllines.RemoveAt(0);
			}
			else
			{
				// Scroll panel
				if(paneloffset + NUM_PANEL_LINES == alllines.Count - 1) paneloffset++;
			}

			// Rebuild panel lines if panel is open
			if(panelopen)
			{
				DestroyPanelLines();
				MakePanelLines();
			}

			// Show message on screen?
			if(onscreen)
			{
				// Go through the list to find the first null entry
				int idx = FirstNullIndex();

				// List full?
				if(idx == screenlines.Length)
				{
					// Move messages up and use last index
					MoveScreenLinesUp();
					idx--;
				}

				// Add message to the list
				screenlines[idx] = MakeMessage(msg, idx);
				linetimeout[idx] = General.currenttime + GConsole.line_time;
			}
		}

		#endregion

		#region ================== Processing

		// This processes the console
		public void Process()
		{
			string cursor = "";

			// Check if there is a top line on screen
			if(screenlines[0] != null)
			{
				// Line timeout?
				if(linetimeout[0] < General.currenttime)
				{
					// Move all lines up
					MoveScreenLinesUp();
				}
			}

			// Panel open?
			if(panelopen)
			{
				// Determine cursor
				if(General.currenttime % 300 < 150) cursor = INPUT_CURSOR;

				// Update the input resource
				panelinput.Text = INPUT_PROMPT + General.TrimColorCodes(inputstr) + cursor;
			}
		}

		// Special key pressed with console open
		public void SpecialKeyPressed(KeyEventArgs e)
		{
			// Check if this is ctrl+v
			if((e.KeyCode == Keys.V) && (e.Modifiers == Keys.Control))
			{
				// Get clipboard data information
				IDataObject data = Clipboard.GetDataObject();
				if(data.GetDataPresent(DataFormats.Text))
				{
					// Paste clipboard text
					inputstr += data.GetData(DataFormats.Text);
				}
			}
			// Check if this is backspace
			else if(e.KeyCode == Keys.Back)
			{
				// Control hold?
				if(e.Modifiers == Keys.Control)
				{
					// Erase all inpt
					inputstr = "";
				}
				else
				{
					// Check if there is any text
					if(inputstr.Length > 0)
					{
						// Remove last character
						inputstr = inputstr.Substring(0, inputstr.Length - 1);
					}
				}
			}
			// Check if this is pageup
			else if(e.KeyCode == Keys.PageUp)
			{
				// Scroll up
				if(paneloffset > 0)
				{
					paneloffset -= PAGE_SCROLL_LINES;
					if(paneloffset < 0) paneloffset = 0;
					DestroyPanelLines();
					MakePanelLines();
				}
			}
			// Check if this is pagedown
			else if(e.KeyCode == Keys.PageDown)
			{
				// Scroll down
				if(paneloffset < alllines.Count - NUM_PANEL_LINES)
				{
					paneloffset += PAGE_SCROLL_LINES;
					if(paneloffset > alllines.Count - NUM_PANEL_LINES) paneloffset = alllines.Count - NUM_PANEL_LINES;
					DestroyPanelLines();
					MakePanelLines();
				}
			}
		}

		// Key pressed with console open
		public void KeyPressed(KeyPressEventArgs e)
		{
			// Check if this is enter
			if(e.KeyChar == (char)Keys.Enter)
			{
				// Any possibly valid input given?
				if(inputstr.Trim().Length > 0)
				{
					// Doesnt start with / ?
					if(inputstr.StartsWith("/") == false)
					{
						// Then make this a say command
						inputstr = "/say " + inputstr;
					}

					// Handle input command
					ProcessInput(inputstr);
				}

				// Erase input
				inputstr = "";
			}
			// Check if this key exists in the charset
			// or this is the color code sign
			else if((panelinput.CharSet.Contains(e.KeyChar)) ||
					(e.KeyChar == panelinput.CharSet.ColorCodeChar))
			{
				// Make the new text string
				string newtext = inputstr + e.KeyChar.ToString();

				// Check if the new text fits in the box
				if(panelinput.CharSet.GetTextSize(newtext + INPUT_PROMPT + INPUT_CURSOR, panelinput.Scale).Width < Direct3D.DisplayWidth)
				{
					// Apply the new text string
					inputstr = newtext;
				}
			}
		}

		#endregion

		#region ================== Commands

		// This handles input commands
		public void ProcessInput(string cmdline)
		{
			string cmd, args;

			// Only process input when connection is available
			if(General.conn != null)
			{
				// If input does not begin with / then leave
				if(cmdline.StartsWith("/") == false)
				{
					// Invalid input given
					AddMessage("Invalid input given", false);
					return;
				}

				// Cut off the slash
				cmdline = cmdline.Substring(1);

				// Get the command
				int firstspace = cmdline.IndexOf(" ");
				if(firstspace == -1) firstspace = cmdline.Length;
				cmd = cmdline.Substring(0, firstspace).ToLower();
				cmd = General.StripColorCodes(cmd);

				// Arguments?
				if(firstspace + 1 > cmdline.Length)
				{
					// No arguments
					args = "";
				}
				else
				{
					// Get the arguments
					args = cmdline.Substring(firstspace + 1);
				}

				// Handle command
				switch(cmd)
				{
					case "echo": AddMessage(args); break;
					case "print": AddMessage(args); break;
					case "say": hSay(args); break;
					case "sayteam": hSayTeam(args); break;
					case "rcon": hRCon(args); break;
					case "exit": hExit(args); break;
					case "quit": hExit(args); break;
					case "disconnect": hExit(args); break;
					case "players": hPlayers(args); break;
					case "join": hJoin(args); break;
					case "simping": hSimPing(args); break;
					case "simloss": hSimLoss(args); break;
					case "dump": hDump(args); break;
					case "crash": hCrash(); break;
					case "kill": hKill(args); break;
					case "screenshot": hScreenshot(args); break;
					case "togglehud": hToggleHUD(args); break;
					case "name": hName(args); break;
					case "vote": hVote(args); break;
					case "callvote": hCallvote(args); break;
					default: AddMessage("Unknown command \"" + cmd + "\"", false); break;
				}
			}
		}

		// This handles the CALLVOTE command
		private void hCallvote(string args)
		{
			// Anything command given?
			if(General.StripColorCodes(args).Trim() != "")
			{
				// Send comamnd to server
				NetMessage msg = General.conn.CreateMessage(MsgCmd.CallvoteRequest, true);
				if(msg != null)
				{
					msg.AddData(args);
					msg.Send();
				}
			}
			else
			{
				// Show usage
				AddMessage("Usage:  /callvote ^0command", false);
				AddMessage("Call a vote to change the map:  /callvote map ^0mapname", false);
				AddMessage("Call a vote to restart the map:  /callvote restartmap", false);
				AddMessage("Call a vote to go to next map:  /callvote nextmap", false);
			}
		}

		// This handles the VOTE command
		private void hVote(string args)
		{
			// Vote in progress?
			if(General.callvotetimeout > 0)
			{
				// Send vote message to server
				NetMessage msg = General.conn.CreateMessage(MsgCmd.CallvoteSubmit, true);
				if(msg != null)
				{
					msg.Send();
				}
			}
			else
			{
				// No vote in progress
				AddMessage("No callvote is in progress.", true);
			}
		}

		// This handles the NAME command
		private void hName(string args)
		{
			// Anything to change name into?
			if(General.StripColorCodes(args).Trim() != "")
			{
				// Send rename message to server
				NetMessage msg = General.conn.CreateMessage(MsgCmd.PlayerNameChange, true);
				if(msg != null)
				{
					msg.AddData(args);
					msg.Send();
				}
			}
			else
			{
				// Show usage
				AddMessage("Usage:  /name ^0newname", false);
			}
		}

		// This handles the TOGGLEHUD command
		private void hToggleHUD(string args)
		{
			// Toggle HUD
			HUD.showhud = !HUD.showhud;
		}

		// This handles the SCREENSHOT command
		private void hScreenshot(string args)
		{
			// Make path and filename
			string filename = General.mapname + "_" + DateTime.Now.ToString("MM\\_dd\\_yyyy\\_HH\\_mm\\_ss") + ".png";
			string pathname = Path.Combine(General.apppath, "Screenshots");
			string filepathname = Path.Combine(pathname, filename);

			// Ensure screenshots directory exists
			if(!Directory.Exists(pathname)) Directory.CreateDirectory(pathname);

			// Save screenshot now
			Direct3D.SaveScreenshot(filepathname);
			AddMessage("Screenshot saved as " + filename, true);
		}

		// This handles the KILL command
		private void hKill(string args)
		{
			// No argument given?
			if(General.StripColorCodes(args).Trim() == "")
			{
				// Send message to server
				NetMessage msg = General.conn.CreateMessage(MsgCmd.Suicide, true);
				if(msg != null) msg.Send();
			}
			else
			{
				// Show usage
				AddMessage("No arguments, no \"" + General.StripColorCodes(args).Trim() + "\", just kill yourself!", false);
			}
		}

		// This handles the JOIN command
		private void hJoin(string args)
		{
			TEAM team;
			bool spect;

			// Determine team and spectator
			if("spectators".StartsWith(args.ToLower()) && (args.Length > 0))
			{
				team = TEAM.NONE;
				spect = true;
			}
			else if("game".StartsWith(args.ToLower()) && (args.Length > 0))
			{
				team = TEAM.NONE;
				spect = false;
			}
			else if("red".StartsWith(args.ToLower()) && (args.Length > 0))
			{
				team = TEAM.RED;
				spect = false;
			}
			else if("blue".StartsWith(args.ToLower()) && (args.Length > 0))
			{
				team = TEAM.BLUE;
				spect = false;
			}
			else
			{
				// Show usage
				AddMessage("Usage:  /join ^0team", false);
				AddMessage("Where ^0team^7 can be:  spectators, game, red, blue", false);
				return;
			}

			// Send the team change
			NetMessage msg = General.conn.CreateMessage(MsgCmd.ChangeTeam, true);
			if(msg != null)
			{
				msg.AddData((int)General.currenttime);
				msg.AddData((int)team);
				msg.AddData((bool)spect);
				msg.Send();
			}
		}

		// This handles the PLAYERS command
		private void hPlayers(string args)
		{
			string line;
			string teamname = "";

			// Go for all clients
			foreach(Client c in General.clients)
			{
				if(c != null)
				{
					// Determine team name
					switch(c.Team)
					{
						case TEAM.NONE: if(c.IsSpectator) teamname = "spectating"; else teamname = "playing"; break;
						case TEAM.RED: teamname = "^4red team^7"; break;
						case TEAM.BLUE: teamname = "^1blue team^7"; break;
					}

					// Make the line string
					line = c.ID.ToString();
					line = "#" + line + new string(' ', 6 - line.Length * 2);
					line += c.Name + "^7";
					line += "  (" + teamname + ")  ";
					line += "ping: " + c.Ping + "ms  ";
					line += "loss: " + c.Loss + "%";
					AddMessage(line, false);
				}
			}
		}

		// This crashes the game on purpose
		private void hCrash()
		{
			// Throw an exception
			General.OutputError(new Exception("Game crashed on purpose through console command."));
			hExit("");
		}

		// This handles the SAY command
		private void hSay(string args)
		{
			// Anything to say?
			if(General.StripColorCodes(args).Trim() != "")
			{
				// Send say message to server
				NetMessage msg = General.conn.CreateMessage(MsgCmd.SayMessage, true);
				if(msg != null)
				{
					msg.AddData(args);
					msg.Send();
				}
			}
			else
			{
				// Show usage
				AddMessage("Usage:  /say ^0message", false);
			}
		}

		// This handles the SAYTEAM command
		private void hSayTeam(string args)
		{
			// Anything to say?
			if(General.StripColorCodes(args).Trim() != "")
			{
				// Send say team message to server
				NetMessage msg = General.conn.CreateMessage(MsgCmd.SayTeamMessage, true);
				if(msg != null)
				{
					msg.AddData(args);
					msg.Send();
				}
			}
			else
			{
				// Show usage
				AddMessage("Usage:  /sayteam ^0message", false);
			}
		}

		// This handles the RCON command
		private void hRCon(string args)
		{
			// Anything to rcon?
			if(General.StripColorCodes(args).Trim() != "")
			{
				// Send rcon arguments to server
				NetMessage msg = General.conn.CreateMessage(MsgCmd.Command, true);
				if(msg != null)
				{
					msg.AddData(args);
					msg.Send();
				}
			}
			else
			{
				// Show usage
				AddMessage("Usage:  /rcon ^0command^7 [^0arguments^7]", false);
			}
		}

		// This handles the EXIT command
		private void hExit(string args)
		{
			// Close the window to end the game
			General.gamewindow.Close();
		}

		// This handles the SIMPING command
		private void hSimPing(string args)
		{
			int newvalue;

			// New ping given?
			if(General.StripColorCodes(args).Trim() != "")
			{
				try { newvalue = int.Parse(args); } catch(Exception) { newvalue = 0; }
				if(newvalue >= 0)
				{
					General.gateway.SimulatePing = newvalue;
					AddMessage("Ping simulation is now " + General.gateway.SimulatePing + "ms", false);
				}
			}
			else
			{
				// Show usage
				AddMessage("Usage:  /simping ^0milliseconds", false);
				AddMessage("Current ping simulation is " + General.gateway.SimulatePing + "ms", false);
				return;
			}
		}

		// This handles the SIMLOSS command
		private void hSimLoss(string args)
		{
			int newvalue;

			// New loss given?
			if(General.StripColorCodes(args).Trim() != "")
			{
				try { newvalue = int.Parse(args); } catch(Exception) { newvalue = 0; }
				if((newvalue >= 0) && (newvalue <= 100))
				{
					General.gateway.SimulateLoss = newvalue;
					AddMessage("Packetloss simulation is now " + General.gateway.SimulateLoss + "%", false);
				}
			}
			else
			{
				// Show usage
				AddMessage("Usage:  /simloss ^0percentage", false);
				AddMessage("Current packetloss simulation is " + General.gateway.SimulateLoss + "%", false);
				return;
			}
		}

		// This handles the DUMP command
		private void hDump(string args)
		{
			StreamWriter log = null;

			// Filename given?
			if(General.StripColorCodes(args).Trim() != "")
			{
				try
				{
					// When no path given, write to current directory
					Directory.SetCurrentDirectory(General.apppath);

					// Open the log file
					log = File.CreateText(args);

					// Go for all console lines
					foreach(string l in alllines)
					{
						// Output line to log file
						log.WriteLine(General.StripColorCodes(l));
					}

					// Close the log file
					log.Flush();
					log.Close();

					// Done
					AddMessage("Wrote console to log file \"" + args + "\".", false);
				}
				catch(Exception e)
				{
					if(log != null) log.Close();
					AddMessage(e.GetType().Name + " while dumping to log file: " + e.Message, false);
				}
			}
			else
			{
				// Show usage
				AddMessage("Usage:  /dump ^0filename", false);
				AddMessage("Dumps the console contents to a log file.", false);
				return;
			}
		}

		#endregion

		#region ================== Rendering

		// This renders the console
		public void Render()
		{
			// Set drawing mode
			Direct3D.SetDrawMode(DRAWMODE.TLMODALPHA);
			Direct3D.d3dd.SetRenderState(RenderState.TextureFactor, General.ARGB(1f, 1f, 1f, 1f));

			// Show the lines?
			if(HUD.showhud && !panelopen)
			{
				// Go for all screen messages
				for(int i = 0; i < NUM_SCREEN_LINES; i++)
				{
					// Render the message
					if(screenlines[i] != null) screenlines[i].Render();
				}
			}

			// Render the console?
			if(panelopen)
			{
				// Render the panel
				Direct3D.d3dd.SetTexture(0, General.console_edge.texture);
				Direct3D.d3dd.SetStreamSource(0, vertices, 0, TLVertex.Stride);
				Direct3D.d3dd.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 4);

				// Go for all panel messages
				for(int i = 0; i < NUM_PANEL_LINES; i++)
				{
					// Render the message
					if(panellines[i] != null) panellines[i].Render();
				}

				// Render the input line
				panelinput.Render();
			}
		}

		#endregion
	}
}
