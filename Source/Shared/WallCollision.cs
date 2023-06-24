/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
#if CLIENT
using CodeImp.Bloodmasters.Client.Graphics;
using Direct3D = CodeImp.Bloodmasters.Client.Direct3D;
using SharpDX;
using CodeImp.Bloodmasters.Client;
using SharpDX.Direct3D9;

#else
using CodeImp.Bloodmasters.Server;
#endif

namespace CodeImp.Bloodmasters
{
	public class WallCollision : Collision
	{
		// Members
		private Linedef line;
		private bool offending;
		private float objradius;
		private bool crossing;
		private Sidedef startside;
		private float objheight;

		// Elements for calculations
		private Vector2D objpos, objvec, tstart, tend, objcp, linenorm, tint;
		private float renderheight;
		private float stepheight;
		private bool objisplayer;

		// Public properties
		public bool IsCrossing { get { return crossing; } }
		public Sidedef CrossSide { get { if(startside != null) return startside.OtherSide; else return null; } }
		//public Linedef Line { get { return line; } }

		/*
			Optimization planning:
			1: Cache values that can be cached, and cache them as early as possible.
			2: Do not calculate anything we do not need to determine collision
			   instead, put it in seperate methods.
		*/

		// Constructor
		public WallCollision(Linedef ld, Vector3D objpos, Vector2D objvec, float objradius, float objheight, float stepheight, bool objisplayer)
		{
			float ldcp, rtcp, objveclen;
			Vector2D linecp, vectonewpos;
			bool otherside;
			bool floorblocks = false;
			bool ceilblocks = false;

			GC.SuppressFinalize(this);

			// Keep references
			this.line = ld;
			this.objpos = objpos;
			this.objvec = objvec;
			this.objradius = objradius;
			this.renderheight = objpos.z;
			this.stepheight = stepheight;
			this.objisplayer = objisplayer;
			this.objheight = objheight;

			// References available?
			if((ld.Front != null) && (ld.Back != null))
			{
				// Determine if floor is blocking
				floorblocks = ((objpos.z + stepheight) < ld.Front.Sector.CurrentFloor) ||
				              ((objpos.z + stepheight) < ld.Back.Sector.CurrentFloor);

				// Determine if there is a ceiling
				if(ld.Front.Sector.HasCeiling || ld.Back.Sector.HasCeiling)
				{
					// Ceiling might block
					if(objpos.z < ld.Front.Sector.FakeHeightCeil)
						ceilblocks = (objpos.z + objheight) > ld.Front.Sector.HeightCeil;
					if(objpos.z < ld.Back.Sector.FakeHeightCeil)
						ceilblocks |= (objpos.z + objheight) > ld.Back.Sector.HeightCeil;
				}
				else
				{
					// No ceiling
					ceilblocks = false;
				}
			}
			else
			{
				// End of the map always blocks
				floorblocks = true;
				ceilblocks = true;
			}

			// Check if this line can collide
			if((ld.Impassable && objisplayer) || floorblocks || ceilblocks || (ld.Action != 0))
			{
				// Check if the object crosses the line
				float side1 = ld.SideOfLine(objpos.x, objpos.y);
				float side2 = ld.SideOfLine(objpos.x + objvec.x, objpos.y + objvec.y);
				otherside = ((side1 >= 0f) && (side2 <= 0f)) || ((side1 <= 0f) && (side2 >= 0f));

				// Calculate distances from object to the line
				float dist1 = ld.DistanceToLine(objpos.x, objpos.y);
				float dist2 = ld.DistanceToLine(objpos.x + objvec.x, objpos.y + objvec.y);

				// Check if the object is offending the line
				if((dist2 < dist1) || otherside)
				{
					// Check on which side of the line we are
					if(side2 <= 0f) startside = ld.Front; else startside = ld.Back;

					// Keep the collision side
					this.collideobj = startside;

					// Check if really touching the line
					if(otherside) crossing = ld.IntersectLine(objpos.x, objpos.y, objpos.x + objvec.x, objpos.y + objvec.y);

					// Determine collision point on the line
					ldcp = ld.NearestOnLine(objpos.x, objpos.y);
					if(ldcp < 0f) ldcp = 0f; else if(ldcp > 1f) ldcp = 1f;
					linecp = ld.CoordinatesAt(ldcp);

					// Calculate line normal from object to line
					linenorm = linecp - this.objpos;
					linenorm.Normalize();

					// Determine closest point at object to the line
					objcp = this.objpos + linenorm * objradius;

					// Start position of reversed trajectory
					// NOTE: Same as linecp??
					tstart = ld.CoordinatesAt(ldcp);

					// End position of reversed trajectory
					tend = tstart - objvec;

					// Length of object velocity
					// (also length of reversed trajectory)
					objveclen = objvec.Length();

					// Calculate nearest point on reversed trajectory
					rtcp = ((objcp.x - tstart.x) * (tend.x - tstart.x) + (objcp.y - tstart.y) * (tend.y - tstart.y)) / (objveclen * objveclen);
					if(rtcp < 0f) rtcp = 0f; else if(rtcp > 1f) rtcp = 1f;
					tint = tstart + rtcp * (tend - tstart);

					// Check if distance at the intersection
					// is close enough for collision
					Vector2D tid = tint - this.objpos;
					if(tid.Length() <= objradius)
					{
						// Calculate closest position near wall
						newobjpos = this.objpos + linecp - objcp;
						vectonewpos = newobjpos - this.objpos;

						// Will collide!
						collide = (ld.Impassable && objisplayer) || floorblocks || ceilblocks;
						offending = true;
						distance = vectonewpos.Length();
					}
					else
					{
						// No collision yet
						collide = false;
						offending = true;
						distance = 10000f + dist1;
					}
				}
				else
				{
					// No collision
					collide = false;
					offending = false;
					distance = 20000f + dist1;
				}
			}
			else
			{
				// No collision
				collide = false;
				offending = false;
				distance = 30000f;
			}
		}

