/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using System.Collections;
using System.IO;
using CodeImp.Bloodmasters.Client.Graphics;
using SharpDX;
using SharpDX.Direct3D9;

namespace CodeImp.Bloodmasters.Client
{
	public class VisualSector
	{
		#region ================== Constants

		public const int LIGHTMAP_HIGH_SIZE = 256;
		public const int LIGHTMAP_LOW_SIZE = 128;
		public const int WALL_LIGHTMAP_X = 128;
		public const int WALL_LIGHTMAP_Y = 3;
		public const int LIGHTMAP_SIZE_FIXED = 32;
		public const float LIGHTMAP_OVERHEAD = 1.0f;
		public const float LIGHTMAP_OFFSET_X = 0.1f;
		public const float LIGHTMAP_OFFSET_Y = 0.1f;
		public const bool LIGHTMAP_DEBUG_COLOR = false;
		public const float TEXTURE_SCALE = 2f;

		#endregion

		#region ================== Variables

		// Reference to sector data
		private ArrayList sectors;
		private int index;

		// This is set to true when sector is in screen
		private bool inscreen = false;

		// Bounds
		private RectangleF lmbounds;
		private RectangleF secbounds;
		private float lmscalex, lmscaley;
		private float secscalex, secscaley;

		// Vertex buffers
		private VertexBuffer mapvertices = null;
		private VertexBuffer shadowvertices = null;

		// SectorShadows on this sector
		private ArrayList sectorshadows;

		// Sidedefs in this sector
		private ArrayList sidedefs;

		// Floor and ceiling textures
		private ArrayList tfloors;
		private ArrayList tceils;

		// Heights
		private float lowestfloor;
		private float highestfloor;

		// Lightmaps
		private Texture lightmap = null;
		private Texture wallslightmap = null;
		public static TextureResource ceillightmap = null;
		public static TextureResource sectorshadowstexture = null;
		private bool updatelightmap = true;
		private bool dynamiclightmap = false;
		private int lightmapsize;
		private float lightmapunit;
		private int wallslightmapsize;
		private float wallslightmapunit;
		private int ambientlight;
		private bool fixedlight;
		private float lightmapaspect;

		// Color for debugging
		private int debugcolor = 0;

		// Lists of nearby lights
		private ArrayList lights = new ArrayList();

		#endregion

		#region ================== Properties

