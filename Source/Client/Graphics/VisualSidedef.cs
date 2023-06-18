/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using System.Collections;
using CodeImp.Bloodmasters.Client.Graphics;

namespace CodeImp.Bloodmasters.Client
{
	public class VisualSidedef
	{
		#region ================== Constants
		
		public const int LIGHTMAP_HEIGHT = 1;
		public const float LIGHTMAP_BIAS_LENGTH = 0.2f;
		public const float TEXTURE_SCALE = 2f;
		
		#endregion
		
		#region ================== Variables
		
		// Reference to the map
		private Sidedef sidedef;
		private VisualSector sector;
		
		// Vertices
		private int vstart, vend;
		
		// Textures
		private TextureResource tlower = null;
		private TextureResource tmiddle = null;
		private TextureResource tupper = null;
		
		// Wall part heights
		private float lowerbottom = 0f;
		private float lowertop = 0f;
		private float middlebottom = 0f;
		private float middletop = 0f;
		private float upperbottom = 0f;
		private float uppertop = 0f;
		
		// Lightmap
		private float lightmapcoordtop;		// pixels
		private float lightmapcoordbottom;	// pixels
		private float lightmapcoordmiddle;  // texture coords
		
		// Color for debugging
		private int debugcolor = 0;
		
		// Geometry info
		private int lowervertexoffset = -1;
		private int middlevertexoffset = -1;
		private int uppervertexoffset = -1;
		
		#endregion
		
		#region ================== Properties
		
		public Sidedef Sidedef { get { return sidedef; } }
		public bool HasLower { get { return (tlower != null) && (sidedef.OtherSide != null) && (lowertop > lowerbottom); } }
		public bool HasMiddle { get { return (tmiddle != null) && (middletop > middlebottom); } }
		public bool HasUpper { get { return (tupper != null) && (sidedef.OtherSide != null) && (uppertop > upperbottom) && (sidedef.OtherSide.Sector.TextureCeil != Sector.NO_FLAT); } }
		public float LowerBottom { get { return lowerbottom; } }
		public float LowerTop { get { return lowertop; } }
		public float MiddleBottom { get { return middlebottom; } }
		public float MiddleTop { get { return middletop; } }
		public float UpperBottom { get { return upperbottom; } }
		public float UpperTop { get { return uppertop; } }
		public VisualSector VisualSector { get { return sector; } set { sector = value; } }
		public float LightmapCoordsTop { get { return lightmapcoordtop; } set { lightmapcoordtop = value; } }
		public float LightmapCoordsBottom { get { return lightmapcoordbottom; } set { lightmapcoordbottom = value; } }
		public float LightmapCoordsMiddle { get { return lightmapcoordmiddle; } set { lightmapcoordmiddle = value; } }
		
		#endregion
		
		#region ================== Constructor / Destructor
		
		// Constructor
		public VisualSidedef(Sidedef sidedef, VisualSector sector)
		{
			string texname, texarch;
			
			// Make references
			this.sidedef = sidedef;
			this.sector = sector;
			sidedef.VisualSidedef = this;
			
			// Determine start and end vertex
			if(sidedef.IsFront)
			{
				vstart = sidedef.Linedef.v1;
				vend = sidedef.Linedef.v2;
			}
			else
			{
				vend = sidedef.Linedef.v1;
				vstart = sidedef.Linedef.v2;
			}
			
			// Color for debugging
			debugcolor = General.RandomColor();
			debugcolor = ColorOperator.Scale(debugcolor, 1.6f);
			debugcolor = ColorOperator.AdjustSaturation(debugcolor, 2);
			
			// Find the middle texture
			texname = sidedef.TextureMiddle + ".bmp";
			texarch = ArchiveManager.FindFileArchive(texname);
			if(texarch != "")
			{
				// Load the middle texture
				string tempfile = ArchiveManager.ExtractFile(texarch + "/" + texname);
				tmiddle = Direct3D.LoadTexture(tempfile, true, !Direct3D.hightextures);
			}
			else
			{
				// If the bmp does not exist, find a tga file
				texname = sidedef.TextureMiddle + ".tga";
				texarch = ArchiveManager.FindFileArchive(texname);
				if(texarch != "")
				{
					// Load the middle texture
					string tempfile = ArchiveManager.ExtractFile(texarch + "/" + texname);
					tmiddle = Direct3D.LoadTexture(tempfile, true, !Direct3D.hightextures);
				}
			}
			
			// Find the lower texture
			texname = sidedef.TextureLower + ".bmp";
			texarch = ArchiveManager.FindFileArchive(texname);
			if(texarch != "")
			{
				// Load the lower texture
				string tempfile = ArchiveManager.ExtractFile(texarch + "/" + texname);
				tlower = Direct3D.LoadTexture(tempfile, true, !Direct3D.hightextures);
			}
			
			// Find the upper texture
			texname = sidedef.TextureUpper + ".bmp";
			texarch = ArchiveManager.FindFileArchive(texname);
			if(texarch != "")
			{
				// Load the lower texture
				string tempfile = ArchiveManager.ExtractFile(texarch + "/" + texname);
				tupper = Direct3D.LoadTexture(tempfile, true, !Direct3D.hightextures);
			}
		}
		