		// Response vectors
		public override Vector2D GetBounceVector()
		{
			// Calculate bouncing vector
			return Vector2D.Reflect(objvec, new Vector2D(line.nY, line.nX));
		}

		// Response vectors
		public override Vector2D GetSlideVector()
		{
			// Calculate sliding vector
			objslidevec = newobjpos - tstart;
			objslidevec.Normalize();
			objslidevec *= Vector2D.DotProduct(objslidevec, objvec);
			objslidevec = objvec - objslidevec;

			// Make sure sliding vector is zero when near zero
			if(objslidevec.Length() < 0.01f) objslidevec = new Vector2D(0f, 0f);

			return objslidevec;
		}

		// This makes an updated collision
		public override Collision Update(Vector3D objpos, Vector2D objvec)
		{
			// Make new collision with updated parameters
			return new WallCollision(this.line, objpos, objvec, this.objradius, this.objheight, this.stepheight, this.objisplayer);
		}

		#if CLIENT

		// This renders the collision information
		public override void Render()
		{
			const float dotsize = 0.3f;
			Color tc = Color.Blue;
			Vector2D tvec = new Vector2D();

			// Using actual coordinates, dont transform them
			Direct3D.d3dd.SetTransform(TransformState.World, Matrix.Identity);

			// Get line vertices
			Vector2D v1 = General.map.Vertices[line.v1];
			Vector2D v2 = General.map.Vertices[line.v2];

			// The linedef
			RenderLine(new Vector2D(v1.x, v1.y), new Vector2D(v2.x, v2.y), Color.Yellow, false);

			// Object vector
			if(objvec.Length() > 0.01f)
			{
				Vector2D ovec = objvec;
				ovec.MakeLength(6f);
				RenderLine(new Vector2D(objpos.x, objpos.y), new Vector2D(objpos.x + ovec.x, objpos.y + ovec.y), Color.Maroon, true);
				RenderLine(new Vector2D(objpos.x, objpos.y), new Vector2D(objpos.x + objvec.x * 20f, objpos.y + objvec.y * 20f), Color.Red, true);
			}

			// Check if offending the line
			// (if not, then these are not even calculated)
			if(offending)
			{
				// Line from object to wall
				RenderLine(new Vector2D(objpos.x, objpos.y), new Vector2D(objpos.x + linenorm.x * 6f, objpos.y + linenorm.y * 6f), Color.LawnGreen, true);

				// Reversed trajectory from wall
				if(collide) tc = Color.SkyBlue;
				tvec.x = tend.x - tstart.x;
				tvec.y = tend.y - tstart.y;
				if(tvec.Length() > 0.01f)
				{
					Vector2D tnvec = tvec;
					tnvec.MakeLength(6f);
					RenderLine(new Vector2D(tstart.x, tstart.y), new Vector2D(tstart.x + tnvec.x, tstart.y + tnvec.y), Color.DarkBlue, true);
					tvec.Scale(12f);
					RenderLine(new Vector2D(tstart.x, tstart.y), new Vector2D(tstart.x + tvec.x, tstart.y + tvec.y), tc, true);
				}

				// Sliding vector
				if(objslidevec.Length() > 0.01f)
				{
					Vector2D svec = objslidevec;
					svec.MakeLength(6f);
					RenderLine(new Vector2D(objpos.x, objpos.y), new Vector2D(objpos.x + svec.x, objpos.y + svec.y), Color.Brown, true);
					RenderLine(new Vector2D(objpos.x, objpos.y), new Vector2D(objpos.x + objslidevec.x * 20f, objpos.y + objslidevec.y * 20f), Color.Orange, true);
				}

				// Bounce vector
				if(objbouncevec.Length() > 0.01f)
				{
					Vector2D bvec = objbouncevec;
					bvec.MakeLength(6f);
					RenderLine(new Vector2D(objpos.x, objpos.y), new Vector2D(objpos.x + bvec.x, objpos.y + bvec.y), Color.Gray, true);
					RenderLine(new Vector2D(objpos.x, objpos.y), new Vector2D(objpos.x + objbouncevec.x * 20f, objpos.y + objbouncevec.y * 20f), Color.WhiteSmoke, true);
				}

				// Collision point on object
				RenderLine(new Vector2D(objcp.x - dotsize, objcp.y - dotsize), new Vector2D(objcp.x + dotsize, objcp.y + dotsize), Color.LightGreen, false);
				RenderLine(new Vector2D(objcp.x + dotsize, objcp.y - dotsize), new Vector2D(objcp.x - dotsize, objcp.y + dotsize), Color.LightGreen, false);

				// Collision point on reversed trajectory
				RenderLine(new Vector2D(tint.x - dotsize, tint.y - dotsize), new Vector2D(tint.x + dotsize, tint.y + dotsize), Color.Magenta, false);
				RenderLine(new Vector2D(tint.x + dotsize, tint.y - dotsize), new Vector2D(tint.x - dotsize, tint.y + dotsize), Color.Magenta, false);
			}
		}

