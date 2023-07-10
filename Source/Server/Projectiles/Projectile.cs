/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using System.Collections;
#if CLIENT
using CodeImp.Bloodmasters.Client;
#endif

namespace CodeImp.Bloodmasters.Server
{
	public abstract class Projectile
	{
		#region ================== Constants

		// Amount of length to move before
		// collision with source allowed
		protected const float FREE_TRAVEL_LENGTH = 15f;

		#endregion

		#region ================== Variables

		// Source
		private Client source;

		// Travel
		private float velocitylength = 0f;
		protected float travellength = 0f;

		// ID
		private string projectileid;
		private PROJECTILE type;

		// Members
		protected PhysicsState state;
		protected Sector sector;
		protected bool teleportable = true;

		#endregion

		#region ================== Properties

		public string ID { get { return projectileid; } }
		public PROJECTILE Type { get { return type; } }
		public Vector3D Pos { get { return state.pos; } }
		public Vector3D Vel { get { return state.vel; } }
		public Client Source { get { return source; } }

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public Projectile(Vector3D start, Vector3D vel, Client source)
		{
			// Check if class has a ProjectileInfo attribute
			if(Attribute.IsDefined(this.GetType(), typeof(ProjectileInfo), false))
			{
				// Get ProjectileInfo attributes
				ProjectileInfo attr = (ProjectileInfo)Attribute.GetCustomAttribute(this.GetType(), typeof(ProjectileInfo), false);

				// Copy settings from attribute
				this.type = attr.Type;
			}

			// Add projectile
			projectileid = General.server.NewProjectile(this);

			// Keep the source
			this.source = source;

			// Copy properties
			state = new ServerPhysicsState(General.server.map);
			state.IsPlayer = false;
			state.Blocking = false;
			state.Bounce = true;
			state.Radius = 1.1f;
			state.Friction = 0.9f;
			state.Redirect = false;
			state.StepUp = false;
			state.pos = start;
			state.vel = vel;

			// Where are we now?
			sector = General.server.map.GetSubSectorAt(state.pos.x, state.pos.y).Sector;

			// Determine velocity length
			velocitylength = vel.Length();

			// Broadcast projectile message
			General.server.BroadcastSpawnProjectile(this);
		}

		// Dispose
		public virtual void Dispose()
		{
			// Clean up
			source = null;
			state.Dispose();
			General.server.disposeprojectiles.Add(this);
		}

		#endregion

		#region ================== Methods

		// This applies effects of crossing lines
		private void ApplyCrossLineEffect(Sidedef crossline)
		{
			// Check the line effect
			switch(crossline.Linedef.Action)
			{
				// Teleport!
				case ACTION.TELEPORT:

					// Teleport when on the floor and crossing from the front side
					if((state.pos.z < sector.CurrentFloor + Consts.TELEPORT_HEIGHT) &&
					   (crossline == crossline.Linedef.Front) && this.teleportable)
						TeleportToThing(crossline.Linedef.Arg[0]);
					break;
			}
		}

		// This teleports the projectile
		private void TeleportToThing(int tag)
		{
			ArrayList dests = new ArrayList(10);
			float zdiff;

			// Keep old position
			Vector3D oldpos = state.pos;

			// Go for all things on the map
			foreach(Thing t in General.server.map.Things)
			{
				// Is this a spawn point with correct tag?
				if((t.Type == (int)THINGTYPE.TELEPORT) && (t.Tag == tag))
				{
					// Add to the list of destinations
					dests.Add(t);
				}
			}

			// Spawn spots found?
			if(dests.Count > 0)
			{
				// Choose a random destination
				Thing ft = (Thing)dests[General.random.Next(dests.Count)];

				// Determine floor height difference
				zdiff = state.pos.z - sector.CurrentFloor;

				// Determine sector where projectile will be at
				sector = General.server.map.GetSubSectorAt(ft.X, ft.Y).Sector;

				// Move the projectile here
				state.pos = new Vector3D(ft.X, ft.Y, sector.CurrentFloor + zdiff);
				state.vel = Vector3D.FromMapAngle(ft.Angle + (float)Math.PI * 0.5f, state.vel.Length());

				// Broadcast projectile message
				General.server.BroadcastTeleportProjectile(oldpos, this);
			}
		}

		// Call this to change the projectile position/velocity
		public virtual void Update(Vector3D newpos, Vector3D newvel)
		{
			// Apply position and velocity
			state.pos = newpos;
			state.vel = newvel;

			// Determine velocity length
			velocitylength = newvel.Length();

			// Broadcast projectile message
			General.server.BroadcastUpdateProjectile(this);
		}

		// Call this to destroy the projectile
		public virtual void Destroy(bool silent, Client hitplayer)
		{
			// Broadcast projectile message
			General.server.BroadcastDestroyProjectile(this, silent, hitplayer);

			// And dispose me
			this.Dispose();
		}

		// Call this when particle collides
		protected virtual void Collide(object hitobj)
		{
			// The specific projectile itsself must
			// respond to this event.
		}

		// Processes the projectile
		public virtual void Process()
		{
			bool collides;
			object hitobj;
			Sidedef crossline;
			Client ignoreclient = null;
			Vector3D oldvel;

			// Ignore source client?
			if(travellength <= FREE_TRAVEL_LENGTH) ignoreclient = source;

			// Keep previous pos/vel
			oldvel = state.vel;

			// Apply velocity
			collides = state.ApplyVelocity(true, true, General.server.clients, ignoreclient, out crossline, out hitobj);

			// Outside the map?
			if(!General.server.map.WithinBoundaries(state.pos.x, state.pos.y))
			{
				// Destroy silently
				this.Destroy(true, null);
				return;
			}

			// Where are we now?
			sector = General.server.map.GetSubSectorAt(state.pos.x, state.pos.y).Sector;

			// Collision?
			if(collides)
			{
				// Restore old pos/vel
				state.vel = oldvel;
				Collide(hitobj);
			}
			else
			{
				// Apply line effects
				if(crossline != null) ApplyCrossLineEffect(crossline);

				// Underneath a floor?
				if(sector.CurrentFloor > state.pos.z)
				{
					// Restore old pos/vel
					state.vel = oldvel;
					Collide(sector);
				}

				// Above a ceiling?
				if(sector.HasCeiling && (state.pos.z > sector.HeightCeil) &&
										(state.pos.z < sector.FakeHeightCeil))
				{
					// Restore old pos/vel
					state.vel = oldvel;
					Collide(sector);
				}
			}

			// Count distance traveled
			travellength += velocitylength;
		}

		#endregion
	}
}