		// Destructor
		public void Dispose()
		{
			// Clean up
			tlower = null;
			tmiddle = null;
			tupper = null;
			sidedef = null;
			GC.SuppressFinalize(this);
		}
		
		#endregion
		
		#region ================== Geometry
		
		// This adds a quad of vertices to an array
		private void AddVertexQuad(ArrayList list, int v1, int v2, float top, float bot,
									float tl, float tt, float tr, float tb,
									int begincolor, int endcolor)
		{
			MVertex v;
			float nx, ny;
			
			// Determine front normal
			if(sidedef.IsFront)
			{
				// Normal adjustment to the front
				nx = sidedef.Linedef.nY * LIGHTMAP_BIAS_LENGTH;
				ny = sidedef.Linedef.nX * LIGHTMAP_BIAS_LENGTH;
			}
			else
			{
				// Normal adjustment to the back
				nx = -sidedef.Linedef.nY * LIGHTMAP_BIAS_LENGTH;
				ny = -sidedef.Linedef.nX * LIGHTMAP_BIAS_LENGTH;
			}
			
			// Create lefttop vertex
			v = new MVertex();
			v.color = begincolor;
			v.x = General.map.Vertices[v1].x;
			v.y = General.map.Vertices[v1].y;
			v.z = top;
			v.t1u = tl;
			v.t1v = tt;
			v.t2u = 0f;
			v.t2v = lightmapcoordmiddle;
			v.t3u = General.map.Vertices[v1].x + nx;
			v.t3v = General.map.Vertices[v1].y - ny;
			list.Add(v);
			
			// Create righttop vertex
			v = new MVertex();
			v.color = begincolor;
			v.x = General.map.Vertices[v2].x;
			v.y = General.map.Vertices[v2].y;
			v.z = top;
			v.t1u = tr;
			v.t1v = tt;
			v.t2u = 1f;
			v.t2v = lightmapcoordmiddle;
			v.t3u = General.map.Vertices[v2].x + nx;
			v.t3v = General.map.Vertices[v2].y - ny;
			list.Add(v);
			
			// Create leftbottom vertex
			v = new MVertex();
			v.color = endcolor;
			v.x = General.map.Vertices[v1].x;
			v.y = General.map.Vertices[v1].y;
			v.z = bot;
			v.t1u = tl;
			v.t1v = tb;
			v.t2u = 0f;
			v.t2v = lightmapcoordmiddle;
			v.t3u = General.map.Vertices[v1].x + nx;
			v.t3v = General.map.Vertices[v1].y - ny;
			list.Add(v);
			
			// Create rightbottom vertex
			v = new MVertex();
			v.color = endcolor;
			v.x = General.map.Vertices[v2].x;
			v.y = General.map.Vertices[v2].y;
			v.z = bot;
			v.t1u = tr;
			v.t1v = tb;
			v.t2u = 1f;
			v.t2v = lightmapcoordmiddle;
			v.t3u = General.map.Vertices[v2].x + nx;
			v.t3v = General.map.Vertices[v2].y - ny;
			list.Add(v);
		}
		
