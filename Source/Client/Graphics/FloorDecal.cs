/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using CodeImp.Bloodmasters.Client.Graphics;
using SharpDX;
using SharpDX.Direct3D9;

namespace CodeImp.Bloodmasters.Client
{
	public class FloorDecal : Decal
	{
		#region ================== Constants

		private const int BLOOD_TEXTURES = 4;
		private const int BULLET_TEXTURES = 1;
		private const int PLASMA_TEXTURES = 4;
		private const int EXPLODE_TEXTURES = 2;
		private const float Z_BIAS = 0.02f;
		private const float INIT_SCALE = 0.04f;
		private const float SIZE_SPEED = 0.0002f;
		private const float BIG_SIZE = 0.1f;
		private const float SMALL_SIZE = 0.05f;

		#endregion

		#region ================== Variables

		// Wall decal textures
		public static TextureResource[] blooddecals = new TextureResource[BLOOD_TEXTURES];
		public static TextureResource[] bulletdecals = new TextureResource[BULLET_TEXTURES];
		public static TextureResource[] plasmadecals = new TextureResource[PLASMA_TEXTURES];
		public static TextureResource[] explodedecals = new TextureResource[EXPLODE_TEXTURES];

		// Decal texture
		private TextureResource texture;

		// Geometry
		private VertexBuffer vertices = null;
		private Matrix decalmatrix;
		private Matrix lightmapmatrix;
		private Matrix dynlightmapoffsets;
		private float angle;
		private float origx, origy;

		// Animating
		private float size;
		private float finalsize;

		#endregion

		#region ================== Properties

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public FloorDecal(ClientSector sector, float nx, float ny, TextureResource texture,
						  bool permanent, bool resize, bool small) : base(permanent)
		{
			// Keep texture reference
			this.texture = texture;

			// Determine coordinates
			origx = nx;
			origy = ny;
			x = nx;
			y = ny;
			z = sector.HeightFloor;

			// Move coordinates slightly by camera vector
			// to stay above the floor
			x += General.arena.CameraVector.X * Z_BIAS;
			y += General.arena.CameraVector.Y * Z_BIAS;
			z += General.arena.CameraVector.Z * Z_BIAS;

			// Get reference to VisualSector
			this.sector = sector.VisualSector;

			// Leave when this decal is in a bottomless sector
			if(!sector.HasFloor) return;

			// Add decal to list
			General.arena.AddDecal(this);

			// Determine angle
			angle = (float)General.random.NextDouble() * (float)Math.PI * 2f;

			// Start at minimum size
			if(small) finalsize = SMALL_SIZE; else finalsize = BIG_SIZE;
			if(!resize) size = finalsize; else size = INIT_SCALE;

			// Create matrices
			CreateMatrices();

			// Create vertices
			CreateGeometry();
		}

		// Dispose
		public override void Dispose()
		{
			// Release references
			texture = null;

			// Remove decal from list
			General.arena.RemoveDecal(this);
			GC.SuppressFinalize(this);
		}

		#endregion

		#region ================== Static Methods

		// This makes a floor decal at the given coordinates
		public static FloorDecal Spawn(ClientSector sector, float nx, float ny,
							TextureResource[] textureset, bool permanent, bool resize, bool small)
		{
			// Dont make a decal when not using decals
			if(!Decal.showdecals) return null;
			if(sector == null) return null;

			// Choose a random decal
			int decal = General.random.Next(textureset.Length);

			// Make the decal!
			return new FloorDecal(sector, nx, ny, textureset[decal], permanent, resize, small);
		}

		// This loads all wall decal textures
		public static void LoadTextures()
		{
			// Go for all blood textures
			for(int i = 0; i < BLOOD_TEXTURES; i++)
			{
				// Load the sprite
				string filename = "sprites/floorblood" + i + ".tga";
				string tempfile = ArchiveManager.ExtractFile(filename);
				blooddecals[i] = null;
				blooddecals[i] = Direct3D.LoadTexture(tempfile, true);
				if(blooddecals[i] == null) throw(new Exception("Cannot load decal '" + filename + "'"));
			}

			// Go for all bullet textures
			for(int i = 0; i < BULLET_TEXTURES; i++)
			{
				// Load the sprite
				string filename = "sprites/floorbullet" + i + ".tga";
				string tempfile = ArchiveManager.ExtractFile(filename);
				bulletdecals[i] = null;
				bulletdecals[i] = Direct3D.LoadTexture(tempfile, true);
				if(bulletdecals[i] == null) throw(new Exception("Cannot load decal '" + filename + "'"));
			}

			// Go for all plasma textures
			for(int i = 0; i < PLASMA_TEXTURES; i++)
			{
				// Load the sprite
				string filename = "sprites/floorplasma" + i + ".tga";
				string tempfile = ArchiveManager.ExtractFile(filename);
				plasmadecals[i] = null;
				plasmadecals[i] = Direct3D.LoadTexture(tempfile, true);
				if(plasmadecals[i] == null) throw(new Exception("Cannot load decal '" + filename + "'"));
			}

			// Go for all explode textures
			for(int i = 0; i < EXPLODE_TEXTURES; i++)
			{
				// Load the sprite
				string filename = "sprites/floorexplode" + i + ".tga";
				string tempfile = ArchiveManager.ExtractFile(filename);
				explodedecals[i] = null;
				explodedecals[i] = Direct3D.LoadTexture(tempfile, true);
				if(explodedecals[i] == null) throw(new Exception("Cannot load decal '" + filename + "'"));
			}
		}