		public RectangleF SectorBounds { get { return secbounds; } }
		public RectangleF LightmapBounds { get { return lmbounds; } }
		public float SectorScaleX { get { return secscalex; } }
		public float SectorScaleY { get { return secscaley; } }
		public float LightmapScaleX { get { return lmscalex; } }
		public float LightmapScaleY { get { return lmscaley; } }
		public float LightmapAspect { get { return lightmapaspect; } }
		public bool UpdateLightmap { get { return updatelightmap; } set { if(dynamiclightmap) updatelightmap = value; } }
		public Texture Lightmap { get { return lightmap; } }
		public bool InScreen { get { return inscreen; } }
		public bool DynamicLightmap { get { return dynamiclightmap; } }
		public ArrayList Sectors { get { return sectors; } }
		public ArrayList VisualSidedefs { get { return sidedefs; } }
		public int LightmapSize { get { return lightmapsize; } }
		public float LightmapUnit { get { return lightmapunit; } }
		public float LowestFloor { get { return lowestfloor; } }
		public float HighestFloor { get { return highestfloor; } }
		public ArrayList Lights { get { return lights; } }
		public int AmbientLight { get { return ambientlight; } }
		public bool FixedLight { get { return fixedlight; } }
		public int Index { get { return index; } }

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public VisualSector(ClientSector sector)
		{
			// Make arrays
			sectors = new ArrayList();
			tfloors = new ArrayList();
			tceils = new ArrayList();
			sectorshadows = new ArrayList();

			// Make references
			sectors.Add(sector);
			sector.VisualSector = this;

			// Color for ambient light
			ambientlight = sector.Color;

			// Color for debugging
			debugcolor = General.RandomColor();
			debugcolor = ColorOperator.Scale(debugcolor, 1.4f);
			debugcolor = ColorOperator.AdjustSaturation(debugcolor, 2);

			// Keep floor heights
			lowestfloor = sector.HeightFloor;
			highestfloor = sector.HeightFloor;

			// Copy sector bounds
			secbounds = sector.Bounds.ToSharpDx();

			// Make lightmap bounds with overhead
			lmbounds = RectangleEx.Inflate(secbounds, LIGHTMAP_OVERHEAD, LIGHTMAP_OVERHEAD);

			// Calculate scalars
			lmscalex = (1f / lmbounds.Width);
			lmscaley = (1f / lmbounds.Height);
			secscalex = (1f / secbounds.Width);
			secscaley = (1f / secbounds.Height);
			lightmapaspect = lmscaley / lmscalex;

			// Check if sector has fixed light
			if(sector.Effect == SECTOREFFECT.FIXEDLIGHT)
			{
				// Fixed light and lightmap size
				fixedlight = true;
				lightmapsize = LIGHTMAP_SIZE_FIXED;
				lightmapunit = 1f / (float)lightmapsize;
			}
			else
			{
				// Lightmapped
				fixedlight = false;
				if(Direct3D.hightextures) lightmapsize = LIGHTMAP_HIGH_SIZE;
				else lightmapsize = LIGHTMAP_LOW_SIZE;
				lightmapunit = 1f / (float)lightmapsize;

				// This sector dynamic?
				if(sector.Dynamic) dynamiclightmap = true;

				// Check all adjacent sectors to see if lightmap should be dynamic
				foreach(Sector s in sector.AdjacentSectors)
				{
					// Sector dynamic? Then keep lightmap dynamic as well.
					if(s.Dynamic) dynamiclightmap = true;
				}
			}

			// Make list of sidedefs
			sidedefs = new ArrayList();
			foreach(SubSector ss in sector.Subsectors)
			{
				// Go for all segments
				foreach(Segment sg in ss.Segments)
				{
					// Segment along a sidedef?
					if(sg.Sidedef != null)
					{
						// Determine if visible
						if((sg.Sidedef.Angle > Math.PI * -0.24f) &&
						   (sg.Sidedef.Angle < Math.PI * 0.76f) &&
						   ((sg.Sidedef.TextureLower.Trim() != "-") ||
						    (sg.Sidedef.TextureUpper.Trim() != "-") ||
						    (sg.Sidedef.TextureMiddle.Trim() != "-")))
						{
							// Make the VisualSidedef and add it to the list
							sidedefs.Add(new VisualSidedef((ClientSidedef)sg.Sidedef, this));
						}
					}
				}
			}

			// Find the floor texture
			tfloors.Add(FindTexture(sector.TextureFloor));

			// Find the ceiling texture
			tceils.Add(FindTexture(sector.TextureCeil));
		}

		// Destructor
		public void Dispose()
		{
			// Go for all sidedefs
			foreach(VisualSidedef sd in sidedefs) sd.Dispose();

			// Destroy geometry
			DestroyGeometry();

			// Destroy lightmap
			lightmap.Dispose();

			// Clean up
			sectorshadows = null;
			tfloors = null;
			tceils = null;
			sidedefs = null;
			sectors = null;
			lightmap = null;
			GC.SuppressFinalize(this);
		}

		#endregion

		#region ================== Debug

		// This outputs map information
		public void WriteSectorDebugInfo(StreamWriter writer)
		{
			// Go for all sectors
			foreach(Sector ss in sectors)
			{
				// Output information
				writer.WriteLine("   Sector " + ss.Index);
			}

			// Go for all lines
			foreach(VisualSidedef sd in sidedefs)
			{
				// Output information
				if(sd.Sidedef.IsFront)
					writer.WriteLine("   Sidedef " + sd.Sidedef.Index + " on linedef " + sd.Sidedef.Linedef.Index + " (front)");
				else
					writer.WriteLine("   Sidedef " + sd.Sidedef.Index + " on linedef " + sd.Sidedef.Linedef.Index + " (back)");
			}
		}

		#endregion

		#region ================== Merging

