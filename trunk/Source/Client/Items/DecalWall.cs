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
	[ClientItem(7002, Temporary=true, OnFloor=false)]
	public class DecalWall : Item
	{
		#region ================== Constants
		
		#endregion
		
		#region ================== Variables
		
		#endregion
		
		#region ================== Constructor / Destructor
		
		// Constructor
		public DecalWall(Thing t) : base(t)
		{
			// Create the decal
			WallDecal.Spawn(t.X, t.Y, t.Z + t.Sector.HeightFloor, 10f, WallDecal.blooddecals, true);
		}
		
		#endregion
	}
}
