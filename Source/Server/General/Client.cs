/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

namespace CodeImp.Bloodmasters.Server
{
	public class Client : IPhysicsState
	{
		#region ================== Constants

		// Limitations
		public const int CLIENT_UPDATE_INTERVAL = 5000;
		public const int MAX_SNAPS = 50;
		public const int MIN_SNAPS = 20;
		public const int MAX_PLAYERS_PER_SNAPSHOT = 5;
		public const int CHAT_FLOOD_TIMEOUT = 5000;
		public const int CHAT_INSANE_TIMEOUT = 300;
		public const int CHAT_MAX_FLOODS = 6;

		// Death reasons
		public const string DEATH_SELFNUKE = "%1 ^7does a kamikaze attack";
		public const string DEATH_NUKE = "%1 ^7was nuked by %2";
		public const string DEATH_SELF = "%1 ^7committed suicide";
		public const string DEATH_SMG = "%1 ^7was shot by %2";
		public const string DEATH_MINIGUN = "%1 ^7was mowed down by %2";
		public const string DEATH_PLASMA = "%1 ^7was electrocuted by %2";
		public const string DEATH_STATIC = "%1 ^7was electrocuted by %2";
		public const string DEATH_EXPLODE = "%1 ^7was blown up by %2";
		public const string DEATH_TELEPORT = "%1 ^7was disintegrated by %2";
		public const string DEATH_FIRE = "%1 ^7was incinerated";
		public const string DEATH_FIRE_SOURCE = "%1 ^7was incinerated by %2";

		// Fire damage
		public const int FIRE_DAMAGE = 3;
		public const int FIRE_DAMAGE_INTERVAL = 400;

		// Spawn delays
		public const int RESPAWN_DELAY = 1000;
		public const int AUTO_RESPAWN_DELAY = 5000;
		public const int TELEPORT_DELAY = 500;

		// Rate at which 'idle' ClientCorrections are sent to the client
		public const int CLIENTCORRECT_INTERVAL = 50;

		#endregion

		#region ================== Variables

		// Connection
		private Connection conn;

		// Client properties
		private int id;
		private string address;
		private string name;
		private bool disposed = false;
		private bool loading = true;
		private bool spectator = true;
		private TEAM team = TEAM.NONE;
		private Item carry = null;
		private int fireintensity = 0;
		private int firedamagetime = 0;
		private int lastflametime = 0;
		private Server.Client firesource = null;
		private int autorespawntime;

		// Movement and aim
		private PhysicsState state;
		private Vector2D pushvector;
		private Sector sector;
		private Sector highestsector;
		private float aimangle;
		private float aimanglez;
		private float walkangle;
		private int respawnlock = 0;
		private int teleportlock = 0;
		private int lastjointime = 0;

		// Health and armor
		private int health;
		private int armor;

		// Weapons and ammo
		private int[] ammo = new int[(int)AMMO.TOTAL_AMMO_TYPES];
		private Weapon[] allweapons = new Weapon[(int)WEAPON.TOTAL_WEAPONS];
		private Weapon switchtoweapon = null;
		private Weapon currentweapon = null;
		private bool shooting = false;
		private bool autoswitchweapon = false;

		// Powerups
		private POWERUP powerup;
		private int powercount;
		private int powerinterval;
		private int lastshieldhittime;
		private int lastshieldhitclient = 255;
		private bool powerfired;

		// Player score
		private int frags;
		private int deaths;
		private int score;

		// Networking
		private int snapsinterval;
		private int snapsendtime;
		private int clientupdatetime;
		private int clienttime;
		private int correctiontime;
		private bool sendcorrection = false;

		// Chat flood protection
		private int chatfloodtime = 0;
		private int chatfloodlines = 0;

		// RCon password
		private string rconpassword = "";

		// Callvote
		private int callvotestate = 0;

		#endregion

		#region ================== Properties

		public int ID { get { return id; } }
		public string Name { get { return name; } }
		public bool Disposed { get { return disposed; } }
		public string Address { get { return address; } }
		public Connection Connection { get { return conn; } }
		public bool Loading { get { return loading; } set { loading = value; } }
		public bool Spectator { get { return spectator; } }
		public TEAM Team { get { return team; } }
		public Sector Sector { get { return sector; } }
		public Sector HighestSector { get { return highestsector; } }
		public PhysicsState State { get { return state; } }
		public bool SendCorrection { get { return sendcorrection; } set { sendcorrection = value; } }
		public int Health { get { return health; } }
		public int Armor { get { return armor; } }
		public float AimAngle { get { return aimangle; } }
		public float AimAngleZ { get { return aimanglez; } }
		public int Frags { get { return frags; } }
		public int Score { get { return score; } }
		public int Deaths { get { return deaths; } }
		public int LastJoinTime { get { return lastjointime; } }
		public int FireIntensity { get { return fireintensity; } }
		public int LastFlameTime { get { return lastflametime; } set { lastflametime = value; } }
		public bool IsAlive { get { return (health > 0) && !spectator && !loading && !disposed && (state != null); } }
		public POWERUP Powerup { get { return powerup; } }
		public Item Carrying { get { return carry; } set { carry = value; } }
		public int CallvoteState { get { return callvotestate; } set { callvotestate = value; } }
		public bool IsOnFloor
		{
			get
			{
				// State available?
				if((state != null) && (highestsector != null))
				{
					return (state.pos.z <= (highestsector.CurrentFloor + Consts.FLOOR_TOUCH_TOLERANCE));
				}
				else
				{
					// Should not matter
					return true;
				}
			}
		}

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public Client(Connection conn, string name, int id, int snaps, bool autoswitch)
		{
			// Keep references
			this.id = id;
			this.conn = conn;
			this.name = Markup.TrimColorCodes(name);
			this.address = conn.Address.ToString();
			this.loading = true;
			this.autoswitchweapon = autoswitch;

			// Limit the snaps
			if(snaps < Client.MIN_SNAPS) snaps = Client.MIN_SNAPS;
			if(snaps > Client.MAX_SNAPS) snaps = Client.MAX_SNAPS;

			// Calculate snapshots interval
			snapsinterval = (int)(1000f / (float)snaps);

			// Set next times
			snapsendtime = SharedGeneral.currenttime;
			clientupdatetime = SharedGeneral.currenttime + CLIENT_UPDATE_INTERVAL;

			// Trim the player name
			while(Markup.StripColorCodes(this.name).Length > Consts.MAX_PLAYER_NAME_LEN)
				this.name = this.name.Substring(0, this.name.Length - 1);

			// Broadcast player update to all clients
			Host.Instance.Server.BroadcastClientUpdate(this);

			// Show connected message to all clients
			string message = name + "^7 connected";
			foreach(Client c in Host.Instance.Server.clients) if(c != null) c.SendShowMessage(message, true);

			// Show in console
            if (Host.Instance.IsConsoleVisible)
            {
                Host.Instance.Server.WriteLine(message + " (" + address + ")", false);
            }

			// Add to server array
			Host.Instance.Server.AddClient(id, this);
		}

		// Dispose
		public void Dispose()
		{
			// Connection still open?
			if(conn != null)
			{
				// Stop sending pings and measurements
				// and dont try keeping this connection alive anymore
				conn.MeasurePings = false;
				conn.SetTimeout(600);
			}

			// Send ClientDisposed to everyone
			Host.Instance.Server.BroadcastClientDisposed(this);

			// Clean up
			firesource = null;
			if(carry != null) carry.Detach();
			carry = null;
			conn = null;
			disposed = true;

			// Remove from server array
			Host.Instance.Server.RemoveClient(id);

			// Callvote in progress?
			if(Host.Instance.Server.callvotetimeout > 0)
			{
				// Send callvote update if any is in progress
				Host.Instance.Server.CheckCallvote();
				if(Host.Instance.Server.callvotetimeout > 0) Host.Instance.Server.BroadcastCallvoteStatus();
			}
		}

		#endregion

		#region ================== Fire Effect

		// This adds fire intensity
		public void AddFireIntensity(int amount, Client source)
		{
			// State available?
			if(state != null)
			{
				// No damage time?
				if(firedamagetime == 0)
					firedamagetime = SharedGeneral.currenttime + FIRE_DAMAGE_INTERVAL;

				// Add to fire intensity
				fireintensity += amount;
				if(fireintensity > 30000) fireintensity = 30000;

				// Apply source
				//if(source != null) firesource = source;
				firesource = source;

				// Broadcast changes
				Host.Instance.Server.BroadcastFireIntensity(this);
			}
		}

		// This kills a fire
		public void KillFire()
		{
			// If fire is burning
			if(fireintensity > 0)
			{
				// No more fire
				fireintensity = 0;

				// Broadcast changes
				Host.Instance.Server.BroadcastFireIntensity(this);
			}
		}

		// This processes the fire effect
		private void ProcessFireEffect()
		{
			string msg = DEATH_FIRE;

			// Decrease intensity
			fireintensity -= Consts.TIMESTEP;
			if(fireintensity < 0) fireintensity = 0;
			if(fireintensity == 0) firesource = null;

			// On fire?
			if(fireintensity > 500)
			{
				// Time to damage player?
				if(firedamagetime < SharedGeneral.currenttime)
				{
					// Hurt the player
					if(firesource != null) msg = DEATH_FIRE_SOURCE;
					this.Hurt(firesource, msg, FIRE_DAMAGE, DEATHMETHOD.NORMAL_NOGIB, true);
					firedamagetime = SharedGeneral.currenttime + FIRE_DAMAGE_INTERVAL;
				}
			}
			else
			{
				// No damage time
				firedamagetime = 0;
			}
		}

		#endregion

		#region ================== Control

