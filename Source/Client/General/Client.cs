/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using FireAndForgetAudioSample;
using System;
using System.Collections;
using System.Drawing;
using System.Globalization;
using System.IO;

namespace CodeImp.Bloodmasters.Client
{
	public class Client
	{
		#region ================== Constants

		// Rate at which 'idle' ClientMoves are sent to the server
		public const int CLIENTMOVE_INTERVAL = 50;

		// Static powerup rate for client only
		private const int POWERUP_STATIC_RATE = 50;

		// Death sound variants
		public readonly int[] DEATH_SOUND_VARS = new int[] {3, 2, 0};

		// Teleport delay
		public const int TELEPORT_DELAY = 500;

		// Shield hitpoint distance
		public const float SHIELD_DISTANCE = 5f;

		#endregion

		#region ================== Variables

		// In-game
		private Actor actor = null;

		// Client properties
		private int id;
		private string name;
		private string formattedname;
		private bool spectator = true;
		private TEAM team = TEAM.NONE;
		private int lastping;
		private int lastloss;
		private bool loading;
		private bool shooting = false;
		private Item carry = null;

		// Local client only
		private ArrayList localmoves;

		// Player status (local only)
		private int health;
		private int armor;

		// Weapons/ammo
		private int[] ammo = new int[(int)AMMO.TOTAL_AMMO_TYPES];
		private Weapon[] allweapons = new Weapon[(int)WEAPON.TOTAL_WEAPONS];
		private Weapon currentweapon = null;
		private Weapon switchweapon = null;
		private bool weaponswitchlock = false;

		// Powerup
		private POWERUP powerup;
		private int powercount;
		private int powerinterval;
		private int powerintcount;
		private bool powerupfired;

		// Player score
		private int score;
		private int frags;
		private int deaths;

		// Networking
		private int clientmovetime;
		private float prevwalkangle = -2f;
		private bool prevshooting = false;
		public static int timenudge;

		// Misc
		public static bool showcollisions;
		public static bool teamcolorednames;
		public bool respawnpressed;
		private ISound hurtsound = null;

		#endregion

		#region ================== Properties

		public int ID { get { return id; } }
		public string Name { get { return name; } }
		public Item Carrying { get { return carry; } set { carry = value; } }
		public string FormattedName { get { return formattedname; } set { formattedname = value; } }
		public bool IsSpectator { get { return spectator; } set { spectator = value; } }
		public bool IsLocal { get { return (General.localclient == this); } }
		public bool IsLoading { get { return loading; } }
		public TEAM Team { get { return team; } set { team = value; } }
		public Actor Actor { get { return actor; } }
		public int Ping { get { return lastping; } }
		public int Loss { get { return lastloss; } }
		public int Health { get { return health; } set { health = value; } }
		public int Armor { get { return armor; } set { armor = value; } }
		public int Score { get { return score; } set { score = value; } }
		public int Frags { get { return frags; } set { frags = value; } }
		public int Deaths { get { return deaths; } set { deaths = value; } }
		public int[] Ammo { get { return ammo; } }
		public Weapon CurrentWeapon { get { return currentweapon; } }
		public Weapon SwitchToWeapon { get { return switchweapon; } }
		public Weapon[] AllWeapons { get { return allweapons; } }
		public POWERUP Powerup { get { return powerup; } }
		public int PowerupCount { get { return powercount; } }
		public bool PowerupFired { get { return powerupfired; } }
		public bool IsShooting { get { return shooting; } }

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public Client(int id, bool spect, TEAM team, string name, bool local)
		{
			// Set properties
			this.id = id;
			this.spectator = spect;
			this.team = team;
			SetName(name);

			// Initialize ClientMove timer
			clientmovetime = SharedGeneral.currenttime;

			// Add to scoreboard
			General.scoreboard.AddClient(this);

			// Check if local client
			if(local)
			{
				// Set up variables for local client
				localmoves = new ArrayList();
			}
		}

		// Disposer
		public void Dispose()
		{
			// Remove from scoreboard
			General.scoreboard.RemoveClient(this);

			// Clean up
			if(hurtsound != null) hurtsound.Dispose();
			DestroyActor(false);
			General.clients[id] = null;
			localmoves = null;
			GC.SuppressFinalize(this);
		}

		#endregion

		#region ================== Weapons / Ammo

		// This gives all weapons
		public void GiveAllWeapons()
		{
			// Give all weapons
			for(int i = 0; i < (int)WEAPON.TOTAL_WEAPONS; i++) GiveWeapon((WEAPON)i);
		}

		// This releases all weapons
		public void ReleaseAllWeapons()
		{
			// Go for all weapons
			for(int i = 0; i < (int)WEAPON.TOTAL_WEAPONS; i++)
			{
				// Release it
				if(allweapons[i] != null) allweapons[i].Released();
			}
		}