		// This makes vertices for a line
		public void RenderLine(Vector2D s, Vector2D e, Color c, bool arrow)
		{
			const float arrowlen = 1.5f;
			const float arrowwidth = 0.15f;
			LVertex[] verts = new LVertex[6];
			Vector2D as1 = e;
			Vector2D as2 = e;
			int primitives = 1;

			float angle = (float)Math.Atan2(-e.y + s.y, e.x - s.x);
			float angle1 = angle + (float)Math.PI * (1.5f + arrowwidth);
			float angle2 = angle + (float)Math.PI * (1.5f - arrowwidth);
			as1.x += (float)Math.Sin(angle1) * arrowlen;
			as1.y += (float)Math.Cos(angle1) * arrowlen;
			as2.x += (float)Math.Sin(angle2) * arrowlen;
			as2.y += (float)Math.Cos(angle2) * arrowlen;

			// Start vertex
			verts[0].color = c.ToArgb();
			verts[0].x = s.x;
			verts[0].y = s.y;
			verts[0].z = renderheight;

			// End vertex
			verts[1].color = c.ToArgb();
			verts[1].x = e.x;
			verts[1].y = e.y;
			verts[1].z = renderheight;

			// Arrow?
			if(arrow)
			{
				// More lines
				primitives = 3;

				// Arrow side 1 start vertex
				verts[2].color = c.ToArgb();
				verts[2].x = e.x;
				verts[2].y = e.y;
				verts[2].z = renderheight;

				// Arrow side 1 end vertex
				verts[3].color = c.ToArgb();
				verts[3].x = as1.x;
				verts[3].y = as1.y;
				verts[3].z = renderheight;

				// Arrow side 2 start vertex
				verts[4].color = c.ToArgb();
				verts[4].x = e.x;
				verts[4].y = e.y;
				verts[4].z = renderheight;

				// Arrow side 2 end vertex
				verts[5].color = c.ToArgb();
				verts[5].x = as2.x;
				verts[5].y = as2.y;
				verts[5].z = renderheight;
			}

			// Draw line
			Direct3D.SetDrawMode(DRAWMODE.NLINES);
			Direct3D.d3dd.DrawUserPrimitives(PrimitiveType.LineList, primitives, verts);
		}

		#endif
	}
}
