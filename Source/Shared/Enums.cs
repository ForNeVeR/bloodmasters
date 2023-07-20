/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;

namespace CodeImp.Bloodmasters
{
	// Global constants
	public class Consts
	{
		// Time each frame will be processed
		public const int TIMESTEP = 10;
		
		// Walk deceleration (divide over time)
		public const float WALK_DECELERATION = 0.1f;
		public const float AIR_DECELERATION = 0.03f;
		public const float DEAD_DECELERATION = 0.04f;
		public const float PUSH_DECELERATION = 0.06f;
		
		// Gravity
		public const float GRAVITY = 0.1f; // 0.12f;
		
		// Walking velocity
		public const float WALK_LENGTH = 1f;
		public const float AIRWALK_LENGTH = 0.5f;
		public const float MAX_WALK_LENGTH = 0.25f;
		public const float MAX_SPEED_WALK_LENGTH = 0.40f;
		
		// Color codes
		public const string COLOR_CODE_SIGN = "^";
		
		// Player sizes
		public const float PLAYER_RADIUS = 1.75f;
		public const float PLAYER_RADIUS_SQ = PLAYER_RADIUS * PLAYER_RADIUS;
		public const float PLAYER_DIAMETER = PLAYER_RADIUS * 2f;
		public const float PLAYER_DIAMETER_SQ = PLAYER_DIAMETER * PLAYER_DIAMETER;
		public const float PLAYER_HEIGHT = 14f;
		public const float PLAYER_BLOCK_HEIGHT = 10f;
		
		// Player friction
		public const float PLAYER_FRICTION = 1.1f;
		
		// Max step up height
		public const float MAX_STEP_HEIGHT = 4f;
		
		// Tolerance permitted when determining
		// if an actor is on the floor
		public const float FLOOR_TOUCH_TOLERANCE = 0.2f;
		
		// Teleport height
		public const float TELEPORT_HEIGHT = 8f;
		
		// Default game server port
		public const int DEFAULT_SERVER_PORT = 6969;
		
		// Player gibbing threshold
		public const int GIB_THRESHOLD = -10;
		
		// Player name restrictions
		public const int MAX_PLAYER_NAME_LEN = 14;
		public const int MAX_PLAYER_NAME_STR_LEN = 30;
		public const string REQ_PLAYER_CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
		
		// Item respawn time -> Never respawn
		public const int NEVER_RESPAWN_TIME = 9999999;
		
		// Maximum ammo
		public static readonly int[] MAX_AMMO = new int[] { 600, 200, 20, 50, 200 };
		
		// Grenade physics
		public const float GRENADE_GRAVITY = 0.04f;
		public const float GRENADE_DECELERATE_AIR = 0.01f;
		public const float GRENADE_DECELERATE_FLOOR = 0.06f;
		public const float GRENADE_BOUNCEVEL = 0.7f;
		
		// Ion ranges
		public const float ION_FLYBY_RANGE = 12;
		public const float ION_EXPLODE_RANGE = 25;
		
		// Phoenix fire physics
		public const float FLAMES_DECELERATE = 0.05f;
		
		// Static powerup
		public const int POWERUP_STATIC_COUNT = 30000;
		public const float POWERUP_STATIC_RANGE = 30f;
		public const float POWERUP_STATIC_Z_SCALE = 0.2f;
		public const int POWERUP_STATIC_DAMAGE = 3;
		public const int POWERUP_STATIC_INTERVAL = 100;
		
		// Shield powerup
		public const int POWERUP_SHIELD_COUNT = 30000;
		public const int POWERUP_SHIELD_INTERVAL = 99;
		
		// Nuke powerup
		public const int POWERUP_NUKE_COUNT = 30000;
		public const int POWERUP_NUKE_FIRECOUNT = 3000;
		
		// Ghost powerup
		public const int POWERUP_GHOST_COUNT = 30000;
		
		// Killer powerup
		public const int POWERUP_KILLER_COUNT = 30000;
		
		// Avenger powerup
		public const int POWERUP_AVENGER_COUNT = 30000;
		
		// Sprinter powerup
		public const int POWERUP_SPEED_COUNT = 30000;
	}
	
	// Liquids
	public enum LIQUID : int
	{
		NONE = 0,
		WATER = 1,
		LAVA = 2,
	}
	
