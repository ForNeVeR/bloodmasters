/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

#if CLIENT
using CodeImp.Bloodmasters.Client;
#endif

namespace CodeImp.Bloodmasters.Server
{
	[ServerItem(4005, RespawnTime=0)]
	public class BlueScavengerItem : ScavengerItem
	{
		#region ================== Constants

		#endregion

		#region ================== Variables

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public BlueScavengerItem(Thing t) : base(t)
		{
			// Set teams
			this.thisteam = TEAM.BLUE;
			this.otherteam = TEAM.RED;

			// For normal Scavenger game, place a White item instead
			if(Global.Instance.Server.GameType == GAMETYPE.SC)
			{
				// Make white item
				Item white = new WhiteScavengerItem(t);
				Global.Instance.Server.items.Add(white.Key, white);
			}

			// If this is not a Team Scavenger game, remove the item
			if(Global.Instance.Server.GameType != GAMETYPE.TSC) this.Temporary = true;
		}

		#endregion
	}
}