		// This builds middle wall vertices
		public ArrayList MakeMiddleWall(int vertexoffset)
		{
			float ttop, tbottom, tleft, tright;
			int endcolor = -1;
			int begincolor = -1;
			float tdelta;
			
			// This will temporarely hold the vertices
			ArrayList verts = new ArrayList();
			
			// Check if a middle wall exists
			if(tmiddle != null)
			{
				// If this line is a black gradient
				if(sidedef.Linedef.Action == ACTION.BLACK_GRADIENT)
				{
					// Fade to black
					endcolor = General.ARGB(1f, 0f, 0f, 0f);
					begincolor = General.RGB(sidedef.Linedef.Arg[0],
											 sidedef.Linedef.Arg[0],
											 sidedef.Linedef.Arg[0]);
				}
				
				// Determine top height
				if(sidedef.OtherSide == null)
				{
					// Nothing on the other side,
					// top height is this ceilings height
					middletop = sidedef.Sector.HeightCeil;
				}
				else
				{
					// Top height is lowest of both ceilings
					if(sidedef.Sector.HeightCeil < sidedef.OtherSide.Sector.HeightCeil)
						middletop = sidedef.Sector.HeightCeil;
					else
						middletop = sidedef.OtherSide.Sector.HeightCeil;
				}
				
				// Determine bottom height
				if(sidedef.OtherSide == null)
				{
					// Nothing on the other side,
					// bottom height is this floor height
					middlebottom = sidedef.Sector.HeightFloor;
				}
				else
				{
					// Bottom height is highest of both floors
					if(sidedef.Sector.HeightFloor > sidedef.OtherSide.Sector.HeightFloor)
						middlebottom = sidedef.Sector.HeightFloor;
					else
						middlebottom = sidedef.OtherSide.Sector.HeightFloor;
				}
				
				// Determine texture delta height
				tdelta = ((middletop - middlebottom) * Map.INV_MAP_SCALE_Z * TEXTURE_SCALE);
				if(tdelta > tmiddle.info.Height) tdelta = tmiddle.info.Height;
				
				// Texture coordinates
				tleft = sidedef.TextureX / tmiddle.info.Width;
				ttop = sidedef.TextureY / tmiddle.info.Height;
				tright = tleft + (sidedef.Length * Map.INV_MAP_SCALE_XY * TEXTURE_SCALE) / tmiddle.info.Width;
				tbottom = ttop + tdelta / tmiddle.info.Height;
				
				// Make vertices and add them to the list
				AddVertexQuad(verts, vstart, vend, middletop, middlebottom,
					tleft, ttop, tright, tbottom, begincolor, endcolor);
				
				// Keep the offset to vertices
				middlevertexoffset = vertexoffset;
			}
			
			// Return result
			return verts;
		}
		
		// This builds lower wall vertices
		public ArrayList MakeLowerWall(int vertexoffset)
		{
			float ttop, tbottom, tleft, tright;
			int endcolor = -1;
			int begincolor = -1;
			
			// This will temporarely hold the vertices
			ArrayList verts = new ArrayList();
			
			// Check if a lower wall is possible
			if((tlower != null) && (sidedef.OtherSide != null))
			{
				// Top height is the height of the other sector
				lowertop = sidedef.OtherSide.Sector.HeightFloor;
				
				// Bottom height is the lowest possible height of this sector
				lowerbottom = sidedef.Sector.LowestFloor;
				
				// Check if lower wall is visible
				if(lowertop > lowerbottom)
				{
					// If this line is black gradient
					if(sidedef.Linedef.Action == ACTION.BLACK_GRADIENT)
					{
						// Fade to black
						endcolor = General.ARGB(1f, 0f, 0f, 0f);
						begincolor = General.RGB(sidedef.Linedef.Arg[0],
												 sidedef.Linedef.Arg[0],
												 sidedef.Linedef.Arg[0]);
					}
					
					// Texture coordinates
					tbottom = sidedef.TextureY / tlower.info.Height;
					tleft = sidedef.TextureX / tlower.info.Width;
					ttop = tbottom - ((lowertop - lowerbottom) * Map.INV_MAP_SCALE_Z * TEXTURE_SCALE) / tlower.info.Height;
					tright = tleft + (sidedef.Length * Map.INV_MAP_SCALE_XY * TEXTURE_SCALE) / tlower.info.Width;
					
					// Make vertices and add them to the list
					AddVertexQuad(verts, vstart, vend, lowertop, lowerbottom,
						tleft, ttop, tright, tbottom, begincolor, endcolor);
					
					// Keep the offset to vertices
					lowervertexoffset = vertexoffset;
				}
			}
			
			// Return result
			return verts;
		}
		