		// This merges another VisualSector into this one
		public void Merge(VisualSector vs)
		{
			// Destroy geometry
			DestroyGeometry();

			// Copy sectors to list and make references
			foreach(ClientSector s in vs.sectors)
			{
				// Copy and make references
				sectors.Add(s);
				s.VisualSector = this;

				// Keep floor heights
				lowestfloor = Math.Min(lowestfloor, s.HeightFloor);
				highestfloor = Math.Max(highestfloor, s.HeightFloor);
			}

			// Copy sides to list and make references
			foreach(VisualSidedef s in vs.sidedefs)
			{
				// Copy and make references
				sidedefs.Add(s);
				s.VisualSector = this;
			}

			// Copy textures
			tfloors.AddRange(vs.tfloors);
			tceils.AddRange(vs.tceils);

			// Union bounds
			secbounds = RectangleF.Union(secbounds, vs.secbounds);

			// Let the other VisualSector know it was merged
			vs.Merged();
		}

		// This is called when merged with another VisualSector
		private void Merged()
		{
			// Clean up, but do not dispose
			tfloors = null;
			tceils = null;
			sidedefs = null;
			sectors = null;
			lightmap = null;
		}

		// This sets the index number for this visual sector
		public void SetIndex(int i)
		{
			// Set index
			index = i;
		}

		#endregion

		#region ================== Lightmap

		// This tests if there is a light nearby
		// with its height in a given range
		public bool LightBetweenHeights(float z1, float z2)
		{
			float zt;

			// Make sure z1 is the higher height
			if(z2 > z1)
			{
				// Swap heights
				zt = z1;
				z1 = z2;
				z2 = zt;
			}

			// Go for all nearby lights
			foreach(StaticLight l in lights)
			{
				// Light between these heights?
				if((l.Z < z1) && (l.Z >= z2)) return true;
			}

			// No light in this range
			return false;
		}

		// Visibility check against reject
		public bool CanBeVisible(int fromsector)
		{
			// Go for all sectors
			foreach(Sector s in sectors)
			{
				// Return true when any portion is visible
				if(General.map.RejectMap.CanBeVisible(fromsector, s.Index))
					return true;
			}

			// Nothing visible
			return false;
		}

		// This adds a light reference
		public void AddNearbyLight(StaticLight l)
		{
			// Add light to list
			if(lights.Contains(l) == false) lights.Add(l);

			// Update lightmap next time
			updatelightmap = true;
		}

		// This removes a light reference
		public void RemoveNearbyLight(StaticLight l)
		{
			// Remove light from list
			lights.Remove(l);

			// Update lightmap next time
			updatelightmap = true;
		}

		// This makes a new lightmap
		public void CreateLightmap()
		{
			// Make sure it is disposed first
			DestroyLightmap();

			// Determine size for walls lightmap
			wallslightmapsize = General.NextPowerOf2(sidedefs.Count * WALL_LIGHTMAP_Y);
			wallslightmapunit = 1f / (float)wallslightmapsize;

			// Make a rendertarget for lightmap
			lightmap = new Texture(Direct3D.d3dd, lightmapsize, lightmapsize, 1,
					Usage.RenderTarget, Direct3D.LightmapFormat, Pool.Default);

			// Make a rendertarget for walls lightmap
			wallslightmap = new Texture(Direct3D.d3dd, WALL_LIGHTMAP_X, wallslightmapsize, 1,
					Usage.RenderTarget, Direct3D.LightmapFormat, Pool.Default);

			// Need to redraw the lightmap
			updatelightmap = true;
		}

		// This destroys the lightmap
		private void DestroyLightmap()
		{
			// Clean up
			if(lightmap != null) lightmap.Dispose();
			lightmap = null;
			if(wallslightmap != null) wallslightmap.Dispose();
			wallslightmap = null;
		}

