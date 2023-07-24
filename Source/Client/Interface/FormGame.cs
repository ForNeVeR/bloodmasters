/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace CodeImp.Bloodmasters.Client
{
	internal sealed class FormGame : System.Windows.Forms.Form
	{
		// Mouse
		private int lastmousex = 0, lastmousey = 0;
		private bool mouseonwindow = false;
		private bool mouseinside = false;
		private MouseButtons mousebuttons = MouseButtons.None;

		// Keys
		private bool alt = false;
		private bool shift = false;
		private bool ctrl = false;
		private Dictionary<int, string> macrokeys = new();
		private Dictionary<int, string> controlkeys = new();
		private HashSet<string> pressedcontrols = new();

		// Properties
		public Point Mouse { get { return new Point(lastmousex, lastmousey); } }
		public int MouseX { get { return lastmousex; } }
		public int MouseY { get { return lastmousey; } }
		public bool MouseOnWindow { get { return mouseonwindow; } }
		public bool MouseInside { get { return mouseinside; } }
		public new MouseButtons MouseButtons { get { return mousebuttons; } }
		public bool AltPressed { get { return alt; } }
		public bool ShiftPressed { get { return shift; } }
		public bool CtrlPressed { get { return ctrl; } }

		// Constructor
		public FormGame(bool borders)
		{
			// Make borders?
			if(borders)
			{
				// Window with border
				this.FormBorderStyle = FormBorderStyle.FixedSingle;
				this.MaximizeBox = false;
				this.MinimizeBox = true;
				this.ControlBox = true;
			}
			else
			{
				// Window without border
				this.FormBorderStyle = FormBorderStyle.None;
				this.MaximizeBox = false;
				this.MinimizeBox = false;
				this.ControlBox = false;
			}

			// Set the window properties
			this.AutoScale = false;
			this.BackColor = Color.Black;
			this.ClientSize = new Size(800, 600);
			this.Cursor = Cursors.AppStarting;
			this.Name = "FormGame";
			this.StartPosition = FormStartPosition.CenterScreen;
			this.Text = "Bloodmasters";
			this.KeyPreview = true;
			this.SetStyle(ControlStyles.UserPaint, true);
			this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);

			// Load all control keys
			IDictionary ctrls = General.config.ReadSetting("controls", new Hashtable());
			foreach(DictionaryEntry de in ctrls)
			{
				// Add control
                controlkeys.TryAdd((int)de.Value, (string)de.Key);
			}

			// Load all macro keys
			IDictionary macros = General.config.ReadSetting("macros", new Hashtable());
			foreach(DictionaryEntry de in macros)
			{
                // Add macro
                if (int.TryParse((string)de.Key, CultureInfo.InvariantCulture, out int macroKey))
                {
                    macrokeys.TryAdd(macroKey, (string)de.Value);
                }
			}
		}

		// This returns true if the given control is pressed
		public bool ControlPressed(string controlname)
		{
			// If the control is pressed, its listed in pressedcontrols
			return pressedcontrols.Contains(controlname);
		}

		// This opens the chat box
		private void OpenChatBox()
		{
			// Chat key pressed?
			if(ControlPressed("say"))
			{
				// Show chat box
				General.chatbox.Show("/say", "Say to all");
				pressedcontrols.Clear();
			}
			// Team chat key pressed?
			else if(ControlPressed("sayteam"))
			{
				// Show chat box
				General.chatbox.Show("/sayteam", "Say to team");
				pressedcontrols.Clear();
			}
		}

		// This opens/closes the console when the console key is pressed
		private void OpenCloseConsole()
		{
			// Console key pressed?
			if(ControlPressed("console"))
			{
				// Open/clode console
				General.console.PanelOpen = !General.console.PanelOpen;

				// No keys pressed
				pressedcontrols.Clear();
			}
		}

		// This opens/closes the menu when the menu key is pressed
		private void OpenCloseMenu()
		{
			// Console key pressed?
			if(ControlPressed("exitgame"))
			{
				// While connecting?
				if(General.connecting)
				{
					// Terminate immediately
					this.Close();
				}
				else
				{
					// Open/clode menu
					General.gamemenu.Visible = !General.gamemenu.Visible;

					// No keys pressed
					pressedcontrols.Clear();
				}
			}
		}

		// This handles macros
		private bool HandleMacroKeys(int k)
		{
			// Pressed key is a macro?
			if(macrokeys.TryGetValue(k, out string macro))
			{
				// Execute macro now
				General.console.ProcessInput(macro);
				return true;
			}
			else
			{
				// No
				return false;
			}
		}

		// This handles impulse keys
		private void HandleImpulseKeys()
		{
			// Must have a local client
			if(General.localclient == null) return;

			// Join Team/Spectators?
			if(ControlPressed("joinspectators")) General.console.ProcessInput("/join spectators");
			if(ControlPressed("joingame")) General.console.ProcessInput("/join game");
			if(ControlPressed("joinred")) General.console.ProcessInput("/join red");
			if(ControlPressed("joinblue")) General.console.ProcessInput("/join blue");

			// Suicide
			if(ControlPressed("suicide")) General.console.ProcessInput("/kill");

			// Screenshot
			if(ControlPressed("screenshot")) General.console.ProcessInput("/screenshot");

			// Vote
			if(ControlPressed("voteyes")) General.console.ProcessInput("/vote");

			// Spectating?
			if(General.localclient.IsSpectator)
			{
				// Spectator keys
				if(ControlPressed("fireweapon") && (General.arena != null)) General.arena.SwitchSpectatorMode();
				if(ControlPressed("walkright") && (General.arena != null)) General.arena.SpectateNextPlayer();
				if(ControlPressed("walkleft") && (General.arena != null)) General.arena.SpectatePrevPlayer();
			}
			// Playing or not dead?
			else
			{
				// Switch weapons
				if(ControlPressed("weaponnext")) General.localclient.RequestSwitchWeaponNext(true);
				if(ControlPressed("weaponprev")) General.localclient.RequestSwitchWeaponNext(false);
				if(ControlPressed("usesmg")) General.localclient.RequestSwitchWeaponTo(WEAPON.SMG, true);
				if(ControlPressed("useminigun")) General.localclient.RequestSwitchWeaponTo(WEAPON.MINIGUN, true);
				if(ControlPressed("useplasma")) General.localclient.RequestSwitchWeaponTo(WEAPON.PLASMA, true);
				if(ControlPressed("userocket")) General.localclient.RequestSwitchWeaponTo(WEAPON.ROCKET_LAUNCHER, true);
				if(ControlPressed("usegrenades")) General.localclient.RequestSwitchWeaponTo(WEAPON.GRENADE_LAUNCHER, true);
				if(ControlPressed("usephoenix")) General.localclient.RequestSwitchWeaponTo(WEAPON.PHOENIX, true);
				if(ControlPressed("useion")) General.localclient.RequestSwitchWeaponTo(WEAPON.IONCANNON, true);

				// Fire powerup
				if(ControlPressed("firepowerup")) General.localclient.FirePowerup();
			}
		}

		// This sets the last mouse coordinates
		private void SetMouseCoordinates(int x, int y)
		{
			// Set the coordinates
			lastmousex = x;
			lastmousey = y;

			// Limit the coordinates
			if(lastmousex > Direct3D.DisplayWidth) lastmousex = Direct3D.DisplayWidth;
			if(lastmousex < 0) lastmousex = 0;
			if(lastmousey > Direct3D.DisplayHeight) lastmousey = Direct3D.DisplayHeight;
			if(lastmousey < 0) lastmousey = 0;
		}

		// Clean up any resources being used.
		protected override void Dispose(bool disposing)
		{
			// Release mouse cursor
			Cursor.Clip = Direct3D.ScreenClipRectangle;

			// Dispose
			base.Dispose(disposing);
		}

		// When the window is being closed
		protected override void OnClosing(CancelEventArgs e)
		{
			// Cancel the close and just hide window
			e.Cancel = true;
			//this.Hide();

			// Disconnect from server
			General.Disconnect(true);

			// Terminate client and server if running
			General.clientrunning = false;
			General.serverrunning = false;

			// Pass this event on to the base class
			base.OnClosing(e);
		}

		// When focus is received
		protected override void OnActivated(EventArgs e)
		{
			// Reset sounds
			DirectSound.ResetPositionalSounds();

			// Capture mouse cursor
			if(Direct3D.DisplayWindowed)
				Cursor.Clip = this.RectangleToScreen(this.ClientRectangle);
			else
				Cursor.Clip = new Rectangle(0, 0, Direct3D.DisplayWidth, Direct3D.DisplayHeight);
		}

		// When focus is lost
		protected override void OnDeactivate(EventArgs e)
		{
			// Release mouse cursor
			Cursor.Clip = Direct3D.ScreenClipRectangle;
		}

		// When mouse enters the window
		protected override void OnMouseEnter(EventArgs e)
		{
			// Mouse is inside the window
			mouseinside = true;

			// Pass this event on to the base class
			base.OnMouseEnter(e);
		}

		// When mouse leaves the window
		protected override void OnMouseLeave(EventArgs e)
		{
			// Mouse is now outside the window
			mouseinside = false;

			// Make arguments
			MouseEventArgs ex = new MouseEventArgs(MouseButtons.None, 0, -100, -100, 0);

			// Let the WindowManager handle this
			//WindowManager.OnMouseMove(ex);
			mouseonwindow = false;

			// Pass this event on to the base class
			base.OnMouseLeave(e);
		}

		// This handles mouse button presses
		protected override void OnMouseDown(MouseEventArgs e)
		{
			Keys mk = Keys.None;

			// Mouse is inside the window
			mouseinside = true;

			// Keep the last know mouse x and y
			SetMouseCoordinates(e.X, e.Y);
			mousebuttons |= e.Button;

			// Determine key code for mouse button
			switch(e.Button)
			{
				case MouseButtons.Left: mk = Keys.LButton; break;
				case MouseButtons.Right: mk = Keys.RButton; break;
				case MouseButtons.Middle: mk = Keys.MButton; break;
				case MouseButtons.XButton1: mk = Keys.XButton1; break;
				case MouseButtons.XButton2: mk = Keys.XButton2; break;
			}

			// Check if this key is configured
			if((mk != Keys.None) && (controlkeys.TryGetValue((int)mk, out string controlName)))
			{
				// No console or menu open?
				if(!General.console.PanelOpen && !General.chatbox.PanelOpen &&
				!General.gamemenu.Visible)
                {
                    // Key is now pressed
                    pressedcontrols.Add(controlName);

                    // Handle impulse keys
					HandleImpulseKeys();
                }
			}

			// Handle macro key
			HandleMacroKeys((int)mk);

			// Open or close something?
			OpenCloseConsole();
			OpenChatBox();
			OpenCloseMenu();

			// Pass this event on to the base class
			base.OnMouseDown(e);
		}

		// This handles mouse button releases
		protected override void OnMouseUp(MouseEventArgs e)
		{
			Keys mk = Keys.None;

			// Mouse is inside the window
			mouseinside = true;

			// Keep the last know mouse x and y
			SetMouseCoordinates(e.X, e.Y);
			mousebuttons &= ~e.Button;

			// No console or menu open?
			if(!General.console.PanelOpen && !General.chatbox.PanelOpen &&
			   !General.gamemenu.Visible)
			{
				// Determine key code for mouse button
				switch(e.Button)
				{
					case MouseButtons.Left: mk = Keys.LButton; break;
					case MouseButtons.Right: mk = Keys.RButton; break;
					case MouseButtons.Middle: mk = Keys.MButton; break;
					case MouseButtons.XButton1: mk = Keys.XButton1; break;
					case MouseButtons.XButton2: mk = Keys.XButton2; break;
				}

				// Check if this key is configured
				if((mk != Keys.None) && (controlkeys.TryGetValue((int)mk, out string controlName)))
				{
					// Key is now released
                    pressedcontrols.Remove(controlName);
                }
			}

			// Pass this event on to the base class
			base.OnMouseUp(e);
		}

		// This handles mouse moves
		protected override void OnMouseMove(MouseEventArgs e)
		{
			// Mouse is inside the window
			mouseinside = true;

			// Keep the last know mouse x and y
			SetMouseCoordinates(e.X, e.Y);

			// Pass this event on to the base class
			base.OnMouseMove(e);
		}

		// This handles mouse wheel changes
		protected override void OnMouseWheel(MouseEventArgs e)
		{
			int count;
			KeyEventArgs k;
			int keycode;

			// Must have a local client
			if(General.localclient == null) return;

			// Not when menu is open
			if(General.gamemenu.Visible) return;

			// Determine count
			if(e.Delta > 0) count = e.Delta / 120; else count = -e.Delta / 120;

			// Go for the number of ticks scrolled
			for(int i = 0; i < count; i++)
			{
				// Console open?
				if(General.console.PanelOpen)
				{
					// Determine equivalent key
					if(e.Delta > 0) k = new KeyEventArgs(Keys.PageUp);
					else k = new KeyEventArgs(Keys.PageDown);

					// Send page up keys to console
					General.console.SpecialKeyPressed(k);
				}
				// Switch weapons?
				else if(General.scrollweapons)
				{
					// Scroll weapons forward or backward
					General.localclient.RequestSwitchWeaponNext(e.Delta > 0);
				}
				else
				{
					// Determine equivalent key
					if(e.Delta > 0) keycode = (int)EXTRAKEYS.MScrollUp;
					else keycode = (int)EXTRAKEYS.MScrollDown;

                    bool pressedConfiguredKey = controlkeys.TryGetValue(keycode, out string configuredControlName);

					// Check if this key is configured
					if(pressedConfiguredKey)
                    {
                        // Key is now pressed
                        pressedcontrols.Add(configuredControlName);
                    }

					// Handle impulse keys
					HandleImpulseKeys();

					// Handle macro key
					HandleMacroKeys(keycode);

					// Check if this key is configured
					if(pressedConfiguredKey)
					{
						// Key is now released
                        pressedcontrols.Remove(configuredControlName);
                    }
				}
			}

			// Pass this event on to the base class
			base.OnMouseWheel(e);
		}

		// This handles key presses (keydown)
		protected override void OnKeyDown(KeyEventArgs e)
		{
			// Save the shift status
			alt = e.Alt;
			shift = e.Shift;
			ctrl = e.Control;

            bool pressedConfiguredKey = controlkeys.TryGetValue((int)e.KeyCode, out string configuredControlName);

			// Console open?
			if(General.console.PanelOpen)
			{
				// Check if this key is configured
				if(pressedConfiguredKey)
				{
					// Check if not the console key
					if(configuredControlName != "console")
					{
						// Pass keys on to console
						General.console.SpecialKeyPressed(e);
						return;
					}
				}
				else
				{
					// Pass keys on to console
					General.console.SpecialKeyPressed(e);
					return;
				}
			}

			// Chatbox open?
			if(General.chatbox.PanelOpen)
			{
				// Pass keys on to chatbox
				General.chatbox.SpecialKeyPressed(e);
				return;
			}

			// Menu open?
			if(General.gamemenu.Visible)
			{
				// Check if this key is configured
				if(pressedConfiguredKey)
				{
					// Check if not the menu key
					if(configuredControlName != "exitgame")
					{
						// Pass keys on to menu
						//General.console.SpecialKeyPressed(e);
						return;
					}
				}
				else
				{
					// Pass keys on to menu
					//General.console.SpecialKeyPressed(e);
					return;
				}
			}

			// Check if this key is configured
			if(pressedConfiguredKey)
            {
                // Key is now pressed
                pressedcontrols.Add(configuredControlName);

                // Handle impulse keys
				HandleImpulseKeys();
            }

			// Handle macro key
			HandleMacroKeys((int)e.KeyCode);

			// Open or close something?
			OpenCloseConsole();
			OpenCloseMenu();

			// Key always handled
			e.Handled = true;

			// Pass this event on to the base class
			base.OnKeyDown(e);
		}

		// This handles key presses
		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			// Console open?
			if(General.console.PanelOpen)
			{
				// Pass keypress on to console
				General.console.KeyPressed(e);
			}
			// Chatbox open?
			else if(General.chatbox.PanelOpen)
			{
				// Pass keypress on to chatbox
				General.chatbox.KeyPressed(e);
			}

			// Open or close something?
			OpenChatBox();

			// Key always handled
			e.Handled = true;

			// Pass this event on to the base class
			base.OnKeyPress(e);
		}

		// This handles key releases
		protected override void OnKeyUp(KeyEventArgs e)
		{
			// Save the shift status
			alt = e.Alt;
			shift = e.Shift;
			ctrl = e.Control;

			// Check if this key is configured
			if(controlkeys.TryGetValue((int)e.KeyCode, out string controlName))
			{
				// Key is now released
                pressedcontrols.Remove(controlName);
            }

			// Key always handled
			e.Handled = true;

			// Pass this event on to the base class
			base.OnKeyUp(e);
		}
	}
}
