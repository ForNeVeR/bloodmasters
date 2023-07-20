/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System.Drawing;

namespace CodeImp.Bloodmasters
{
	public abstract class PlayerCollision : Collision
	{
		// Members
		protected IPhysicsState player;
		protected float objradius;

		// Elements for calculations
		private Vector2D objpos, objvec;
		protected float renderheight;
		protected bool objisplayer;

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
			if(prect.IntersectsWith(orect))
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
    }
}
