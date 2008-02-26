/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using System.Drawing;
using System.Globalization;
using System.Collections;
using CodeImp.Bloodmasters;
using CodeImp;

#if CLIENT
using CodeImp.Bloodmasters.Client;
#endif

namespace CodeImp.Bloodmasters.Server
{
	public class Bullet
	{
		#region ================== Constants
		
		private const float BULLET_Z = 7f;
		private const float BULLET_RANGE = 100f;
		
		#endregion
		
		#region ================== Constructor / Destructor
		
		// Constructor
		public Bullet(Client source, float spread, string deathtext, int damage, float pushforce)
		{
			Vector3D start, pend, phit;
			float u, uline;
			object obj;
			
			// Make start and end points of bullet trajectory
			start = source.State.pos + new Vector3D(0f, 0f, BULLET_Z);
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
			General.server.map.FindRayMapCollision(start, pend, ref phit, ref obj, ref u, ref uline);
			
			// Find ray collision with players
			General.server.FindRayPlayerCollision(start, pend, source, ref phit, ref obj, ref u);
			
			// Collision found?
			if(u < 1f)
			{
				// Hitting a player?
				if(obj is Client)
				{
					Client hitclient = (Client)obj;
					
					// Make push vector
					Vector3D pushvec = pend - start;
					pushvec.MakeLength(pushforce);
					
					// Push the player
					hitclient.Push(pushvec);
					
					// Hurt the player
					hitclient.Hurt(source, deathtext, damage, DEATHMETHOD.NORMAL, start);
				}
			}
			
			// No need for the bullet anymore
			this.Dispose();
		}
		
		// Dispose
		public void Dispose()
		{
		}
		
		#endregion
	}
}