		// This redraws the lightmap if needed
		public void PrepareLightmap()
		{
			Surface lightmapsurface, wlightmapsurface;
			Texture oldlightmap;

			// Update needed?
			if(updatelightmap)
			{
				// Non-dynamic lightmaps must be recreated from scratch
				if(dynamiclightmap) CreateLightmap();

				// Set rendering target
				lightmapsurface = lightmap.GetSurfaceLevel(0);
				Direct3D.d3dd.DepthStencilSurface = null;
				Direct3D.d3dd.SetRenderTarget(0, lightmapsurface);

				// Begin of rendering routine
				Direct3D.d3dd.BeginScene();

				// Check if debugging lightmaps
				if(LIGHTMAP_DEBUG_COLOR)
				{
					// Give sector its random color
					Direct3D.d3dd.Clear(ClearFlags.Target, ColorOperator.FromArgb(debugcolor), 1f, 0);
				}
				else
				{
					// Give sector the ambient color
					Direct3D.d3dd.Clear(ClearFlags.Target, ColorOperator.FromArgb(ambientlight), 1f, 0);

					// Check if sector uses lighting
					if(fixedlight == false)
					{
						// Set drawing mode
						Direct3D.SetDrawMode(DRAWMODE.TLLIGHTDRAW);
						Direct3D.d3dd.SetTexture(0, sectorshadowstexture.texture);
						Direct3D.d3dd.SetStreamSource(0, shadowvertices, 0, TLVertex.Stride);

						// Go for all the sector shadows
						foreach(SectorShadow s in sectorshadows)
						{
							// Sectors higher than this sector?
							if((s.FrontSector.CurrentFloor > this.HighestFloor) ||
							   (s.BackSector.CurrentFloor > this.HighestFloor))
							{
								// Render shadow
								s.Render();
							}
						}

						// Set drawing mode
						Direct3D.SetDrawMode(DRAWMODE.TLLIGHTBLEND);

						// Go for all nearby lights
						foreach(StaticLight l in lights)
						{
							// Blend the lightmaps
							l.BlendSectorLightmap(this);
						}
					}
				}

				// Done rendering
				Direct3D.d3dd.EndScene();

				// Set rendering target
				wlightmapsurface = wallslightmap.GetSurfaceLevel(0);
				Direct3D.d3dd.DepthStencilSurface = null;
				Direct3D.d3dd.SetRenderTarget(0, wlightmapsurface);

				// Begin of rendering routine
				Direct3D.d3dd.BeginScene();

				// Check if debugging lightmaps
				if(VisualSector.LIGHTMAP_DEBUG_COLOR)
				{
					// Give sector its random color
					Direct3D.d3dd.Clear(ClearFlags.Target, ColorOperator.FromArgb(debugcolor), 1f, 0);
				}
				else
				{
					// Give sector the ambient color
					Direct3D.d3dd.Clear(ClearFlags.Target, ColorOperator.FromArgb(ambientlight), 1f, 0);

					// Set drawing mode
					Direct3D.SetDrawMode(DRAWMODE.TLLIGHTBLEND);

					// Check if sector uses lighting
					if(fixedlight == false)
					{
						// Update lightmaps of sidedefs
						foreach(VisualSidedef vs in sidedefs) vs.UpdateLightmap();
					}
				}

				// Done rendering
				Direct3D.d3dd.EndScene();

				// Clean up
				//Direct3D.d3dd.SetRenderTarget(0, null);
				//Direct3D.d3dd.DepthStencilSurface = null;
				lightmapsurface.Dispose();
				lightmapsurface = null;
				wlightmapsurface.Dispose();
				wlightmapsurface = null;

				// Make the lightmap managed?
				if(!dynamiclightmap)
				{
					oldlightmap = lightmap;
					lightmap = Direct3D.CreateManagedTexture(lightmap);
					oldlightmap.Dispose();
					oldlightmap = wallslightmap;
					wallslightmap = Direct3D.CreateManagedTexture(wallslightmap);
					oldlightmap.Dispose();
				}

				// Lightmap updated
				updatelightmap = false;
			}
		}

		#endregion

		#region ================== Resource Management

		// This unloads all unstable resources
		public void UnloadResources()
		{
			// Destroy lightmap
			if(dynamiclightmap) DestroyLightmap();

			// Destroy geometry
			//DestroyGeometry();

			// Go for all sidedefs
			foreach(VisualSidedef vs in sidedefs)
				vs.UnloadResources();
		}

		// This rebuilds unstable resources
		public void ReloadResources()
		{
			// Create lightmap
			if(dynamiclightmap) CreateLightmap();

			// Build geometry
			//BuildGeometry();

			// Go for all sidedefs
			foreach(VisualSidedef vs in sidedefs)
				vs.ReloadResources();
		}