		#endregion

		#region ================== Resource Management

		// This unloads all unstable resources
		public override void UnloadResources()
		{
			// Destroy vertices
			DestroyGeometry();
		}

		// This rebuilds unstable resources
		public override void ReloadResources()
		{
			// Create vertices
			CreateGeometry();
		}

		#endregion

		#region ================== Geometry

		// This creates the matrices
		private void CreateMatrices()
		{
			// Create decal matrix
			float w = texture.info.Width * size;
			float h = texture.info.Height * size;
			decalmatrix = Matrix.Identity;
			decalmatrix *= Matrix.Scaling(w, h, 1f);
			decalmatrix *= Matrix.RotationZ(angle);
			decalmatrix *= Matrix.Translation(x, y, z);

			// Create lightmap matrix
			float lx = sector.LightmapScaledX(origx);
			float ly = sector.LightmapScaledY(origy);
			lightmapmatrix = Matrix.Identity;
			lightmapmatrix *= Matrix.Scaling(sector.LightmapScaleX * w * 0.7f,
											 sector.LightmapScaleY * h * 0.7f, 1f);
			lightmapmatrix *= Direct3D.MatrixTranslateTx(lx, ly);

			// Make dynamic lightmap matrix
			dynlightmapoffsets = Matrix.Identity;
			dynlightmapoffsets *= Matrix.Scaling(w, h, 1f);
			dynlightmapoffsets *= Matrix.RotationZ(angle);
			dynlightmapoffsets *= Direct3D.MatrixTranslateTx(x, y);
		}

		// This creates the generic item vertices
		public unsafe void CreateGeometry()
		{
			// Create vertex buffer
			vertices = new VertexBuffer(Direct3D.d3dd, sizeof(MVertex) * 4,
				Usage.WriteOnly, MVertex.Format, Pool.Default);

			// Lock vertex buffer
			var verts = vertices.Lock<MVertex>(0, 4);

			// Lefttop
			verts[0].x = -0.5f;
			verts[0].y = -0.5f;
			verts[0].z = 0f;
			verts[0].t1u = 0f;
			verts[0].t1v = 0f;
			verts[0].color = -1;
			verts[0].t2u = (float)Math.Sin(angle + Math.PI * -0.25f);
			verts[0].t2v = -(float)Math.Cos(angle + Math.PI * -0.25f);

			// Righttop
			verts[1].x = 0.5f;
			verts[1].y = -0.5f;
			verts[1].z = 0f;
			verts[1].t1u = 1f;
			verts[1].t1v = 0f;
			verts[1].color = -1;
			verts[1].t2u = (float)Math.Sin(angle + Math.PI * 0.25f);
			verts[1].t2v = -(float)Math.Cos(angle + Math.PI * 0.25f);

			// Leftbottom
			verts[2].x = -0.5f;
			verts[2].y = 0.5f;
			verts[2].z = 0f;
			verts[2].t1u = 0f;
			verts[2].t1v = 1f;
			verts[2].color = -1;
			verts[2].t2u = (float)Math.Sin(angle + Math.PI * 1.25f);
			verts[2].t2v = -(float)Math.Cos(angle + Math.PI * 1.25f);

			// Rightbottom
			verts[3].x = 0.5f;
			verts[3].y = 0.5f;
			verts[3].z = 0f;
			verts[3].t1u = 1f;
			verts[3].t1v = 1f;
			verts[3].color = -1;
			verts[3].t2u = (float)Math.Sin(angle + Math.PI * 0.75f);
			verts[3].t2v = -(float)Math.Cos(angle + Math.PI * 0.75f);

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

		// Process this decal
		public override void Process()
		{
			// Not disposed?
			if(texture != null)
			{
				// Within resize time?
				if(size < finalsize)
				{
					// Calculate new size
					size += SIZE_SPEED;
					if(size > finalsize) size = finalsize;

					// Create matrices
					CreateMatrices();
				}

				// Process base class
				base.Process();
			}
		}

		#endregion

		#region ================== Rendering

		// Render decal
		public override void Render()
		{
			// Check if visible
			if((sector.InScreen) && (texture != null))
			{
				// Get the sector
				Sector sc = (Sector)sector.Sectors[0];

				// Sector dynamic?
				if(sc.Dynamic)
				{
					// Make world matrix
					var m = Matrix.Translation(0f, 0f, sc.CurrentFloor - sc.HeightFloor);
					Direct3D.d3dd.SetTransform(TransformState.World, decalmatrix * m);
				}
				else
				{
					// Reset world matrix
					Direct3D.d3dd.SetTransform(TransformState.World, decalmatrix);
				}

				// Apply lightmap matrix
				Direct3D.d3dd.SetTransform(TransformState.Texture1, lightmapmatrix);
				Direct3D.d3dd.SetTransform(TransformState.Texture2, dynlightmapoffsets * General.arena.LightmapMatrix);

				// Set the texture and vertices stream
				Direct3D.d3dd.SetTexture(0, texture.texture);
				Direct3D.d3dd.SetStreamSource(0, vertices, 0, MVertex.Stride);

				// Set the lightmap from visual sector
				Direct3D.d3dd.SetTexture(1, sector.Lightmap);

				// Set transparency
				Direct3D.d3dd.SetRenderState(RenderState.TextureFactor, base.fadecolor);

				// Render it!
				Direct3D.d3dd.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
			}
		}

		#endregion
	}
}