		// This respawns the client
		public void Respawn()
		{
			// Ignore when gamestate does not allow it
			if((Host.Instance.Server.GameState == GAMESTATE.GAMEFINISH) ||
				(Host.Instance.Server.GameState == GAMESTATE.ROUNDFINISH)) return;

			// In the game?
			if( ((team == TEAM.NONE) && (!Host.Instance.Server.IsTeamGame)) ||
				((team != TEAM.NONE) && (Host.Instance.Server.IsTeamGame)) )
			{
				// Not a spectator
				if(!spectator)
				{
					// Dead?
					if(state == null)
					{
						// Out of respawn lock?
						if(respawnlock < SharedGeneral.currenttime)
						{
							// Respawn!
							if(!Spawn(true)) SendShowMessage("Problem: no spawn spot found!", true);
						}
					}
				}
			}
		}

		// This stops the client movements
		public void Stop()
		{
			// Stop now
			walkangle = -2f;
			shooting = false;
		}

		// This removes the playing actor from the game
		// A respawn is then required to get back to playing
		public void RemoveActor()
		{
			// Cannot move or shoot without actor
			Stop();

			// No state, but dont change spectator state
			if(state != null) state.Dispose();
			state = null;
			pushvector = new Vector2D();
			sector = null;
			highestsector = null;
			walkangle = -2f;
			shooting = false;
			if(carry != null) carry.Detach();

			// No more fire
			KillFire();

			// Remove weapon/ammo
			ClearWeapons();
			ClearAmmo();

			// Remove powerup
			RemovePowerup();
		}

		// This hurts the player
		// also kills the player when health <= 0
		public void Hurt(Client source, string deathtext, int damage, DEATHMETHOD method, bool noshield)
		{
			Vector3D hitpos;

			// Shields?
			if(noshield == false)
			{
				// Make random coordinate around player
				hitpos = state.pos + Vector3D.Random(Host.Instance.Random, 10f, 10f, 0f);
			}
			else
			{
				// Shields do not help here
				hitpos = new Vector3D(float.NaN, float.NaN, float.NaN);
			}

			// Hurt player
			this.Hurt(source, deathtext, damage, method, hitpos);
		}

		// This hurts the player
		// also kills the player when health <= 0
		public void Hurt(Client source, string deathtext, int damage, DEATHMETHOD method, Vector3D hitpos)
		{
			int healthdamage;
			int armordamage;
			int sourceid = 255;
			Vector3D delta;
			float hitangle;
			float fadeout;

			// Determine source
			if(source != null)
			{
				// Keep source id
				sourceid = source.id;

				// Source carrying killer powerup?
				if(source.powerup == POWERUP.KILLER)
				{
					// Double the damage!
					damage = damage * 2;
				}
			}

			// Carrying shield powerup?
			if((powerup == POWERUP.SHIELDS) && !float.IsNaN(hitpos.x))
			{
				// Less damage
				damage = damage / 3;

				// Send a shield hit?
				if(((lastshieldhittime + Consts.POWERUP_SHIELD_INTERVAL) < SharedGeneral.currenttime) ||
				   (lastshieldhitclient != sourceid))
				{
					// Calculate hit angle
					delta = hitpos - state.pos;
					hitangle = (float)Math.Atan2(delta.y, delta.x);

					// Calculate fadeout
					fadeout = (float)(50 - damage) * 0.002f;
					if(fadeout < 0.001f) fadeout = 0.001f;

					// Broadcast shield hit
					Host.Instance.Server.BroadcastShieldHit(this, hitangle, fadeout);

					// Save last hit settings
					lastshieldhittime = SharedGeneral.currenttime;
					lastshieldhitclient = sourceid;
				}
			}

			// When this is NOT a teammate
			if((source == null) || (source == this) ||
			   (source.team != this.team) || !Host.Instance.Server.IsTeamGame)
			{
				// Carrying avenger powerup and not shooting myself?
				if((powerup == POWERUP.AVENGER) && (source != null) && (source != this) && source.IsAlive)
				{
					// Inflict half the damage on the source as well
					source.Hurt(source, deathtext, damage / 2, method, hitpos);
				}

				// Calculate damages
				armordamage = damage / 2;
				if(armordamage > armor) armordamage = armor;
				healthdamage = damage - (int)((float)armordamage * 1.5f);
				if(healthdamage < 0) healthdamage = 0;

				// Taking any damage?
				if((armordamage > 0) || (healthdamage > 0))
				{
					// Decrease health
					armor -= armordamage;
					health -= healthdamage;
					if((health <= Consts.GIB_THRESHOLD) && (method == DEATHMETHOD.NORMAL)) method = DEATHMETHOD.GIBBED;
					if(armor <= 0) armor = 0;
					if(health <= 0) health = 0;
					if(method == DEATHMETHOD.NORMAL_NOGIB) method = DEATHMETHOD.NORMAL;

					// Send messages
					SendFullStatusUpdate();
					if(health > 0) Host.Instance.Server.BroadcastTakeDamage(this, damage, health, method);
					if((source != null) && (source != this)) source.SendDamageGiven();

					// Kill player when health is 0
					if(health == 0) Die(source, deathtext, method);
				}
			}
		}

		// This kills the player
		public void Die(Client source, string deathtext, DEATHMETHOD method)
		{
			// Source the same as target?
			if((source == this) || (source == null))
			{
				// Killed myself
				if(source == this) deathtext = DEATH_SELF.Replace("%1", this.name);
				else deathtext = deathtext.Replace("%1", this.name);

				// Adjust score
				if((Host.Instance.Server.GameType == GAMETYPE.DM) ||
				   (Host.Instance.Server.GameType == GAMETYPE.TDM)) this.AddToScore(-1);
			}
			else
			{
				// Make proper death text
				deathtext = deathtext.Replace("%1", this.name);
				if(source != null) deathtext = deathtext.Replace("%2", source.name);

				// Not on the same team?
				if((source != null) && ((source.team != team) || !Host.Instance.Server.IsTeamGame))
				{
					// Count frag
					source.frags++;

					// Adjust score
					if((Host.Instance.Server.GameType == GAMETYPE.DM) ||
					   (Host.Instance.Server.GameType == GAMETYPE.TDM)) source.AddToScore(1);
				}
			}

			// Count the death
			this.deaths++;

			// Scavenger mode: lose 10 points!
			if((Host.Instance.Server.GameType == GAMETYPE.SC) ||
			   (Host.Instance.Server.GameType == GAMETYPE.TSC)) AddToScore(-10);

			// Delay the respawn time
			respawnlock = SharedGeneral.currenttime + RESPAWN_DELAY;

			// Set auto-respawn  time
			autorespawntime = SharedGeneral.currenttime + AUTO_RESPAWN_DELAY;

			// Show death message in console
			Host.Instance.Server.WriteLine(deathtext, false);

			// Broadcast the death
			Host.Instance.Server.BroadcastClientDeath(source, this, deathtext, method, state, pushvector);

			// Remove the actor from the game
			RemoveActor();
		}

		// This adjustes score for the player
		public void AddToScore(int amount)
		{
			// Add to score
			score += amount;

			// In team game this counts as team score as well
			if(Host.Instance.Server.IsTeamGame)
				Host.Instance.Server.teamscore[(int)team] += amount;
		}

		// This changes status
		public void AddToStatus(int addhealth, int healthlimit, int addarmor, int armorlimit)
		{
			// Add to health
			if((addhealth > 0) && (health < healthlimit))
			{
				health += addhealth;
				if(health > healthlimit) health = healthlimit;
			}

			// Add to armor
			if((addarmor > 0) && (armor < armorlimit))
			{
				armor += addarmor;
				if(armor > armorlimit) armor = armorlimit;
			}

			// Send update to client
			SendFullStatusUpdate();
		}

		// This makes the player spectate
		// This does NOT change the team
		public void Spectate()
		{
			// No state
			if(state != null) state.Dispose();
			state = null;
			pushvector = new Vector2D();
			spectator = true;
			sector = null;
			highestsector = null;
			walkangle = -2f;
			shooting = false;
			if(carry != null) carry.Detach();

			// Reset scores
			ResetScores();

			// No more fire
			KillFire();

			// Remove weapon/ammo
			ClearWeapons();
			ClearAmmo();

			// Remove powerup
			RemovePowerup();
		}

