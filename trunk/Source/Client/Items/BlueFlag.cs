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
	[ClientItem(4001, Sprite="blueflag.tga",
					  Bob = true,
					  Description="Blue Flag",
					  Sound="pickuppowerup.wav")]
	public class BlueFlag : Flag
	{
		#region ================== Constants
		
		#endregion
		
		#region ================== Variables
		
		#endregion
		
		#region ================== Constructor / Destructor
		
		// Constructor
		public BlueFlag(Thing t) : base(t)
		{
			// Create dynamic light
			this.CreateLight(General.ARGB(1f, 0.2f, 0.4f, 1f));
			
			// Set team
			SetTeam(TEAM.BLUE);
		}
		
		#endregion
	}
}
