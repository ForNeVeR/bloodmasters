/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using System.Drawing;
using System.Collections;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using CodeImp.Bloodmasters;
using CodeImp;

namespace CodeImp.Bloodmasters.Client
{
	[ClientItem(4005, Sprite="sc_blue.tga",
					  Bob = true,
					  Description="Scavenger Item",
					  Sound="pickuphealth.wav")]
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
			// Set team
			SetTeam(TEAM.BLUE);
			
			// For normal Scavenger game, place a White item instead
			if(General.gametype == GAMETYPE.SC)
			{
				// Make white item
				Item white = new WhiteScavengerItem(t);
				General.arena.Items.Add(white.Key, white);
			}
			
			// If this is not a Team Scavenger game, remove the item
			if(General.gametype != GAMETYPE.TSC) this.Temporary = true;
		}
		
		#endregion
	}
}