		// This spawns the player at the furthest spawn point
		// Returns True on success, False when no spawn points available
		public bool Spawn(bool bruteforce)
		{
			Thing ft = null;
			float ftdist = -1f;
			float tdist;

			// Go for all things on the map
			foreach(Thing t in Host.Instance.Server.map.Things)
			{
				// Is this a spawn point for my team?
				if(((Host.Instance.Server.GameType != GAMETYPE.CTF) && (t.Type == (int)THINGTYPE.PLAYER_DM)) ||
				   ((Host.Instance.Server.GameType == GAMETYPE.CTF) && (team == TEAM.RED) && (t.Type == (int)THINGTYPE.PLAYER_RED)) ||
				   ((Host.Instance.Server.GameType == GAMETYPE.CTF) && (team == TEAM.BLUE) && (t.Type == (int)THINGTYPE.PLAYER_BLUE)))
				{
					// Initialize distance calculation
					// with a little random value to get a random
					// spawn spot when no other clients are around
					tdist = (float)Host.Instance.Random.NextDouble();

					// Go for all other clients
					foreach(Client c in Host.Instance.Server.clients)
					{
						// Client playing and not the spawning client?
						if((c != null) && (c != this) && !c.Spectator && !c.Loading && c.IsAlive)
						{
							// Check if this is an opponent
							//if(!Host.Instance.Server.IsTeamGame || (c.team != team))
							{
								// Calculate distance between client and spot
								float dx = t.X - c.state.pos.x;
								float dy = t.Y - c.state.pos.y;
								float spotdist = (float)Math.Sqrt(dx * dx + dy * dy);

								// Spot occupied?
								if(spotdist <= Consts.PLAYER_DIAMETER * 2f)
								{
									// This spot cannot be used right now
									tdist = -2f;
									break;
								}
								else
								{
									// Add the distance
									tdist += spotdist;
								}
							}
						}
					}

					// Longer than previous find?
					if(tdist > ftdist)
					{
						// Keep this spot
						ftdist = tdist;
						ft = t;
					}
				}
			}

			// No spawn spot found?
			if((ft == null) && bruteforce)
			{
				// Then try again, but this time just spawn on top of someone if needed

				// Go for all things on the map
				foreach(Thing t in Host.Instance.Server.map.Things)
				{
					// Is this a spawn point for my team?
					if(((Host.Instance.Server.GameType != GAMETYPE.CTF) && (t.Type == (int)THINGTYPE.PLAYER_DM)) ||
						((Host.Instance.Server.GameType == GAMETYPE.CTF) && (team == TEAM.RED) && (t.Type == (int)THINGTYPE.PLAYER_RED)) ||
						((Host.Instance.Server.GameType == GAMETYPE.CTF) && (team == TEAM.BLUE) && (t.Type == (int)THINGTYPE.PLAYER_BLUE)))
					{
						// Initialize distance calculation
						// with a little random value to get a random
						// spawn spot when no other clients are around
						tdist = (float)Host.Instance.Random.NextDouble();

						// Longer than previous find?
						if(tdist > ftdist)
						{
							// Keep this spot
							ftdist = tdist;
							ft = t;
						}
					}
				}
			}

			// When spot found
			if(ft != null)
			{
				// Reset game elements
				ResetPlayerStatus();

				// Determine sector where player will be at
				sector = Host.Instance.Server.map.GetSubSectorAt(ft.X, ft.Y).Sector;

				// Spawn the player here
				if(state != null) state.Dispose();
				state = new ServerPhysicsState(Host.Instance.Server.map);
				state.Radius = Consts.PLAYER_RADIUS;
				state.Height = Consts.PLAYER_BLOCK_HEIGHT;
				state.Friction = Consts.PLAYER_FRICTION;
				state.StepUp = true;
				state.Redirect = true;
				state.Bounce = false;
				state.pos.x = ft.X;
				state.pos.y = ft.Y;
				state.pos.z = sector.CurrentFloor;

				// Initialize variables
				walkangle = -2f;
				correctiontime = SharedGeneral.currenttime + CLIENTCORRECT_INTERVAL;
				fireintensity = 0;

				// Go for all other clients
				foreach(Client c in Host.Instance.Server.clients)
				{
					// Client playing and not myself?
					if((c != null) && (c != this) && !c.Spectator && !c.Loading && c.IsAlive)
					{
						// Calculate distance between client and spot
						float dx = ft.X - c.state.pos.x;
						float dy = ft.Y - c.state.pos.y;
						float spotdist = dx * dx + dy * dy;

						// Kill the player if on the teleport
						if(spotdist <= Consts.PLAYER_DIAMETER_SQ)
							c.Die(this, DEATH_TELEPORT, DEATHMETHOD.GIBBED);
					}
				}

				// Broadcast the spawn
				Host.Instance.Server.BroadcastSpawnActor(this, false);

				// Spawned
				return true;
			}
			else
			{
				// Not spawned
				return false;
			}
		}

		// This let the player say a message to everyone
		public void Say(string msg)
		{
			// Make the complete string
			string fullmsg = name + "^7:  ^2" + Markup.StripColorCodes(msg);

			// Broadcast message
			Host.Instance.Server.BroadcastSayMessage(this, fullmsg);
		}

		// This let the player say a message to his team
		public void SayTeam(string msg)
		{
			// Not a team game? then normal say
			if(!Host.Instance.Server.IsTeamGame)
			{
				// Normal say
				Say(msg);
			}
			else
			{
				// Make the complete string
				string fullmsg = name + "^7 TEAM:  ^2" + msg;

				// Spectator?
				if(spectator)
				{
					// Broadcast message
					Host.Instance.Server.BroadcastSayMessageSpectators(this, fullmsg);
				}
				// Red team?
				else if(team == TEAM.RED)
				{
					// Broadcast message
					Host.Instance.Server.BroadcastSayMessageTeam(this, fullmsg, TEAM.RED);
				}
				// Blue team?
				else if(team == TEAM.BLUE)
				{
					// Broadcast message
					Host.Instance.Server.BroadcastSayMessageTeam(this, fullmsg, TEAM.BLUE);
				}
			}
		}

		// This pushes the client
		public void Push(Vector2D force)
		{
			// State available?
			if(state != null)
			{
				// Apply the force
				pushvector += force;
				state.vel += (Vector3D)force;
				SendClientCorrection();
				sendcorrection = true;
			}
		}

		// This teleports the player to a tagged thing
		// Returns false when no spawn spots found
		public bool TeleportToThing(int tag, Vector3D oldpos)
		{
			List<Thing> dests = new List<Thing>(10);

			// Go for all things on the map
			foreach(Thing t in Host.Instance.Server.map.Things)
			{
				// Is this a spawn point with correct tag?
				if((t.Type == (int)THINGTYPE.TELEPORT) && (t.Tag == tag))
				{
					// Add to the list of destinations
					dests.Add(t);
				}
			}

			// No spawn spot found?
			if(dests.Count == 0)
			{
				// No spots available
				return false;
			}
			else
			{
				// Choose a random destination
				Thing ft = dests[Host.Instance.Random.Next(dests.Count)];

				// Go for all other clients
				foreach(Client c in Host.Instance.Server.clients)
				{
					// Client playing and not the teleporting client?
					if((c != null) && (c != this) && !c.Spectator && !c.Loading && c.IsAlive)
					{
						// Calculate distance between client and spot
						float dx = ft.X - c.state.pos.x;
						float dy = ft.Y - c.state.pos.y;
						float spotdist = dx * dx + dy * dy;

						// Kill the player if on the teleport
						if(spotdist <= Consts.PLAYER_DIAMETER_SQ)
							c.Hurt(this, DEATH_TELEPORT, 1000, DEATHMETHOD.NORMAL, true);
					}
				}

				// Determine sector where player will be at
				sector = Host.Instance.Server.map.GetSubSectorAt(ft.X, ft.Y).Sector;

				// Move the player here
				state.pos = new Vector3D(ft.X, ft.Y, sector.CurrentFloor);
				state.vel = new Vector3D(0f, 0f, 0f);
				walkangle = -2f;
				sendcorrection = true;

				// Now teleporting
				teleportlock = SharedGeneral.currenttime + TELEPORT_DELAY;

				// Broadcast the teleportation
				Host.Instance.Server.BroadcastTeleportClient(this, oldpos, state.pos);

				// Spawned
				return true;
			}
		}

		#endregion

		#region ================== RCon Commands

		// Handle login command
		private void cLogin(string args)
		{
			// Arguments given?
			if(args.Trim().Length == 0)
			{
				// Show info
				SendShowMessage("Usage:  /rcon login ^0password^7", false);
				return;
			}

			// Apply password
			rconpassword = args;

			// Check password
			if((rconpassword != Host.Instance.Server.RConPassword) ||
				(Host.Instance.Server.RConPassword == ""))
			{
				// Incorrect password or rcon disabled
				SendShowMessage("Not authorized for remote control", false);
			}
			else
			{
				// Password correct
				SendShowMessage("Remote control enabled", false);
			}
		}

		#endregion

		#region ================== Receiving

		// This votes for the callvote
		public void hCallvoteSubmit(NetMessage msg)
		{
			// Callvote in progress?
			if(Host.Instance.Server.callvotetimeout > 0)
			{
				// Not already voted?
				if(callvotestate == 0)
				{
					// Set the vote status to 1
					callvotestate = 1;
					Host.Instance.Server.callvotes++;
					Host.Instance.Server.BroadcastCallvoteStatus();

					// Re-check the state of the call
					Host.Instance.Server.CheckCallvote();
				}
			}
		}

		// This changes the player name
		public void hPlayerNameChange(NetMessage msg)
		{
			string playernameerror;
			string newname;

			// Get the new name
			newname = Markup.TrimColorCodes(msg.GetString());

			// Check the player name
			playernameerror = GameServer.ValidatePlayerName(newname);
			if(playernameerror == null)
			{
				// Apply name
				this.name = newname;

				// Trim the player name
				while(Markup.StripColorCodes(this.name).Length > Consts.MAX_PLAYER_NAME_LEN)
					this.name = this.name.Substring(0, this.name.Length - 1);

				// Let everyone know
				Host.Instance.Server.BroadcastPlayerNameChange(this);
			}
			else
			{
				// Invalid player name
				SendShowMessage(playernameerror, false);
			}
		}

		// Switch weapon request
		public void hSwitchWeapon(NetMessage msg)
		{
			// Get the weapon id
			WEAPON wid = (WEAPON)(int)msg.GetByte();

			// Weapon available?
			if(HasWeapon(wid))
			{
				// Switch to this weapon
				switchtoweapon = allweapons[(int)wid];
			}
		}

		// Suicide request
		public void hSuicide(NetMessage msg)
		{
			// Kill when alive
			if(!this.Loading && !this.Spectator && this.IsAlive)
				this.Hurt(null, DEATH_SELF, 1000, DEATHMETHOD.NORMAL_NOGIB, true);
		}

		// Fire Powerup request
		public void hFirePowerup(NetMessage msg)
		{
			// Alive?
			if(!this.Loading && !this.Spectator && this.IsAlive)
			{
				// Do we have the Nuke powerup?
				if(powerup == POWERUP.NUKE)
				{
					// Not already fired?
					if(!powerfired)
					{
						// Fire now!
						powerfired = true;
						powercount = Consts.POWERUP_NUKE_FIRECOUNT;
						SendPowerupCountUpdate();
					}
				}
			}
		}

		// The client wishes to respawn
		public void hRespawnRequest(NetMessage msg)
		{
			// Respawn if possible
			this.Respawn();
		}

