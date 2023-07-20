/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

namespace CodeImp.Bloodmasters.Client
{
	[ClientItem(7003, Temporary=true)]
	public class DecalFloor : Item
	{
		#region ================== Constants

		#endregion

		#region ================== Variables

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public DecalFloor(Thing t) : base(t)
		{
			// Create the decal
			FloorDecal.Spawn(t.Sector, t.X, t.Y, FloorDecal.blooddecals, true, false, false);
		}

		#endregion
	}
}