		// This finds and loads the texture by name
		private ITextureResource FindTexture(string tex)
		{
			// Check if this is a liquid
			switch(tex)
			{
				// Nothing?
				case Sector.NO_FLAT:

					// Return nothing
					return null;

				// Water?
				case Arena.LIQUID_TEX_WATER:

					// Reference liquid
					return General.arena.liquidwater;

				// Lava?
				case Arena.LIQUID_TEX_LAVA:

					// Reference liquid
					return General.arena.liquidlava;

				default:

					// Find and load normal texture
					string texname = tex + ".bmp";
					string texarch = ArchiveManager.FindFileArchive(texname);
					if(texarch != "")
					{
						// Load the texture
						string tempfile = ArchiveManager.ExtractFile(texarch + "/" + texname);
						return Direct3D.LoadTexture(tempfile, true, !Direct3D.hightextures);
					}
					else
					{
						// Nothing
						return null;
					}
			}
		}

		#endregion

		#region ================== Geometry

		// This builds the entire geometry for the sectors
		public unsafe void BuildGeometry()
		{
			ITextureResource tc, tf;
			ClientSector sc;
			int i;
			float lmtop = 0f;
			ArrayList newverts;

			// Make shadows geometry
			MakeSectorShadows();

			// This will temporarely hold the vertices
			ArrayList verts = new ArrayList();

			// Go for all sectors
			for(i = 0; i < sectors.Count; i++)
			{
				// Get the sector and textures
				sc = (ClientSector)sectors[i];
				tc = (ITextureResource)tceils[i];
				tf = (ITextureResource)tfloors[i];

				// Floor?
				if(tf != null)
				{
					// Keep start vertex index
					sc.FirstFloorVertex = verts.Count;

					// Go for all subsectors
					foreach(SubSector s in sc.Subsectors)
					{
						// Build floor vertices
						newverts = MakeSubSectorPolygon(s, true, tf);
						verts.AddRange(newverts);
						sc.NumFaces += newverts.Count;
					}

					// Calculate actual number of faces
					sc.NumFaces /= 3;
				}

				// Ceiling?
				if(tc != null)
				{
					// Keep start vertex index
					sc.FirstCeilVertex = verts.Count;

					// Go for all subsectors
					foreach(SubSector s in sc.Subsectors)
					{
						// Build ceiling vertices
						verts.AddRange(MakeSubSectorPolygon(s, false, tc));
					}
				}
			}

			// Go for all sidedefs
			foreach(VisualSidedef sd in sidedefs)
			{
				// Set lightmap coordinates
				sd.LightmapCoordsTop = lmtop;
				sd.LightmapCoordsMiddle = (lmtop + ((float)WALL_LIGHTMAP_Y * 0.5f)) * wallslightmapunit;
				lmtop += (float)WALL_LIGHTMAP_Y;
				sd.LightmapCoordsBottom = lmtop;

				// Build wall pieces
				verts.AddRange(sd.MakeMiddleWall(verts.Count));
				verts.AddRange(sd.MakeLowerWall(verts.Count));
				verts.AddRange(sd.MakeUpperWall(verts.Count));
			}

			// Any vertices?
			if(verts.Count > 0)
			{
				// Create vertex buffer
				mapvertices = new VertexBuffer(Direct3D.d3dd, sizeof(MVertex) * verts.Count,
							Usage.WriteOnly, MVertex.Format, Pool.Managed);

				// Lock vertex buffer
				var vertsa = mapvertices.Lock<MVertex>(0, verts.Count);

				// Fill vertex buffer
				for(i = 0; i < verts.Count; i++) vertsa[i] = (MVertex)verts[i];

				// Done filling the vertex buffer
				mapvertices.Unlock();
			}
		}

		// This destroys the geometry
		public void DestroyGeometry()
		{
			if(mapvertices != null)
			{
				mapvertices.Dispose();
				mapvertices = null;
			}

			if(shadowvertices != null)
			{
				shadowvertices.Dispose();
				shadowvertices = null;
				sectorshadows.Clear();
			}
		}