		// The client wishes to make this move
		public void hClientMove(NetMessage msg)
		{
			// Get the move information
			clienttime = msg.GetInt(); //- Consts.TIMESTEP;
			float new_walkangle = msg.GetFloat();
			float new_aimangle = msg.GetFloat();
			float new_aimanglez = msg.GetFloat();
			bool new_shooting = msg.GetBool();

			// Not at game or round finish
			if((Host.Instance.Server.GameState != GAMESTATE.ROUNDFINISH) &&
			   (Host.Instance.Server.GameState != GAMESTATE.GAMEFINISH))
			{
				// Allowed to aim
				aimangle = new_aimangle;
				aimanglez = new_aimanglez;

				// Not during teleport lock
				if(teleportlock < SharedGeneral.currenttime)
				{
					// Set the walk angles
					walkangle = new_walkangle;
				}

				// Not at countdown or spawning
				if((Host.Instance.Server.GameState != GAMESTATE.COUNTDOWN) &&
				   (Host.Instance.Server.GameState != GAMESTATE.SPAWNING))
				{
					// Check if enough ammo to fire
					if(new_shooting && (currentweapon != null))
						shooting = currentweapon.CanShoot();
					else
						shooting = false;
				}
			}
			else
			{
				// No walking or shooting allowed
				walkangle = -2f;
				shooting = false;
			}
		}

		// Change team/spectator
		public void hChangeTeam(NetMessage msg)
		{
			string desc;

			// Get arguments
			clienttime = msg.GetInt();
			TEAM t = (TEAM)msg.GetInt();
			bool s = msg.GetBool();

			// Ignore request when still in respawn lock
			if(respawnlock > SharedGeneral.currenttime) return;

			// Only allow blue or red when no automatic team is forced
			if(Host.Instance.Server.JoinSmallest && ((t == TEAM.RED) || (t == TEAM.BLUE))) return;

			// When no team given and this IS a team game...
			if((t == TEAM.NONE) && Host.Instance.Server.IsTeamGame && this.spectator)
			{
				// Find the smallest team
				int redplayers = Host.Instance.Server.CountPlayingClients(TEAM.RED);
				int blueplayers = Host.Instance.Server.CountPlayingClients(TEAM.BLUE);

				// When there is no smallest team...
				if(redplayers == blueplayers)
				{
					// When score is even...
					if(Host.Instance.Server.teamscore[1] == Host.Instance.Server.teamscore[2])
					{
						// Join random team
						t = (TEAM)(Host.Instance.Random.Next(2) + 1);
					}
					// Else join the team with lowest score
					else if(Host.Instance.Server.teamscore[1] > Host.Instance.Server.teamscore[2])
					{
						// Join blue
						t = TEAM.BLUE;
					}
					else
					{
						// Join red
						t = TEAM.RED;
					}
				}
				// Else join the smallest team
				else if(redplayers > blueplayers)
				{
					// Join blue
					t = TEAM.BLUE;
				}
				else
				{
					// Join red
					t = TEAM.RED;
				}
			}

			// Delay the respawn time
			respawnlock = SharedGeneral.currenttime + RESPAWN_DELAY;

			// Check if actually changing
			if((t != team) || (s != spectator))
			{
				if(s)
				{
					// Join spectators
					team = TEAM.NONE;
					spectator = true;
					Host.Instance.Server.BroadcastShowMessage(name + "^7 joins the spectators", true, true);

					// Broadcast player update to all clients and confirm team change
					SendChangeTeam();
					Host.Instance.Server.BroadcastClientUpdate(this);
					Spectate();
				}
				else if((t == TEAM.NONE) && (!Host.Instance.Server.IsTeamGame))
				{
					// Previously spectator?
					if(spectator)
					{
						// Check for max players
						if((Host.Instance.Server.MaxPlayers > 0) &&
						   (Host.Instance.Server.MaxPlayers <= Host.Instance.Server.CountPlayingClients()))
						{
							// Max players hit, sorry
							SendShowMessage("You cannot join the game, maximum number of players on this server is " + Host.Instance.Server.MaxPlayers, true);
							return;
						}
					}

					// Join game
					team = TEAM.NONE;
					spectator = Host.Instance.Server.JoinTeamSpectating;
					Host.Instance.Server.BroadcastShowMessage(name + "^7 joins the game", true, true);

					// Broadcast player update to all clients
					if(carry != null) carry.Detach();
					SendChangeTeam();
					Host.Instance.Server.BroadcastClientUpdate(this);
					if(!spectator) if(!Spawn(true)) SendShowMessage("Problem: no spawn spot found!", true);
					lastjointime = SharedGeneral.currenttime;
				}
				else if((t != TEAM.NONE) && (Host.Instance.Server.IsTeamGame))
				{
					// Previously spectator?
					if(spectator)
					{
						// Check for max players
						if((Host.Instance.Server.MaxPlayers > 0) &&
						   (Host.Instance.Server.MaxPlayers <= Host.Instance.Server.CountPlayingClients()))
						{
							// Max players hit, sorry
							SendShowMessage("You cannot join the game, maximum number of players on this server is " + Host.Instance.Server.MaxPlayers, true);
							return;
						}
					}

					// Join team
					team = t;
					spectator = Host.Instance.Server.JoinTeamSpectating;

					// Determine description
					if(team == TEAM.RED) desc = "red team"; else desc = "blue team";
					Host.Instance.Server.BroadcastShowMessage(name + "^7 joins the " + desc, true, true);

					// Broadcast player update to all clients
					if(carry != null) carry.Detach();
					SendChangeTeam();
					Host.Instance.Server.BroadcastClientUpdate(this);
					if(!spectator) if(!Spawn(true)) SendShowMessage("Problem: no spawn spot found!", true);
					lastjointime = SharedGeneral.currenttime;
				}
			}
		}

		// Pascal 11-01-2006: Fixing invisible players
		// Handle NeedActor
		public void hNeedActor(NetMessage msg)
		{
			// Get the client id
			int cid = msg.GetByte();

			// id specified?
			if((cid > 0) && (cid < Host.Instance.Server.clients.Length))
			{
				// Get the client reference
				Client c = Host.Instance.Server.clients[cid];

				// Check if slot is used and playing
				if((c != null) && !c.Loading && !c.Spectator && c.IsAlive)
				{
					// Send spawn actor
					this.SendSpawnActor(c, true);
				}
			}
		}

		// Handle GameStarted
		public void hGameStarted(NetMessage msg)
		{
			// Done loading the map
			loading = false;

			// Reset connection timeout to normal
			conn.SetTimeout(Connection.DEFAULT_TIMEOUT);

			// Send all current clients
			SendAllCurrentClients();

			// Send spawn actors for all players in the game
			foreach(Client c in Host.Instance.Server.clients)
			{
				// Check if slot is used and playing
				if((c != null) && !c.Loading && !c.Spectator && c.IsAlive)
				{
					// Send spawn actor
					this.SendSpawnActor(c, true);
				}
			}

			// Send all items picked up
			SendAllItemPickups();

			// Send sector movements
			SendAllSectorMovements();

			// Send the current gamestate
			SendGameStateChange();

			// Send a snapshot
			SendGameSnapshot();

			// Send callvote if any is in progress
			if(Host.Instance.Server.callvotetimeout > 0) SendCallvoteStatus();

			// Broadcast player update to all clients
			Host.Instance.Server.BroadcastClientUpdate(this);
		}

		// Handle Disconnect
		public void hDisconnect(NetMessage msg)
		{
			// Client disconnects
			Disconnect();
		}

		// Handle SayMessage
		public void hSayMessage(NetMessage msg)
		{
			// Let the client say this
			if(ChatFloodContinue()) Say(msg.GetString());
		}

		// Handle SayTeamMessage
		public void hSayTeamMessage(NetMessage msg)
		{
			// Let the client say this to his team only
			if(ChatFloodContinue()) SayTeam(msg.GetString());
		}

		// Handle Command
		public void hCommand(NetMessage msg)
		{
			string cmd, args;
			string cmdline = msg.GetString();

			// Get the command
			int firstspace = cmdline.IndexOf(" ");
			if(firstspace == -1) firstspace = cmdline.Length;
			cmd = cmdline.Substring(0, firstspace).ToLower();
			cmd = Markup.StripColorCodes(cmd);

			// Arguments?
			if(firstspace + 1 > cmdline.Length)
			{
				// No arguments
				args = "";
			}
			else
			{
				// Get the arguments
				args = cmdline.Substring(firstspace + 1);
			}

			// Check if rcon password is correct
			if((rconpassword != Host.Instance.Server.RConPassword) ||
			   (Host.Instance.Server.RConPassword == ""))
			{
				// Not authorized!
				// Only allow login command

				// Handle command
				switch(cmd)
				{
					case "login": cLogin(args); break;
					default: SendShowMessage("Not authorized for remote control", false); break;
				}
			}
			else
			{
				// Handle command
				Host.Instance.Server.PerformCommand(this, cmd, args);
			}
		}

		// Handle Callvote Request
		public void hCallvoteRequest(NetMessage msg)
		{
			string cmd, args;
			string cmdline = msg.GetString();

			// No callvote in progress?
			if(Host.Instance.Server.callvotetimeout == 0)
			{
				// Get the command
				int firstspace = cmdline.IndexOf(" ");
				if(firstspace == -1) firstspace = cmdline.Length;
				cmd = cmdline.Substring(0, firstspace).ToLower();
				cmd = Markup.StripColorCodes(cmd);

				// Arguments?
				if(firstspace + 1 > cmdline.Length)
				{
					// No arguments
					args = "";
				}
				else
				{
					// Get the arguments
					args = cmdline.Substring(firstspace + 1);
				}

				// Check what callvote command is given
				switch(cmd)
				{
					// Map change
					case "map":
						// Check if this map exists
						if(ArchiveManager.FindFileArchive(args.Trim() + ".wad") != "")
						{
							// Start callvote
							Host.Instance.Server.StartCallvote(this, "map", args.Trim(), "map change to " + args.Trim());
						}
						else
						{
							// No such map on the server
							SendShowMessage("This server does not have a map named \"" + args.Trim() + "\".", true);
						}
						break;

					// Map restart
					case "restartmap": Host.Instance.Server.StartCallvote(this, "restartmap", "", "map restart"); break;

					// Next map
					case "nextmap": Host.Instance.Server.StartCallvote(this, "nextmap", "", "next map"); break;
				}
			}
		}

