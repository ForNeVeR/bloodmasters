/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

#region ================== Shading model
/*

   light
	*
	| \
	|   \
	|     \
	|       \
	|         \
	|          *  v1
	|         / \\
	|       /    \ \
	|     /       \  \
    |   / wall     \   \
    | /             \    \
 v2 *                \     \
    |\                \      \
    | \                \       * v4
    |  \                * v3
    |   \
    *    *
   v6    v5


   i suck at ascii art.
*/
#endregion

using System;
using System.Collections;
using System.IO;
using CodeImp.Bloodmasters.Client.Graphics;
using SharpDX;
using SharpDX.Direct3D9;

namespace CodeImp.Bloodmasters.Client
{
	public class StaticLight
	{
		#region ================== Constants

		public const int NUM_LIGHT_TEMPLATES = 6;
		public const int LIGHTMAP_BASE_SIZE = 1024;
		public const float EDGE_WIDTH_SCALE = 0.5f;
		public const float SHADOW_COORD_X1A = 0.3934f;
		public const float SHADOW_COORD_X2B = 0.5997f;
		public const float SHADOW_COORD_END = 1f - 1f / 64f;
		public const float SHADOW_COORD_BEGIN = 1f / 64f;
		public const float SHADOW_RANGE_END = 5f;
		public const int WALL_PIXELS_Y = 3;
		public const int WALL_PIXELS_X = 128;
		public const float OCCLUSION_HEIGHT = 7f;

		#endregion

		#region ================== Variables

		// Settings
		public static bool highlightmaps;

		// Static light/shadow templates
		public static SurfaceResource[] lightimages = new SurfaceResource[NUM_LIGHT_TEMPLATES];
		public static TextureResource lightshadow;

		// Properties
		private int thingindex;
		private int basecolor;
		private int color;
		private float range;
		private RectangleF rangerect;
		private float x;
		private float y;
		private float z;
		private Sector sector;
		private bool disposed = false;
		private bool shadows;

		// Lightmap
		private Texture lightmap = null;
		protected bool updatelightmap = true;
		private int lightmapsize;
		private float lightmapscale;
		private int template;

		// Walls lightmaps
		private Texture wallslightmap = null;
		private int wallslightmapsize;
		private float wallslightmapunit;

		// List of nearby sectors
		private ArrayList sectors = new ArrayList();

		// List of nearby sides
		private ArrayList sides = new ArrayList();
		private ArrayList visualsides = new ArrayList();

		#endregion

		#region ================== Properties

		public int ThingIndex { get { return thingindex; } }
		protected int BaseColor { get { return basecolor; } }
		public int Color { get { return color; } }
		public float X { get { return x; } }
		public float Y { get { return y; } }
		public float Z { get { return z; } }
		public int LightmapSize { get { return lightmapsize; } }
		public bool Disposed { get { return disposed; } }

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public StaticLight(Thing t, ArrayList vsectors, bool shadows)
		{
			// Keep properties
			this.thingindex = t.Index;
			this.shadows = shadows;

			// Get constructor arguments from thing
			float trange = (float)t.Arg[0] * 10f * Map.MAP_SCALE_XY;
			int tcolor = General.ARGB(1f, (float)t.Arg[1] / 255f,
										  (float)t.Arg[2] / 255f,
										  (float)t.Arg[3] / 255f);
			// Setup Light
			SetupLight(t.X, t.Y, t.Sector.HeightFloor + t.Z, t.Sector, trange, tcolor, vsectors, t.Arg[4]);
		}

		// Constructor
		public StaticLight(float x, float y, float z, float range, int basecolor,
					int template, ArrayList vsectors, bool shadows, bool permanent)
		{
			// Keep properties
			this.thingindex = -1;
			this.shadows = shadows;

			// Find the sector
			Sector sector = General.map.GetSubSectorAt(x, y).Sector;

			// Setup Light
			SetupLight(x, y, z, sector, range, basecolor, vsectors, template);
		}

