/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

// The TextResource provides a way to display rasterized text
// from a font texture on the screen. Use the CreateTextResource
// function from the Direct3D class to create a text resource
// of this type.

using System;
using System.Text;
using System.Collections;
using System.Drawing;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using CodeImp.Bloodmasters;
using CodeImp;

namespace CodeImp.Bloodmasters.Client
{
	public sealed class TextResource : Resource
	{
		#region ================== Constants
		
		// Number of color code values
		private const int COLOR_CODE_VALS = 10;
		
		// Bright text color
		public static TextColors color_brighttext =
			new TextColors(General.RGB(255, 255, 255), General.RGB(255, 255, 255),
						   General.RGB(255, 255, 255), General.RGB(160, 160, 160));
		
		#endregion
		
		#region ================== Variables
		
		// The text is stored as a polygon in a vertex buffer
		public VertexBuffer textbuffer = null;
		public int numfaces = 0;
		
		// Texture resource to use for rendering
		private Texture texture;
		
		// Viewport in which the text is clipped and aligned
		private RectangleF viewport;
		
		// Information that makes up this text resource
		// This is needed to recreate the VertexBuffer after reset.
		private string text = "";
		private TextColors textcolors = new TextColors(Color.White.ToArgb(), Color.White.ToArgb(), Color.White.ToArgb(), Color.White.ToArgb());
		private CharSet charset;
		private float scale = 1;
		private int alignx = 0;
		private int aligny = 0;
		
		// Text size
		private SizeF textsize;
		
		// This keeps track if changes were made
		private bool updateneeded = true;
		
		// Color codes
		public static TextColors[] color_code = new TextColors[COLOR_CODE_VALS];
		public static byte[] color_code_vals = new byte[COLOR_CODE_VALS];
		private int modcolor = -1;
		
		#endregion
		
		#region ================== Properties
		
		// Properties
		public RectangleF Viewport { get { return viewport; } set { viewport = value; updateneeded = true; } }
		public float Left { get { return viewport.X; } set { viewport.X = value; updateneeded = true; } }
		public float Top { get { return viewport.Y; } set { viewport.Y = value; updateneeded = true; } }
		public float Width { get { return viewport.Width; } set { viewport.Width = value; updateneeded = true; } }
		public float Height { get { return viewport.Height; } set { viewport.Height = value; updateneeded = true; } }
		public float Right { get { return viewport.Right; } set { viewport.Width = value - viewport.X + 1f; updateneeded = true; } }
		public float Bottom { get { return viewport.Bottom; } set { viewport.Height = value - viewport.Y + 1f; updateneeded = true; } }
		public string Text { get { return text; } set { if(text != value) { text = value; updateneeded = true; } } }
		public float TextWidth { get { return textsize.Width; } }
		public float TextHeight { get { return textsize.Width; } }
		public SizeF TextSize { get { return textsize; } }
		public TextColors Colors { get { return textcolors; } set { if(!textcolors.Equals(value)) { textcolors = value; updateneeded = true; } } }
		public Texture Texture { get { return texture; } set { texture = value; } }
		public CharSet CharSet { get { return charset; } set { charset = value; updateneeded = true; } }
		public float Scale { get { return scale; } set { scale = value; updateneeded = true; } }
		public TextAlignX HorizontalAlign { get { return (TextAlignX)alignx; } set { alignx = (int)value; updateneeded = true; } }
		public TextAlignY VerticalAlign { get { return (TextAlignY)aligny; } set { aligny = (int)value; updateneeded = true; } }
		public int ModulateColor { get { return modcolor; } set { if(modcolor != value) { modcolor = value; updateneeded = true; } } }
		
		#endregion
		
		#region ================== Color Codes
		
		/*
		// This strips color codes from a string
		public static string StripColorCodes(string str, CharSet charset)
		{
			bool colorcode = false;
			int nb = 0;
			
			// If the charset does not use color codes
			// the result will stay unchanged anyway
			if(!charset.UsesColorCode) return str;
			
			// Get original bytes
			byte[] ostr = Encoding.ASCII.GetBytes(str);
			byte[] nstr = new byte[ostr.Length];
			
			// Go for all bytes
			for(int b = 0; b < ostr.Length; b++)
			{
				// Color code sign?
				if(ostr[b] == charset.ColorCodeByte)
				{
					// Skip this
					colorcode = true;
				}
				// Is this a color code value?
				else if(colorcode)
				{
					// Skip this too
					colorcode = false;
				}
				else
				{
					// Add byte
					nstr[nb++] = ostr[b];
				}
			}
			
			// Make final string
			return Encoding.ASCII.GetString(nstr, 0, nb);
		}
		*/
		