		// This gives a weapon
		public void GiveWeapon(WEAPON weaponid)
		{
			// If I dont have this weapon yet
			if(!HasWeapon(weaponid))
			{
				// Give me this weapon
				allweapons[(int)weaponid] = Weapon.CreateFromID(this, weaponid);
				if(General.autoswitchweapon && !shooting) switchweapon = allweapons[(int)weaponid];

				// Update weapon display
				if(this == General.localclient) General.weapondisplay.UpdateWeaponSet();
			}
		}

		// This checks if a weapon is available
		public bool HasWeapon(WEAPON weaponid)
		{
			return (allweapons[(int)weaponid] != null);
		}

		// This removes a weapon
		public void RemoveWeapon(WEAPON weaponid)
		{
			// Weapon exists?
			if(HasWeapon(weaponid))
			{
				// Current weapon?
				if(currentweapon == allweapons[(int)weaponid])
				{
					// No more current weapon
					currentweapon = null;
				}

				// Switching to weapon?
				if(switchweapon == allweapons[(int)weaponid])
				{
					// No more switching to this weapon
					switchweapon = null;
				}

				// Dispose and remove weapon
				allweapons[(int)weaponid].Dispose();
				allweapons[(int)weaponid] = null;

				// Update weapon display
				if(this == General.localclient) General.weapondisplay.UpdateWeaponSet();
			}
		}

		// This removes all weapons
		public void ClearWeapons()
		{
			// Remove all weapons
			for(int i = 0; i < (int)WEAPON.TOTAL_WEAPONS; i++) RemoveWeapon((WEAPON)i);
			currentweapon = null;
			switchweapon = null;
			weaponswitchlock = false;
			powerup = POWERUP.NONE;
			powercount = 0;
			powerupfired = false;
		}

		// This switches the current weapon
		public void SwitchWeapon(WEAPON weaponid, bool silent)
		{
			// Weapon available?
			if(allweapons[(int)weaponid] != null)
			{
				// Set the current weapon
				currentweapon = allweapons[(int)weaponid];

				// Release lock if this is the wanted weapon
				//if((currentweapon == switchweapon) || (switchweapon == null))
				{
					// Release switch lock
					weaponswitchlock = false;
					switchweapon = null;
				}

				// Update weapon display
				if(this == General.localclient) General.weapondisplay.UpdateSelection();

				// Show the switch?
				if(!silent)
				{
					// Make sound
					if(actor != null) DirectSound.PlaySound("weaponswitch.wav", actor.Position);

					// Show weapon name
					General.hud.ShowItemMessage(currentweapon.Description);
					General.weapondisplay.Show();
				}
			}
		}

		// This requests switching weapon
		public void RequestSwitchWeaponNext(bool forward)
		{
			int dir, idx, count = 0;

			// Must have an actor and a weapon
			if((actor == null) || (currentweapon == null)) return;

			// Determine switching direction
			if(forward) dir = 1; else dir = -1;

			// Start from current weapon if not switching yet
			if(switchweapon == null) switchweapon = currentweapon;

			// Determine start
			idx = (int)switchweapon.WeaponID;

			// Go for all weapons to find the next
			while(count <= (int)WEAPON.TOTAL_WEAPONS)
			{
				// Next weapon
				idx += dir;
				if(idx < 0) idx = (int)WEAPON.TOTAL_WEAPONS - 1;
				if(idx >= (int)WEAPON.TOTAL_WEAPONS) idx = 0;
				count++;

				// This weapon available?
				if(HasWeapon((WEAPON)idx))
				{
					// Not the same as previous weapon?
					if(idx != (int)switchweapon.WeaponID)
					{
						// Switch to here
						switchweapon = allweapons[idx];
						SendWeaponSwitch();
						weaponswitchlock = true;
					}

					// Done here
					break;
				}
			}

			// Update weapon display
			General.weapondisplay.UpdateSelection();

			// Show weapon name
			General.hud.ShowItemMessage(switchweapon.Description);
			General.weapondisplay.Show();
		}

		// This requests switching weapon
		public void RequestSwitchWeaponTo(WEAPON weaponid, bool check)
		{
			// Must have an actor and a weapon
			if((actor == null) || (currentweapon == null)) return;

			// Perform checks?
			if(check)
			{
				// This weapon available?
				if(HasWeapon(weaponid))
				{
					// Not the same as current weapon?
					if(weaponid != currentweapon.WeaponID)
					{
						// Switch to here
						switchweapon = allweapons[(int)weaponid];
						SendWeaponSwitch();
						weaponswitchlock = true;
					}

					// Update weapon display
					General.weapondisplay.UpdateSelection();

					// Show weapon name
					General.hud.ShowItemMessage(allweapons[(int)weaponid].Description);
					General.weapondisplay.Show();
				}
			}
			else
			{
				// Switch without question
				SendWeaponSwitchEx(weaponid);
				weaponswitchlock = true;
			}
		}

		// This sets the switch lock
		public void SetWeaponSwitchLock()
		{
			// Set the lock
			weaponswitchlock = true;
		}

		#endregion

		#region ================== Powerups

