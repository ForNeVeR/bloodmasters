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
using Color = System.Drawing.Color;

namespace CodeImp.Bloodmasters.Client
{
	public class Bullet : VisualObject
	{
		#region ================== Constants

		private const float BULLET_Z = 8f;
		private const float BULLET_RANGE = 100f;
		private const float FLASH_FADE_CHANGE = -0.1f;
		private const float FLASH_LENGTH = 30f;
		private const float FLASH_WIDTH = 0.2f;
		private const float FLASH_Z_BIAS = -5f;
		private const bool SHOW_BULLET_TRAJECTORIES = false;

		#endregion

		#region ================== Variables

		// Static components
		private static VertexBuffer vertices = null;
		public static TextureResource bulletflash;

		// Bullet variables
		private float flashalpha;
		private Matrix matflash;

		// DEBUG:
		Vector3D trj_start, trj_end;

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public Bullet(Actor source, float spread)
		{
			Vector3D start, pend, spawnat, pdelta;
			Vector3D phit, flashbegin, flashend, flashpos;
			float u, uline;
			object obj;
			Actor actor;
			Sector s;

			// Make start and end points of bullet trajectory
			start = source.Position + new Vector3D(0f, 0f, BULLET_Z);
			pend = start + Vector3D.FromActorAngle(source.AimAngle, source.AimAngleZ, BULLET_RANGE);

			// Add spread circle
			pend += new Vector3D(((float)General.random.NextDouble() - 0.5f) * spread * 2f,
								  ((float)General.random.NextDouble() - 0.5f) * spread * 2f,
								  ((float)General.random.NextDouble() - 0.5f) * spread * 2f);

			// No collision yet
			phit = pend;
			u = 2f;
			uline = 0f;
			obj = null;

			// Find ray collision with the map
			General.map.FindRayMapCollision(start, pend, ref phit, ref obj, ref u, ref uline);

			// Find ray collision with players
			General.FindRayPlayerCollision(start, pend, source, ref phit, ref obj, ref u);

			// Collision found?
			if(obj != null)
			{
				// Calculate bullet final coordinates
				spawnat = phit - start;
				spawnat.MakeLength(0.3f);
				spawnat = phit - spawnat;

				// When hitting a wall
				if(obj is Sidedef)
				{
					// Create particles
					for(int i = 0; i < 3; i++)
						General.arena.p_dust.Add(spawnat, Vector3D.Random(General.random, 0.1f, 0.1f, 0.1f), General.ARGB(1f, 0.1f, 0.1f, 0.1f));

					// Create a wall decals
					WallDecal.Spawn((Sidedef)obj, uline, phit.z, WallDecal.bulletdecals, false);
				}
				// When hitting a floor
				else if(obj is Sector)
				{
					// Liquid floor?
					s = (Sector)obj;
					if((SECTORMATERIAL)s.Material == SECTORMATERIAL.LIQUID)
					{
						// Determine type of splash to make
						switch(s.LiquidType)
						{
							case LIQUID.WATER: FloodedSector.SpawnWaterParticles(spawnat + new Vector3D(0f, 0f, 0.3f), new Vector3D(0f, 0f, 0.5f), 1); break;
							case LIQUID.LAVA: FloodedSector.SpawnLavaParticles(spawnat + new Vector3D(0f, 0f, 0.3f), new Vector3D(0f, 0f, 0.5f), 1); break;
						}
					}
					else
					{
						// Create particles
						for(int i = 0; i < 3; i++)
							General.arena.p_dust.Add(spawnat + new Vector3D(0f, 0f, 0.3f), Vector3D.Random(General.random, 0.1f, 0.1f, 0.1f), General.ARGB(1f, 0.1f, 0.1f, 0.1f));

						// Create a floor decal
						FloorDecal.Spawn((Sector)obj, spawnat.x, spawnat.y, FloorDecal.bulletdecals, false, false, false);
					}
				}
				// When hitting a player
				else if(obj is Client)
				{
					// Player isnot carrying a shield?
					if(((Client)obj).Powerup != POWERUP.SHIELDS)
					{
						// Get the actor
						actor = ((Client)obj).Actor;

						// Calculate particle velocity
						pdelta = phit - start;
						pdelta.MakeLength(0.04f);

						// Create particles
						General.arena.p_blood.Add(spawnat + new Vector3D(0f, 0f, -5f), pdelta, General.ARGB(1f, 1f, 0.0f, 0.0f));

						// Floor decal
						if((actor.Sector != null) && (actor.Sector.Material != (int)SECTORMATERIAL.LIQUID) && (General.random.Next(100) < 20))
							FloorDecal.Spawn(actor.Sector, actor.Position.x, actor.Position.y, FloorDecal.blooddecals, false, true, false);

						// Wall decal
						if(General.random.Next(100) < 50)
							WallDecal.Spawn(spawnat.x, spawnat.y, spawnat.z + (float)General.random.NextDouble() * 10f - 6f, Consts.PLAYER_DIAMETER, WallDecal.blooddecals, false);
					}
					else
					{
						// Create particles
						for(int i = 0; i < 2; i++)
							General.arena.p_dust.Add(spawnat, Vector3D.Random(General.random, 0.1f, 0.1f, 0.1f), General.ARGB(1f, 0.1f, 0.1f, 0.1f));
					}
				}
			}

			// Make a flash?
			//if(General.random.Next(20) < 10)
			if(true)
			{
				// Determine coordinates to use for VisualObject position
				if(VisualObject.Compare(start, 0f, phit, 0f) > 0)
					this.pos = start;
				else
					this.pos = phit;

				// Check if enough space for flash
				pdelta = phit - start;
				if(pdelta.Length() > Consts.PLAYER_DIAMETER)
				{
					// Calculate min and max ends for flash
					pdelta.MakeLength(Consts.PLAYER_DIAMETER);
					flashbegin = start + pdelta;
					flashend = phit;

					// Choose random flash position
					float flashu = (float)General.random.NextDouble();
					flashpos = (flashend * flashu) + (flashbegin * (1f - flashu));

					// Clip flash length to boundaries
					float flashlen = FLASH_LENGTH;
					Vector3D fbegindelta = flashpos - flashbegin;
					Vector3D fenddelta = flashend - flashpos;
					float fbeginlen = fbegindelta.Length();
					float fendlen = fenddelta.Length();
					if(flashlen > fbeginlen) flashlen = fbeginlen;
					if(flashlen > fendlen) flashlen = fendlen;

					// Make full delta again
					pdelta = phit - start;

					// Make flash matrix
					matflash = Matrix.Scaling(flashlen, FLASH_WIDTH, 1f);
					matflash *= Matrix.RotationY((float)Math.Asin(pdelta.z / pdelta.Length()));
					matflash *= Matrix.RotationZ((float)Math.Atan2(-pdelta.y, -pdelta.x));
					matflash *= Matrix.Translation(flashpos.x, flashpos.y, flashpos.z + FLASH_Z_BIAS);

					// Set timeout
					flashalpha = 1f;
				}
			}
			else
			{
				// No need for the bullet anymore
				if(!SHOW_BULLET_TRAJECTORIES) this.Dispose();
			}

			// DEBUG:
			if(SHOW_BULLET_TRAJECTORIES)
			{
				trj_start = start;
				trj_end = phit;
				flashalpha = 6f;
			}
		}

