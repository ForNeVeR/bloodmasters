/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

// This controls the physics in the game

using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using CodeImp;

#if CLIENT
using CodeImp.Bloodmasters.Client;
#else
using CodeImp.Bloodmasters.Server;
#endif

namespace CodeImp.Bloodmasters
{
	public class PhysicsState
	{
		#region ================== Members

		// Settings
		private float radius;			// Object radius
		private bool stepup;			// Step up small heights?
		private bool redirect;			// Continue after collision?
		private bool bounce;			// Bounce at collisions?
		private float friction;			// Power at which to bounce or slide
		private bool blocking;			// Blocks other objects?
		private float height;			// Height of this object
		private bool isplayer;

		// Position
		public Vector3D pos;

		// Total Velocity
		public Vector3D vel;

		// Map
		private Map map;

		// Collision to render
		public Collision showcol;

		// Resources
		private bool disposed = false;

		#endregion

		#region ================== Properties

		public float Radius { get { return radius; } set { radius = value; } }
		public float Diameter { get { return radius * 2f; } set { radius = value * 0.5f; } }
		public bool StepUp { get { return stepup; } set { stepup = value; } }
		public bool Redirect { get { return redirect; } set { redirect = value; } }
		public bool Bounce { get { return bounce; } set { bounce = value; } }
		public float Friction { get { return friction; } set { friction = value; } }
		public bool Blocking { get { return blocking; } set { blocking = value; } }
		public bool IsPlayer { get { return isplayer; } set { isplayer = value; } }
		public float Height { get { return height; } set { height = value; } }

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public PhysicsState(Map map)
		{
			// Keep reference
			this.map = map;

			// Default
			this.blocking = true;
			this.isplayer = true;
		}

		// Disposer
		public void Dispose()
		{
			// Clean up
			this.map = null;
			this.showcol = null;
			this.disposed = true;
			GC.SuppressFinalize(this);
		}

		#endregion

		#region ================== Methods

		// This moves the position with the total velocity.
		// Returns true when collided with a wall
		public bool ApplyVelocity(bool wallcollisions)
		{
			ArrayList clientslist = null;
			Sidedef crossline = null;
			return ApplyVelocity(wallcollisions, false, clientslist, null, out crossline);
		}

		// This moves the position with the total velocity.
		// Returns true when collided with a wall
		public bool ApplyVelocity(bool wallcollisions, bool clientcollisions,
						IPhysicsState[] allclients, IPhysicsState thisclient)
		{
			ArrayList clientslist = new ArrayList(allclients);
			Sidedef crossline = null;
			return ApplyVelocity(wallcollisions, clientcollisions, clientslist, thisclient, out crossline);
		}

		// This moves the position with the total velocity.
		// Returns true when collided with a wall
		public bool ApplyVelocity(bool wallcollisions, bool clientcollisions,
						IPhysicsState[] allclients, IPhysicsState thisclient,
						out Sidedef crossline)
		{
			ArrayList clientslist = new ArrayList(allclients);
			return ApplyVelocity(wallcollisions, clientcollisions, clientslist, thisclient, out crossline);
		}

		// This moves the position with the total velocity.
		// Returns true when collided with a wall
		public bool ApplyVelocity(bool wallcollisions, bool clientcollisions,
						IPhysicsState[] allclients, IPhysicsState thisclient,
						out Sidedef crossline, out object hitobj)
		{
			ArrayList clientslist = new ArrayList(allclients);
			return ApplyVelocity(wallcollisions, clientcollisions, clientslist, thisclient, out crossline, out hitobj);
		}

		// This moves the position with the total velocity.
		// Returns true when collided with a wall
		public bool ApplyVelocity(bool wallcollisions, bool clientcollisions,
								ArrayList allclients, IPhysicsState thisclient)
		{
			Sidedef crossline = null;
			return ApplyVelocity(wallcollisions, clientcollisions, allclients, thisclient, out crossline);
		}

		// This moves the position with the total velocity.
		// Returns true when collided with a wall
		public bool ApplyVelocity(bool wallcollisions, bool clientcollisions,
								ArrayList allclients, IPhysicsState thisclient,
								out Sidedef crossline)
		{
			object obj = null;
			return ApplyVelocity(wallcollisions, clientcollisions, allclients, thisclient, out crossline, out obj);
		}