	// Projectiles
	public enum PROJECTILE : int
	{
		PLASMABALL = 1,
		ROCKET = 2,
		GRENADE = 3,
		FLAMES = 4,
		NUKEDETONATION = 5,
		IONBALL = 6,
	}
	
	// Death methods
	public enum DEATHMETHOD : int
	{
		NORMAL = 1,
		GIBBED = 2,
		QUIET = 3,
		NORMAL_NOGIB = 4,
	}
	
	// Powerups
	public enum POWERUP : int
	{
		NONE = 0,
		KILLER = 1,
		SHIELDS = 2,
		SPEED = 3,
		STATIC = 4,
		AVENGER = 5,
		GHOST = 6,
		NUKE = 7,
		TOTAL_POWERUPS = 7,
	}
	
	// Weapons
	public enum WEAPON : int
	{
		SMG = 0,
		MINIGUN = 1,
		PLASMA = 2,
		ROCKET_LAUNCHER = 3,
		GRENADE_LAUNCHER = 4,
		PHOENIX = 5,
		IONCANNON = 6,
		TOTAL_WEAPONS = 7,
	}
	
	// Ammo types
	public enum AMMO : int
	{
		BULLETS = 0,
		PLASMA = 1,
		ROCKETS = 2,
		GRENADES = 3,
		FUEL = 4,
		TOTAL_AMMO_TYPES = 5,
	}
	
	// Draw modes
	public enum DRAWMODE : int
	{
		UNDEFINED = -1,
		NALPHA = 0,
		TLMODALPHA = 1,
		NLIGHTMAP = 2,
		TLLIGHTDRAW = 3,
		TLLIGHTBLEND = 4,
		NLIGHTMAPALPHA = 5,
		NLINES = 6,
		PNORMAL = 7,
		PADDITIVE = 8,
		NADDITIVEALPHA = 9,
		NLIGHTBLEND = 10,
	}
	
	// Things that do not have their own classes
	public enum THINGTYPE : int
	{
		PLAYER_DM = 1,
		PLAYER_BLUE = 2,
		PLAYER_RED = 3,
		TELEPORT = 100,
	}
	
	// Thing flags
	[Flags] public enum THINGFLAG : int
	{
		NONE = 0,
		DM = 1,
		TDM = 2,
		CTF = 4,
		SC = 8,
		TSC = 16,
	}
	
	// Line/Thing actions
	public enum ACTION : int
	{
		NONE = 0,
		BLACK_GRADIENT = 1,
		TELEPORT = 2,
		INSTANTGIB = 3,
	}
	
	// Linedef flags
	[Flags] public enum LINEFLAG : int
	{
		NONE = 0,
		SOLID = 1,
		IMPASSABLE = 2,
		DOUBLESIDED = 4,
		NOSHADOW = 8,
	}
	
	// Sector effects
	public enum SECTOREFFECT : int
	{
		NONE = 0,
		DOOR = 1,
		DOORFIELD = 2,
		SLOWDAMAGE = 3,
		FASTDAMAGE = 4,
		INSTANTDEATH = 5,
		INVISIBLE = 6,
		FIXEDLIGHT = 7,
		TECHDOORFAST = 8,
		TECHDOORSLOW = 9,
		PLATFORMLOW = 10,
		PLATFORMHIGH = 11,
		PLATFORMFIELD = 12,
		NOMERGE = 13,
	}
	
	// Sector meterials
	public enum SECTORMATERIAL : int
	{
		NORMAL = 0,
		METAL = 1,
		LIQUID = 2,
	}
	
	// Door status
	public enum DOORSTATUS : int
	{
		CLOSED = 0,
		OPENING = 1,
		OPEN = 2,
		CLOSING = 3,
	}
	
	// Teams
	public enum TEAM : int
	{
		NONE = 0,	// TEAMLESS GAME OR NEUTRAL SPECTATOR
		RED = 1,
		BLUE = 2,
	}
	
	// Game types
	public enum GAMETYPE : int
	{
		DM = 0,
		TDM = 1,
		CTF = 2,
		SC = 3,
		TSC = 4,
	}
	
	// Game states
	public enum GAMESTATE : int
	{
		WAITING = 0,
		SPAWNING = 1,
		COUNTDOWN = 2,
		PLAYING = 3,
		ROUNDFINISH = 4,
		GAMEFINISH = 5,
	}
	
	public enum EXTRAKEYS : int
	{
		MScrollUp = 65530,
		MScrollDown = 65531,
	}
}
