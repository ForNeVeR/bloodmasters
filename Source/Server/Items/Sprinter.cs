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
	[ServerItem(3002, RespawnTime=90000)]
	public class Sprinter : Item
	{
		#region ================== Constants
		
		#endregion
		
		#region ================== Variables
		
		#endregion
		
		#region ================== Constructor / Destructor
		
		// Constructor
		public Sprinter(Thing t) : base(t)
		{
		}
		
		#endregion
		
		#region ================== Control
		
		// This is calledwhen the item is being touched by a player
		public override void Pickup(Client c)
		{
			// Do what you have to do
			base.Pickup(c);
			
			// Take the item
			this.Take(c);
			
			// Give powerup to player
			c.GivePowerup(POWERUP.SPEED, Consts.POWERUP_SPEED_COUNT);
		}
		
		#endregion
	}
}