		// This sets up the light
		private void SetupLight(float x, float y, float z, Sector sector,
								float range, int basecolor, ArrayList vsectors, int template)
		{
			ClientSidedef s;
			ArrayList lines;

			// Properties for all lights
			this.x = x;
			this.y = y;
			this.z = z;
			this.sector = sector;
			this.range = range;
			this.basecolor = basecolor;
			this.color = basecolor;
			this.template = template;

			// Check template
			if((template < 0) || (template >= NUM_LIGHT_TEMPLATES)) throw(new Exception("Invalid light template specified (" + template + ")"));

			// Determine lightmap size
			if(range < 25f) lightmapsize = LIGHTMAP_BASE_SIZE / 4;
			else if(range < 40f) lightmapsize = LIGHTMAP_BASE_SIZE / 2;
			else lightmapsize = LIGHTMAP_BASE_SIZE;
			if(highlightmaps == false) lightmapsize /= 2;
			if(Direct3D.hightextures == false) lightmapsize /= 2;

			// Lightmap scale relative to map coordinates
			lightmapscale = (1f / (range * 2f)) * (float)lightmapsize;

			// Make rectangle for my range
			rangerect = new RectangleF(x - range, y - range, range * 2, range * 2);

			// Go for all sectors
			for(int i = 0; i < vsectors.Count; i++)
			{
				// Get VisualSector
				VisualSector vs = (VisualSector)vsectors[i];

				// Check if this light can be seen from the sector
				if(vs.CanBeVisible(sector.Index))
				{
					// Test if this light touches the sector
					if(rangerect.Intersects(vs.LightmapBounds))
					{
						// Add sector to list
						sectors.Add(vs);

						// Add light to sector
						vs.AddNearbyLight(this);
					}
				}
			}

			// Go for all nearby lines
			lines = General.map.BlockMap.GetCollisionLines(rangerect.ToSystemDrawing());
			for(int l = lines.Count - 1; l >= 0; l--)
			{
				// Get the line
				Linedef ld = (Linedef)lines[l];

				// Check side of line
				float side = ld.SideOfLine(x, y);

				// No shadow when (nearly) on the line
				if(Math.Abs(side) > 0.0001f)
				{
					// Get the sidedef
					if(side < 0) s = (ClientSidedef)ld.Front; else s = (ClientSidedef)ld.Back;

					// Sidedef here?
					if(s != null)
					{
						// Check if this side can be lit by this light
						if(General.map.RejectMap.CanBeVisible(sector.Index, s.Sector.Index))
						{
							// Add the sidedef
							sides.Add(s);
							if(s.VisualSidedef != null) visualsides.Add(s.VisualSidedef);
						}
					}
				}
			}

			// Determine size for walls lightmap
			wallslightmapsize = General.NextPowerOf2(visualsides.Count * WALL_PIXELS_Y);
			wallslightmapunit = 1f / (float)wallslightmapsize;

			// Add to lights
			General.arena.StaticLights.Add(this);
		}

		// Destructor
		public void Dispose()
		{
			// Destroy lightmap
			DestroyLightmap();

			// Remove from sectors?
			if(sectors != null)
			{
				// Go for all nearby sectors
				foreach(VisualSector vs in sectors)
				{
					// Remove this light from this sector
					vs.RemoveNearbyLight(this);
				}
			}

			// Remove from lights
			General.arena.StaticLights.Remove(this);

			// Release references
			sectors = null;
			sides = null;
			visualsides = null;
			disposed = true;
			GC.SuppressFinalize(this);
		}

		#endregion

		#region ================== Debug