		#endregion

		#region ================== Sending

		// This sends a callvote update
		public void SendCallvoteStatus()
		{
			// Dont send in these cases
			if((conn == null) || conn.Disposed) return;

			// Make the status update message and send it
			NetMessage msg = conn.CreateMessage(MsgCmd.CallvoteStatus, true);
			if(msg != null)
			{
				msg.AddData((string)Host.Instance.Server.callvotedesc);
				msg.AddData((int)Host.Instance.Server.callvotes);
				msg.AddData((int)(Host.Instance.Server.callvotetimeout - Host.Instance.RealTime));
				msg.Send();
			}
		}

		// This sends a callvote end
		public void SendCallvoteEnd()
		{
			// Dont send in these cases
			if((conn == null) || conn.Disposed) return;

			// Make the status update message and send it
			NetMessage msg = conn.CreateMessage(MsgCmd.CallvoteEnd, true);
			if(msg != null)
			{
				msg.Send();
			}
		}

		// This sends an update for the powerup countdown
		public void SendPowerupCountUpdate()
		{
			// Dont send in these cases
			if((conn == null) || conn.Disposed) return;

			// Make the status update message and send it
			NetMessage msg = conn.CreateMessage(MsgCmd.PowerupCountUpdate, true);
			if(msg != null)
			{
				msg.AddData((int)clienttime);
				msg.AddData((int)powercount);
				msg.AddData((bool)powerfired);
				msg.Send();
			}
		}

		// This sends a player name change
		public void SendPlayerNameChange(Client c)
		{
			// Dont send in these cases
			if((conn == null) || conn.Disposed) return;

			// Make the message and send it
			NetMessage msg = conn.CreateMessage(MsgCmd.PlayerNameChange, true);
			if(msg != null)
			{
				msg.AddData((int)c.id);
				msg.AddData((string)c.name);
				msg.Send();
			}
		}

		// This sends a shield hit
		public void SendDamageGiven()
		{
			// Dont send in these cases
			if((conn == null) || conn.Disposed) return;

			// Make the status update message and send it
			NetMessage msg = conn.CreateMessage(MsgCmd.DamageGiven, false);
			if(msg != null)
			{
				msg.Send();
			}
		}

		// This sends a flag score
		public void SendScoreFlag(Client scorer, Item opponentflag)
		{
			// Dont send in these cases
			if((conn == null) || conn.Disposed) return;

			// Make the status update message and send it
			NetMessage msg = conn.CreateMessage(MsgCmd.ScoreFlag, true);
			if(msg != null)
			{
				msg.AddData((byte)scorer.id);
				msg.AddData((string)opponentflag.Key);
				msg.Send();
			}
		}

		// This sends a flag return
		public void SendReturnFlag(Client returner, Item flag)
		{
			int returnerid = 255;

			// Dont send in these cases
			if((conn == null) || conn.Disposed) return;

			// Determine returner id
			if(returner != null) returnerid = returner.id;

			// Make the status update message and send it
			NetMessage msg = conn.CreateMessage(MsgCmd.ReturnFlag, true);
			if(msg != null)
			{
				msg.AddData((byte)returnerid);
				msg.AddData((string)flag.Key);
				msg.Send();
			}
		}

		// This sends a shield hit
		public void SendShieldHit(Client c, float angle, float fadeout)
		{
			// Dont send in these cases
			if((conn == null) || conn.Disposed) return;

			// Make the status update message and send it
			NetMessage msg = conn.CreateMessage(MsgCmd.ShieldHit, false);
			if(msg != null)
			{
				msg.AddData((byte)c.id);
				msg.AddData((float)angle);
				msg.AddData((float)fadeout);
				msg.Send();
			}
		}

		// This sends a weapon switch
		public void SendSwitchWeapon(bool silent)
		{
			// Dont send in these cases
			if((conn == null) || conn.Disposed) return;

			// Make the status update message and send it
			NetMessage msg = conn.CreateMessage(MsgCmd.SwitchWeapon, true);
			if(msg != null)
			{
				msg.AddData((int)currentweapon.WeaponID);
				msg.AddData((bool)silent);
				msg.Send();
			}
		}

		// This sends a teleport event
		public void SendTeleportClient(Client c, Vector3D oldpos, Vector3D newpos)
		{
			// Dont send in these cases
			if((conn == null) || conn.Disposed) return;

			// Make the message and send it
			NetMessage msg = conn.CreateMessage(MsgCmd.TeleportClient, true);
			if(msg != null)
			{
				msg.AddData((byte)c.id);
				msg.AddData((float)oldpos.x);
				msg.AddData((float)oldpos.y);
				msg.AddData((float)oldpos.z);
				msg.AddData((float)newpos.x);
				msg.AddData((float)newpos.y);
				msg.AddData((float)newpos.z);
				msg.Send();
			}
		}

		// This sends player status
		public void SendFullStatusUpdate()
		{
			int i;

			// Dont send in these cases
			if((conn == null) || conn.Disposed) return;

			// Make the status update message and send it
			NetMessage msg = conn.CreateMessage(MsgCmd.StatusUpdate, true);
			if(msg != null)
			{
				msg.AddData((byte)health);
				msg.AddData((byte)armor);
				for(i = 0; i < (int)AMMO.TOTAL_AMMO_TYPES; i++) msg.AddData((short)ammo[i]);
				for(i = 0; i < (int)WEAPON.TOTAL_WEAPONS; i++) msg.AddData((bool)HasWeapon((WEAPON)i));
				msg.Send();
			}
		}

		// This sends a gamestate change signal
		public void SendGameStateChange()
		{
			// Dont send in these cases
			if((conn == null) || conn.Disposed) return;

			// Send the current gamestate to the client
			NetMessage msg = conn.CreateMessage(MsgCmd.GameStateChange, true);
			if(msg != null)
			{
				msg.AddData((byte)Host.Instance.Server.GameState);
				msg.AddData(Host.Instance.Server.GameStateEndTime - SharedGeneral.currenttime);
				msg.Send();
			}
		}

		// This sends a damage packet
		public void SendTakeDamage(Client target, int damage, int health, DEATHMETHOD method)
		{
			int targetid = 255;
			int hurtlevel = 0;

			// Dont send in these cases
			if((conn == null) || conn.Disposed) return;

			// Dont send in these cases
			if((conn == null) || conn.Disposed) return;

			// Determine target id
			if(target != null) targetid = target.id;

			// Unless death by falling
			if(method != DEATHMETHOD.QUIET)
			{
				// Determine hurt level
				if(health < 20) hurtlevel = 5;
				else if(health < 40) hurtlevel = 4;
				else if(health < 60) hurtlevel = 3;
				else if(health < 80) hurtlevel = 2;
				else hurtlevel = 1;
			}

			// Make the message and send it
			NetMessage msg = conn.CreateMessage(MsgCmd.TakeDamage, true);
			if(msg != null)
			{
				msg.AddData((byte)targetid);
				msg.AddData((int)damage);
				msg.AddData((byte)hurtlevel);
				msg.Send();
			}
		}

		// This sends a death packet
		public void SendClientDeath(Client source, Client target, string message, DEATHMETHOD method, PhysicsState targetstate, Vector2D targetpush)
		{
			int sourceid = 255;

			// Dont send in these cases
			if((conn == null) || conn.Disposed) return;

			// Determine source id
			if(source != null) sourceid = source.id;

			// Make the message and send it
			NetMessage msg = conn.CreateMessage(MsgCmd.ClientDead, true);
			if(msg != null)
			{
				msg.AddData((byte)target.id);
				msg.AddData((byte)sourceid);
				msg.AddData((string)message);
				msg.AddData((byte)(int)method);
				msg.AddData((float)targetstate.pos.x);
				msg.AddData((float)targetstate.pos.y);
				msg.AddData((float)targetstate.pos.z);
				msg.AddData((float)targetstate.vel.x);
				msg.AddData((float)targetstate.vel.y);
				msg.AddData((float)targetstate.vel.z);
				msg.AddData((float)targetpush.x);
				msg.AddData((float)targetpush.y);
				msg.Send();
			}
		}

		// This sends an item pickup
		public void SendItemPickup(Client clnt, Item item, bool attach, bool silent)
		{
			// Dont send in these cases
			if((conn == null) || conn.Disposed) return;

			// Make the pickup message and send it
			NetMessage msg = conn.CreateMessage(MsgCmd.ItemPickup, true);
			if(msg != null)
			{
				msg.AddData((byte)clnt.id);
				msg.AddData((string)item.Key);
				msg.AddData((int)item.RespawnDelay);
				msg.AddData((bool)attach);
				msg.AddData((bool)silent);
				msg.Send();
			}
		}

		// This sends an actor spawn
		public void SendSpawnActor(Client clnt, bool start)
		{
			// Dont send in these cases
			if((conn == null) || conn.Disposed) return;

			// Make the message and send it
			NetMessage msg = conn.CreateMessage(MsgCmd.SpawnActor, true);
			if(msg != null)
			{
				msg.AddData((byte)clnt.id);
				msg.AddData((bool)start);
				msg.AddData((float)clnt.state.pos.x);
				msg.AddData((float)clnt.state.pos.y);
				msg.AddData((byte)clnt.team);
				msg.Send();
			}
		}

