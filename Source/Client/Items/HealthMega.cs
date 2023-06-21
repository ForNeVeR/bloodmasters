/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

namespace CodeImp.Bloodmasters.Client
{
	[ClientItem(2003, Sprite="health3.tga",
					  Description="100% Health",
					  Sound="pickuphealth.wav")]
	public class HealthMega : Item
	{
		#region ================== Constants

		#endregion

		#region ================== Variables

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public HealthMega(Thing t) : base(t)
		{
		}

		#endregion
	}
}