		// This fires the powerup
		public void FirePowerup()
		{
			// Only the nuke powerup can be fired
			if(powerup == POWERUP.NUKE)
			{
				// Check if not already being fired
				if(!powerupfired)
				{
					// Fire now!
					powercount = Consts.POWERUP_NUKE_FIRECOUNT;
					SendFirePowerup();
				}
			}
		}

		// This sets the powerup countdown
		public void SetPowerupCountdown(int count, bool fired)
		{
			// Nuke beign fired?
			if((powerup == POWERUP.NUKE) &&
			   (powerupfired == false) && (fired == true))
			{
                // Play the nuke countdown sound
                string snd = DirectSound.GetSound("countdownnuke.wav", false);
                var сachedSound = new CachedSound(snd);
                AudioPlaybackEngine.Instance.PlaySound(сachedSound);
                //DirectSound.PlaySound("countdownnuke.wav");
			}

			// Set the timeout
			powerupfired = fired;
			powercount = count;
			powerinterval = SharedGeneral.currenttime;
		}

		// This processes powerups, if any
		private void ProcessPowerup()
		{
			// No static powerup?
			if(powerup != POWERUP.STATIC)
			{
				// Actor still here?
				if(actor != null)
				{
					// Then remove lightnings
					for(int i = actor.Lightnings.Count - 1; i >= 0; i--)
					{
						// Get the lightning object
						Lightning l = (Lightning)actor.Lightnings[i];

						// Remove lightning if coming from me
						if(l.Source == this.Actor) l.Dispose();
					}
				}
			}

			// Update actor?
			if(actor != null)
			{
				// Update actor for powerup
				actor.ShowNuke = (powerup == POWERUP.NUKE);
				actor.ShowRage = (powerup == POWERUP.KILLER) || (powerup == POWERUP.AVENGER);

				// Killer powerup?
				if(powerup == POWERUP.KILLER) actor.RageColor = General.ARGB(1f, 1f, 0.2f, 0.1f);

				// Avenger powerup?
				if(powerup == POWERUP.AVENGER) actor.RageColor = General.ARGB(1f, 0.9f, 0.2f, 1f);

				// Ghost powerup?
				if(powerup == POWERUP.GHOST)
				{
					// Invisible
					actor.Alpha = 0.05f;
					if((General.localclient != this) && (actor.Name != "")) actor.Name = "";
				}
				else
				{
					// Normal
					actor.Alpha = 1f;
					if(actor.Name != formattedname) actor.Name = formattedname;
				}
			}

			// Leave when no powerup
			if(powerup == POWERUP.NONE) return;

			// Countdown
			powercount -= Consts.TIMESTEP;
			if(powercount < 0) powercount = 0;

			// Powerup process time?
			if(powerinterval <= SharedGeneral.currenttime)
			{
				// Determine what powerup we are carrying
				switch(powerup)
				{
					case POWERUP.STATIC: ProcessStaticPowerup(); break;
				}
			}
		}

		// This does stuff for the Static powerup
		public void ProcessStaticPowerup()
		{
			Vector3D dpos, cpos;
			bool haslightning;

			// Advance interval time
			powerinterval = SharedGeneral.currenttime + POWERUP_STATIC_RATE;
			if(powerintcount == 0) powerintcount = 1; else powerintcount = 0;

			// Must have an actor to continue
			if(actor == null) return;

			// Spawn a random shock?
			if(actor.Sector.VisualSector.InScreen && (General.random.Next(10) < 2))
			{
				// Determine shock coordinates
				cpos = actor.Position + new Vector3D(0f, 0f, 7f);
				dpos = actor.Position + Vector3D.Random(General.random, 12f, 12f, 0f);

				// Spawn shock around player
				//DirectSound.PlaySound("lightning_e.wav", actor.Position);

                string snd = DirectSound.GetSound("lightning_e.wav", false);
                var сachedSound = new CachedSound(snd);
                AudioPlaybackEngine.Instance.PlaySound(сachedSound);

                new Shock(cpos, dpos, -0.5f);
				new ShockLight(cpos, 100);
				new ShockLight(dpos, 100);
			}

			// Determine my position
			dpos = actor.State.pos + new Vector3D(0f, 0f, 6f);

			// Go for all playing clients
			foreach(Client c in General.clients)
			{
				// Client on this spot?
				if((c != null) && (c != this))
				{
					// Presume no lightning
					haslightning = false;

					// Client alive and not myself?
					if((!c.loading) && (!c.IsSpectator) && (c.Actor != null))
					{
						// No team game or on other team?
						if(!General.teamgame || (c.team != team))
						{
							// Determine client position
							cpos = c.Actor.State.pos + new Vector3D(0f, 0f, 6f);

							// Calculate distance to this player
							Vector3D delta = cpos - dpos;
							delta.z *= Consts.POWERUP_STATIC_Z_SCALE;
							float distance = delta.Length();
							delta.Normalize();

							// Within static range?
							if(distance < Consts.POWERUP_STATIC_RANGE)
							{
								// Check if nothing blocks in between clients
								if(!General.map.FindRayMapCollision(dpos, cpos))
								{
									// Check if no lighting to this client yet
									foreach(Lightning l in actor.Lightnings) if((l.Source == this.Actor) && (l.Target == c.Actor)) haslightning = true;

									// Create lighting
									if(!haslightning) new Lightning(this.Actor, 8f, c.Actor, 8f, true, true);
									haslightning = true;
								}
							}
						}
					}

					// Check if lightning should be found and removed
					if(!haslightning)
					{
						// Go for all lightnings
						foreach(Lightning l in actor.Lightnings)
						{
							// This lightning on this target?
							if(l.Target == c.Actor)
							{
								// Remove lightning
								l.Dispose();
								break;
							}
						}
					}
				}
			}
		}