		// This builds floor vertices for subsectors
		private ArrayList MakeSubSectorPolygon(SubSector s, bool floor, ITextureResource tex)
		{
			MVertex v1, v2, v3;

			// This will temporarely hold the vertices
			ArrayList verts = new ArrayList();

			// First vertex is always begin of first segment
			v1 = MakeSectorVertex(s.Segments[0].v1, s, floor, tex);

			// First triangle's second vertex
			v2 = MakeSectorVertex(s.Segments[0].v2, s, floor, tex);

			// Go for all segments to create vertices
			// but ignore the first and last segments
			for(int i = 1; i < s.Segments.Length - 1; i++)
			{
				// Make last vertex for triangle
				v3 = MakeSectorVertex(s.Segments[i].v2, s, floor, tex);

				// Add vertices to list
				verts.Add(v1);
				verts.Add(v2);
				verts.Add(v3);

				// Move v3 to v2 for next triangle
				v2 = v3;
			}

			// Return list of vertices
			return verts;
		}

		// This formats a vertex for a sector
		private MVertex MakeSectorVertex(int mapvertex, SubSector s, bool floor, ITextureResource tex)
		{
			// Make the vertex
			MVertex v = new MVertex();

			// Coordinates
			v.x = General.map.Vertices[mapvertex].x;
			v.y = General.map.Vertices[mapvertex].y;

			// Floor or ceiling?
			if(floor)
			{
				// Z height and texture from floor
				v.z = s.Sector.HeightFloor;
				v.color = -1;
			}
			else
			{
				// Z height and texture from fake ceiling
				v.z = s.Sector.FakeHeightCeil;
				v.color = General.map.CeilingLight;
			}

			// Texture coordinates
			v.t1u = General.map.Vertices[mapvertex].x / (tex.Info.Width * Map.MAP_SCALE_XY) * TEXTURE_SCALE;
			v.t1v = -General.map.Vertices[mapvertex].y / (tex.Info.Height * Map.MAP_SCALE_XY) * TEXTURE_SCALE;

			// Lightmap coordinates
			v.t2u = LightmapScaledX(v.x + LIGHTMAP_OFFSET_X);
			v.t2v = LightmapScaledY(v.y + LIGHTMAP_OFFSET_Y);

			// Lightmap coordinates
			v.t3u = v.x;
			v.t3v = v.y;

			// Return vertex
			return v;
		}

		// This makes the geometry for the sector shadows
		public unsafe void MakeSectorShadows()
		{
			SectorShadow ss;

			// This will temporarely hold the vertices
			ArrayList verts = new ArrayList();

			// Determine in which area linedefs should be used
			RectangleF shadowarea = RectangleEx.Inflate(secbounds, 10f, 10f);

			// Go for all nearby linedefs
			ArrayList lines = General.map.BlockMap.GetCollisionLines(shadowarea.ToSystemDrawing());
			foreach(Linedef l in lines)
			{
				// Make sector shadow
				ss = new SectorShadow();
				if(ss.MakeSectorShadow(verts, this, l))
				{
					// Add when successfull
					sectorshadows.Add(ss);
				}
			}

			// Any vertices?
			if(verts.Count > 0)
			{
				// Create vertex buffer
				shadowvertices = new VertexBuffer(Direct3D.d3dd, sizeof(TLVertex) * verts.Count, Usage.WriteOnly, TLVertex.Format, Pool.Default);

				// Lock vertex buffer
				var vertsa = shadowvertices.Lock<TLVertex>(0, verts.Count);

				// Fill vertex buffer
				for(int i = 0; i < verts.Count; i++) vertsa[i] = (TLVertex)verts[i];

				// Done filling the vertex buffer
				shadowvertices.Unlock();
			}
		}

		// This returns a scaled X coordinate for the boundaries
		// of this sector with a map coordinate as input
		public float BoundsScaledX(float mapx)
		{
			return (mapx - secbounds.Left) * secscalex;
		}

		// This returns a scaled Y coordinate for the boundaries
		// of this sector with a map coordinate as input
		public float BoundsScaledY(float mapy)
		{
			return (mapy - secbounds.Top) * secscaley;
		}

		// This returns a scaled X coordinate for the boundaries
		// of this lightmap with a map coordinate as input
		public float LightmapScaledX(float mapx)
		{
			return (mapx - lmbounds.Left) * lmscalex;
		}

		// This returns a scaled Y coordinate for the boundaries
		// of this lightmap with a map coordinate as input
		public float LightmapScaledY(float mapy)
		{
			return (mapy - lmbounds.Top) * lmscaley;
		}

		#endregion

		#region ================== Processing