		// Dispose
		public override void Dispose()
		{
			// Dispose base
			base.Dispose();
			GC.SuppressFinalize(this);
		}

		#endregion

		#region ================== Geometry

		// This creates the generic item vertices
		public static unsafe void CreateGeometry()
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

			// Righttop
			verts[1].x = 0.5f;
			verts[1].y = -0.5f;
			verts[1].z = 0f;
			verts[1].t1u = 1f;
			verts[1].t1v = 0f;
			verts[1].color = -1;

			// Leftbottom
			verts[2].x = -0.5f;
			verts[2].y = 0.5f;
			verts[2].z = 0f;
			verts[2].t1u = 0f;
			verts[2].t1v = 1f;
			verts[2].color = -1;

			// Rightbottom
			verts[3].x = 0.5f;
			verts[3].y = 0.5f;
			verts[3].z = 0f;
			verts[3].t1u = 1f;
			verts[3].t1v = 1f;
			verts[3].color = -1;

			// Done filling the vertex buffer
			vertices.Unlock();
		}

		// This destroys the vertices
		public static void DestroyGeometry()
		{
			if(vertices != null)
			{
				vertices.Dispose();
				vertices = null;
			}
		}

		#endregion

		#region ================== Methods

		// Processes the flash
		public override void Process()
		{
			// Fade flash
			flashalpha += FLASH_FADE_CHANGE;

			// Trash when timed out
			if(flashalpha < 0f) this.Dispose();
		}

		// Render the flash
		public override void Render()
		{
			// Render bullet flash
			if(flashalpha > 0f)
			{
				// Set render mode
				Direct3D.SetDrawMode(DRAWMODE.NADDITIVEALPHA);
				Direct3D.d3dd.SetRenderState(RenderState.ZWriteEnable, false);
				if(flashalpha > 1f)
					Direct3D.d3dd.SetRenderState(RenderState.TextureFactor, General.ARGB(1f, 0.8f, 0.7f, 0.4f));
				else
					Direct3D.d3dd.SetRenderState(RenderState.TextureFactor, General.ARGB(flashalpha, 0.8f, 0.7f, 0.4f));

				// Apply matrices
				Direct3D.d3dd.SetTransform(TransformState.World, matflash);

				// Set the sprite texture
				Direct3D.d3dd.SetTexture(0, bulletflash.texture);
				Direct3D.d3dd.SetTexture(1, null);

				// Render
				Direct3D.d3dd.SetStreamSource(0, vertices, 0, MVertex.Stride);
				Direct3D.d3dd.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);

				// DEBUG:
				if(SHOW_BULLET_TRAJECTORIES)
					General.arena.RenderLine(trj_start, trj_end, Color.LightBlue);
			}
		}

		#endregion
	}
}
