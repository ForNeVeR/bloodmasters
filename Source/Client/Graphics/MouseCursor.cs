/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

// This class provides functionality to render the mouse cursor.
// The static part of the class determines what cursor to render
// and has a Render function. Make objects from this class for
// different mouse cursors.

using System.Drawing;
using SharpDX.Direct3D9;

namespace CodeImp.Bloodmasters.Client
{
	internal sealed class MouseCursor
	{
		#region ================== Constants

		// Default size
		public const float CURSOR_SIZE = 0.02f;

		#endregion

		#region ================== Static Variables

		// Current mouse cursor
		private static MouseCursor current;
		private static TLVertex[] vertices = new TLVertex[4];

		// Available cursor
		public static MouseCursor Normal;

		#endregion

		#region ================== Variables

		// Texture
		private TextureResource texture;

		// Size
		private float cursorsize;

		// Color
		private static int color = -1;

		#endregion

		#region ================== Properties

		// This sets or gets the current cursor
		public static MouseCursor Current { get { return current; } set { current = value; } }
		public static int CursorColor { get { return color; } set { color = value; } }

		// This sets or gets the cursor size
		public float Size { get { return cursorsize; } set { cursorsize = value; } }

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public MouseCursor(string texturename, float size)
		{
			// Set the size
			cursorsize = size;

			// Create cursor texture
			texture = Direct3D.LoadTexture(ArchiveManager.ExtractFile("general.rar/" + texturename), false);
		}

		// Destructor
		~MouseCursor()
		{
			// Unload texture
			if(texture != null) texture.Dispose();
			texture = null;
		}

		#endregion

		#region ================== Methods

		// This sets up the mouse cursor
		public static void Initialize()
		{
			// Create lefttop
			vertices[0].color = Color.White.ToArgb();
			vertices[0].tu = 0f; //1f / 64f;
			vertices[0].tv = 0f; //1f / 64f;
			vertices[0].rhw = 1f;

			// Create leftbottom
			vertices[1].color = Color.White.ToArgb();
			vertices[1].tu = 0f; //1f / 64f;
			vertices[1].tv = 1f; //1f - (1f / 64f);
			vertices[1].rhw = 1f;

			// Create righttop
			vertices[2].color = Color.White.ToArgb();
			vertices[2].tu = 1f; //1f - (1f / 64f);
			vertices[2].tv = 0f; //1f / 64f;
			vertices[2].rhw = 1f;

			// Create rightbottom
			vertices[3].color = Color.White.ToArgb();
			vertices[3].tu = 1f; //1f - (1f / 64f);
			vertices[3].tv = 1f; //1f - (1f / 64f);
			vertices[3].rhw = 1f;


			// Load the available cursors here
			Normal = new MouseCursor("cursor_normal.tga", CURSOR_SIZE);


			// Set the default cursor
			Current = Normal;
		}

		// This cleans up
		public static void Terminate()
		{
			// Clean up cursors
			Normal = null;

			// Clean up current
			current = null;
		}

		// This renders the mouse cursor
		public static void Render()
		{
			// Get the mouse coordinates
			float x = (float)General.gamewindow.MouseX;
			float y = (float)General.gamewindow.MouseY;
			float halfsize = (float)Direct3D.DisplayWidth * current.cursorsize * 0.5f;

			// Adjust polygon coordinates and color
			vertices[0].x = x - halfsize - 0.5f;
			vertices[0].y = y - halfsize - 0.5f;
			vertices[0].color = color;
			vertices[1].x = x - halfsize - 0.5f;
			vertices[1].y = y + halfsize - 0.5f;
			vertices[1].color = color;
			vertices[2].x = x + halfsize - 0.5f;
			vertices[2].y = y - halfsize - 0.5f;
			vertices[2].color = color;
			vertices[3].x = x + halfsize - 0.5f;
			vertices[3].y = y + halfsize - 0.5f;
			vertices[3].color = color;

			// Render the poly
			Direct3D.SetDrawMode(DRAWMODE.TLMODALPHA);
			Direct3D.d3dd.SetTexture(0, current.texture.texture);
			Direct3D.d3dd.SetRenderState(RenderState.TextureFactor, -1);
			Direct3D.d3dd.DrawUserPrimitives(PrimitiveType.TriangleStrip, 2, vertices);
		}

		#endregion
	}
}
