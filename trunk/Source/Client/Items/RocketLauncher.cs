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
	[ClientItem(1004, Sprite="rocketl.tga",
					  Bob = true,
					  Description="Rocket Launcher",
					  SpriteOffset=-0.6f,
					  Sound="weaponpickup.wav")]
	public class RocketLauncher : Item
	{
		#region ================== Constants
		
		#endregion
		
		#region ================== Variables
		
		#endregion
		
		#region ================== Constructor / Destructor
		
		// Constructor
		public RocketLauncher(Thing t) : base(t)
		{
		}
		
		#endregion
		
		// When picked up / taken
		public override void Take(Client clnt)
		{
			// Taken by me?
			if(General.localclient == clnt)
			{
				// Display item description
				General.hud.ShowItemMessage(this.Description);
				
				// Request switch weapon when automatically switching
				if(General.autoswitchweapon && !General.localclient.IsShooting)
					clnt.RequestSwitchWeaponTo(WEAPON.ROCKET_LAUNCHER, false);
			}
		}
	}
}