		// This outputs light information
		public void WriteLightDebugInfo(StreamWriter writer)
		{
			// Go for all sectors
			foreach(VisualSector ss in sectors)
			{
				foreach(Sector s in ss.Sectors) writer.WriteLine("   Sector " + s.Index);
			}

			// Go for all sides
			foreach(Sidedef sd in sides)
			{
				if(sd.IsFront)
					writer.WriteLine("   Sidedef " + sd.Index + " on linedef " + sd.Linedef.Index + " (front)");
				else
					writer.WriteLine("   Sidedef " + sd.Index + " on linedef " + sd.Linedef.Index + " (back)");
			}
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

		#region ================== Methods

		// This makes a new lightmap
		private void CreateLightmap()
		{
			Format lmformat;

			// Determine format to use
			lmformat = (Format)Direct3D.DisplayFormat;

			// Make a rendertarget for lightmap
			lightmap = new Texture(Direct3D.d3dd, lightmapsize, lightmapsize, 1,
					Usage.RenderTarget, lmformat, Pool.Default);

			// Make a rendertarget for walls lightmap
			wallslightmap = new Texture(Direct3D.d3dd, WALL_PIXELS_X, wallslightmapsize, 1,
						Usage.RenderTarget, lmformat, Pool.Default);

			// Need to redraw the lightmap
			updatelightmap = true;
		}

		// This destroys the lightmap
		private void DestroyLightmap()
		{
			// Clean up
			if(lightmap != null) lightmap.Dispose();
			if(wallslightmap != null) wallslightmap.Dispose();
			lightmap = null;
			wallslightmap = null;
		}

		// This calculates the vertices needed to cast a shadow
		private void CalculateShadowVertices(Vector2D v1, out Vector2D v3,
											 out Vector2D v4, float side)
		{
			Vector2D d;

			// Normalized vector from light to wall corner
			d = new Vector2D(v1.x - x, v1.y - y);
			d.Normalize();

			// Edge shade distances
			float rx = d.y * (range * EDGE_WIDTH_SCALE);
			float ry = d.x * (range * EDGE_WIDTH_SCALE);

			// Make coordinates of shadow end
			v3 = v1 + d * range * SHADOW_RANGE_END;

			// Determine edge shade direction
			if(side < 0)
			{
				// Make the edge coordinates
				v4.x = v3.x + rx;
				v4.y = v3.y - ry;
			}
			else
			{
				// Make the edge coordinates
				v4.x = v3.x - rx;
				v4.y = v3.y + ry;
			}
		}

		// This makes a polygon to cast a shadow
		private TLVertex[] MakeShadowPolygon(Vector2D v1, Vector2D v2, Vector2D v3,
											 Vector2D v4, Vector2D v5, Vector2D v6)
		{
			// Make vertices
			TLVertex[] verts = new TLVertex[6];

			// Lightmap-map offsets
			Vector2D o = new Vector2D(x - range, y - range);

			// V6
			verts[0].x = (v6.x - o.x) * lightmapscale;
			verts[0].y = (v6.y - o.y) * lightmapscale;
			verts[0].tu = 0f;
			verts[0].tv = SHADOW_COORD_END;
			verts[0].rhw = 1f;

			// V2
			verts[1].x = (v2.x - o.x) * lightmapscale;
			verts[1].y = (v2.y - o.y) * lightmapscale;
			verts[1].tu = SHADOW_COORD_X1A;
			verts[1].tv = 0f;
			verts[1].rhw = 1f;

			// V5
			verts[2].x = (v5.x - o.x) * lightmapscale;
			verts[2].y = (v5.y - o.y) * lightmapscale;
			verts[2].tu = SHADOW_COORD_X1A;
			verts[2].tv = SHADOW_COORD_END;
			verts[2].rhw = 1f;

			// V1
			verts[3].x = (v1.x - o.x) * lightmapscale;
			verts[3].y = (v1.y - o.y) * lightmapscale;
			verts[3].tu = SHADOW_COORD_X2B;
			verts[3].tv = 0f;
			verts[3].rhw = 1f;

			// V3
			verts[4].x = (v3.x - o.x) * lightmapscale;
			verts[4].y = (v3.y - o.y) * lightmapscale;
			verts[4].tu = SHADOW_COORD_X2B;
			verts[4].tv = SHADOW_COORD_END;
			verts[4].rhw = 1f;

			// V4
			verts[5].x = (v4.x - o.x) * lightmapscale;
			verts[5].y = (v4.y - o.y) * lightmapscale;
			verts[5].tu = 1f;
			verts[5].tv = SHADOW_COORD_END;
			verts[5].rhw = 1f;

			// Return polygon vertices
			return verts;
		}

		// This tests if the given side must cast a shadow
		public bool TestSideOcclusion(Sidedef s)
		{
			// Void NEVER casts a shadow
			if(s.OtherSide == null) return false;

			// Never cast shadow when marked as NO SHADOW
			if((s.Linedef.Flags & LINEFLAG.NOSHADOW) != 0) return false;

			// Solid lines ALWAYS cast a shadow
			if(s.Linedef.Solid) return true;

			// Sides on dynamic sectors do not cast shadows
			if(s.OtherSide.Sector.Dynamic) return false;

			// Cast a shadow if the floor on other side is higher than this light
			if(s.OtherSide.Sector.HeightFloor > z + OCCLUSION_HEIGHT) return true;

			// No shadow
			return false;
		}

		// This calculates the amount of light for the height differences
		public static float CalculateHeightAlpha(Sector s, float z)
		{
			float a;

			// Light higher or lower than sector?
			if(z > s.CurrentFloor)
			{
				// Higher than sector
				a = 1f - (float)Math.Abs(s.CurrentFloor - z) * 0.02f;
			}
			else
			{
				// Lower than sector
				a = 1f - (float)Math.Abs(s.CurrentFloor - z) * 0.1f;
			}

			// Return result
			if(a > 1f) return 1f;
			else if(a < 0f) return 0f;
			else return a;
		}

		#endregion

		#region ================== Processing

		// This tells all nearby sectors to update their lightmap
		protected void UpdateSectors()
		{
			// Go for all sectors
			foreach(VisualSector vs in sectors)
			{
				// Sector must update its lightmap
				vs.UpdateLightmap = true;
			}
		}

		#endregion

		#region ================== Rendering

		// This redraws the lightmap for a wall
		private void RenderWallLightmap(ClientSidedef sd)
		{
			float i, vob, vot;
			Vector2D v1, v2, v3, t1, t2, o;

			// Find index of wall in sidedefs and
			// calculate vertical offsets of wall lightmap
			i = (float)visualsides.IndexOf(sd.VisualSidedef);
			vot = i * (float)WALL_PIXELS_Y - 0.5f;
			vob = (i + 1f) * (float)WALL_PIXELS_Y - 0.5f;

			// Get the map coordinates
			v1 = General.map.Vertices[sd.Linedef.v1];
			v2 = General.map.Vertices[sd.Linedef.v2];

			// Swap vertices if the sidedef is on the back
			if(!sd.IsFront) { v3 = v1; v1 = v2; v2 = v3; }

			// Lightmap-map offsets
			o = new Vector2D(x - range, y - range);

			// Determine coordinates on the lightmap
			t1 = ((v1 - o) * lightmapscale) / (float)lightmapsize;
			t2 = ((v2 - o) * lightmapscale) / (float)lightmapsize;

			// Make vertices
			TLVertex[] rect = Direct3D.TLRect(-0.5f, vot, t1.x, t1.y,
						 (float)WALL_PIXELS_X -0.5f, vot, t2.x, t2.y,
											  -0.5f, vob, t1.x, t1.y,
						 (float)WALL_PIXELS_X -0.5f, vob, t2.x, t2.y);

			// Render the vertices
			Direct3D.d3dd.DrawUserPrimitives(PrimitiveType.TriangleStrip, 2, rect);
		}

		// This renders wall shadows on a wall
		private void RenderWallShadows(ClientSidedef sd)
		{
			float i, vob, vot;
			Vector2D v1, v2, v3, v4, v5, v6;
			float s1, s2, s3, s4, u1, u2, u3, u4, lp;
			bool sh1, sh2;
			TLVertex[] rect;

			// Find index of wall in sidedefs and
			// calculate vertical offsets of wall lightmap
			i = (float)visualsides.IndexOf(sd.VisualSidedef);
			vot = i * (float)WALL_PIXELS_Y - 0.5f;
			vob = (i + 1f) * (float)WALL_PIXELS_Y - 0.5f;

			// Go for all walls
			foreach(Sidedef s in sides)
			{
				// Test if this line must cast a shadow
				// and this is not the wall being rendered
				if(TestSideOcclusion(s) && (s.Linedef != sd.Linedef))
				{
					// Get vertex coordinates
					v1 = General.map.Vertices[s.Linedef.v1];
					v2 = General.map.Vertices[s.Linedef.v2];

					// Calculate sides
					float side = (v2.y - y) * (v1.x - x) - (v2.x - x) * (v1.y - y);

					// Calculate other vertices
					CalculateShadowVertices(v1, out v3, out v4, -side);
					CalculateShadowVertices(v2, out v5, out v6, side);

					// Determine intersection points on the wall sd
					sd.Linedef.IntersectLine(v1.x, v1.y, v3.x, v3.y, out u1, out s1);
					sd.Linedef.IntersectLine(v2.x, v2.y, v5.x, v5.y, out u2, out s2);

					// Determine shared vertices
					sh1 = (s.Linedef.v1 == sd.Linedef.v1) || (s.Linedef.v1 == sd.Linedef.v2);
					sh2 = (s.Linedef.v2 == sd.Linedef.v1) || (s.Linedef.v2 == sd.Linedef.v2);

					// Check if the shadow will hit the wall
					if(((u1 > 0f) && !float.IsNaN(u1)) || ((u2 > 0f) && !float.IsNaN(u2)))
					{
						// Do the same intersection tests for the edges
						sd.Linedef.IntersectLine(v1.x, v1.y, v4.x, v4.y, out u3, out s3);
						sd.Linedef.IntersectLine(v2.x, v2.y, v6.x, v6.y, out u4, out s4);

						// Determine nearest point on the line of the light
						lp = sd.Linedef.NearestOnLine(x, y);

						// Check if edge 1 is away from the line
						if(((u1 < 0f) || float.IsNaN(u1)) && !sh1)
						{
							if(s2 > lp) { s1 = 2f; s3 = 2f; }
							else { s1 = -1f; s3 = -1f; }
						}

						// Check if edge 2 is away from the line
						if(((u2 < 0f) || float.IsNaN(u2)) && !sh2)
						{
							if(s1 > lp) { s2 = 2f; s4 = 2f; }
							else { s2 = -1f; s4 = -1f; }
						}

						// Swap coordinates if the sidedef is on the back
						if(!sd.IsFront)
						{
							s1 = 1f - s1;
							s2 = 1f - s2;
							s3 = 1f - s3;
							s4 = 1f - s4;
						}

						// Scale to wall lightmap coordinates
						s1 *= (float)WALL_PIXELS_X;
						s2 *= (float)WALL_PIXELS_X;
						s3 *= (float)WALL_PIXELS_X;
						s4 *= (float)WALL_PIXELS_X;

						// Make vertices for first edge
						rect = Direct3D.TLRect(s1 - 0.5f, vot, SHADOW_COORD_X1A, SHADOW_COORD_END,
											   s3 - 0.5f, vot, SHADOW_COORD_BEGIN, SHADOW_COORD_END,
											   s1 - 0.5f, vob, SHADOW_COORD_X1A, SHADOW_COORD_END,
											   s3 - 0.5f, vob, SHADOW_COORD_BEGIN, SHADOW_COORD_END);

						// Render first edge
						Direct3D.d3dd.DrawUserPrimitives(PrimitiveType.TriangleStrip, 2, rect);

						// Make vertices for shadow middle
						rect = Direct3D.TLRect(s1 - 0.5f, vot, SHADOW_COORD_X1A, SHADOW_COORD_END,
											   s2 - 0.5f, vot, SHADOW_COORD_X2B, SHADOW_COORD_END,
											   s1 - 0.5f, vob, SHADOW_COORD_X1A, SHADOW_COORD_END,
											   s2 - 0.5f, vob, SHADOW_COORD_X2B, SHADOW_COORD_END);

						// Render shadow middle
						Direct3D.d3dd.DrawUserPrimitives(PrimitiveType.TriangleStrip, 2, rect);

						// Make vertices for second edge
						rect = Direct3D.TLRect(s4 - 0.5f, vot, SHADOW_COORD_END, SHADOW_COORD_END,
											   s2 - 0.5f, vot, SHADOW_COORD_X2B, SHADOW_COORD_END,
											   s4 - 0.5f, vob, SHADOW_COORD_END, SHADOW_COORD_END,
											   s2 - 0.5f, vob, SHADOW_COORD_X2B, SHADOW_COORD_END);

						// Render second edge
						Direct3D.d3dd.DrawUserPrimitives(PrimitiveType.TriangleStrip, 2, rect);
					}
				}
			}
		}

		// This renders wall shadows on the floor
		private void RenderFloorShadows()
		{
			Vector2D v1, v2, v3, v4, v5, v6;

			// Go for all nearby sides
			foreach(Sidedef s in sides)
			{
				// Test if this line must cast a shadow
				if(TestSideOcclusion(s))
				{
					// Get vertex coordinates
					v1 = General.map.Vertices[s.Linedef.v1];
					v2 = General.map.Vertices[s.Linedef.v2];

					// Calculate sides
					float side = (v2.y - y) * (v1.x - x) - (v2.x - x) * (v1.y - y);

					// Calculate other vertices
					CalculateShadowVertices(v1, out v3, out v4, -side);
					CalculateShadowVertices(v2, out v5, out v6, side);

					// Make shadow polygon
					TLVertex[] poly = MakeShadowPolygon(v1, v2, v3, v4, v5, v6);

					// Render the vertices
					Direct3D.d3dd.DrawUserPrimitives(PrimitiveType.TriangleStrip, 4, poly);
				}
			}
		}

		// This redraws the lightmap if needed
		public void PrepareLightmap()
		{
			Surface lightmapsurface, wallslightmapsurface;
			Rectangle imagerect, maprect;
			Texture oldlightmap;

			// Update needed?
			if(updatelightmap && !disposed)
			{
				// Recreate lightmap
				DestroyLightmap();
				CreateLightmap();

				// Get lightmap rendertargets
				lightmapsurface = lightmap.GetSurfaceLevel(0);
				wallslightmapsurface = wallslightmap.GetSurfaceLevel(0);

				// Copy original light image
				imagerect = new Rectangle(0, 0, lightimages[template].Width, lightimages[template].Height);
				maprect = new Rectangle(0, 0, lightmapsize, lightmapsize);
				Direct3D.d3dd.StretchRectangle(lightimages[template].surface, imagerect, lightmapsurface, maprect, TextureFilter.None);

				// Begin of rendering routine
				Direct3D.d3dd.DepthStencilSurface = null;
				Direct3D.d3dd.SetRenderTarget(0, wallslightmapsurface);
				Direct3D.d3dd.BeginScene();

					// Clear all wall lightmaps
					Direct3D.d3dd.Clear(ClearFlags.Target, ColorOperator.FromArgb(General.ARGB(1f, 1f, 0f, 0f)), 1f, 0);

					// Set drawing modes
					Direct3D.SetDrawMode(DRAWMODE.TLLIGHTDRAW, true);

					// Render all the wall lightmaps
					Direct3D.d3dd.SetRenderState(RenderState.AlphaBlendEnable, false);
					Direct3D.d3dd.SetTexture(0, lightmap);
					foreach(VisualSidedef sd in visualsides) RenderWallLightmap(sd.Sidedef);

					// Shadows on the walls?
					if(shadows)
					{
						// Render all the wall shadows
						Direct3D.d3dd.SetRenderState(RenderState.AlphaBlendEnable, true);
						Direct3D.d3dd.SetTexture(0, lightshadow.texture);
						foreach(VisualSidedef sd in visualsides) RenderWallShadows(sd.Sidedef);
					}

				// Done rendering
				Direct3D.d3dd.SetTexture(0, null);
				Direct3D.d3dd.EndScene();

				// Shadows on the floor?
				if(shadows)
				{
					// Begin of rendering routine
					Direct3D.d3dd.SetRenderTarget(0, lightmapsurface);
					Direct3D.d3dd.BeginScene();

						// Set drawing mode
						Direct3D.SetDrawMode(DRAWMODE.TLLIGHTDRAW, true);

						// Render shadows on the floor
						Direct3D.d3dd.SetTexture(0, lightshadow.texture);
						RenderFloorShadows();

					// Done rendering
					Direct3D.d3dd.SetTexture(0, null);
					Direct3D.d3dd.EndScene();
				}

				// Clean up
				//Direct3D.d3dd.SetRenderTarget(0, null);
				//Direct3D.d3dd.DepthStencilSurface = null;
				lightmapsurface.Dispose();
				wallslightmapsurface.Dispose();
				lightmapsurface = null;
				wallslightmapsurface = null;

				// Make permanent
				oldlightmap = lightmap;
				lightmap = Direct3D.CreateManagedTexture(lightmap);
				oldlightmap.Dispose();
				oldlightmap = wallslightmap;
				wallslightmap = Direct3D.CreateManagedTexture(wallslightmap);
				oldlightmap.Dispose();

				// All nearby sectors require updating
				UpdateSectors();

				// Lightmap updated
				updatelightmap = false;
			}
		}

		// This blends the lightmap on a sector's lightmap
		public void BlendSectorLightmap(VisualSector vs)
		{
			// Check if possible to render
			if(!disposed)
			{
				// Split color in RGB
				System.Drawing.Color c = System.Drawing.Color.FromArgb(color);

				// Determine alpha component
				float ca = StaticLight.CalculateHeightAlpha((Sector)vs.Sectors[0], this.z);

				// Make vertex colors
				int vc = System.Drawing.Color.FromArgb((int)(ca * 255f), c).ToArgb();
				/*
				int vc = color;
				*/

				// Make rectangle in sector lightmap coordinates
				float rl = vs.LightmapScaledX(x - range) * (float)vs.LightmapSize;
				float rt = vs.LightmapScaledY(y - range) * (float)vs.LightmapSize;
				float rr = vs.LightmapScaledX(x + range) * (float)vs.LightmapSize;
				float rb = vs.LightmapScaledY(y + range) * (float)vs.LightmapSize;

				// Make vertices
				TLVertex[] rect = Direct3D.TLRect(rl, rt, 0f, 0f,
												  rr, rt, 1f, 0f,
												  rl, rb, 0f, 1f,
												  rr, rb, 1f, 1f, vc);

				// Set the lightmap as texture
				Direct3D.d3dd.SetTexture(0, lightmap);

				// Render the vertices
				Direct3D.d3dd.DrawUserPrimitives(PrimitiveType.TriangleStrip, 2, rect);

				// Unset the lightmap as texture
				Direct3D.d3dd.SetTexture(0, null);
			}
		}

		// This blends the lightmap on a wall's lightmap
		public void BlendWallLightmap(VisualSidedef vs, float pix_top, float pix_bottom)
		{
			// Check if usefull and possible to render
			if(!disposed)
			{
				// Find index of wall in sidedefs
				int i = visualsides.IndexOf(vs);

				// Is this wall lit by this light?
				if(i > -1)
				{
					// Determine lightmap coordinates
					float po = i * WALL_PIXELS_Y;
					float vo = (po + (float)WALL_PIXELS_Y * 0.5f) * wallslightmapunit;

					// Make vertices
					TLVertex[] rect = Direct3D.TLRect(0f, pix_top, 1f / WALL_PIXELS_X, vo,
													  VisualSector.WALL_LIGHTMAP_X, pix_top, 1f - 1f / WALL_PIXELS_X, vo,
										              0f, pix_bottom, 1f / WALL_PIXELS_X, vo,
													  VisualSector.WALL_LIGHTMAP_X, pix_bottom, 1f - 1f / WALL_PIXELS_X, vo, color);

					// Set the lightmap as texture
					Direct3D.d3dd.SetTexture(0, wallslightmap);

					// Render the vertices
					Direct3D.d3dd.DrawUserPrimitives(PrimitiveType.TriangleStrip, 2, rect);

					// Unset the lightmap as texture
					Direct3D.d3dd.SetTexture(0, null);
				}
			}
		}

		#endregion
	}
}
