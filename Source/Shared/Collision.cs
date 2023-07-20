/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using System.Drawing;
using System.Collections;
using CodeImp;

namespace CodeImp.Bloodmasters
{
	public abstract class Collision : IComparable<Collision>
	{
		protected bool collide;
		protected float distance;
		protected Vector2D newobjpos;
		protected Vector2D objslidevec;
		protected Vector2D objbouncevec;
		protected object collideobj;

		// Properties
		public bool IsColliding { get { return collide; } }
		public float Distance { get { return distance; } }
		public Vector2D NewObjPos { get { return newobjpos; } }
		public Vector2D ObjSlideVec { get { return objslidevec; } }
		public Vector2D ObjBounceVec { get { return objbouncevec; } }
		public object CollideObj{ get { return collideobj; } }

		// Constructor
		public Collision()
		{
		}

		// This compares one collision with another
		public int CompareTo(Collision other)
		{

            // If other is not a valid object reference, this instance is greater.
            if (other is null) return 1;

			// Return 1 if this collides earlier, otherwise return -1
            return distance.CompareTo(other.distance);
        }

		// This makes updates to the subject position
		// and returns the new collision
		public abstract Collision Update(Vector3D objpos, Vector2D objvec);

		// Response vectors
		public abstract Vector2D GetBounceVector();
		public abstract Vector2D GetSlideVector();

		#if CLIENT

		// This renders the collision
		public abstract void Render();

		#endif
	}
}