		// This sends fire intensity of a player
		public void SendFireIntensity(Client clnt)
		{
			// Dont send in these cases
			if((conn == null) || conn.Disposed) return;

			// Make the message and send it
			NetMessage msg = conn.CreateMessage(MsgCmd.FireIntensity, true);
			if(msg != null)
			{
				msg.AddData((byte)clnt.id);
				msg.AddData((short)clnt.fireintensity);
				msg.Send();
			}
		}

		// This sends a team change to the client
		public void SendChangeTeam()
		{
			// Dont send in these cases
			if((conn == null) || conn.Disposed) return;

			// Make the message and send it
			NetMessage msg = conn.CreateMessage(MsgCmd.ChangeTeam, true);
			if(msg != null)
			{
				msg.AddData((byte)team);
				msg.AddData((bool)spectator);
				msg.Send();
			}
		}

		// This sends a sector movement event
		public void SendSectorMovements()
		{
			// Dont send in these cases
			if((conn == null) || conn.Disposed) return;

			// Make the update message
			NetMessage msg = conn.CreateMessage(MsgCmd.SectorMovement, true);
			if(msg != null)
			{
				// Go for all dynamic sectors
				foreach(DynamicSector ds in Host.Instance.Server.dynamics)
				{
					// Needs an update to the client?
					if(ds.SendSectorUpdate)
					{
						// Add sector movements information
						ds.AddSectorMovement(msg);
					}
				}

				// Send it
				msg.Send();
			}
		}

		// This sends a client disposed to this client
		public void SendClientDisposed(Client clnt)
		{
			// Dont send in these cases
			if((conn == null) || conn.Disposed) return;

			// Make the update message and send it
			NetMessage msg = conn.CreateMessage(MsgCmd.ClientDisposed, true);
			if(msg != null)
			{
				msg.AddData((byte)clnt.id);
				msg.Send();
			}
		}

		// This sends a client update to this client
		public void SendClientUpdate(Client clnt)
		{
			// Dont send in these cases
			if((conn == null) || conn.Disposed) return;

			// Make the update message and send it
			NetMessage msg = conn.CreateMessage(MsgCmd.ClientUpdate, true);
			if(msg != null)
			{
				msg.AddData((byte)clnt.id);
				msg.AddData((short)clnt.conn.LastPing);
				msg.AddData((byte)clnt.conn.LastLoss);
				msg.AddData((short)clnt.frags);
				msg.AddData((short)clnt.deaths);
				msg.AddData((short)clnt.score);
				msg.AddData((bool)clnt.loading);

				// Extra information when not sending to myself
				if(clnt != this)
				{
					msg.AddData((string)clnt.name);
					msg.AddData((byte)clnt.team);
					msg.AddData((bool)clnt.spectator);
				}

				// Send it
				msg.Send();
			}
		}

		// This sends a snapshot of all playing clients to this client
		public void SendSnapshots()
		{
			int numplayers = 0;
			int curplayer = 0;

			// Dont send in these cases
			if((conn == null) || conn.Disposed) return;

			// Continue until all players done
			while(curplayer < Host.Instance.Server.clients.Length)
			{
				// Snapshots message
				NetMessage msg = conn.CreateMessage(MsgCmd.Snapshot, false);
				if(msg != null)
				{
					// Go for all clients
					while((numplayers < MAX_PLAYERS_PER_SNAPSHOT) &&
					      (curplayer < Host.Instance.Server.clients.Length))
					{
						// Get current player
						Client c = Host.Instance.Server.clients[curplayer];

						// Check if slot is used, playing
						if((c != null) && !c.Loading && !c.Spectator)
						{
							// Add snapshot information to message
							c.AddSnapshotInfo(msg);
							numplayers++;
						}

						// Next player
						curplayer++;
					}

					// Send the message
					msg.Send();
					numplayers = 0;
				}
			}
		}

		// This adds my snapshot information to a message
		public void AddSnapshotInfo(NetMessage msg)
		{
			// State available?
			if(state != null)
			{
				// Add states
				msg.AddData((byte)id);
				msg.AddData((float)state.pos.x);
				msg.AddData((float)state.pos.y);
				msg.AddData((float)state.vel.x);
				msg.AddData((float)state.vel.y);
				msg.AddData((float)aimangle);
				msg.AddData((float)aimanglez);
				msg.AddData((byte)(int)powerup);
				if(shooting)
					msg.AddData((byte)(int)currentweapon.WeaponID);
				else
					msg.AddData((byte)255);
			}
		}

		// This sends all sector movements to this client
		public void SendAllSectorMovements()
		{
			// Dont send in these cases
			if((conn == null) || conn.Disposed) return;

			// Make message
			NetMessage msg = conn.CreateMessage(MsgCmd.SectorMovement, true);
			if(msg != null)
			{
				// Go for all dynamic sectors
				foreach(DynamicSector ds in Host.Instance.Server.dynamics)
				{
					// Add movement information
					ds.AddSectorMovement(msg);
				}

				// Send the message
				msg.Send();
			}
		}

		// This sends item pickups for items already picked up
		public void SendAllItemPickups()
		{
			// Go for all items
			foreach(Item i in Host.Instance.Server.items.Values)
			{
                // Send pickup message if taken
				if(i.IsTaken || i.IsAttached)
					SendItemPickup(i.Owner, i, i.IsAttached, true);
			}
		}

		// This sends client updates for all clients to this client
		public void SendAllCurrentClients()
		{
			// Dont send in these cases
			if((conn == null) || conn.Disposed) return;

			// Go for all clients
			foreach(Client c in Host.Instance.Server.clients)
			{
				// Check if slot is used
				// and this is not ourself
				if((c != null) && (c != this))
				{
					// Send client update
					SendClientUpdate(c);
				}
			}
		}

		// This sends GameSnapshot to this client
		// NOTE: This is sent regulary to keep the current values on the client correct
		public void SendGameSnapshot()
		{
			// Dont send in these cases
			if((conn == null) || conn.Disposed) return;

			// Make the message
			NetMessage msg = conn.CreateMessage(MsgCmd.GameSnapshot, true);
			if(msg != null)
			{
				// Add general information
				msg.AddData((int)Host.Instance.Server.teamscore[1]);
				msg.AddData((int)Host.Instance.Server.teamscore[2]);

				// Send the message
				msg.Send();
			}
		}

		// This sends MapChange to this client
		public void SendMapChange(string mapname, string maptitle)
		{
			// Join spectators
			team = TEAM.NONE;
			spectator = true;
			Spectate();

			// Dont send in these cases
			if((conn == null) || conn.Disposed) return;

			// Make the message
			NetMessage msg = conn.CreateMessage(MsgCmd.MapChange, true);
			if(msg != null)
			{
				msg.AddData((string)mapname);
				msg.AddData((string)maptitle);
				msg.Send();

				// Client is now loading this
				loading = true;
			}
		}

		// This sends GameStartInfo to this client
		public void SendGameStartInfo()
		{
			// Dont send in these cases
			if((conn == null) || conn.Disposed) return;

			// Make the message
			NetMessage msg = conn.CreateMessage(MsgCmd.StartGameInfo, true);
			if(msg != null)
			{
				msg.AddData((string)Host.Instance.Server.Title);
				msg.AddData((string)Host.Instance.Server.Website);
				msg.AddData((string)name);
				msg.AddData((string)Host.Instance.Server.map.Name);
				msg.AddData((string)Host.Instance.Server.map.Title);
				msg.AddData((byte)Host.Instance.Server.GameType);
				msg.AddData((int)Host.Instance.Server.Scorelimit);
				msg.AddData((short)Host.Instance.Server.MaxClients);
				msg.AddData((byte)id);
				msg.AddData((bool)Host.Instance.Server.IsTeamGame);
				msg.AddData((bool)Host.Instance.Server.JoinSmallest);
				msg.Send();

				// Client is now loading this
				loading = true;
			}
		}

		// This sends Disconnect to this client
		public void SendDisconnect(string reason)
		{
			// Dont send in these cases
			if((conn == null) || conn.Disposed) return;

			// Make the message
			NetMessage msg = conn.CreateMessage(MsgCmd.Disconnect, true);
			if(msg != null)
			{
				msg.AddData((string)reason);
				msg.Send();
			}
		}

		// This sends SayMessage to this client
		public void SendSayMessage(Client speaker, string message)
		{
			// Dont send in these cases
			if((conn == null) || conn.Disposed) return;
			if(loading) return;

			// Make the message
			NetMessage msg = conn.CreateMessage(MsgCmd.SayMessage, true);
			if(msg != null)
			{
				msg.AddData((string)message);
				msg.Send();
			}
		}

		// This sends ShowMessage to this client
		public void SendShowMessage(string message, bool onscreen)
		{
			// Dont send in these cases
			if((conn == null) || conn.Disposed) return;
			if(loading) return;

			// Make the message
			NetMessage msg = conn.CreateMessage(MsgCmd.ShowMessage, true);
			if(msg != null)
			{
				msg.AddData((bool)onscreen);
				msg.AddData((string)message);
				msg.Send();
			}
		}

		// Client disconnects
		public void Disconnect()
		{
			// Client disconnects
			this.Dispose();

			// Show disconnected message to all clients
			Host.Instance.Server.BroadcastShowMessage(name + "^7 disconnected", true, true);
		}

		// This sends a correction to the client
		public void SendClientCorrection()
		{
			// Dont send in these cases
			if((conn == null) || conn.Disposed) return;

			// Make the message
			NetMessage msg = conn.CreateMessage(MsgCmd.ClientCorrection, false);
			if(msg != null)
			{
				msg.AddData((int)clienttime);
				msg.AddData((float)state.pos.x);
				msg.AddData((float)state.pos.y);
				msg.AddData((float)state.pos.z);
				msg.AddData((float)state.vel.x);
				msg.AddData((float)state.vel.y);
				msg.AddData((float)state.vel.z);
				msg.AddData((float)pushvector.x);
				msg.AddData((float)pushvector.y);
				msg.Send();
			}
		}