		#endregion

		#region ================== Methods

		// This stops the actor from moving
		public void StopActor()
		{
			// Stop moving
			if(actor != null) actor.State.vel = new Vector3D(0f, 0f, actor.State.vel.z);
		}

		// This sets the player name
		public void SetName(string newname)
		{
			// Set the player name
			name = newname;

			// Set the formatted name
			if(Client.teamcolorednames)
			{
				// Determine color by team number
				switch(team)
				{
					case TEAM.NONE: formattedname = name; break;
					case TEAM.RED: formattedname = "^4" + General.StripColorCodes(name); break;
					case TEAM.BLUE: formattedname = "^1" + General.StripColorCodes(name); break;
				}
			}
			else
			{
				// With original colors
				formattedname = name;
			}

			// If an actor exists, apply the new name
			if(actor != null) actor.Name = formattedname;
		}

		// This kills the client and leaves the actor dead
		public void Kill(DEATHMETHOD method)
		{
			// Actor in the game?
			if(actor != null)
			{
				// In screen?
				if(actor.Sector.VisualSector.InScreen)
				{
					// Make sound
					int variations = DEATH_SOUND_VARS[(int)method-1];
					if(variations > 0)
					{
						int var = General.random.Next(variations);

                        string death_variant = "death" + (int)method + "var" + var + ".wav";
                        string snd = DirectSound.GetSound(death_variant, false);
                        var сachedSound = new CachedSound(snd);
                        AudioPlaybackEngine.Instance.PlaySound(сachedSound);


					}
				}

				// Spawn floor blood here
				if((method != DEATHMETHOD.QUIET) && (actor.HighestSector != null) && (actor.HighestSector.Material != (int)SECTORMATERIAL.LIQUID))
					FloorDecal.Spawn(actor.HighestSector, actor.Position.x, actor.Position.y, FloorDecal.blooddecals, false, true, false);

				// Kill and detach actor
				actor.Die(method);
				actor = null;

				// Remove weapons
				ClearWeapons();

				// Update scoreboard
				//General.scoreboard.Update();
			}
		}

		// This removes the client actor from game
		public void DestroyActor(bool silent)
		{
			int teamcolor;

			// Carrying something?
			if(carry != null)
			{
				// Drop if its a flag, otherwise detach normally
				if(carry is Flag) (carry as Flag).Drop(); else carry.Detach();
			}

			// Actor in the game?
			if(actor != null)
			{
				// Make sound and effect?
				if(!silent)
				{
                    // Play leave sound here
                    //DirectSound.PlaySound("playerleave.wav", actor.Position);
                    string snd = DirectSound.GetSound("playerleave.wav", false);
                    var сachedSound = new CachedSound(snd);
                    AudioPlaybackEngine.Instance.PlaySound(сachedSound);

                    // Determines team color
                    switch (team)
					{
						case TEAM.NONE: teamcolor = General.ARGB(1f, 0f, 0.6f, 0f); break;
						case TEAM.RED: teamcolor = General.ARGB(1f, 1f, 0f, 0f); break;
						case TEAM.BLUE: teamcolor = General.ARGB(1f, 0f, 0.6f, 1f); break;
						default: teamcolor = Color.White.ToArgb(); break;
					}

					// Add particles
					for(int i = 0; i < 20; i++)
						General.arena.p_dust.Add(actor.Position + Vector3D.Random(General.random, 2.5f, 2.5f, 2f) + new Vector3D(0f, 0f, 8f),
							Vector3D.Random(General.random, 0.02f, 0.02f, 0.1f) + actor.State.vel * 0.2f, teamcolor);
					for(int i = 0; i < 20; i++)
						General.arena.p_dust.Add(actor.Position + Vector3D.Random(General.random, 4f, 4f, 10f),
							Vector3D.Random(General.random, 0.02f, 0.02f, 0.1f) + actor.State.vel * 0.2f, Color.NavajoWhite.ToArgb());
					for(int i = 0; i < 10; i++)
						General.arena.p_dust.Add(actor.Position + Vector3D.Random(General.random, 3f, 3f, 4f),
							Vector3D.Random(General.random, 0.02f, 0.02f, 0.1f) + actor.State.vel * 0.2f, Color.LightGray.ToArgb());
					for(int i = 0; i < 10; i++)
						General.arena.p_dust.Add(actor.Position + Vector3D.Random(General.random, 1f, 1f, 2f) + new Vector3D(0f, 0f, 10f),
							Vector3D.Random(General.random, 0.02f, 0.02f, 0.1f) + actor.State.vel * 0.2f, General.ARGB(1f, 1.0f, 0.8f, 0.6f));
					for(int i = 0; i < 30; i++)
						General.arena.p_smoke.Add(actor.Position + Vector3D.Random(General.random, 3f, 3f, 0f),
							Vector3D.Random(General.random, 0.02f, 0.02f, 0.02f) + actor.State.vel * 0.1f, General.ARGB(1f, 0.6f, 0.6f, 0.6f));
				}

				// Remove the actor from game
				actor.Dispose(); actor = null;
				ClearWeapons();
			}

			// Clear moves when local client
			if(this.IsLocal) localmoves.Clear();

			// Not walking or shooting anymore
			prevwalkangle = -2f;
			prevshooting = false;
		}

