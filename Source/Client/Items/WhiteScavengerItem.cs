/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

namespace CodeImp.Bloodmasters.Client
{
	[ClientItem(4003, Sprite="sc_white.tga",
					  Bob = true,
					  Description="Scavenger Item",
					  Sound="pickuphealth.wav")]
	public class WhiteScavengerItem : ScavengerItem
	{
		#region ================== Constants

		#endregion

		#region ================== Variables

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public WhiteScavengerItem(Thing t) : base(t)
		{
			// Set team
			SetTeam(TEAM.NONE);

			// If this is not a Scavenger game, remove the item
			if(General.gametype != GAMETYPE.SC) this.Temporary = true;
		}

		#endregion
	}
}