		// Processing
		public void Process()
		{
			// Test if the sector is visible
			inscreen = secbounds.Intersects(General.arena.ScreenArea);
		}

		#endregion

		#region ================== Rendering

		// This will render the sector floor geometry
		public void RenderFlat()
		{
            ClientSector sc;

			// Set the vertex stream
			Direct3D.d3dd.SetStreamSource(0, mapvertices, 0, MVertex.Stride);

			// Go for all sectors
			for(int i = 0; i < sectors.Count; i++)
			{
				// Get the sector
				sc = (ClientSector)sectors[i];

				// Does it have a floor?
				if(tfloors[i] != null)
				{
					// Render the subsector floor
					Direct3D.d3dd.DrawPrimitives(PrimitiveType.TriangleList,
										sc.FirstFloorVertex, sc.NumFaces);
				}
			}
		}

		// This will render the sector geometry
		public void RenderGeometry()
		{
			ITextureResource t;
			ClientSector sc;
			Matrix m;
			int i;

			// Is there anything to render and is sector visible?
			if((mapvertices != null) && inscreen)
			{
				// Change render mode
				Direct3D.SetDrawMode(DRAWMODE.NLIGHTMAP);

				// Reset world matrix
				Direct3D.d3dd.SetTransform(TransformState.World, Matrix.Identity);

				// Set the vertex stream
				Direct3D.d3dd.SetStreamSource(0, mapvertices, 0, MVertex.Stride);

				// Set the lightmap texture
				Direct3D.d3dd.SetTexture(1, VisualSector.ceillightmap.texture);

				// Go for all sectors
				for(i = 0; i < sectors.Count; i++)
				{
					// Get the sector and texture
					sc = (ClientSector)sectors[i];
					t = (ITextureResource)tceils[i];

					// Ceiling?
					if(t != null)
					{
						// Set the sector ceiling texture
						Direct3D.d3dd.SetTexture(0, t.Texture);

						// Render the sector ceiling
						Direct3D.d3dd.DrawPrimitives(PrimitiveType.TriangleList,
												sc.FirstCeilVertex, sc.NumFaces);
					}
				}

				// Set the lightmap texture
				Direct3D.d3dd.SetTexture(1, lightmap);

				// Go for all sectors
				for(i = 0; i < sectors.Count; i++)
				{
					// Get the sector and texture
					sc = (ClientSector)sectors[i];
					t = (ITextureResource)tfloors[i];

					// Floor?
					if(t != null)
					{
						// Dynamic?
						if(sc.Dynamic)
						{
							// Make world matrix
							m = Matrix.Translation(0f, 0f, sc.CurrentFloor - sc.HeightFloor);
							Direct3D.d3dd.SetTransform(TransformState.World, m);
						}
						else
						{
							// Reset world matrix
							Direct3D.d3dd.SetTransform(TransformState.World, Matrix.Identity);
						}

						// Set the sector floor texture
						Direct3D.d3dd.SetTexture(0, t.Texture);

						// Render the subsector floor
						Direct3D.d3dd.DrawPrimitives(PrimitiveType.TriangleList,
												sc.FirstFloorVertex, sc.NumFaces);
					}
				}

				// Set the lightmap texture
				Direct3D.d3dd.SetTexture(1, wallslightmap);

				// Go for all sidedefs
				foreach(VisualSidedef sd in sidedefs)
				{
					// Get back sector
					if(sd.Sidedef.OtherSide != null) sc = (ClientSector)sd.Sidedef.OtherSide.Sector; else sc = null;

					// Back sector dynamic?
					if((sc != null) && sc.Dynamic)
					{
						// Make world matrix
						m = Matrix.Translation(0f, 0f, sc.CurrentFloor - sc.HeightFloor);
						Direct3D.d3dd.SetTransform(TransformState.World, m);
					}
					else
					{
						// Reset world matrix
						Direct3D.d3dd.SetTransform(TransformState.World, Matrix.Identity);
					}

					// Render the sidedef
					sd.Render();
				}

				// Unset textures and stream
				//Direct3D.d3dd.SetStreamSource(0, null, 0);
				//Direct3D.d3dd.SetTexture(0, null);
				//Direct3D.d3dd.SetTexture(1, null);
			}
		}

		#endregion
	}
}
