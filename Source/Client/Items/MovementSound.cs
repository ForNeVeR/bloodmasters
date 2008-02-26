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
	[ClientItem(9999, Temporary=true)]
	public class MovementSound : Item
	{
		// Constructor
		public MovementSound(Thing t) : base(t)
		{
			// Indicate that this sector must play movement sounds
			t.Sector.PlayMovementSound = true;
		}
	}
}