		// This spawns the player in a new actor
		// NOTE: The id has already been taken from the message
		public void SpawnActor(NetMessage msg)
		{
			int teamcolor;

			// Read the details from message
			bool start = msg.GetBool();
			float x = msg.GetFloat();
			float y = msg.GetFloat();
			team = (TEAM)msg.GetByte();

			// Get the sector height at xy
			SubSector ssec = General.map.GetSubSectorAt(x, y);
			float z = ssec.Sector.HeightFloor;

			// Make actor position
			Vector3D actorpos = new Vector3D(x, y, z);

			// Clear moves when spawning local client
			if(this.IsLocal) localmoves.Clear();

			// When not local, create all weapons
			if(!this.IsLocal) GiveAllWeapons();

			// Pascal 11-01-2006: Fix for invisible players
			if(actor == null)
			{
				// Make an actor here
				actor = new Actor(actorpos, (int)team, id);
			}

			// Not walking or shooting
			prevwalkangle = -2f;
			prevshooting = false;

			// Set the actor name
			//this.SetName(name);
			actor.Name = formattedname;

			// Check if this is not
			// the initial setup
			if(!start)
			{
                // Play spawn sound here
                // For local client, always play at full volume
                // because the view screen may not be at this location yet
                string snd = DirectSound.GetSound("playerspawn.wav", false);
                var сachedSound = new CachedSound(snd);

                if (this.IsLocal)
                    AudioPlaybackEngine.Instance.PlaySound(сachedSound); //DirectSound.PlaySound("playerspawn.wav");
                else if(actor.Sector.VisualSector.InScreen)
                    AudioPlaybackEngine.Instance.PlaySound(сachedSound);//DirectSound.PlaySound("playerspawn.wav", actor.Position);

                // In screen or local?
                if (this.IsLocal || actor.Sector.VisualSector.InScreen)
				{
					// Determines team color
					switch(team)
					{
						case TEAM.NONE: teamcolor = General.ARGB(1f, 0.5f, 1f, 0.5f); break;
						case TEAM.RED: teamcolor = General.ARGB(1f, 1f, 0.4f, 0.4f); break;
						case TEAM.BLUE: teamcolor = General.ARGB(1f, 0.4f, 0.5f, 1f); break;
						default: teamcolor = Color.White.ToArgb(); break;
					}

					// Create spawn light
					new SpawnLight(actorpos, teamcolor);

					// Add particles
					for(int i = 0; i < 20; i++)
						General.arena.p_magic.Add(actorpos + Vector3D.Random(General.random, 2f, 2f, 2f),
							Vector3D.Random(General.random, 0.04f, 0.04f, 0.4f), teamcolor);
					for(int i = 0; i < 20; i++)
						General.arena.p_magic.Add(actorpos + Vector3D.Random(General.random, 2f, 2f, 2f),
							Vector3D.Random(General.random, 0.04f, 0.04f, 0.4f), -1);
					for(int i = 0; i < 10; i++)
						General.arena.p_smoke.Add(actor.Position + Vector3D.Random(General.random, 3f, 3f, 0f),
							Vector3D.Random(General.random, 0.02f, 0.02f, 0.1f), General.ARGB(1f, 0.6f, 0.6f, 0.6f));
				}

				// Update scoreboard
				General.scoreboard.Update();
			}
		}

		#endregion

		#region ================== Receiving

