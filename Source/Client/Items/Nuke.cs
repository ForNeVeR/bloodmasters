/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

namespace CodeImp.Bloodmasters.Client
{
	[ClientItem(3005, Sprite="nuke.cfg",
					  Bob = true,
					  Description="Nuke",
					  Sound="pickuppowerup.wav")]
	[PowerupItem(R=0.4f, G=0.4f, B=0.2f)]
	public class Nuke : Powerup
	{
		#region ================== Constants

		#endregion

		#region ================== Variables

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public Nuke(Thing t) : base(t)
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
				clnt.SetPowerupCountdown(Consts.POWERUP_NUKE_COUNT, false);
			}

			// Call the base class
			base.Take(clnt);
		}
	}
}
