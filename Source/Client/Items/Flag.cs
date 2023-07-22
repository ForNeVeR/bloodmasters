/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using System.Globalization;

namespace CodeImp.Bloodmasters.Client
{
	public class Flag : Item
	{
		#region ================== Constants

		// Render bias
		private const float RENDER_BIAS_NORMAL = 1.4f;
		private const float RENDER_BIAS_ATTACHED = 2f;

		// Position when attached
		private const float ATTACHED_DISTANCE = 1f;

		#endregion

		#region ================== Variables

		// Flag emits light
		private DynamicLight light;

		// Original location
		private Vector3D origpos;

		// Last carrier position
		private Vector3D lastpos;

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
		public Flag(Thing t) : base(t)
		{
			// Keep original position
			this.origpos = this.pos;
			this.renderbias = RENDER_BIAS_NORMAL;

			// If this is not a CTF game, remove the flag
			if(General.gametype != GAMETYPE.CTF) this.Temporary = true;
		}

		// Disposer
		public override void Dispose()
		{
			// Clean up
			light.Dispose();

			// Dispose base
			base.Dispose();
		}

		#endregion

		// This sets team names and colors
		protected void SetTeam(TEAM team)
		{
			// Check what team this flag is on
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
			else
			{
				// Set variables
				thisteam = TEAM.BLUE;
				otherteam = TEAM.RED;
				thisteamname = "blue";
				thisteamcolor = "^1";
				otherteamname = "red";
				otherteamcolor = "^4";
			}
		}

		// This creates the dynamic light
		protected void CreateLight(int color)
		{
			// Create dynamic light
			light = new DynamicLight(this.pos, 15f, color, 3);
		}

		// When picked up / taken
		public override void Take(Client clnt)
		{
			// Attached (taken by other team) or picked up?
			if(this.IsAttached)
			{
				// Flag taken!
				General.console.AddMessage(clnt.Name + "^7 has taken the " + thisteamname + " flag");
				General.hud.ShowBigMessage(thisteamcolor + thisteamname.ToUpper() + " FLAG TAKEN!", 2000);
				DirectSound.PlaySound("flagtaken.wav");

				// Taken by me?
				if(clnt == General.localclient)
				{
					// You have the flag!
				}
				// Taken by teammate?
				else if(clnt.Team == General.localclient.Team)
				{
					// Your team has the enemy flag!
				}
				else
				{
					// The enemy has your flag!
				}

				// Update scoreboard
				General.scoreboard.Update();
			}

			// Call the base class
			base.Take(clnt);
		}

		// When respawn is called
		public override void Respawn(bool playsound)
		{
			// Return to original position
			this.Detach();
			this.Move(this.origpos.x, this.origpos.y, this.origpos.z);
			base.Respawn(playsound);
		}

		// When scored or returned
		public void Return(Client clnt, bool score)
		{
			// Scoring?
			if(score)
			{
				// Flag scored
				General.console.AddMessage(clnt.Name + "^7 scored for the " + otherteamname + " team");
				General.hud.ShowBigMessage(otherteamcolor + otherteamname.ToUpper() + " TEAM SCORES", 2000);
				DirectSound.PlaySound("flagcapture.wav");

				// Count score
				General.teamscore[(int)clnt.Team]++;
				clnt.Score++;
			}
			else
			{
				// Determine message to show
				if(clnt != null)
					General.console.AddMessage(clnt.Name + "^7 returned the " + thisteamname + " flag");
				else
					General.console.AddMessage(CultureInfo.InvariantCulture.TextInfo.ToTitleCase(thisteamname) + " flag returned");

				// Flag returned
				General.hud.ShowBigMessage(thisteamcolor + thisteamname.ToUpper() + " FLAG RETURNED", 2000);
				DirectSound.PlaySound("flagreturn.wav");
			}

			// Return to original position
			this.Detach();
			this.Move(this.origpos.x, this.origpos.y, this.origpos.z);
			this.RespawnEffect();

			// Update scoreboard
			General.scoreboard.Update();
		}

		// When processed
		public override void Process()
		{
			// Process base class
			base.Process();

			// Check if attached
			if(this.IsAttached)
			{
				// Same renderbias as actor
				this.renderbias = RENDER_BIAS_ATTACHED;

				// Check if owner is alive
				if((this.Owner.Actor != null) && (!this.Owner.Actor.IsDead))
				{
					// Get the vector for actor angle
					float a = this.Owner.Actor.AimAngle + (float)Math.PI * 0.5f;
					Vector3D anglevec = Vector3D.FromMapAngle(a, ATTACHED_DISTANCE);

					// Move flag with client
					lastpos = this.Owner.Actor.Position;
					Vector3D pos = lastpos - anglevec;
					this.Move(pos.x, pos.y, pos.z);
				}
				else
				{
					// Drop flag
					this.Drop();
				}
			}

			// Update light
			light.Position = this.pos + new Vector3D(0f, 0f, 8f);
		}

		// When dropping
		public void Drop()
		{
			// Check if attached
			if(this.IsAttached)
			{
				// Flag dropped
				General.console.AddMessage(this.Owner.Name + "^7 dropped the " + thisteamname + " flag");
				DirectSound.PlaySound("flagdropped.wav");

				// Detach flag
				this.renderbias = RENDER_BIAS_NORMAL;
				this.Detach();

				// Move flag to last client position
				this.Move(lastpos.x, lastpos.y, lastpos.z);

				// Update scoreboard
				General.scoreboard.Update();
			}
		}
	}
}