		// This teleports the client
		// NOTE: The id has already been taken from the message
		public void Teleport(NetMessage msg)
		{
			// Read data from message
			float oldx = msg.GetFloat();
			float oldy = msg.GetFloat();
			float oldz = msg.GetFloat();
			float newx = msg.GetFloat();
			float newy = msg.GetFloat();
			float newz = msg.GetFloat();

			// Make vectors
			Vector3D oldpos = new Vector3D(oldx, oldy, oldz);
			Vector3D newpos = new Vector3D(newx, newy, newz);

			// Spawn teleport effects
			new TeleportEffect(oldpos, this.team, false);
			new TeleportEffect(newpos, this.team, false);

			// Play teleport sound at both locations
			//DirectSound.PlaySound("teleport.wav", oldpos);
			//DirectSound.PlaySound("teleport.wav", newpos);

            string snd = DirectSound.GetSound("teleport.wav", false);
            var сachedSound = new CachedSound(snd);
            AudioPlaybackEngine.Instance.PlaySound(сachedSound);

            // Check if we have an actor
            if (actor != null)
			{
				// Remove lightning
				actor.RemoveAllLightnings();

				// Apply position
				actor.State.pos = newpos;

				// Reset velocity
				actor.State.vel = new Vector3D(0f, 0f, 0f);

				// Position the actor
				actor.Move(newpos);

				// Clear local moves
				if(localmoves != null) localmoves.Clear();
			}
		}

		// This acts upon damage given to this player
		// NOTE: The id has already been taken from the message
		public void TakeDamage(NetMessage msg)
		{
			// Read the details from message
			int damage = msg.GetInt();
			int hurtlevel = msg.GetByte();

			// Unless quiet hurting
			if(hurtlevel > 0)
			{
				// Randomize the hurt sound a little
				hurtlevel += General.random.Next(3) - 1;
				if(hurtlevel > 5) hurtlevel = 5;
				if((damage > 7) && (hurtlevel < 1)) hurtlevel = 1;

				// Can only play sound when actor exists
				if((this.actor != null) && (hurtlevel > 0))
				{
					// Should we play a hurt sound?
					if((hurtsound == null) || (hurtsound.Playing == false))
					{
						// Dispose old sound
						if(hurtsound != null) hurtsound.Dispose();

                        // Make hurt sound
                        //hurtsound = DirectSound.GetSound("hurt" + hurtlevel.ToString(CultureInfo.InvariantCulture) + ".wav", true);
                        //hurtsound.Position = this.actor.Position;
                        //hurtsound.Play();
                        string snd = DirectSound.GetSound("hurt" + hurtlevel.ToString(CultureInfo.InvariantCulture)+ ".wav", false);
                        var сachedSound = new CachedSound(snd);
                        AudioPlaybackEngine.Instance.PlaySound(сachedSound);
                    }
				}

				// Flash when local player is hurt
				if(this == General.localclient)
					General.hud.FlashScreen((float)damage / 20f);
			}
		}

		// This receives a ClientCorrection for the local client
		public void ClientCorrection(NetMessage msg)
		{
			LocalMove lm;
			Vector2D pushvec;
			int i = 0;

			// Check if we have an actor
			if(actor != null)
			{
				// Get information and move actor to start position
				int basetime = msg.GetInt();
				actor.State.pos.x = msg.GetFloat();
				actor.State.pos.y = msg.GetFloat();
				actor.State.pos.z = msg.GetFloat();
				actor.State.vel.x = msg.GetFloat();
				actor.State.vel.y = msg.GetFloat();
				actor.State.vel.z = msg.GetFloat();
				pushvec.x = msg.GetFloat();
				pushvec.y = msg.GetFloat();

				// Set start push vector
				actor.PushVector = pushvec;

				// Go for all local moves
				while(i < localmoves.Count)
				{
					// Get the move object
					lm = (LocalMove)localmoves[i];

					// Correct the move
					if(lm.CorrectMove(basetime))
					{
						// Apply the move to the actor
						lm.ApplyTo(actor);

						// Process the actor's physics
						actor.ProcessMovement();

						// Next move
						i++;
					}
					else
					{
						// Move is outdated, discard it
						localmoves.RemoveAt(i);
					}
				}
			}
		}

		// This gets a snapshot from message
		// NOTE: The id has already been taken from the message
		public void GetSnapshotFromMessage(NetMessage msg)
		{
			// Read data from message
			float px = msg.GetFloat();
			float py = msg.GetFloat();
			float vx = msg.GetFloat();
			float vy = msg.GetFloat();
			float aa = msg.GetFloat();
			float az = msg.GetFloat();
			POWERUP pu = (POWERUP)(int)msg.GetByte();
			byte shootweapon = msg.GetByte();

			// Check if we have an actor
			if(actor != null)
			{
				// Remote client?
				if(!this.IsLocal)
				{
					// Apply position
					actor.State.pos = new Vector3D(px, py, actor.State.pos.z);

					// Apply velocity
					actor.State.vel = new Vector3D(vx, vy, actor.State.vel.z);

					// Apply aim angle
					actor.AimAngle = aa;
					actor.AimAngleZ = az;

					// Shooting a weapon?
					if(shootweapon < 255)
					{
						// Pull the trigger
						if(allweapons[shootweapon] != null)
							allweapons[shootweapon].Trigger();
					}
					else
					{
						// Release weapons
						ReleaseAllWeapons();
					}

					// Advance state by timenudge
					actor.AdvanceByTimenudge();
				}

				// Apply powerup
				if(pu != powerup)
				{
					// Powerup changed
					powerup = pu;
					powerupfired = false;
				}
			}
			else
			{
				// Pascal 11-01-2006: Fixing invisible players
				// We need an actor here!
				SendNeedActor();
			}
		}

