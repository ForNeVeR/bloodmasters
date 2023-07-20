/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

namespace CodeImp.Bloodmasters.Client
{
	[ClientItem(9005, Visible=false, OnFloor=false)]
	public class SteamParticles : Item
	{
		#region ================== Constants

		private const int SPAWN_MIN_INTERVAL = 20;
		private const int SPAWN_RND_INTERVAL = 60;

		#endregion

		#region ================== Variables

		private int nextspawntime;

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public SteamParticles(Thing t) : base(t)
		{
		}

		#endregion

		#region ================== Processing

		// Processing
		public override void Process()
		{
			// Time to spawn a particle?
			if(nextspawntime < SharedGeneral.currenttime)
			{
				// In screen?
				if(Sector.VisualSector.InScreen)
				{
					// Spawn particle
					General.arena.p_smoke.Add(pos + Vector3D.Random(General.random, 6f, 6f, 0f),
						Vector3D.Random(General.random, 0.01f, 0.01f, 0.2f), General.ARGB(1f, 0.5f, 0.5f, 0.5f));

					// Make new spawn time
					nextspawntime = SharedGeneral.currenttime + SPAWN_MIN_INTERVAL + General.random.Next(SPAWN_RND_INTERVAL);
				}
			}

			// Pass control to base class
			base.Process();
		}

		#endregion
	}
}
