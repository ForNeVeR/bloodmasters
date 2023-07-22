/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System.Collections;

namespace CodeImp.Bloodmasters.Client
{
	public class ScavengerItem : Item
	{
		#region ================== Variables

		// Team info
		protected TEAM thisteam;
		protected TEAM otherteam;
		protected string otherteamname;
		protected string otherteamcolor;
		protected string thisteamname;
		protected string thisteamcolor;

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public ScavengerItem(Thing t) : base(t)
		{
		}

		// Disposer
		public override void Dispose()
		{
			// Dispose base
			base.Dispose();
		}

		#endregion

		#region ================== Methods

		// This counts the total items for a team
		public static int CountTotalItems(TEAM team)
		{
			int result = 0;

			// Go for all items in the map
			foreach(DictionaryEntry de in General.arena.Items)
			{
				// Is this a scavenger item?
				if(de.Value is ScavengerItem)
				{
					// Get the object
					ScavengerItem si = (ScavengerItem)de.Value;

					// Count item when for this team
					if(si.thisteam == team) result++;
				}
			}

			// Return result
			return result;
		}

		// This counts the remaining items for a team
		public static int CountRemainingItems(TEAM team)
		{
			int result = 0;

			// Go for all items in the map
			foreach(DictionaryEntry de in General.arena.Items)
			{
				// Is this a scavenger item?
				if(de.Value is ScavengerItem)
				{
					// Get the object
					ScavengerItem si = (ScavengerItem)de.Value;

					// Item for this team?
					if(si.thisteam == team)
					{
						// Count when item is not taken yet
						if(si.IsTaken == false) result++;
					}
				}
			}

			// Return result
			return result;
		}

		// This respawns all items for a team
		public static void RespawnItems(TEAM team)
		{
			// Go for all items in the map
			foreach(DictionaryEntry de in General.arena.Items)
			{
				// Is this a scavenger item?
				if(de.Value is ScavengerItem)
				{
					// Get the object
					ScavengerItem si = (ScavengerItem)de.Value;

					// Item for this team?
					if(si.thisteam == team)
					{
						// Respawn the item now
						si.Respawn(false);
					}
				}
			}
		}

		// This sets team names and colors
		protected void SetTeam(TEAM team)
		{
			// Check what team this item is on
			if(team == TEAM.RED)
			{
				// Set variables
				thisteam = TEAM.RED;
				otherteam = TEAM.BLUE;
				thisteamname = "red";
				thisteamcolor = "^4";
				otherteamname = "blue";
				otherteamcolor = "^1";
			}
			else if(team == TEAM.BLUE)
			{
				// Set variables
				thisteam = TEAM.BLUE;
				otherteam = TEAM.RED;
				thisteamname = "blue";
				thisteamcolor = "^1";
				otherteamname = "red";
				otherteamcolor = "^4";
			}
			else
			{
				// Set variables
				thisteam = TEAM.NONE;
				otherteam = TEAM.NONE;
				thisteamname = "white";
				thisteamcolor = "^7";
				otherteamname = "white";
				otherteamcolor = "^7";
			}
		}

		#endregion

		#region ================== Control

		// When picked up / taken
		public override void Take(Client clnt)
		{
			int remaining = 0;

			// Call the base class
			base.Take(clnt);

			// Item on same team as client?
			if(thisteam == clnt.Team)
			{
				// Count score
				clnt.Score++;
				General.teamscore[(int)clnt.Team]++;
			}
			else
			{
				// Decrease score
				clnt.Score--;
				General.teamscore[(int)clnt.Team]--;
			}

			// Count remaining items
			remaining = CountRemainingItems(thisteam);

			// Last item for this team?
			if(remaining == 0)
			{
				// Play capture sound
				DirectSound.PlaySound("flagcapture.wav");

				// Show message?
				if(thisteam != TEAM.NONE)
				{
					// Show message
					General.hud.ShowBigMessage(thisteamcolor + thisteamname.ToUpper() + " TEAM FINISHED A ROUND!", 3000);
				}

				// Respawn them all!
				RespawnItems(thisteam);
			}

			// Update scoreboard and hud
			General.scoreboard.Update();
			General.hud.UpdateScore();
		}

		// When processed
		public override void Process()
		{
			// Process base class
			base.Process();
		}

		#endregion
	}
}