		// This updates client information from message
		// NOTE: The id has already been taken from the message
		public void UpdateFromMessage(NetMessage msg)
		{
			// Read the details from message
			lastping = msg.GetShort();
			lastloss = msg.GetByte();
			frags = msg.GetShort();
			deaths = msg.GetShort();
			score = msg.GetShort();
			loading = msg.GetBool();

			// Only for other players there is more information
			if(!this.IsLocal)
			{
				// Read more details from message
				SetName(msg.GetString());
				TEAM newteam = (TEAM)msg.GetByte();
				spectator = msg.GetBool();

				// When going spectating or changing team
				if(spectator || (newteam != team))
				{
					// Remove actor from game, if any
					DestroyActor(false);
					team = newteam;
					this.SetName(name);
				}
			}

			// Update scoreboard
			General.scoreboard.Update();
		}

		#endregion

		#region ================== Sending

		// This sends a ClientMove to the server
		private void SendClientMove(float moveangle, float aimangle, float aimanglez, bool shooting)
		{
			// Check if we have an actor
			if(actor != null)
			{
				// Send a ClientMove message
				NetMessage msg = General.conn.CreateMessage(MsgCmd.ClientMove, false);
				if(msg != null)
				{
					msg.AddData((int)SharedGeneral.currenttime);
					msg.AddData((float)moveangle);
					msg.AddData((float)aimangle);
					msg.AddData((float)aimanglez);
					msg.AddData((bool)shooting);
					msg.Send();
				}
			}
		}

		// Pascal 11-01-2006: Fixing invisible players
		// This sends a request for an actor
		private void SendNeedActor()
		{
			// Send a RespawnRequest message
			NetMessage msg = General.conn.CreateMessage(MsgCmd.NeedActor, false);
			if(msg != null)
			{
				// Send it
				msg.AddData((byte)id);
				msg.Send();
			}
		}

		// This sends a respawn request to the server
		private void SendRespawnRequest()
		{
			// Send a RespawnRequest message
			NetMessage msg = General.conn.CreateMessage(MsgCmd.RespawnRequest, true);
			if(msg != null)
			{
				// Send it
				msg.Send();
			}
		}

		// This sends message to fire powerup
		private void SendFirePowerup()
		{
			// Send a RespawnRequest message
			NetMessage msg = General.conn.CreateMessage(MsgCmd.FirePowerup, true);
			if(msg != null)
			{
				// Send it
				msg.Send();
			}
		}

		// This sends a SwitchWeapon request
		private void SendWeaponSwitch()
		{
			// Send a RespawnRequest message
			SendWeaponSwitchEx(switchweapon.WeaponID);
		}

		// This sends a SwitchWeapon request
		private void SendWeaponSwitchEx(WEAPON weaponid)
		{
			// Send a RespawnRequest message
			NetMessage msg = General.conn.CreateMessage(MsgCmd.SwitchWeapon, true);
			if(msg != null)
			{
				// Send it
				msg.AddData((byte)(int)weaponid);
				msg.Send();
			}
		}

		#endregion

		#region ================== Processing

		// This processes the client
		public void Process()
		{
			float walkangle = -2f;
			float aimangle = 0f;
			float aimanglez = 0f;
			LocalMove lm;

			// Presume not shooting
			shooting = false;

			// Check if this is the local client
			if(this.IsLocal)
			{
				// Are we playing in game?
				if(actor != null)
				{
					// Determine input
					walkangle = WalkingControl(actor.IsOnFloor);
					aimangle = AimingControl();
					aimanglez = AimingControlZ();
					shooting = ShootingControl();

					// Apply input to the actor
					actor.AimAngle = aimangle;
					actor.AimAngleZ = aimanglez;

					// Make a local move
					lm = new LocalMove(SharedGeneral.currenttime, walkangle);
					localmoves.Add(lm);

					// Apply move to actor
					lm.ApplyTo(actor);

					// When the walking angle changed, we must send
					// a ClientMove immediately. Otherwise, send a
					// ClientMove at 'idle' interval.
					if(((prevwalkangle != walkangle) ||
						(prevshooting != shooting) ||
						(clientmovetime < SharedGeneral.currenttime)) &&
						(General.conn != null))
					{
						// Send a ClientMove and update send time
						SendClientMove(walkangle, aimangle, aimanglez, shooting);
						clientmovetime += CLIENTMOVE_INTERVAL;
						prevwalkangle = walkangle;
						prevshooting = shooting;
					}
				}
				// Not playing but not spectating? (dead)
				else if(spectator == false)
				{
					// Send a spawn request
					RespawnControl();
				}
			}

			// Process powerups
			this.ProcessPowerup();
		}