		// This initializes the color codes
		public static void Initialize()
		{
			// Black (dark)
			color_code_vals[0] = Encoding.ASCII.GetBytes("0")[0];
			color_code[0] = new TextColors(
				General.RGB(180, 180, 180), General.RGB(180, 180, 180),
				General.RGB(180, 180, 180), General.RGB(130, 130, 130));
			
			// Blue
			color_code_vals[1] = Encoding.ASCII.GetBytes("1")[0];
			color_code[1] = new TextColors(
				General.RGB(180, 180, 255), General.RGB(150, 150, 255),
				General.RGB(150, 150, 255), General.RGB(120, 120, 200));
			
			// Green
			color_code_vals[2] = Encoding.ASCII.GetBytes("2")[0];
			color_code[2] = new TextColors(
				General.RGB(180, 255, 180), General.RGB(150, 255, 150),
				General.RGB(150, 255, 150), General.RGB(120, 200, 120));
			
			// Cyan
			color_code_vals[3] = Encoding.ASCII.GetBytes("3")[0];
			color_code[3] = new TextColors(
				General.RGB(180, 255, 255), General.RGB(150, 255, 255),
				General.RGB(150, 255, 255), General.RGB(120, 200, 200));
			
			// Red
			color_code_vals[4] = Encoding.ASCII.GetBytes("4")[0];
			color_code[4] = new TextColors(
				General.RGB(255, 180, 150), General.RGB(255, 150, 130),
				General.RGB(255, 150, 130), General.RGB(200, 120, 120));
			
			// Magenta
			color_code_vals[5] = Encoding.ASCII.GetBytes("5")[0];
			color_code[5] = new TextColors(
				General.RGB(255, 180, 255), General.RGB(255, 150, 255),
				General.RGB(255, 150, 255), General.RGB(200, 120, 200));
			
			// Yellow
			color_code_vals[6] = Encoding.ASCII.GetBytes("6")[0];
			color_code[6] = new TextColors(
				General.RGB(255, 255, 180), General.RGB(255, 255, 150),
				General.RGB(255, 255, 150), General.RGB(200, 200, 120));
			
			// White
			color_code_vals[7] = Encoding.ASCII.GetBytes("7")[0];
			color_code[7] = new TextColors(
				General.RGB(255, 255, 255), General.RGB(255, 255, 255),
				General.RGB(255, 255, 255), General.RGB(160, 160, 160));
			
			// Orange
			color_code_vals[8] = Encoding.ASCII.GetBytes("8")[0];
			color_code[8] = new TextColors(
				General.RGB(255, 230, 120), General.RGB(255, 200, 110),
				General.RGB(255, 200, 110), General.RGB(200, 160, 60));
			
			// Pink
			color_code_vals[9] = Encoding.ASCII.GetBytes("9")[0];
			color_code[9] = new TextColors(
				General.RGB(255, 220, 220), General.RGB(255, 200, 200),
				General.RGB(255, 200, 200), General.RGB(200, 160, 160));
		}
		
		#endregion
		
		#region ================== Constructor / Destructor
		
		// Constructor
		public TextResource(CharSet textcharset, string referencename) : base(referencename)
		{
			// Set the charset
			charset = textcharset;
		}
		
		#endregion
		
		#region ================== Methods
		
		// This reloads the text if changed were made
		public void Prepare()
		{
			// Load if update needed
			if(updateneeded) this.Load();
		}
		
		// This renders the text manually
		public void Render()
		{
			// Check if there is text for rendering
			if(text.Length > 0)
			{
				// Prepare text for rendering
				this.Prepare();
				
				// Something to render?
				if(numfaces > 0)
				{
					// Set the texture
					Direct3D.d3dd.SetTexture(0, texture);
					
					// Render the text
					Direct3D.d3dd.SetStreamSource(0, textbuffer, 0, TLVertex.Stride);
					Direct3D.d3dd.DrawPrimitives(PrimitiveType.TriangleStrip, 0, numfaces);
				}
			}
		}
		
