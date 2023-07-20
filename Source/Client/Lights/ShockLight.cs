/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

namespace CodeImp.Bloodmasters.Client
{
	public class ShockLight : DynamicLight
	{
		#region ================== Constants

		#endregion

		#region ================== Variables

		private int timeout;

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public ShockLight(Vector3D pos, int duration) : base(pos, 10f, General.ARGB(0.2f, 0.2f, 0.6f, 1f), 2)
		{
			// Set the timeout
			timeout = SharedGeneral.currenttime + duration;
		}

		// Disposer
		public override void Dispose()
		{
			// Dispose base
			base.Dispose();
		}

		#endregion

		#region ================== Processing

		// Processing
		public override void Process()
		{
			// Process base
			base.Process();

			// Dispose when timed out
			if(timeout < SharedGeneral.currenttime) this.Dispose();
		}

		#endregion
	}
}
