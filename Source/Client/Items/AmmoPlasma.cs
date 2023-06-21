/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

namespace CodeImp.Bloodmasters.Client
{
	[ClientItem(8002, Sprite="ammo_plasma.tga",
					  Description="Plasma",
					  Sound="pickuphealth.wav")]
	public class AmmoPlasma : Item
	{
		#region ================== Constants

		#endregion

		#region ================== Variables

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public AmmoPlasma(Thing t) : base(t)
		{
		}

		#endregion
	}
}
