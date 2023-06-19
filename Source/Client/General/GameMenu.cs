/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using System.Text;
using System.Drawing;
using System.Globalization;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;
using System.Windows.Forms;
using CodeImp.Bloodmasters;
using CodeImp;

namespace CodeImp.Bloodmasters.Client
{
	public class GameMenu
	{
		#region ================== Constants

		private const float TEXT_SIZE = 0.6f;
		private const float VIEWPORT_HEIGHT = 0.03f;
		private const float BORDER_SIZE = 0.05f;
		private const float TABLE_X = 0.3f;
		private const float TABLE_Y = 0.36f;
		private const float TABLE_WIDTH = 0.4f;
		private const float TABLE_HEIGHT = 0.4f;
		private const int ITEMS = 5;

		#endregion

		#region ================== Variables

		// Appearance
		private WindowBorder window;
		private TextResource[] item = new TextResource[ITEMS];
		private int[] state = new int[ITEMS];

		// Interaction
		private bool visible = false;

		#endregion

		#region ================== Properties

		public bool Visible { get { return visible; } set { visible = value; } }

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public GameMenu()
		{
			// Create window
			window = new WindowBorder(TABLE_X, TABLE_Y, TABLE_WIDTH, TABLE_HEIGHT, BORDER_SIZE);

			// Make the options
			item[0] = CreateTextResource("JOIN GAME", new RectangleF(TABLE_X, 0.44f, TABLE_WIDTH, VIEWPORT_HEIGHT));
			item[1] = CreateTextResource("JOIN RED TEAM", new RectangleF(TABLE_X, 0.48f, TABLE_WIDTH, VIEWPORT_HEIGHT));
			item[2] = CreateTextResource("JOIN BLUE TEAM", new RectangleF(TABLE_X, 0.52f, TABLE_WIDTH, VIEWPORT_HEIGHT));
			item[3] = CreateTextResource("SPECTATE", new RectangleF(TABLE_X, 0.56f, TABLE_WIDTH, VIEWPORT_HEIGHT));
			item[4] = CreateTextResource("EXIT", new RectangleF(TABLE_X, 0.64f, TABLE_WIDTH, VIEWPORT_HEIGHT));
		}

		// Dispose
		public void Dispose()
		{
			// Clean up
			foreach(TextResource tr in item) tr.Destroy();
			window.Dispose();
			GC.SuppressFinalize(this);
		}

		#endregion

		#region ================== Resource Management

		// This unloads all unstable resources
		public void UnloadResources()
		{
			// Clean up
			window.DestroyGeometry();
		}

		// This rebuilds unstable resources
		public void ReloadResources()
		{
			// Reload
			window.CreateGeometry();
		}

		// This sets up a standard font textresource
		private TextResource CreateTextResource(string text, RectangleF viewport)
		{
			// Make text resource
			TextResource t = Direct3D.CreateTextResource(General.charset_shaded);
			t.Texture = General.font_shaded.texture;
			t.HorizontalAlign = TextAlignX.Center;
			t.VerticalAlign = TextAlignY.Top;
			t.Viewport = viewport;
			t.Colors = TextResource.color_code[0];
			t.Scale = TEXT_SIZE;
			t.Text = text;
			return t;
		}

		#endregion

		#region ================== Processing

		// This checks if the mouse is inside the given viewport
		private bool MouseInside(RectangleF viewport)
		{
			// Determine scaled coordinates
			PointF sm = new PointF((float)General.gamewindow.Mouse.X / Direct3D.DisplayWidth,
			                       (float)General.gamewindow.Mouse.Y / Direct3D.DisplayHeight);

			// Check if intersection
			return ((sm.X >= viewport.Left) && (sm.X <= viewport.Right) &&
			        (sm.Y >= viewport.Top) && (sm.Y <= viewport.Bottom));
		}

		// This processes the console
		public void Process()
		{
			// Only when visible
			if(visible)
			{
				// Local client required
				if(General.localclient == null) return;

				// Determine menu item colors
				if((General.gamestate == GAMESTATE.GAMEFINISH) ||
				   (General.gamestate == GAMESTATE.ROUNDFINISH) ||
				   !General.localclient.IsSpectator) state[0] = 0;
				else if(MouseInside(item[0].Viewport)) state[0] = 1;
				else state[0] = 2;

				// Determine menu item colors
				if((General.gamestate == GAMESTATE.GAMEFINISH) ||
				   (General.gamestate == GAMESTATE.ROUNDFINISH) ||
				   General.teamgame == false ||
				   General.joinsmallestteam) state[1] = 0;
				else if(MouseInside(item[1].Viewport)) state[1] = 1;
				else state[1] = 2;

				// Determine menu item colors
				if((General.gamestate == GAMESTATE.GAMEFINISH) ||
				   (General.gamestate == GAMESTATE.ROUNDFINISH) ||
				   General.teamgame == false ||
				   General.joinsmallestteam) state[2] = 0;
				else if(MouseInside(item[2].Viewport)) state[2] = 1;
				else state[2] = 2;

				// Determine menu item colors
				if(General.localclient.IsSpectator) state[3] = 0;
				else if(MouseInside(item[3].Viewport)) state[3] = 1;
				else state[3] = 2;

				// Determine menu item colors
				if(MouseInside(item[4].Viewport)) state[4] = 1;
				else state[4] = 2;

				// Set item colors
				for(int i = 0; i < ITEMS; i++)
				{
					// Determine color by state
					switch(state[i])
					{
						case 0:
							item[i].Colors = TextResource.color_code[0];
							item[i].ModulateColor = General.ARGB(0.5f, 0.6f, 0.6f, 0.6f);
							break;
						case 1:
							item[i].Colors = TextResource.color_code[8];
							item[i].ModulateColor = General.ARGB(1f, 1f, 1f, 1f);
							break;
						case 2:
							item[i].Colors = TextResource.color_code[7];
							item[i].ModulateColor = General.ARGB(1f, 1f, 1f, 1f);
							break;
					}
				}

				// Check if mouse is being clicked
				if((General.gamewindow.MouseButtons == MouseButtons.Left) ||
				   (General.gamewindow.MouseButtons == MouseButtons.Right))
				{
					// Check if this item is selected
					if(state[0] == 1)
					{
						// Join game
						DirectSound.PlaySound("weaponswitch.wav");
						General.console.ProcessInput("/join game");
						this.visible = false;
					}

					// Check if this item is selected
					if(state[1] == 1)
					{
						// Join game
						DirectSound.PlaySound("weaponswitch.wav");
						General.console.ProcessInput("/join red");
						this.visible = false;
					}

					// Check if this item is selected
					if(state[2] == 1)
					{
						// Join game
						DirectSound.PlaySound("weaponswitch.wav");
						General.console.ProcessInput("/join blue");
						this.visible = false;
					}

					// Check if this item is selected
					if(state[3] == 1)
					{
						// Join game
						DirectSound.PlaySound("weaponswitch.wav");
						General.console.ProcessInput("/join spectators");
						this.visible = false;
					}

					// Check if this item is selected
					if(state[4] == 1)
					{
						// Join game
						DirectSound.PlaySound("weaponswitch.wav");
						General.console.ProcessInput("/exit");
						this.visible = false;
					}
				}
			}
		}

		#endregion

		#region ================== Rendering

		// This renders the console
		public void Render()
		{
			// Visible?
			if(visible)
			{
				// Render the window
				window.Render();

				// Render the options
				foreach(TextResource tr in item) tr.Render();
			}
		}

		#endregion
	}
}