		// This takes care of walking control and returns
		// the walking angle or a negative value when not walking
		private float WalkingControl(bool onfloor)
		{
			bool cd, cu, cl, cr;
			float angle = -2f;

			// Allowed to move?
			if(actor.TeleportLock < SharedGeneral.currenttime)
			{
				// Get the control key states
				cd = General.gamewindow.ControlPressed("walkdown");
				cu = General.gamewindow.ControlPressed("walkup");
				cl = General.gamewindow.ControlPressed("walkleft");
				cr = General.gamewindow.ControlPressed("walkright");

				// Anything pressed at all?
				if(cd || cu || cl || cr)
				{
					// Determine walk angle for pressed keys
					if(!cd && cu  && !cl && !cr) angle = 1.75f;
					else if(!cd &&  cu && !cl &&  cr) angle = 0.00f;
					else if(!cd && !cu && !cl &&  cr) angle = 0.25f;
					else if( cd && !cu && !cl &&  cr) angle = 0.50f;
					else if( cd && !cu && !cl && !cr) angle = 0.75f;
					else if( cd && !cu &&  cl && !cr) angle = 1.00f;
					else if(!cd && !cu &&  cl && !cr) angle = 1.25f;
					else if(!cd &&  cu &&  cl && !cr) angle = 1.50f;

					// Make correct walking angle
					if(angle > -1f) angle *= (float)Math.PI;

					// Actor-relative movement
					if(General.movemethod == 2)
						angle -= actor.AimAngle - (float)Math.PI * 0.75f;

					// Map-relative movement
					if(General.movemethod == 1)
						angle += (float)Math.PI * 0.25f;

					// Wrap around
					while(angle > (float)Math.PI * 2f) angle -= (float)Math.PI * 2f;
					while(angle < 0) angle += (float)Math.PI * 2f;
				}

				// Return walking angle
				return angle;
			}
			else
			{
				// Not allowed to move
				return -2f;
			}
		}

		// This takes care of aiming and returns
		// the current aim angle
		private float AimingControl()
		{
			// Calculate the aim angle
			// this disregards Z height
			float dx = General.arena.MouseAtMap.X - actor.State.pos.x;
			float dy = General.arena.MouseAtMap.Y - actor.State.pos.y;
			float angle = (float)Math.Atan2(dy, dx);

			// Return aim angle
			return angle;
		}

		// This takes care of aiming and returns
		// the current Z aim angle
		private float AimingControlZ()
		{
			// Calculate the aim angle over Z
			float dz = General.arena.MouseAtMap.Z - actor.State.pos.z;
			float dx = General.arena.MouseAtMap.X - actor.State.pos.x;
			float dy = General.arena.MouseAtMap.Y - actor.State.pos.y;
			Vector2D dxy = new Vector2D(dx, dy);
			float anglez = -(float)Math.Atan2(dxy.Length(), dz);

			// Return aim angle
			return anglez;
		}

		// This takes care of shooting input
		private bool ShootingControl()
		{
			Weapon ww;

			// Not allowed to shoot during finish
			if((General.gamestate == GAMESTATE.ROUNDFINISH) ||
			   (General.gamestate == GAMESTATE.GAMEFINISH)) return false;

			// Not allowed to shoot during countdown or spawning
			if((General.gamestate == GAMESTATE.COUNTDOWN) ||
			   (General.gamestate == GAMESTATE.SPAWNING)) return false;

			// Not allowed to shoot when no current weapon
			if(currentweapon == null) return false;

			// Determine if shooting input is given
			if(General.gamewindow.ControlPressed("fireweapon"))
			{
				// Check if the local client has ammo
				if(ammo[(int)currentweapon.AmmoType] >= currentweapon.UseAmmo)
				{
					// Check if not locked for switching
					if(weaponswitchlock == false)
					{
						// Make the local weapon fire
						currentweapon.Trigger();

						// Shooting
						return true;
					}
					else
					{
						// Release the trigger
						currentweapon.Released();

						// Locked for switching
						return false;
					}
				}
				else
				{
					// Release the trigger
					currentweapon.Released();

					// Find the next best weapon
					foreach(WEAPON w in General.bestweapons)
					{
						// Do we have this weapon?
						if(HasWeapon(w))
						{
							// Get a reference to this weapon
							ww = allweapons[(int)w];

							// Check if the weapon has enough ammo to fire
							if(ammo[(int)ww.AmmoType] >= ww.UseAmmo)
							{
								// Switch to this weapon
								RequestSwitchWeaponTo(w, false);
								break;
							}
						}
					}

					// No ammo
					return false;
				}
			}
			else
			{
				// Release the trigger
				currentweapon.Released();

				// No shooting
				return false;
			}
		}

		// This takes care of respawn input
		private void RespawnControl()
		{
			// Determine if respawn input is given
			if(General.gamewindow.ControlPressed("respawn"))
			{
				// Not already pressed?
				if(respawnpressed == false)
				{
					// Send a respawn request
					SendRespawnRequest();

					// Now pressed
					respawnpressed = true;
				}
			}
			else
			{
				// No longer pressed
				respawnpressed = false;
			}
		}

		#endregion
	}
}
