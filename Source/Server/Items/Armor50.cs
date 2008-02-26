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
	[ServerItem(2005, RespawnTime=10000)]
	public class Armor50 : Item
	{
		#region ================== Constants
		
		#endregion
		
		#region ================== Variables
		
		#endregion
		
		#region ================== Constructor / Destructor
		
		// Constructor
		public Armor50(Thing t) : base(t)
		{
		}
		
		#endregion
		
		#region ================== Control
		
		// This is calledwhen the item is being touched by a player
		public override void Pickup(Client c)
		{
			// Check if the client needs health
			if(c.Armor < 100)
			{
				// Do what you have to do
				base.Pickup(c);
				
				// Take the item
				this.Take(c);
				
				// Add 50% armor to the client
				c.AddToStatus(0, 100, 50, 100);
			}
		}

		
		#endregion
	}
}