		// This sends a projectile spawn to the client
		public void SendSpawnProjectile(Projectile p)
		{
			// Dont send in these cases
			if((conn == null) || conn.Disposed) return;

			// Make the message
			NetMessage msg = conn.CreateMessage(MsgCmd.SpawnProjectile, true);
			if(msg != null)
			{
				msg.AddData((string)p.ID);
				msg.AddData((byte)(int)p.Type);
				msg.AddData((float)p.Pos.x);
				msg.AddData((float)p.Pos.y);
				msg.AddData((float)p.Pos.z);
				msg.AddData((float)p.Vel.x);
				msg.AddData((float)p.Vel.y);
				msg.AddData((float)p.Vel.z);
				msg.AddData((byte)p.Source.id);
				msg.AddData((byte)(int)p.Source.Team);
				msg.Send();
			}
		}

		// This sends a projectile update to the client
		public void SendUpdateProjectile(Projectile p)
		{
			// Dont send in these cases
			if((conn == null) || conn.Disposed) return;

			// Make the message
			NetMessage msg = conn.CreateMessage(MsgCmd.UpdateProjectile, false);
			if(msg != null)
			{
				msg.AddData((string)p.ID);
				msg.AddData((byte)(int)p.Type);
				msg.AddData((float)p.Pos.x);
				msg.AddData((float)p.Pos.y);
				msg.AddData((float)p.Pos.z);
				msg.AddData((float)p.Vel.x);
				msg.AddData((float)p.Vel.y);
				msg.AddData((float)p.Vel.z);
				msg.AddData((byte)p.Source.id);
				msg.AddData((byte)(int)p.Source.Team);
				msg.Send();
			}
		}

		// This sends a projectile teleport to the client
		public void SendTeleportProjectile(Vector3D oldpos, Projectile p)
		{
			// Dont send in these cases
			if((conn == null) || conn.Disposed) return;

			// Make the message
			NetMessage msg = conn.CreateMessage(MsgCmd.TeleportProjectile, true);
			if(msg != null)
			{
				msg.AddData((string)p.ID);
				msg.AddData((byte)(int)p.Type);
				msg.AddData((float)oldpos.x);
				msg.AddData((float)oldpos.y);
				msg.AddData((float)oldpos.z);
				msg.AddData((float)p.Pos.x);
				msg.AddData((float)p.Pos.y);
				msg.AddData((float)p.Pos.z);
				msg.AddData((float)p.Vel.x);
				msg.AddData((float)p.Vel.y);
				msg.AddData((float)p.Vel.z);
				msg.Send();
			}
		}

		// This sends a projectile destroy to the client
		public void SendDestroyProjectile(Projectile p, bool silent, Client hitplayer)
		{
			int hitplayerid = 255;

			// Dont send in these cases
			if((conn == null) || conn.Disposed) return;

			// Determine source id
			if(hitplayer != null) hitplayerid = hitplayer.id;

			// Make the message
			NetMessage msg = conn.CreateMessage(MsgCmd.DestroyProjectile, true);
			if(msg != null)
			{
				msg.AddData((string)p.ID);
				msg.AddData((byte)(int)p.Type);
				msg.AddData((bool)silent);
				msg.AddData((byte)hitplayerid);
				msg.AddData((float)p.Pos.x);
				msg.AddData((float)p.Pos.y);
				msg.AddData((float)p.Pos.z);
				msg.Send();
			}
		}

		#endregion

		#region ================== Powerups

		// This gives a powerup
		public void GivePowerup(POWERUP pup, int count)
		{
			// Replace the powerup with the new one
			powerup = pup;
			powercount = count;
			powerinterval = SharedGeneral.currenttime;
			powerfired = false;
		}

		// This removes any powerup
		public void RemovePowerup()
		{
			// Remove any powerup
			powerup = POWERUP.NONE;
			powercount = 0;
			powerfired = false;
		}

		// This dereases the powerup countdown
		public void DecreasePowerupCount(Client source, int amount)
		{
			// When this is NOT a teammate
			if((source == null) || (source == this) ||
			   (source.team != this.team) || !Host.Instance.Server.IsTeamGame)
			{
				// Decrease powerup countdown
				powercount -= amount;

				// Lose the powerup when worn out
				if(powercount <= 0) RemovePowerup();

				// Send update
				SendPowerupCountUpdate();
			}
		}

		// This processes the powerup
		public void ProcessPowerup()
		{
			// Leave when no powerup
			if(powerup == POWERUP.NONE) return;

			// Countdown
			powercount -= Consts.TIMESTEP;

			// Check if countdown finished
			if(powercount <= 0)
			{
				// Nuke powerup fired?
				if((powerup == POWERUP.NUKE) && powerfired)
				{
					// Spawn the nuke detonation
					new NukeDetonation(state.pos, this);
				}

				// Lose the powerup
				RemovePowerup();
				return;
			}

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

			// Advance interval time
			powerinterval += Consts.POWERUP_STATIC_INTERVAL;

			// Must have a state myself to continue
			if(state == null) return;

			// Determine my position
			dpos = state.pos + new Vector3D(0f, 0f, 6f);

			// Go for all playing clients
			foreach(Client c in Host.Instance.Server.clients)
			{
				// Client alive and not myself?
				if((c != null) && (!c.Loading) && (c.IsAlive) && (c != this))
				{
					// Determine client position
					cpos = c.State.pos + new Vector3D(0f, 0f, 6f);

					// Calculate distance to this player
					Vector3D delta = cpos - dpos;
					delta.z *= Consts.POWERUP_STATIC_Z_SCALE;
					float distance = delta.Length();

					// Within static range?
					if(distance < Consts.POWERUP_STATIC_RANGE)
					{
						// Check if nothing blocks in between clients
						if(!Host.Instance.Server.map.FindRayMapCollision(dpos, cpos))
						{
							// Hurt the other client slowly
							c.Hurt(this, Client.DEATH_STATIC, Consts.POWERUP_STATIC_DAMAGE, DEATHMETHOD.NORMAL, dpos);
						}
					}
				}
			}
		}

		#endregion

		#region ================== Weapons / Ammo

		// This uses ammo
		// Returns false when not enough ammo
		public bool UseAmmo(AMMO ammotype, int amount)
		{
			// Check if enough ammo available
			if(ammo[(int)ammotype] >= amount)
			{
				// Use the ammo
				ammo[(int)ammotype] -= amount;
				SendFullStatusUpdate();
				return true;
			}
			else
			{
				// No ammo
				return false;
			}
		}

		// This checks for ammo
		// Returns false when not enough ammo
		public bool CheckAmmo(AMMO ammotype, int amount)
		{
			// Check if enough ammo available
			return (ammo[(int)ammotype] >= amount);
		}

		// This adds ammo
		// Returns false when nothing added
		public bool AddAmmo(AMMO ammotype, int amount)
		{
			// Can we add ammo?
			if(ammo[(int)ammotype] < Consts.MAX_AMMO[(int)ammotype])
			{
				// Add the ammo
				ammo[(int)ammotype] += amount;
				if(ammo[(int)ammotype] > Consts.MAX_AMMO[(int)ammotype])
					ammo[(int)ammotype] = Consts.MAX_AMMO[(int)ammotype];
				SendFullStatusUpdate();
				return true;
			}
			else
			{
				// Nothing added
				return false;
			}
		}

		// This sets the ammo
		public void SetAmmo(AMMO ammotype, int amount)
		{
			// Add the ammo
			ammo[(int)ammotype] = amount;
			if(ammo[(int)ammotype] > Consts.MAX_AMMO[(int)ammotype])
				ammo[(int)ammotype] = Consts.MAX_AMMO[(int)ammotype];
			SendFullStatusUpdate();
		}

		// This clears all ammo
		public void ClearAmmo()
		{
			// Remove all ammo
			for(int i = 0; i < (int)AMMO.TOTAL_AMMO_TYPES; i++) ammo[i] = 0;
			SendFullStatusUpdate();
		}

		// This checks if the client has a weapon
		public bool HasWeapon(WEAPON weaponid)
		{
			// Return result
			return (allweapons[(int)weaponid] != null);
		}

		// This gives the client a weapon
		public void GiveWeapon(WEAPON weaponid)
		{
			// Check the weapon
			if(!HasWeapon(weaponid))
			{
				// Give the weapon
				Weapon w = Weapon.CreateFromID(this, weaponid);
				allweapons[(int)weaponid] = w;

				// Give initial ammo
				if(!AddAmmo(w.AmmoType, w.InitialAmmo)) SendFullStatusUpdate();

				// No weapon selected yet?
				if(currentweapon == null)
				{
					// Select this weapon now
					currentweapon = w;

					// Send weapon switch
					SendSwitchWeapon(true);
				}

				// Switch automatically?
				if(autoswitchweapon && (currentweapon != w) && !shooting)
				{
					// Switch to this weapon
					switchtoweapon = w;
				}
			}
		}

		// This removes a weapon
		public void RemoveWeapon(WEAPON weaponid, bool sendupdate)
		{
			// Check the weapon
			if(HasWeapon(weaponid))
			{
				// Check if weapon is the current weapon
				if(currentweapon == allweapons[(int)weaponid])
				{
					// No more current weapon
					currentweapon = null;
				}

				// Check if switching to this weapon
				if(switchtoweapon == allweapons[(int)weaponid])
				{
					// No more switching
					switchtoweapon = null;
				}

				// Dispose the weapon
				allweapons[(int)weaponid].Dispose();
				allweapons[(int)weaponid] = null;

				// Send status update
				if(sendupdate) SendFullStatusUpdate();
			}
		}

		// This clears all weapons
		public void ClearWeapons()
		{
			// Remove all weapons
			for(int i = 0; i < (int)WEAPON.TOTAL_WEAPONS; i++) RemoveWeapon((WEAPON)i, false);
			currentweapon = null;
			switchtoweapon = null;
			SendFullStatusUpdate();
		}