		// This formats and loads the text in the VertexBuffer
		// Call this after making changes to the text object to update the text
		public override void Load()
		{
			CharInfo charinfo;
			byte[] btext;
			float sizex = 0, sizey = 0;
			float beginx = 0, beginy = 0, nwidth = 0;
			RectangleF view;
			int v = 0;
			TextColors curcol = textcolors;
			bool colorcode = false;
			int charsmade = 0;
			
			// Check if a VertexBuffer is already created
			if(textbuffer != null)
			{
				// Trash the old VertexBuffer
				textbuffer.Dispose();
				textbuffer = null;
			}
			
			// Cannot make this when theres no text to render
			if(text.Length == 0) return;
			
			// Calculate the number of vertices
			int numvertices = General.StripColorCodes(text).Length * 4;
			
			// Cannot make this when theres no vertices
			numfaces = 0;
			if(numvertices == 0) return;
			
			// Create VertexBuffer
			textbuffer = new VertexBuffer(typeof(TLVertex), numvertices, Direct3D.d3dd,
										  Usage.WriteOnly | Usage.DoNotClip,
										  TLVertex.Format, Pool.Default);
			
			// Lock the buffer
			TLVertex[] vertices = (TLVertex[])textbuffer.Lock(0, typeof(TLVertex),
												LockFlags.None, numvertices);
			
			// Calculate the absolute viewport
			view = new RectangleF(viewport.X * Direct3D.DisplayWidth, viewport.Y * Direct3D.DisplayHeight,
								  viewport.Width * Direct3D.DisplayWidth, viewport.Height * Direct3D.DisplayHeight);
			
			// Calculate the text size
			textsize = charset.GetTextSize(text, scale);
			sizex = textsize.Width;
			sizey = textsize.Height;
			
			// Align the text horizontally
			switch(alignx)
			{
				case 0: beginx = view.X; break;
				case 1: beginx = view.X + (view.Width - sizex) / 2; break;
				case 2: beginx = view.X + view.Width - sizex; break;
			}
			
			// Align the text vertically
			switch(aligny)
			{
				case 0: beginy = view.Y; break;
				case 1: beginy = view.Y + (view.Height - sizey) / 2; break;
				case 2: beginy = view.Y + view.Height - sizey; break;
			}
			
			// Modulate color
			curcol.lt = ColorOperator.Modulate(curcol.lt, modcolor);
			curcol.lb = ColorOperator.Modulate(curcol.lb, modcolor);
			curcol.rt = ColorOperator.Modulate(curcol.rt, modcolor);
			curcol.rb = ColorOperator.Modulate(curcol.rb, modcolor);
			
			// Get the ASCII bytes for the text
			btext = Encoding.ASCII.GetBytes(text);
			
			// Go for all chars in text to create the polygon
			foreach(byte b in btext)
			{
				// Is this a color code sign?
				if((b == charset.ColorCodeByte) && charset.UsesColorCode)
				{
					// Next byte is the color code value
					colorcode = true;
				}
				// Is this a color code value?
				else if(colorcode)
				{
					// Switch colors
					for(int c = 0; c < COLOR_CODE_VALS; c++)
					{
						// Color we are looking for?
						if(b == color_code_vals[c])
						{
							// Select this color now
							curcol = color_code[c];
							
							// Modulate color
							curcol.lt = ColorOperator.Modulate(curcol.lt, modcolor);
							curcol.lb = ColorOperator.Modulate(curcol.lb, modcolor);
							curcol.rt = ColorOperator.Modulate(curcol.rt, modcolor);
							curcol.rb = ColorOperator.Modulate(curcol.rb, modcolor);
							break;
						}
					}
					
					// Done
					colorcode = false;
				}
				else
				{
					// Get the character information
					charinfo = charset.Chars[b];
					nwidth = charinfo.width * scale;
					
					// Create lefttop vertex
					vertices[v].color = curcol.lt;
					vertices[v].tu = charinfo.u1;
					vertices[v].tv = charinfo.v1;
					vertices[v].x = beginx;
					vertices[v].y = beginy;
					vertices[v].rhw = 1f;
					v++;
					
					// Create leftbottom vertex
					vertices[v].color = curcol.lb;
					vertices[v].tu = charinfo.u1;
					vertices[v].tv = charinfo.v2;
					vertices[v].x = beginx;
					vertices[v].y = beginy + sizey;
					vertices[v].rhw = 1f;
					v++;
					
					// Moving on
					beginx += nwidth;
					
					// Create righttop vertex
					vertices[v].color = curcol.rt;
					vertices[v].tu = charinfo.u2;
					vertices[v].tv = charinfo.v1;
					vertices[v].x = beginx;
					vertices[v].y = beginy;
					vertices[v].rhw = 1f;
					v++;
					
					// Create rightbottom vertex
					vertices[v].color = curcol.rb;
					vertices[v].tu = charinfo.u2;
					vertices[v].tv = charinfo.v2;
					vertices[v].x = beginx;
					vertices[v].y = beginy + sizey;
					vertices[v].rhw = 1f;
					v++;
					
					// Count character
					charsmade++;
				}
			}
			
			// Calculate number of triangles
			numfaces = charsmade * 4 - 2;
			
			// Done filling the vertex buffer
			textbuffer.Unlock();
			
			// Text updated
			updateneeded = false;
			
			// Inform the base class about this load
			base.Load();
		}
		
		// This unloads the resource
		public override void Unload()
		{
			// Unload the textbuffer
			if(textbuffer != null) textbuffer.Dispose();
			textbuffer = null;
			updateneeded = true;
			
			// Inform the base class about this unload
			base.Unload();
		}
		
		#endregion
	}
	
	// This struct is used to define the colors of a character
	public struct TextColors
	{
		public TextColors(int clt, int crt, int clb, int crb) { lt = clt; rt = crt; lb = clb; rb = crb; }
		public int lt;
		public int rt;
		public int lb;
		public int rb;
		
		public bool Equals(TextColors obj)
		{
			return (obj.lt == this.lt) && (obj.rt == this.rt) &&
			       (obj.lb == this.lb) && (obj.rb == this.rb);
		}
	}
	
	// This enumeration defines horizontal alignment
	public enum TextAlignX : int
	{
		Left = 0,
		Center = 1,
		Right = 2
	}
	
	// This enumeration defines vertical alignment
	public enum TextAlignY : int
	{
		Top = 0,
		Middle = 1,
		Bottom = 2
	}
}
