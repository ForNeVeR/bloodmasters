/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

#if CLIENT
using CodeImp.Bloodmasters.Client;
#endif

namespace CodeImp.Bloodmasters.Server
{
	public class ProxyDoor : DynamicSector
	{
		#region ================== Constants

		// Timing and speed
		private const int CLOSE_DELAY = 800;
		private const float SPEED = 2f;

		#endregion

		#region ================== Variables

		// Door status
		private DOORSTATUS status;

		// Time when to close the door
		// This is 0 when door is not to be closed!
		private int closetime;

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public ProxyDoor(Sector s) : base(s)
		{
			// Initialize
			this.status = DOORSTATUS.CLOSED;
		}

		// Disposer
		public override void Dispose()
		{
			// Dispose base class
			base.Dispose();
		}

		#endregion

		#region ================== Processing

		// This processes te door
		public override void Process()
		{
			bool occupied = false;

			// Process the sector movement
			sector.Process();

			// Go for all clients to check if anyone
			// is in this sector or a proximity sector
			foreach(Client c in General.server.clients)
			{
				// Client in the game?
				if((c != null) && !c.Loading && !c.Spectator && c.IsAlive)
				{
					// Client touching this floor?
					if(c.State.pos.z <= c.Sector.CurrentFloor + Consts.FLOOR_TOUCH_TOLERANCE)
					{
						// Check if the sector has the same tag
						if(sector.Tag == c.Sector.Tag)
						{
							// Someone is in the door proximity
							occupied = true;
							break;
						}
					}
				}
			}

			// If the door was opening and the sector reached
			// its open height then the door is now fully opened
			if((status == DOORSTATUS.OPENING) &&
			   (sector.CurrentFloor == sector.LowestFloor))
			{
				// Door fully open
				status = DOORSTATUS.OPEN;
				SendSectorUpdate = true;
			}

			// If the door was closing and the sector reached
			// its closed height then the door is now fully closed
			if((status == DOORSTATUS.CLOSING) &&
			   (sector.CurrentFloor == sector.HeightFloor))
			{
				// Door fully closed
				status = DOORSTATUS.CLOSED;
				SendSectorUpdate = true;
			}

			// Check if occupied
			if(occupied)
			{
				// Is the door closed or closing?
				if((status == DOORSTATUS.CLOSED) ||
				   (status == DOORSTATUS.CLOSING))
				{
					// Open the door now!
					sector.MoveTo(sector.LowestFloor, SPEED);
					status = DOORSTATUS.OPENING;
					SendSectorUpdate = true;
				}

				// Door may not close now
				closetime = 0;
			}
			else
			{
				// Is the door opened or opening?
				if((status == DOORSTATUS.OPEN) ||
				   (status == DOORSTATUS.OPENING))
				{
					// Timer to close not set?
					if(closetime == 0)
					{
						// Set timer to close the door
						closetime = SharedGeneral.currenttime + CLOSE_DELAY;
					}
					// Time to close the door?
					else if(closetime < SharedGeneral.currenttime)
					{
						// Close the door now!
						sector.MoveTo(sector.HeightFloor, SPEED);
						status = DOORSTATUS.CLOSING;
						SendSectorUpdate = true;
					}
				}
			}

			// Process base class
			base.Process();
		}

		#endregion
	}
}