		// This builds upper wall vertices
		public ArrayList MakeUpperWall(int vertexoffset)
		{
			float ttop, tbottom, tleft, tright;
			float topdiff;
			
			// This will temporarely hold the vertices
			ArrayList verts = new ArrayList();
			
			// Check if a upper wall is possible
			if((tupper != null) && (sidedef.OtherSide != null))
			{
				// Top height is the height of this sector
				uppertop = sidedef.Sector.HeightCeil;
				
				// Bottom height is the height of the other sector
				upperbottom = sidedef.OtherSide.Sector.HeightCeil;
				
				// Check if upper wall is visible
				if((uppertop > upperbottom) && (sidedef.OtherSide.Sector.TextureCeil != Sector.NO_FLAT))
				{
					// Texture coordinates
					topdiff = (uppertop - upperbottom) * Map.INV_MAP_SCALE_Z * TEXTURE_SCALE;
					tleft = sidedef.TextureX / tupper.info.Width;
					ttop = (sidedef.TextureY - topdiff) / tupper.info.Height;
					tright = tleft + (sidedef.Length * Map.INV_MAP_SCALE_XY * TEXTURE_SCALE) / tupper.info.Width;
					tbottom = ttop + topdiff / tupper.info.Height;
					
					// Make vertices and add them to the list
					AddVertexQuad(verts, vstart, vend, uppertop, upperbottom,
							tleft, ttop, tright, tbottom, -1, -1);
					
					// Keep the offset to vertices
					uppervertexoffset = vertexoffset;
				}
			}
			
			// Return result
			return verts;
		}
		
		#endregion
		
		#region ================== Resource Management
		
		// This unloads all unstable resources
		public void UnloadResources()
		{
		}
		
		// This rebuilds unstable resources
		public void ReloadResources()
		{
		}
		
		#endregion
		
		#region ================== Lightmap
		
		// This redraws the lightmap if needed
		public void UpdateLightmap()
		{
			// Go for all nearby lights
			foreach(StaticLight l in sector.Lights)
			{
				// Blend the lightmaps
				l.BlendWallLightmap(this, lightmapcoordtop, lightmapcoordbottom);
			}
		}
		
		#endregion
		
		#region ================== Rendering
		
		// This renders the sidedef
		public void Render()
		{
			// If there is a middle wall
			if(middlevertexoffset > -1)
			{
				// Set the middle wall texture
				Direct3D.d3dd.SetTexture(0, tmiddle.texture);
				
				// Render the middle wall
				Direct3D.d3dd.DrawPrimitives(PrimitiveType.TriangleStrip,
													middlevertexoffset, 2);
			}
			
			// If there is a lower wall
			if(lowervertexoffset > -1)
			{
				// Set the lower wall texture
				Direct3D.d3dd.SetTexture(0, tlower.texture);
				
				// Render the lower wall
				Direct3D.d3dd.DrawPrimitives(PrimitiveType.TriangleStrip,
													lowervertexoffset, 2);
			}
			
			// If there is an upper wall
			if(uppervertexoffset > -1)
			{
				// Set the lower wall texture
				Direct3D.d3dd.SetTexture(0, tupper.texture);
				
				// Render the lower wall
				Direct3D.d3dd.DrawPrimitives(PrimitiveType.TriangleStrip,
													uppervertexoffset, 2);
			}
		}
		
		#endregion
	}
}