		// This switches weapons when current weapon is ready
		public void SwitchWeapons(bool silent)
		{
			// Switching?
			if(switchtoweapon != null)
			{
				// Check if weapon is ready
				if((currentweapon == null) || (currentweapon.IsIdle()))
				{
					// Switch now
					currentweapon = switchtoweapon;
					switchtoweapon = null;
					SendSwitchWeapon(silent);
				}
			}
		}

		#endregion

		#region ================== Methods

		// This clears the players frags/deaths/score
		public void ResetScores()
		{
			// Reset scores
			score = 0;
			frags = 0;
			deaths = 0;
		}

		// This sets the player status of this client
		// to their initial values for spawning
		private void ResetPlayerStatus()
		{
			// Clear status
			ClearWeapons();
			ClearAmmo();

			// Remove powerup
			RemovePowerup();

			// Set default elements
			health = 100;
			armor = 0;
			GiveWeapon(WEAPON.SMG);
			currentweapon = allweapons[(int)WEAPON.SMG];

			// Send changes
			SendFullStatusUpdate();
		}

		// This tests collisions with items and picks them up
		private void PickupItems()
		{
			// Do we have an in-game state?
			if(state != null)
			{
				// Go for all items
				foreach(Item i in Host.Instance.Server.items.Values)
				{
                    // Item not taken or attached?
					if(!i.IsTaken && !i.IsAttached)
					{
						// Check if within acceptable Z level
						if((i.Position.z >= (state.pos.z - 2f)) &&
						   (i.Position.z < (state.pos.z + Consts.PLAYER_HEIGHT)))
						{
							// Check distance to item
							Vector3D delta = i.Position - this.state.pos;
							float distance = delta.Length();
							if((distance - Consts.PLAYER_RADIUS) <= i.Radius)
							{
								// Pickup item!
								i.Pickup(this);
							}
						}
					}
				}
			}
		}

		// This drops the client to the floor of highest sector immediately
		public void DropImmediately()
		{
			// Drop to floor of highest sector
			state.pos.z = highestsector.CurrentFloor;
			state.vel.z = 0f;
		}

		// This applies gravity and takes care of stepping up stairs
		private void PerformZChanges()
		{
			float highestz = float.MinValue;
            List<Sector> sectors;

			// Find touching sectors
			sectors = Host.Instance.Server.map.FindTouchingSectors(state.pos.x, state.pos.y, Consts.PLAYER_RADIUS);

			// Find the highest sector floor
			foreach(Sector s in sectors)
			{
				// Check if higher but not blocking
				if((s.CurrentFloor > highestz) &&
					(s.CurrentFloor - Consts.MAX_STEP_HEIGHT < state.pos.z))
				{
					// This height is higher and stil valid
					highestz = s.CurrentFloor;
					highestsector = s;
				}
			}

			// Should we fall down?
			if(highestz < state.pos.z)
			{
				// Apply gravity
				state.vel.z -= Consts.GRAVITY;
			}
			else
			{
				// No gravity
				state.vel.z = 0f;

				// Should we stay above floor?
				if(highestz > state.pos.z)
				{
					// Stay above floor
					state.pos.z = highestz;
				}
			}
		}

		// This applies static sector effects
		private void ApplySectorFloorEffects()
		{
			// Do we have a highest sector?
			if(highestsector != null)
			{
				// Check sector effect
				switch(highestsector.Effect)
				{
					// Instant death
					case SECTOREFFECT.INSTANTDEATH:

						// Kill the player immediately
						Hurt(null, DEATH_SELF, 1000, DEATHMETHOD.QUIET, true);
						break;
				}
			}
		}

		// This applies effects of crossing lines
		private void ApplyCrossLineEffect(Sidedef crossline)
		{
			// Check the line effect
			switch(crossline.Linedef.Action)
			{
				// Teleport!
				case ACTION.TELEPORT:

					// Teleport when on the floor and crossing from the front side
					if((state.pos.z < sector.CurrentFloor + Consts.TELEPORT_HEIGHT) &&
					   (crossline == crossline.Linedef.Front))
						TeleportToThing(crossline.Linedef.Arg[0], this.state.pos);

					break;

				// Gib!
				case ACTION.INSTANTGIB:

					// Gib the player
					if(state.pos.z < sector.CurrentFloor + Consts.TELEPORT_HEIGHT)
						Hurt(null, DEATH_SELF, 10000, DEATHMETHOD.GIBBED, true);

					break;
			}
		}

		// This tests and applies chat flood protection
		// Returns false when the chat line is not allowed
		private bool ChatFloodContinue()
		{
			// Get time difference
			int diff = SharedGeneral.currenttime - chatfloodtime;

			// Store this time
			chatfloodtime = SharedGeneral.currenttime;

			// Check if this is insane
			if(diff <= CHAT_INSANE_TIMEOUT)
			{
				// Denied
				return false;
			}
			else
			{
				// Check if this time is considered a flood
				if(diff <= CHAT_FLOOD_TIMEOUT)
				{
					// Check if already enough floods
					if(chatfloodlines >= (CHAT_MAX_FLOODS - 1))
					{
						// Count this flood
						SendShowMessage("Please do not spam the chat. This is a game, not a kiddies chatbox.", true);
						chatfloodlines++;

						// Denied
						return false;
					}
					else
					{
						// Count this flood
						chatfloodlines++;

						// Allow it
						return true;
					}
				}
				else
				{
					// No floods anymore
					chatfloodlines = 0;

					// Allow this
					return true;
				}
			}
		}

		#endregion

		#region ================== Processing

		// This processes a client
		public void Process()
		{
			float walkpower, walklimit;
			Vector2D vel2d;
			string reason;
			Sidedef crossline = null;

			// Check if connection is lost
			if((conn != null) && conn.Disposed)
			{
				// Keep disconnect reason
				reason = conn.DisconnectReason;

				// Client timed out
				this.Dispose();

				// Show timeout message to all clients
				Host.Instance.Server.BroadcastShowMessage(name + "^7 disconnected (" + reason.ToLower() + ")", true, true);
			}

			// State available?
			if(state != null)
			{
				// Walking?
				if(walkangle > -1f)
				{
					// Check if the actor can move
					if(state.vel.Length() < Consts.MAX_WALK_LENGTH)
					{
						// Determine walking power
						if(!IsOnFloor) walkpower = Consts.AIRWALK_LENGTH;
						else walkpower = Consts.WALK_LENGTH;

						// Determine walking limit
						if(powerup != POWERUP.SPEED) walklimit = Consts.MAX_WALK_LENGTH;
						else walklimit = Consts.MAX_SPEED_WALK_LENGTH;

						// Add to walk velocity
						state.vel.x += (float)Math.Sin(walkangle) * walkpower;
						state.vel.y += (float)Math.Cos(walkangle) * walkpower;

						// Scale to match walking length
						vel2d = state.vel;
						if(vel2d.Length() > walklimit)
							vel2d.MakeLength(walklimit);
						state.vel.Apply2D(vel2d);

						// Apply push vector over velocity
						state.vel += (Vector3D)pushvector;
					}
				}

				// Handle the weapon
				if(shooting && (currentweapon != null))
				{
					// Shooting a weapon
					currentweapon.Trigger();
				}
				else
				{
					// Not shooting
					if(currentweapon != null) currentweapon.Released();
					SwitchWeapons(false);
				}

				// Only continue when we still have a state
				if(state != null)
				{
					// Make changes to Z velocity
					this.PerformZChanges();

					// Decelerate
					if(IsOnFloor)
						state.vel /= 1f + Consts.WALK_DECELERATION;
					else
						state.vel /= 1f + Consts.AIR_DECELERATION;

					// Decelerate push vector
					pushvector /= 1f + Consts.PUSH_DECELERATION;

					// Advance the position of this player
					sendcorrection = state.ApplyVelocity(true, true, Host.Instance.Server.clients, this, out crossline);

					// Determine sector where player is in
					sector = Host.Instance.Server.map.GetSubSectorAt(state.pos.x, state.pos.y).Sector;

					// Pickup items
					this.PickupItems();
				}

				// Only continue when we still have a state
				if(state != null)
				{
					// Process powerup
					this.ProcessPowerup();
				}

				// Only continue when we still have a state
				if(state != null)
				{
					// Process fire effect
					this.ProcessFireEffect();
				}

				// Only continue when we still have a state
				if(state != null)
				{
					// Apply effect of crossing lines
					if((crossline != null)) ApplyCrossLineEffect(crossline);
				}

				// Only continue when we still have a state
				if(state != null)
				{
					// Send client correction?
					if((sendcorrection || (correctiontime <= SharedGeneral.currenttime)))
					{
						// Send correction to this client
						SendClientCorrection();
						correctiontime += CLIENTCORRECT_INTERVAL;
						sendcorrection = false;
					}

					// When on the floor, apply sector floor effects
					if(IsOnFloor) this.ApplySectorFloorEffects();
				}

				// Advance client time
				clienttime += Consts.TIMESTEP;
			}
			else
			{
				// Player dead and respawn time over
				if((autorespawntime > 0) && (SharedGeneral.currenttime > autorespawntime))
				{
					// Respawn if possible
					this.Respawn();
				}
			}

			// Time to send snapshots to this player?
			if((snapsendtime < SharedGeneral.currenttime) && !disposed && !loading)
			{
				// Send snapshots to this client
				this.SendSnapshots();
				snapsendtime += snapsinterval;
			}

			// Time to broadcast update for this player?
			if((clientupdatetime < SharedGeneral.currenttime) && !disposed)
			{
				// Broadcast update for me
				Host.Instance.Server.BroadcastClientUpdate(this);
				clientupdatetime += CLIENT_UPDATE_INTERVAL;

				// Also send a new gamesnapshot to me
				SendGameSnapshot();
			}
		}

		#endregion
	}
}
