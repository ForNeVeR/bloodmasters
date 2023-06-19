/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

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
	public class ChatBox
	{
		#region ================== Constants

		private const float PANEL_BAR_HEIGHT = 0.05f;
		private const float PANEL_TEXTURE_REPEAT = 40f;
		private const string INPUT_CURSOR = "^7_";
		private const float INPUT_OFFSET_X = 0.12f;
		private const float INPUT_OFFSET_Y = 0.01f;
		private const float INPUT_HEIGHT = 0.015f;

		#endregion

		#region ================== Variables

		// Text input
		private TextResource prefix;
		private TextResource panelinput;
		private string inputstr = "";

		// Panel
		private VertexBuffer vertices;
		private bool panelopen = false;

		// Action
		private string command;

		#endregion

		#region ================== Properties

		public bool PanelOpen
		{
			get { return panelopen; }
		}

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public ChatBox()
		{
			// Initialize prefix resource
			prefix = Direct3D.CreateTextResource(General.charset_shaded);
			prefix.Texture = General.font_shaded.texture;
			prefix.HorizontalAlign = TextAlignX.Right;
			prefix.VerticalAlign = TextAlignY.Top;
			prefix.Viewport = new RectangleF(0, INPUT_OFFSET_Y, INPUT_OFFSET_X, 0f);
			prefix.Colors = TextResource.color_brighttext;
			prefix.Scale = 0.4f;

			// Initialize input resource
			panelinput = Direct3D.CreateTextResource(General.charset_shaded);
			panelinput.Texture = General.font_shaded.texture;
			panelinput.HorizontalAlign = TextAlignX.Left;
			panelinput.VerticalAlign = TextAlignY.Top;
			panelinput.Viewport = new RectangleF(INPUT_OFFSET_X, INPUT_OFFSET_Y, 1f - INPUT_OFFSET_X, 0f);
			panelinput.Colors = TextResource.color_brighttext;
			panelinput.Scale = 0.4f;

			// Initialize geometry
			CreateGeometry();
		}

		// Dispose
		public void Dispose()
		{
			// Clean up
			DestroyGeometry();
			prefix.Destroy();
			panelinput.Destroy();
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
			verts[2].y = INPUT_HEIGHT * Direct3D.DisplayHeight;
			verts[2].z = 0f;
			verts[2].tu = 0f;
			verts[2].tv = 0.015625f;
			verts[2].color = -1;
			verts[2].rhw = 1f;

			// Rightbottom
			verts[3].x = Direct3D.DisplayWidth;
			verts[3].y = INPUT_HEIGHT * Direct3D.DisplayHeight;
			verts[3].z = 0f;
			verts[3].tu = PANEL_TEXTURE_REPEAT;
			verts[3].tv = 0.015625f;
			verts[3].color = -1;
			verts[3].rhw = 1f;

			// Leftbottom
			verts[4].x = 0f;
			verts[4].y = (INPUT_HEIGHT + PANEL_BAR_HEIGHT) * Direct3D.DisplayHeight;
			verts[4].z = 0f;
			verts[4].tu = 0f;
			verts[4].tv = 0.984375f;
			verts[4].color = -1;
			verts[4].rhw = 1f;

			// Rightbottom
			verts[5].x = Direct3D.DisplayWidth;
			verts[5].y = (INPUT_HEIGHT + PANEL_BAR_HEIGHT) * Direct3D.DisplayHeight;
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

		#region ================== Processing

		// This processes the console
		public void Process()
		{
			string cursor = "";

			// Panel open?
			if(panelopen)
			{
				// Determine cursor
				if(General.currenttime % 300 < 150) cursor = INPUT_CURSOR;

				// Update the input resource
				panelinput.Text = General.TrimColorCodes(inputstr) + cursor;
			}
		}

		// This opens the chat box
		public void Show(string command, string description)
		{
			// Open chat box
			inputstr = "";
			panelopen = true;
			this.command = command;
			prefix.Text = description + ":  ";
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
					// Then make the command
					inputstr = command + " " + inputstr;

					// Handle input command
					General.console.ProcessInput(inputstr);
				}

				// Close chat box
				inputstr = "";
				panelopen = false;
			}
			// Check if this is ESC
			else if(e.KeyChar == (char)Keys.Escape)
			{
				// Close chat box
				inputstr = "";
				panelopen = false;
			}
			// Check if this key exists in the charset
			// or this is the color code sign
			else if((panelinput.CharSet.Contains(e.KeyChar)) ||
					(e.KeyChar == panelinput.CharSet.ColorCodeChar))
			{
				// Make the new text string
				string newtext = inputstr + e.KeyChar.ToString();

				// Check if the new text fits in the box
				if(panelinput.CharSet.GetTextSize(newtext + INPUT_CURSOR, panelinput.Scale).Width < (Direct3D.DisplayWidth * panelinput.Width))
				{
					// Apply the new text string
					inputstr = newtext;
				}
			}
		}

		#endregion

		#region ================== Rendering

		// This renders the console
		public void Render()
		{
			// Render the console?
			if(panelopen)
			{
				// Set drawing mode
				Direct3D.SetDrawMode(DRAWMODE.TLMODALPHA);
				Direct3D.d3dd.SetRenderState(RenderState.TextureFactor, -1);

				// Render the panel
				Direct3D.d3dd.SetTexture(0, General.console_edge.texture);
				Direct3D.d3dd.SetStreamSource(0, vertices, 0, TLVertex.Stride);
				Direct3D.d3dd.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 4);

				// Render the prefix and input
				prefix.Render();
				panelinput.Render();
			}
		}

		#endregion
	}
}