		// This moves the position with the total velocity.
		// Returns true when collided with a wall
		public bool ApplyVelocity(bool wallcollisions, bool clientcollisions,
								ArrayList allclients, IPhysicsState thisclient,
								out Sidedef crossline, out object hitobj)
		{
			float stepheight;
			Vector3D tgt, sv, corr;
			bool collision = false;

			// No crossings yet
			crossline = null;
			hitobj = null;

			// Not disposed?
			if(disposed) return false;

			// For testing the collision detection
			showcol = null;

			// DEBUG:
			//General.DisplayAndLog("Position: " + pos.x + ", " + pos.y + "   Velocity: " + vel.x + ", " + vel.y);

			// Copy velocity
			sv = vel;

			// Determine target coordinates without collision
			tgt = pos + sv;

			// Determine step up height
			if(stepup) stepheight = Consts.MAX_STEP_HEIGHT; else stepheight = 0f;

			// Make collisions array
			var collisions = new List<Collision>();

			// Player collision detection?
			if(clientcollisions)
			{
				// Go for all objects
				foreach(IPhysicsState plr in allclients)
				{
					// Check if this object must be included
					if((plr != null) && (plr.State != null) && (plr != thisclient) && plr.State.blocking)
					{
						// Make object collision
						collisions.Add(new PlayerCollision(plr, pos, sv, radius, isplayer));
					}
				}
			}

			// Wall collision detection?
			if(wallcollisions)
			{
				// Get all the nearby lines and make collisions
				ArrayList lines = map.BlockMap.GetCollisionLines(pos.x, pos.y, tgt.x, tgt.y, radius);
				for(int i = 0; i < lines.Count; i++)
				{
					// Get the linedef
					Linedef ld = (Linedef)lines[i];

					// Make possible collision
					WallCollision wc = new WallCollision(ld, pos, sv, radius, height, stepheight, isplayer);
					collisions.Add(wc);

					// Return the crossing sidedef, if crossing
					if(!wc.IsColliding && wc.IsCrossing) crossline = wc.CrossSide;
				}
			}

			// Sort the collisions by order in which we will collide with them
			// The collisions we dont actually collide with will be sorted to the end
			collisions.Sort();

			// DEBUG:
			//string wallslist = "";
			//foreach(Collision c in colls) if(c is WallCollision) wallslist += (c as WallCollision).Line.Index + "(" + c.Distance + "), ";
			//General.DisplayAndLog("Sorted walls: " + wallslist);

			// Go for all possible collisions
			for(int i = 0; i < collisions.Count; i++)
			{
				// Get the collisions
				Collision coll = collisions[i];

				// For testing the collision detection
				if(showcol == null) showcol = coll;

				// Will we collide with this?
				if(coll.IsColliding)
				{
					// Collision!
					collision = true;

					// DEBUG:
					//General.DisplayAndLog("COLLISION! Line: " + (coll as WallCollision).Line.Index);

					// Return collision object
					hitobj = coll.CollideObj;

					// Apply new position at collision
					pos.Apply2D(coll.NewObjPos);

					// DEBUG:
					//General.DisplayAndLog("Correction: " + pos.x + ", " + pos.y);

					// Continue after collision?
					if(redirect)
					{
						// Choose the bounce or slide vector
						if(bounce) corr = coll.GetBounceVector(); else corr = coll.GetSlideVector();

						// Scale by friction
						corr.Scale(friction);

						// Apply new velocity at collision
						sv.Apply2D(corr);

						// Apply change to source velocity as well
						vel.Apply2D(corr);

						// DEBUG:
						//General.DisplayAndLog("New velocity: " + vel.x + ", " + vel.y);

						// Go for all following collisions to update
						for(int k = i + 1; k < collisions.Count; k++)
						{
							// Get old collision
							Collision oldcoll = collisions[k];

							// Update collision
							collisions[k] = oldcoll.Update(pos, sv);
						}

						// Sort the lines again, except for those already tested
						if(i < collisions.Count - 1) collisions.Sort(i + 1, collisions.Count - (i + 1), null);
					}
					else
					{
						// Zero velocity
						vel = new Vector3D(0f, 0f, vel.z);
						sv = new Vector3D(0f, 0f, sv.z);

						// Done
						break;
					}
				}
				else
				{
					#if CLIENT
					// No more collisions
					break;
					#endif
				}
			}

			// Apply the remaining velocity
			pos.x += sv.x;
			pos.y += sv.y;
			pos.z += sv.z;

			// Return collision result
			return collision;
		}

		#endregion
	}
}

