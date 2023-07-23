/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System.Collections;
#if CLIENT
using CodeImp.Bloodmasters.Client;
#endif

namespace CodeImp.Bloodmasters.Server
{
	public class ScavengerItem : Item
	{
		#region ================== Constants

		#endregion

		#region ================== Variables

		// Team info
		protected TEAM thisteam;
		protected TEAM otherteam;

		#endregion

		#region ================== Properties

		public TEAM ThisTeam { get { return thisteam; } }
		public TEAM OtherTeam { get { return otherteam; } }

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public ScavengerItem(Thing t) : base(t)
		{
		}

		#endregion

		#region ================== Methods

		// This counts the remaining items for a team
		public static int CountRemainingItems(TEAM team)
		{
			int result = 0;

			// Go for all items in the map
			foreach(DictionaryEntry de in Global.Instance.Server.items)
			{
				// Is this a scavenger item?
				if(de.Value is ScavengerItem)
				{
					// Get the object
					ScavengerItem si = (ScavengerItem)de.Value;

					// Item for this team?
					if(si.ThisTeam == team)
					{
						// Count when item is not taken yet
						if(si.IsTaken == false) result++;
					}
				}
			}

			// Return result
			return result;
		}

		// This counts the remaining items for a team
		public static void RespawnItems(TEAM team)
		{
			// Go for all items in the map
			foreach(DictionaryEntry de in Global.Instance.Server.items)
			{
				// Is this a scavenger item?
				if(de.Value is ScavengerItem)
				{
					// Get the object
					ScavengerItem si = (ScavengerItem)de.Value;

					// Item for this team?
					if(si.ThisTeam == team)
					{
						// Set the respawn time to respawn immediately
						si.RespawnDelay = 1;
					}
				}
			}
		}

		#endregion

		#region ================== Control

		// This is called when the item is being touched by a player
		public override void Pickup(Client c)
		{
			// Only when playing!
			if(Global.Instance.Server.GameState == GAMESTATE.PLAYING)
			{
				// Do what you have to do
				base.Pickup(c);

				// Take the item
				this.RespawnDelay = 0;
				this.Take(c);

				// Taken by client on this team?
				if(c.Team == thisteam)
				{
					// Add to score
					c.AddToScore(1);
				}
				else
				{
					// Remove from score
					c.AddToScore(-1);
				}

				// Last item of this team?
				if(CountRemainingItems(thisteam) == 0)
				{
					// Respawn them all!
					RespawnItems(thisteam);
				}
			}
		}

		#endregion
	}
}
