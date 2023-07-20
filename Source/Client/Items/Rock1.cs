/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

namespace CodeImp.Bloodmasters.Client
{
	[ClientItem(7004, Sprite="rock1.tga", Description="Rock", Bob=false)]
	public class Rock1 : Item
	{
		#region ================== Constants

		#endregion

		#region ================== Variables

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public Rock1(Thing t) : base(t)
		{
			// Lower than everything else
			renderbias = -30f;
			renderpass = 0;
		}

		#endregion

		#region ================== Constructor / Destructor

		// Override RenderShadow so no shadow is rendered
		public override void RenderShadow()
		{
			// No shadow
		}

		#endregion
	}
}
