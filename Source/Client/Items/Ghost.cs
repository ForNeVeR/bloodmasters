/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

namespace CodeImp.Bloodmasters.Client
{
	[ClientItem(3004, Sprite="invisible.cfg",
					  Bob = true,
					  Description="Ghost",
					  Sound="pickuppowerup.wav")]
	[PowerupItem(R=0.5f, G=0.5f, B=0.5f)]
	public class Ghost : Powerup
	{
		#region ================== Constants

		#endregion

		#region ================== Variables

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public Ghost(Thing t) : base(t)
		{
		}

		#endregion

		// When picked up / taken
		public override void Take(Client clnt)
		{
			// Taken by me?
			if(General.localclient == clnt)
			{
				// Set the powerup countdown
				clnt.SetPowerupCountdown(Consts.POWERUP_GHOST_COUNT, false);
			}

			// Call the base class
			base.Take(clnt);
		}
	}
}
