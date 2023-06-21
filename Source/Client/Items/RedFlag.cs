/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

namespace CodeImp.Bloodmasters.Client
{
	[ClientItem(4002, Sprite="redflag.tga",
					  Bob = true,
					  Description="Red Flag",
					  Sound="pickuppowerup.wav")]
	public class RedFlag : Flag
	{
		#region ================== Constants

		#endregion

		#region ================== Variables

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public RedFlag(Thing t) : base(t)
		{
			// Create dynamic light
			this.CreateLight(General.ARGB(1f, 1f, 0.5f, 0.4f));

			// Set team
			SetTeam(TEAM.RED);
		}

		#endregion
	}
}
