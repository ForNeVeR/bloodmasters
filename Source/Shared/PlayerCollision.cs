/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using CodeImp.Bloodmasters.Client.Graphics;
using SharpDX.Direct3D9;
using Direct3D = CodeImp.Bloodmasters.Client.Direct3D;
#if CLIENT
using SharpDX;
using CodeImp.Bloodmasters.Client;
#endif

namespace CodeImp.Bloodmasters
{
	public class PlayerCollision : Collision
	{
		// Members
		private IPhysicsState player;
		private float objradius;

		// Elements for calculations
		private Vector2D objpos, objvec;
		private float renderheight;
		private bool objisplayer;

		// Constructor
		public PlayerCollision(IPhysicsState pl, Vector3D objpos, Vector2D objvec, float objradius, bool objisplayer)
		{
			float u, ul, objveclen, ncolen, objcolldist, bp, ncptocp;
			Vector2D objend, ncp, ncpl, nco, colldist, vectonewpos;

			GC.SuppressFinalize(this);

			// Keep references
			this.player = pl;
			this.objpos = objpos;
			this.objvec = objvec;
			this.objradius = objradius;
			this.renderheight = objpos.z;
			this.collideobj = pl;
			this.objisplayer = objisplayer;

			// Determine end coordinates
			objend = this.objpos + objvec;

			// Make player rect
			RectangleF prect = new RectangleF(pl.State.pos.x - pl.State.Radius,
											  pl.State.pos.y - pl.State.Radius,
											  pl.State.Radius * 2f, pl.State.Radius * 2f);

			// Make object rect
			RectangleF orect = new RectangleF(objend.x - objradius,
											  objend.y - objradius,
											  objradius * 2f, objradius * 2f);

			// Likely intersection?
			if(prect.Intersects(orect))
			{
				// Check if within Z ranges
				if(((pl.State.pos.z + Consts.PLAYER_HEIGHT) < objpos.z) ||
				   (pl.State.pos.z > (objpos.z + Consts.PLAYER_HEIGHT)))
				{
					// No collision in trajectory
					collide = false;
					distance = 20000f;
					return;
				}

				// Calculate length of object vector
				objveclen = objvec.Length();

				// Calculate nearest collision point (NCP) on travel vector
				u = ((pl.State.pos.x - this.objpos.x) * (objend.x - this.objpos.x) + (pl.State.pos.y - this.objpos.y) * (objend.y - this.objpos.y)) / (objveclen * objveclen);

				// Limit the NCP to the trajectory
				if(u < 0f) ul = 0f; else if(u > 1f) ul = 1f; else ul = u;

				// Determine coordinates of limited NCP
				ncpl = this.objpos + ul * (objend - this.objpos);

				// Determine collision distance
				colldist = (Vector2D)pl.State.pos - ncpl;

				// Check for player collision
				if(colldist.Length() < (objradius + Consts.PLAYER_RADIUS))
				{
					// Calculate NCP coordinates
					ncp = this.objpos + u * (objend - this.objpos);

					// Get the vector from unlimited NCP to object
					nco = (Vector2D)pl.State.pos - ncp;
					ncolen = nco.Length();

					// We know the collision distance between the two objects
					// (which is simply objradius * 2) and now we know the
					// distance between the trajectory and the other object,
					// so we can now calculate the distance over the trajectory
					// at which collision will occur.
					objcolldist = objradius + Consts.PLAYER_RADIUS;
					bp = objcolldist * objcolldist - ncolen * ncolen;
					if(bp > 0f) ncptocp = (float)Math.Sqrt(bp); else ncptocp = 0f;

					// Scale this to the trajectory length
					ncptocp /= objvec.Length();

					// Negative NCP?
					if(u <= 0f)
					{
						// Make the distance vector
						//colldist = (u + ncptocp) * objvec;
						colldist = objvec;
					}
					else
					{
						// Make the distance vector
						colldist = (u - ncptocp) * objvec;
					}

					// Calculate position after collision occurs
					newobjpos = this.objpos + colldist;
					vectonewpos = newobjpos - this.objpos;

					// Collision!
					collide = true;
					distance = vectonewpos.Length();
				}
				else
				{
					// No collision
					collide = false;
					distance = 10000f + (colldist.Length() - (objradius + Consts.PLAYER_RADIUS));
				}
			}
			else
			{
				// No collision
				collide = false;
				distance = 20000f;
			}
		}

		// Response vectors
		public override Vector2D GetBounceVector()
		{
			// Calculate bouncing vector
			objbouncevec = newobjpos - (Vector2D)player.State.pos;
			objbouncevec.Normalize();
			objbouncevec = Vector2D.Reflect(-objvec, objbouncevec);
			return objbouncevec;
		}

		// Response vectors
		public override Vector2D GetSlideVector()
		{
			// Calculate sliding vector
			objslidevec = newobjpos - (Vector2D)player.State.pos;
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
			return new PlayerCollision(this.player, objpos, objvec, this.objradius, this.objisplayer);
		}

		#if CLIENT

		// This renders the collision information
		public override void Render()
		{
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
