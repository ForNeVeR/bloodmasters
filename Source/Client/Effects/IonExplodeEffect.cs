/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using System.Collections;
using SharpDX.Direct3D9;

namespace CodeImp.Bloodmasters.Client
{
	public class IonExplodeEffect : VisualObject, ILightningNode
	{
		#region ================== Constants

		private const int SHOCK_DURATION = 500;

		#endregion

		#region ================== Variables

		private Sprite sprite;
		private Animation ani;
		private ClientSector sector;
		private bool disposed;
		private int shockendtime;
		private ArrayList lightnings = new ArrayList();
		private int source;
		private TEAM team;

		#endregion

		#region ================== Properties

		public Vector3D Velocity { get { return new Vector3D(0f, 0f, 0f); } }

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public IonExplodeEffect(Vector3D spawnpos, int sourceid, TEAM sourceteam)
		{
			Vector3D cpos;

			// Position
			this.pos = spawnpos;
			this.renderbias = 50f;
			this.renderpass = 2;

			// Source and team
			this.source = sourceid;
			this.team = sourceteam;

			// Determine current sector
			sector = (ClientSector)General.map.GetSubSectorAt(pos.x, pos.y).Sector;

			// Determine shock end time
			shockendtime = SharedGeneral.currenttime + SHOCK_DURATION;

			// Spawn the light
			if(DynamicLight.dynamiclights)
				new IonExplodeLight(spawnpos);

			// Only when in the screen
			if(sector.VisualSector.InScreen)
			{
				// Spawn particles
				for(int i = 0; i < 12; i++)
					General.arena.p_magic.Add(spawnpos + Vector3D.Random(General.random, 4f, 4f, 2f), Vector3D.Random(General.random, 0.2f, 0.2f, 0.2f), General.ARGB(1f, 1f, 1f, 1f));
				for(int i = 0; i < 12; i++)
					General.arena.p_magic.Add(spawnpos + Vector3D.Random(General.random, 4f, 4f, 2f), Vector3D.Random(General.random, 0.2f, 0.2f, 0.2f), General.ARGB(1f, 0.4f, 0.6f, 1f));
				for(int i = 0; i < 12; i++)
					General.arena.p_magic.Add(spawnpos + Vector3D.Random(General.random, 4f, 4f, 2f), Vector3D.Random(General.random, 0.2f, 0.2f, 0.2f), General.ARGB(1f, 0.1f, 0.2f, 1f));
			}

			// Make effect
			sprite = new Sprite(spawnpos + new Vector3D(2f, -2f, 15f), 15f, false, true);
			ani = Animation.CreateFrom("sprites/ionexplode.cfg");

			// Go for all actors
			foreach(Actor a in General.arena.Actors)
			{
				// Find client
				if(General.clients[a.ClientID] != null)
				{
					// Get reference to the client
					Client c = General.clients[a.ClientID];

					// Actor to shoot at?
					if(!a.DeadThreshold && (a.ClientID != this.source))
					{
						// No team game or on other team?
						if(!General.teamgame || (a.Team != team))
						{
							// Determine client position
							cpos = a.Position + new Vector3D(0f, 0f, 6f);

							// Calculate distance to this player
							Vector3D delta = cpos - this.Position;
							delta.z *= Consts.POWERUP_STATIC_Z_SCALE;
							float distance = delta.Length();
							delta.Normalize();

							// Within range?
							if(distance < Consts.ION_EXPLODE_RANGE)
							{
								// Check if nothing blocks in between
								if(!General.map.FindRayMapCollision(this.Position, cpos))
								{
									// Create lighting
									new Lightning(this, 1f, a, 8f, true, false);
								}
							}
						}
					}
				}
			}
		}

		// Disposer
		public override void Dispose()
		{
			// Clean up
			RemoveAllLightnings();
			sprite = null;
			ani = null;
			disposed = true;
			base.Dispose();
			GC.SuppressFinalize(this);
		}

		#endregion

		#region ================== Processing

		// Processing
		public override void Process()
		{
			// Not disposed?
			if(!disposed)
			{
				// Out of shocking time?
				if((shockendtime > 0) && (SharedGeneral.currenttime > shockendtime))
				{
					// Remove all lightnings
					RemoveAllLightnings();
					shockendtime = 0;
				}

				// Process lightnings
				foreach(Lightning l in lightnings) l.Process();

				// Process animation
				ani.Process();

				// Dispose me when animation has ended
				if(ani.Ended) this.Dispose();
			}
		}

		// This removes a lightning
		public void RemoveLightning(Lightning l)
		{
			if(lightnings.Contains(l)) lightnings.Remove(l);
		}

		// This adds a lightning
		public void AddLightning(Lightning l)
		{
			if(!lightnings.Contains(l)) lightnings.Add(l);
		}

		// This removes all lightnings
		private void RemoveAllLightnings()
		{
			// Are there any lightnings?
			if(lightnings.Count > 0)
			{
				// Dispose them all
				for(int i = lightnings.Count - 1; i >= 0; i--)
					((Lightning)lightnings[i]).Dispose();
			}
		}

		#endregion

		#region ================== Rendering

		// Rendering
		public override void Render()
		{
			// Within the map and not disposed?
			if((sector != null) && !disposed)
			{
				// Check if in screen
				if(sector.VisualSector.InScreen)
				{
					// Set render mode
					Direct3D.SetDrawMode(DRAWMODE.NADDITIVEALPHA);
					Direct3D.d3dd.SetRenderState(RenderState.TextureFactor, -1);
					//Direct3D.d3dd.SetRenderState(RenderState.ZEnable, false);

					// No lightmap
					Direct3D.d3dd.SetTexture(1, null);

					// Set animation frame
					Direct3D.d3dd.SetTexture(0, ani.CurrentFrame.texture);

					// Render sprite
					sprite.Render();

					// Restore Z buffer
					//Direct3D.d3dd.SetRenderState(RenderState.ZEnable, true);
				}
			}
		}

		#endregion
	}
}
