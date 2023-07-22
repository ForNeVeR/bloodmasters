/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

namespace CodeImp.Bloodmasters.Client
{
	[ClientItem(2001, Sprite="health1.tga",
					  Description="5% Health",
					  Sound="pickuphealth.wav")]
	public class Health5 : Item
	{
		#region ================== Constants

		#endregion

		#region ================== Variables

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public Health5(Thing t) : base(t)
		{
		}

		#endregion
	}
}
