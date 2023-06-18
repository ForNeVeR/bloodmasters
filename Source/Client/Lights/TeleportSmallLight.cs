/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using CodeImp.Bloodmasters.Client.Graphics;

namespace CodeImp.Bloodmasters.Client
{
	public class TeleportSmallLight : DynamicLight
	{
		#region ================== Constants
		
		private const float CHANGE_BRIGHT_SCALE = 0.03f;
		
		#endregion
		
		#region ================== Variables
		
		private float alpha = 1f;
		
		#endregion
		
		#region ================== Constructor / Destructor
		
		// Constructor
		public TeleportSmallLight(Vector3D pos) : base(pos, 8f, General.ARGB(1f, 1f, 1f, 1f), 2)
		{
			// Reposition
			this.Position = pos + new Vector3D(0f, 0f, 3f);
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
			
			// Change alpha
			alpha -= CHANGE_BRIGHT_SCALE;
			if(alpha <= 0f)
			{
				// Dispose
				this.Dispose();
			}
			else
			{
				// Change color
				this.Color = ColorOperator.Scale(this.BaseColor, alpha);
			}
		}
		
		#endregion
	}
}
