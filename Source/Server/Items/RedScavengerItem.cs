/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using System.Drawing;
using System.Collections;
using CodeImp.Bloodmasters;
using CodeImp;

#if CLIENT
using CodeImp.Bloodmasters.Client;
#endif

namespace CodeImp.Bloodmasters.Server
{
	[ServerItem(4004, RespawnTime=0)]
	public class RedScavengerItem : ScavengerItem
	{
		#region ================== Constants
		
		#endregion
		
		#region ================== Variables
		
		#endregion
		
		#region ================== Constructor / Destructor
		
		// Constructor
		public RedScavengerItem(Thing t) : base(t)
		{
			// Set teams
			this.thisteam = TEAM.RED;
			this.otherteam = TEAM.BLUE;
			
			// For normal Scavenger game, place a White item instead
			if(General.server.GameType == GAMETYPE.SC)
			{
				// Make white item
				Item white = new WhiteScavengerItem(t);
				General.server.items.Add(white.Key, white);
			}
			
			// If this is not a Team Scavenger game, remove the item
			if(General.server.GameType != GAMETYPE.TSC) this.Temporary = true;
		}
		
		#endregion
	}
}
