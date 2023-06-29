/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using CodeImp.Bloodmasters.Client.Graphics;

namespace CodeImp.Bloodmasters.Client
{
	public class SpawnLight : DynamicLight
	{
		#region ================== Constants

		private const int CHANGE_INTERVAL = 50;
		private const float CHANGE_BRIGHT_SCALE = 0.08f;
		private const float START_BRIGHTNESS = 3f;
		private const float END_BRIGHTNESS = 0.01f;

		#endregion

		#region ================== Variables

		private int nextchangetime;
		private float brightness;

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public SpawnLight(Vector3D spawnpos, int basecolor) :
			base(spawnpos, 10f, basecolor, 3)
		{
			// Start
			brightness = START_BRIGHTNESS;
			nextchangetime = SharedGeneral.currenttime + General.random.Next(CHANGE_INTERVAL);
			Position = spawnpos + new Vector3D(0f, 0f, 3f);
		}

		#endregion

		#region ================== Processing

		// Processing
		public override void Process()
		{
			// Do not go here when disposed
			if(base.Disposed) return;

			// Check if time to change
			if(nextchangetime <= SharedGeneral.currenttime)
			{
				// Change light color
				brightness -= CHANGE_BRIGHT_SCALE;
				if(brightness <= END_BRIGHTNESS)
				{
					// Destroy the light
					this.Dispose();
				}
				else
				{
					// Change color
					this.Color = ColorOperator.Scale(this.BaseColor, brightness);

					// Make next change time
					nextchangetime += CHANGE_INTERVAL;
				}
			}

			// Process base light
			base.Process();
		}

		#endregion
	}
}
