/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using System.Collections.Generic;
using System.Globalization;
using SharpDX;
using SharpDX.Direct3D9;
using RectangleF = System.Drawing.RectangleF;

namespace CodeImp.Bloodmasters.Client
{
	public class Actor : VisualObject, IPhysicsState, ILightningNode
	{
		#region ================== Constants

		// Billboard angles
		private const float SPRITE_ANGLE_X = (float)Math.PI * -0.25f;
		private const float SPRITE_ANGLE_Z = (float)Math.PI * 0.25f;

		// Body part scales
		private const float TORSO_SCALE = 8f;
		private const float LEGS_SCALE = 8f;

		// Body part offsets
		public const float TORSO_OFFSET_X = 2.4f;
		public const float TORSO_OFFSET_Y = -2.4f;
		public const float TORSO_OFFSET_Z = 4.0f;
		public const float LEGS_OFFSET_X = 2.4f;
		public const float LEGS_OFFSET_Y = -2.4f;
		public const float LEGS_OFFSET_Z = 4.0f;
		public const float TORSO_DEAD_OFFSET_X = 2.4f;
		public const float TORSO_DEAD_OFFSET_Y = -2.4f;
		public const float TORSO_DEAD_OFFSET_Z = 1.0f;

		// Shadow
		private const float SHADOW_SIZE = 5f;
		private const float SHADOW_ALPHA_MUL = 1f;

		// Prefixes and body parts for animations
		private const string PFX_SEPERATOR = "_";
		private const string PFX_IDLE = "idle";
		private const string PFX_WALKING = "running";
		private const string PFX_SHOOTING = "shooting";
		private const string PFX_DEATH = "death";
		private const string PRT_TORSO = "torso";
		private const string PRT_LEGS = "legs";

		// Walking settings
		private const float WALK_MIN_AMOUNT = 0.02f;
		private const float WALK_MAX_AMOUNT = 2f;
		private const float WALK_FRAMERATE_SCALE = 35f; // 60f;

		// Sound settings
		private const int WALK_STEP_SOUNDS = 4;
		private const int WALK_STEP_INTERVAL = 280;
		private const int FALL_STEP_INTERVAL = 50;

		// HUD Name
		private const float NAME_OFFSET = 1f;

		// Dying
		private const int BLOOD_SPAWN_DELAY = 600;
		private const int DISSOLVE_SPEED = 5000;
		private const float GIB_VEL_FACTOR = 0.2f;
		private const float GIB_RND_FACTOR = 0.4f;

		#endregion

		#region ================== Variables

		// Settings
		public static bool showgibbing;

		// References
		private ClientSector sector;
		private ClientSector highestsector;

		// States
		private float aimangle = 0f;
		private float aimanglez = 0f;
		private int teamcolor = 0;
		private TEAM team;
		private int clientid;
		private PhysicsState state;
		private Vector2D pushvec;
		private bool onfloor = true;
		private bool dead = false;
		private bool deadthreshold = false;
		private bool disposed = false;
		private bool shownuke = false;
		private bool showrage = false;
		private bool showring = false;

		// Walking state
		private bool walking = false;
		private float walkangle = 0f;
		private int teleportlock;

		// Animations
		private Animation torso_ani;
		private Animation legs_ani;
		private bool ani_legswalking = false;
		private bool ani_torsowalking = false;
		private bool ani_torsoshooting = false;
		private float alpha = 0f;

		// Dying
		private int bloodspawntime;
		private int dissolvetime;

		// Geometry
		private Matrix torso_scalerotate;
		private Matrix legs_scalerotate;
		private Matrix lightmapoffsets = Matrix.Identity;
		private Matrix dynlightmapoffsets = Matrix.Identity;

		// Sounds
		private int stepsoundtime = 0;
		private bool fallsoundplayed = false;

		// Effects
		private FireEffect fireeffect = null;
		private List<Lightning> lightnings = new();
		private RageEffect rageeffect = null;
		private int ragecolor = -1;

		// HUD name
		private TextResource name = null;

		#endregion

		#region ================== Properties

		public float AimAngle { get { return aimangle; } set { aimangle = value; } }
		public float AimAngleZ { get { return aimanglez; } set { aimanglez = value; } }
		public bool IsLocal { get { return (General.localclient.Actor == this); } }
		public bool IsWalking { get { return walking; } set { walking = value; } }
		public bool IsOnFloor { get { return onfloor; } }
		public float WalkAngle { get { return walkangle; } set { walkangle = value; } }
		public ClientSector Sector { get { return sector; } }
		public ClientSector HighestSector { get { return highestsector; } }
		public int TeamColor { get { return teamcolor; } set { teamcolor = value; } }
		public string Name { get { if(name != null) return name.Text; else return ""; } set { if(name != null) name.Text = value; } }
		public PhysicsState State { get { return state; } }
		public bool IsDead { get { return dead; } }
		public bool Disposed { get { return disposed; } }
		public bool DeadThreshold { get { return deadthreshold; } }
		public float Alpha { get { return alpha; } set { alpha = value; } }
		public bool FallSoundPlayed { get { return fallsoundplayed; } set { fallsoundplayed = value; } }
		public int TeleportLock { get { return teleportlock; } set { teleportlock = value; } }
		public Vector2D PushVector { get { return pushvec; } set { pushvec = value; } }
		public bool ShowNuke { get { return shownuke; } set { shownuke = value; } }
		public bool ShowRing { get { return showring; } set { showring = value; } }
		public Vector3D Velocity { get { return state.vel; } }
		public List<Lightning> Lightnings { get { return lightnings; } }
		public TEAM Team { get { return team; } }
		public int ClientID { get { return clientid; } }
		public bool ShowRage { get { return showrage; } set { showrage = value; } }
		public int RageColor { get { return ragecolor; } set { ragecolor = value; } }

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public Actor(Vector3D v, int team, int clientid)
		{
			Matrix mscale1, mscale2;
			Matrix mrot1, mrot2, mrotate;

			// Create physics state
			state = new ClientPhysicsState(General.map);
			state.Radius = Consts.PLAYER_RADIUS;
			state.Height = Consts.PLAYER_BLOCK_HEIGHT;
			state.Friction = Consts.PLAYER_FRICTION;
			state.StepUp = true;
			state.Redirect = true;
			state.Bounce = false;
			state.pos = v;

			// Team/client
			this.teamcolor = team;
			this.team = (TEAM)team;
			this.clientid = clientid;

			// Render bias
			renderbias = 2f;

			// Make default animations
			torso_ani = Animation.CreateFrom(AniFilename(PFX_IDLE, PRT_TORSO, teamcolor));
			legs_ani = Animation.CreateFrom(AniFilename(PFX_IDLE, PRT_LEGS, teamcolor));
			alpha = 1f;

			// Create HUD name
			name = Direct3D.CreateTextResource(General.charset_shaded);
			name.Texture = General.font_shaded.texture;
			name.HorizontalAlign = TextAlignX.Center;
			name.VerticalAlign = TextAlignY.Middle;
			name.Viewport = new RectangleF(0f, 0f, 0f, 0f);
			name.Colors = TextResource.color_brighttext;
			name.Scale = 0.3f;
			//name.ModulateColor = General.ARGB(1f, 0.8f, 0.8f, 0.8f);

			// Position the actor
			this.Move(v);

			// Scale sprite
			mscale1 = Matrix.Scaling(TORSO_SCALE, 1f, TORSO_SCALE);
			mscale2 = Matrix.Scaling(LEGS_SCALE, 1f, LEGS_SCALE);

			// Rotate sprite
			mrot1 = Matrix.RotationX(SPRITE_ANGLE_X);
			mrot2 = Matrix.RotationZ(SPRITE_ANGLE_Z);
			mrotate = Matrix.Multiply(mrot1, mrot2);

			// Combine scale and rotation
			torso_scalerotate = Matrix.Multiply(mscale1, mrotate);
			legs_scalerotate = Matrix.Multiply(mscale2, mrotate);

			// Add to arena
			General.arena.AddActor(this);
		}

		// Disposer
		public override void Dispose()
		{
			// Remove from arena
			RemoveAllLightnings();
			General.arena.RemoveActor(this);
			if(name != null) name.Destroy();
			if(state != null) state.Dispose();
			if(fireeffect != null) fireeffect.Dispose();
			if(rageeffect != null) rageeffect.Dispose();
			disposed = true;

			// Dispose base resources
			base.Dispose();
			GC.SuppressFinalize(this);
		}

		#endregion

		#region ================== Fire Effect

		// This makes sure the actor is on fire
		public void SetOnFire(int intensity)
		{
			// Create a fire effect if none yet
			if(fireeffect == null)
				fireeffect = new FireEffect(this);

			// Set intensity
			fireeffect.Intensity = intensity;
		}

		// This kills the fire
		public void KillFire()
		{
			// Set intensity
			if(fireeffect != null)
				if(fireeffect.Intensity > 1000) fireeffect.Intensity = 1000;
		}

		#endregion

		#region ================== Lightning Effect

		// This removes a lightning
		public void RemoveLightning(Lightning l)
		{
			if(lightnings.Contains(l)) lightnings.Remove(l);
		}

		// This adds a lightning
		public void AddLightning(Lightning l)
		{
			if(!lightnings.Contains(l)) lightnings.Add(l);
		}

		// This removes all lightnings
		public void RemoveAllLightnings()
		{
			// Are there any lightnings?
			if(lightnings.Count > 0)
			{
				// Dispose them all
				for(int i = lightnings.Count - 1; i >= 0; i--)
					lightnings[i].Dispose();
			}
		}

		#endregion

		#region ================== Animations

		// This loads all required animations
		public static void LoadAnimations(bool teams)
		{
			int b = 0, e = 0;

			// Load animations for teams?
			if(teams) { b = 1; e = 2; }

			// Go for all colors needed
			for(int i = b; i <= e; i++)
			{
				// Load animations
				Animation.Load(AniFilename(PFX_IDLE, PRT_TORSO, i));
				Animation.Load(AniFilename(PFX_WALKING, PRT_TORSO, i));
				Animation.Load(AniFilename(PFX_SHOOTING + "1", PRT_TORSO, i));
				Animation.Load(AniFilename(PFX_SHOOTING + "2", PRT_TORSO, i));
				Animation.Load(AniFilename(PFX_DEATH + "1", PRT_TORSO, i));
				Animation.Load(AniFilename(PFX_DEATH + "2", PRT_TORSO, i));
			}

			// Legs are not different for teams
			Animation.Load(AniFilename(PFX_IDLE, PRT_LEGS, 0));
			Animation.Load(AniFilename(PFX_WALKING + "1", PRT_LEGS, 0));
			Animation.Load(AniFilename(PFX_WALKING + "2", PRT_LEGS, 0));
			Animation.Load(AniFilename(PFX_WALKING + "3", PRT_LEGS, 0));
			Animation.Load(AniFilename(PFX_WALKING + "4", PRT_LEGS, 0));
			Animation.Load(AniFilename(PFX_WALKING + "5", PRT_LEGS, 0));
			Animation.Load(AniFilename(PFX_WALKING + "6", PRT_LEGS, 0));
			Animation.Load(AniFilename(PFX_WALKING + "7", PRT_LEGS, 0));
			Animation.Load(AniFilename(PFX_WALKING + "8", PRT_LEGS, 0));
		}

		// This changes the legs animation if needed
		public void UpdateLegsAnimation(bool forcerestart)
		{
			string anifile;
			bool preserveframeindex = false;
			int lastframeindex;

			// Dead?
			if(dead)
			{
				// No animation
				return;
			}
			// Walking?
			else if(walking)
			{
				// Determine walking direction relative to aimangle
				float wangle = walkangle - aimangle;
				wangle -= (float)Math.PI * 0.25f;

				// Make integral direction
				int walkdir = DirFromAngle(wangle, 1, 8);
				string walkdirstr = walkdir.ToString(CultureInfo.InvariantCulture);

				// Walking legs animation
				anifile = AniFilename(PFX_WALKING + walkdirstr, PRT_LEGS, teamcolor);
				//General.console.AddMessage(anifile);
				preserveframeindex = ani_legswalking;
				ani_legswalking = true;
			}
			// Stopped
			else
			{
				// Stopped legs
				anifile = AniFilename(PFX_IDLE, PRT_LEGS, teamcolor);
				ani_legswalking = false;
			}

			// Check if animation must be changed
			if((anifile != legs_ani.Filename) || forcerestart)
			{
				// Check if animation is loaded
				if(Animation.IsLoaded(anifile))
				{
					// Change animation now
					lastframeindex = legs_ani.CurrentFrameIndex;
					legs_ani = Animation.CreateFrom(anifile);
					if(preserveframeindex) legs_ani.CurrentFrameIndex = lastframeindex;
				}
				else
				{
					// Cannot play animation
					//General.console.AddMessage("Cannot play animation \"" + anifile + "\"");
				}
			}
		}

		// This changes the torso animation if needed
		public void UpdateTorsoAnimation(bool forcerestart)
		{
			string anifile;

			// Dead?
			if(dead)
			{
				// Do not change animation
				return;
			}
			// Walking?
			else if(walking)
			{
				// Do not interrupt a playing shooting animation
				if(ani_torsoshooting && !torso_ani.Ended) return;

				// Walking torso animation
				anifile = AniFilename(PFX_WALKING, PRT_TORSO, teamcolor);
				ani_torsowalking = true;
				ani_torsoshooting = false;
			}
			// Stopped
			else
			{
				// Do not interrupt a playing shooting animation
				if(ani_torsoshooting && !torso_ani.Ended) return;

				// Stopped torso
				anifile = AniFilename(PFX_IDLE, PRT_TORSO, teamcolor);
				ani_torsowalking = false;
				ani_torsoshooting = false;
			}

			// Check if animation must be changed
			if((anifile != torso_ani.Filename) || forcerestart)
			{
				// Check if animation is loaded
				if(Animation.IsLoaded(anifile))
				{
					// Change animation now
					torso_ani = Animation.CreateFrom(anifile);
				}
				else
				{
					// Cannot play animation
					//General.console.AddMessage("Cannot play animation \"" + anifile + "\"");
				}
			}
		}

		// This plays the shooting animation on the torso
		// A framerate of -1 will force the configured framerate
		public void PlayShootingAnimation(int kind, int framerate)
		{
			// Shooting animation
			string anifile = AniFilename(PFX_SHOOTING + kind.ToString(CultureInfo.InvariantCulture), PRT_TORSO, teamcolor);
			ani_torsoshooting = true;
			ani_torsowalking = false;

			// Check if animation must be changed
			if(anifile != torso_ani.Filename)
			{
				// Check if animation is loaded
				if(Animation.IsLoaded(anifile))
				{
					// Change animation now
					torso_ani = Animation.CreateFrom(anifile);

					// Set the shooting framerate
					if(framerate > 0) torso_ani.FrameTime = (int)(1000f / (float)framerate);
				}
			}
			else
			{
				// Restart animation
				torso_ani.CurrentFrameIndex = 0;

				// Set the shooting framerate
				if(framerate > 0) torso_ani.FrameTime = (int)(1000f / (float)framerate);

				// Force the framerate?
				if(framerate == -1)
				{
					// Force framerate
					torso_ani.FrameTime = torso_ani.OrigFrameTime;
					torso_ani.CurrentFrameIndex = torso_ani.CurrentFrameIndex;
				}
			}
		}

		#endregion

		#region ================== Physics

		// This drops the actor to the floor of highest sector immediately
		public void DropImmediately()
		{
			// Find highest sector
			FindHighestSector();

			// Drop to floor of highest sector
			state.pos.z = highestsector.CurrentFloor;
			state.vel.z = 0f;
		}

		// This applies gravity and takes care of stepping up stairs
		// Returns true when feet at aon the floor
		private bool PerformZChanges()
		{
			// Find highest sector
			FindHighestSector();

			// Check if on the floor
			onfloor = ((highestsector.CurrentFloor + Consts.FLOOR_TOUCH_TOLERANCE) > state.pos.z);

			// Should we fall down?
			if(highestsector.CurrentFloor < state.pos.z)
			{
				// Apply gravity
				state.vel.z -= Consts.GRAVITY;
			}
			else
			{
				// Were we falling down?
				if(state.vel.z < -1f)
				{
					// Play hit floor sound now
					if(!dead) PlayFallSound(FALL_STEP_INTERVAL);
				}

				// No gravity
				state.vel.z = 0f;

				// Stay above floor
				state.pos.z = highestsector.CurrentFloor;
			}

			// Return result
			return onfloor;
		}

		// This advances the actors position by velocity * timenudge
		public void AdvanceByTimenudge()
		{
			for(int i = 0; i < Client.timenudge; i++)
			{
				// Advance state
				state.ApplyVelocity(true, !dead, General.arena.Actors, this);
			}
		}

		#endregion

		#region ================== Control

		// This finds the highest sector
		public void FindHighestSector()
		{
			// Find touching sectors
            List<Sector> sectors = General.map.FindTouchingSectors(state.pos.x, state.pos.y, Consts.PLAYER_RADIUS);

			// Start with the current
			highestsector = sector;
			float highestz = sector.CurrentFloor;

			// Find the highest sector floor
            foreach(ClientSector s in sectors)
			{
				// Check if higher but not blocking
                if((s.CurrentFloor > highestz) &&
                   (s.CurrentFloor - Consts.MAX_STEP_HEIGHT <= state.pos.z))
				{
					// This height is higher and still valid
					highestz = s.CurrentFloor;
					highestsector = s;
				}
			}
		}

		// Move the actor instantly to a specific location
		public void Move(Vector3D v)
		{
			float framerate;

			// Find the new sector
			sector = (ClientSector)General.map.GetSubSectorAt(v.x, v.y).Sector;

			// Get positions on lightmap
			float lx = sector.VisualSector.LightmapScaledX(v.x);
			float ly = sector.VisualSector.LightmapScaledY(v.y);

			// Make the lightmap matrix
			lightmapoffsets = Matrix.Identity;
			lightmapoffsets *= Matrix.Scaling(TORSO_SCALE * sector.VisualSector.LightmapScaleX, 0f, 1f);
			lightmapoffsets *= Matrix.RotationZ(SPRITE_ANGLE_Z);
			lightmapoffsets *= Matrix.Scaling(1f, sector.VisualSector.LightmapAspect, 1f);
			lightmapoffsets *= Direct3D.MatrixTranslateTx(lx, ly);

			// Make dynamic lightmap matrix
			dynlightmapoffsets = Direct3D.MatrixTranslateTx(v.x, v.y);

			// Check if we can consider this "walking"
			//float dx = pos.x - v.x;
			//float dy = pos.y - v.y;
			//float walkspeed = (float)Math.Sqrt(dx * dx + dy * dy);
			float walkspeed = ((Vector2D)state.vel).Length();
			walking = (walkspeed > WALK_MIN_AMOUNT) && (walkspeed < WALK_MAX_AMOUNT);
			//walkangle = (float)Math.Atan2(dy, dx);
			walkangle = (float)Math.Atan2(-state.vel.y, -state.vel.x);

			// Update animations
			UpdateLegsAnimation(false);
			UpdateTorsoAnimation(false);

			// Walking and not dead?
			if(walking && !dead)
			{
				// Determine framerate for the walking animations
				framerate = (float)Math.Sqrt(walkspeed) * WALK_FRAMERATE_SCALE;
				legs_ani.FrameTime = (int)(1000f / framerate);
				if(ani_torsowalking) torso_ani.FrameTime = (int)(1000f / framerate);

				// Time to make stepping sound?
				if(stepsoundtime < SharedGeneral.currenttime)
				{
					// Play step sound when on the floor
					if(onfloor) PlayStepSound(WALK_STEP_INTERVAL);
				}
			}

			// Apply new coordinates
			pos = v;
			state.pos = v;
		}

		// Plays death animation and marks actor as dead
		public void Die(DEATHMETHOD method)
		{
			// Determine animation file
			int aninum = General.random.Next(2) + 1;
			string anifile = AniFilename(PFX_DEATH + aninum.ToString(CultureInfo.InvariantCulture), PRT_TORSO, teamcolor);

			// Check if animation is loaded
			if(Animation.IsLoaded(anifile))
			{
				// Play death animation
				torso_ani = Animation.CreateFrom(anifile);
			}

			// Now dead
			dead = true;
			state.Blocking = false;
			state.StepUp = false;
			shownuke = false;
			showrage = false;

			// No more fire
			KillFire();

			// No more rage
			if(rageeffect != null)
			{
				rageeffect.Dispose();
				rageeffect = null;
			}

			// Do not display name anymore
			name.Destroy();
			name = null;

			// Make normal death method when no gibbing preferred
			if(!showgibbing && (method == DEATHMETHOD.GIBBED)) method = DEATHMETHOD.NORMAL;

			// Normal death animation keeps the actor
			if(method == DEATHMETHOD.NORMAL)
			{
				// Spawn blood particles
				for(int p = 0; p < 12; p++)
					General.arena.p_blood.Add(state.pos + Vector3D.Random(General.random, 2f, 2f, 10f),
											state.vel * 0.5f * (float)General.random.NextDouble() + Vector3D.Random(General.random, 0.05f, 0.05f, 0.01f),
											General.ARGB(1f, 1f, 0.0f, 0.0f));

				// Spawn blood after time period
				bloodspawntime = SharedGeneral.currenttime + BLOOD_SPAWN_DELAY;

				// Create dissolve time
				dissolvetime = SharedGeneral.currenttime + Decal.decaltimeout;
			}
			// Gibbed death animation keeps the actor
			else if(method == DEATHMETHOD.GIBBED)
			{
				// On screen?
				if(sector.VisualSector.InScreen)
				{
					// Spawn blood particles
					for(int p = 0; p < 30; p++)
						General.arena.p_blood.Add(state.pos + Vector3D.Random(General.random, 2f, 2f, 10f),
												state.vel * 0.5f * (float)General.random.NextDouble() + Vector3D.Random(General.random, 0.05f, 0.05f, 0.2f),
												General.ARGB(1f, 1f, 0.0f, 0.0f));

					// Smokey blood
					for(int p = 0; p < 6; p++)
						General.arena.p_smoke.Add(state.pos + Vector3D.Random(General.random, 2f, 2f, 10f),
							state.vel * 0.03f * (float)General.random.NextDouble() + Vector3D.Random(General.random, 0.03f, 0.03f, -0.02f),
							General.ARGB(1f, 0.8f, 0.0f, 0.0f));

					// Spawn a leg or flesh
					if(General.random.Next(100) < 60)
					{
						new FleshDebris(state.pos + new Vector3D(2f, -2f, 3f),
								state.vel * (GIB_VEL_FACTOR + (float)General.random.NextDouble() * GIB_RND_FACTOR) + Vector3D.Random(General.random, 0.2f, 0.2f, 1f), 0);
					}
					else
					{
						new FleshDebris(state.pos + new Vector3D(2f, -2f, 3f),
								state.vel * (GIB_VEL_FACTOR + (float)General.random.NextDouble() * GIB_RND_FACTOR) + Vector3D.Random(General.random, 0.2f, 0.2f, 1f), FleshDebris.RandomFlesh());
					}

					// Spawn a leg or flesh
					if(General.random.Next(100) < 60)
					{
						new FleshDebris(state.pos + new Vector3D(-2f, 2f, 3f),
								state.vel * (GIB_VEL_FACTOR + (float)General.random.NextDouble() * GIB_RND_FACTOR) + Vector3D.Random(General.random, 0.2f, 0.2f, 1f), 0);
					}
					else
					{
						new FleshDebris(state.pos + new Vector3D(-2f, 2f, 3f),
								state.vel * (GIB_VEL_FACTOR + (float)General.random.NextDouble() * GIB_RND_FACTOR) + Vector3D.Random(General.random, 0.2f, 0.2f, 1f), FleshDebris.RandomFlesh());
					}

					// Spawn a arm or flesh
					if(General.random.Next(100) < 60)
					{
						new FleshDebris(state.pos + new Vector3D(2f, 0f, 5f),
								state.vel * (GIB_VEL_FACTOR + (float)General.random.NextDouble() * GIB_RND_FACTOR) + Vector3D.Random(General.random, 0.2f, 0.2f, 1f), 1);
					}
					else
					{
						new FleshDebris(state.pos + new Vector3D(2f, 0f, 5f),
								state.vel * (GIB_VEL_FACTOR + (float)General.random.NextDouble() * GIB_RND_FACTOR) + Vector3D.Random(General.random, 0.2f, 0.2f, 1f), FleshDebris.RandomFlesh());
					}

					// Spawn a arm or flesh
					if(General.random.Next(100) < 60)
					{
						new FleshDebris(state.pos + new Vector3D(0f, 2f, 5f),
								state.vel * (GIB_VEL_FACTOR + (float)General.random.NextDouble() * GIB_RND_FACTOR) + Vector3D.Random(General.random, 0.2f, 0.2f, 1f), 1);
					}
					else
					{
						new FleshDebris(state.pos + new Vector3D(0f, 2f, 5f),
								state.vel * (GIB_VEL_FACTOR + (float)General.random.NextDouble() * GIB_RND_FACTOR) + Vector3D.Random(General.random, 0.2f, 0.2f, 1f), FleshDebris.RandomFlesh());
					}

					// Spawn a head or flesh
					if(General.random.Next(100) < 60)
					{
						new FleshDebris(state.pos + new Vector3D(0f, 0f, 8f),
								state.vel * (GIB_VEL_FACTOR + (float)General.random.NextDouble() * GIB_RND_FACTOR) + Vector3D.Random(General.random, 0.2f, 0.2f, 1f), 2);
					}
					else
					{
						new FleshDebris(state.pos + new Vector3D(0f, 0f, 8f),
								state.vel * (GIB_VEL_FACTOR + (float)General.random.NextDouble() * GIB_RND_FACTOR) + Vector3D.Random(General.random, 0.2f, 0.2f, 1f), FleshDebris.RandomFlesh());
					}

					// Spawn flesh
					new FleshDebris(state.pos + new Vector3D(0f, 2f, 3f),
							state.vel * (GIB_VEL_FACTOR + (float)General.random.NextDouble() * GIB_RND_FACTOR) + Vector3D.Random(General.random, 0.2f, 0.2f, 1f), FleshDebris.RandomFlesh());

					// Spawn flesh
					new FleshDebris(state.pos + new Vector3D(-2f, 0f, 5f),
							state.vel * (GIB_VEL_FACTOR + (float)General.random.NextDouble() * GIB_RND_FACTOR) + Vector3D.Random(General.random, 0.2f, 0.2f, 1f), FleshDebris.RandomFlesh());

					// Spawn flesh
					new FleshDebris(state.pos + new Vector3D(0f, -2f, 5f),
							state.vel * (GIB_VEL_FACTOR + (float)General.random.NextDouble() * GIB_RND_FACTOR) + Vector3D.Random(General.random, 0.2f, 0.2f, 1f), FleshDebris.RandomFlesh());

					// Spawn flesh
					new FleshDebris(state.pos + new Vector3D(2f, 2f, 8f),
							state.vel * (GIB_VEL_FACTOR + (float)General.random.NextDouble() * GIB_RND_FACTOR) + Vector3D.Random(General.random, 0.2f, 0.2f, 1f), FleshDebris.RandomFlesh());

					// Spawn flesh
					new FleshDebris(state.pos + new Vector3D(-2f, -2f, 8f),
							state.vel * (GIB_VEL_FACTOR + (float)General.random.NextDouble() * GIB_RND_FACTOR) + Vector3D.Random(General.random, 0.2f, 0.2f, 1f), FleshDebris.RandomFlesh());
				}
				else
				{
					// Just spawn blood here
					if((highestsector != null) && (highestsector.Material != (int)SECTORMATERIAL.LIQUID))
						FloorDecal.Spawn(highestsector, pos.x, pos.y, FloorDecal.blooddecals, false, true, false);
				}

				// We don't have a torso animation, dispose the actor
				this.Dispose();
			}
			else
			{
				// Other methods dispose the actor
				this.Dispose();
			}
		}

		#endregion

		#region ================== Methods

		// This makes the filename for an animation
		public static string AniFilename(string aniprefix, string partprefix, int color)
		{
			// Torso?
			if(partprefix == PRT_TORSO)
			{
				// Return filename for this frame
				return "sprites/" + aniprefix + PFX_SEPERATOR + partprefix +
					PFX_SEPERATOR + color.ToString("0", CultureInfo.InvariantCulture) + ".cfg";
			}
			else
			{
				// Return filename for this frame
				return "sprites/" + aniprefix + PFX_SEPERATOR + partprefix + ".cfg";
			}
		}

		// This makes a texture matrix for a given direction number
		private Matrix DirectionCellMatrix(int dirnumber)
		{
			Matrix cell;

			// Determine cell x and y
			float cellx = dirnumber % 4;
			float celly = dirnumber / 4;

			// Make the matrix for the cell
			cell = Matrix.Identity;
			cell *= Matrix.Scaling(0.25f, 0.25f, 1f);
			cell *= Direct3D.MatrixTranslateTx(cellx * 0.25f, celly * 0.25f);

			// Return result
			return cell;
		}

		// This calculates the direction number from an angle
		public static int DirFromAngle(float angle, int offset, int numdirs)
		{
			// Offset angle (we're viewing at 45 deg)
			angle -= (float)Math.PI * 0.25f;

			// Adjust angle (cant work with negative values)
			while(angle < 0f) angle += (float)Math.PI * 2f;

			// Scale angle 0-1
			angle /= ((float)Math.PI * 2f);

			// Scale angle 0-numdirs
			angle *= (float)numdirs;

			// Return integral direction
			return offset + ((int)Math.Round(angle, 0) % numdirs);
		}

		// This calculates the angle from a direction number
		public static float AngleFromDir(int dir, int offset, int numdirs)
		{
			// Scale to 0-1
			float angle = (float)(dir - offset) / (float)numdirs;

			// Scale to PI*2
			angle *= (float)Math.PI * 2;

			// Offset angle (we're viewing at 45 deg)
			angle += (float)Math.PI * 0.25f;

			// Return angle
			return angle;
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
					if(this.IsOnFloor && (crossline == crossline.Linedef.Front))
						teleportlock = SharedGeneral.currenttime + Client.TELEPORT_DELAY;
					break;
			}
		}

		// This plays a step sound
		private void PlayStepSound(int nextsounddelay)
		{
			// In screen?
			if(sector.VisualSector.InScreen)
			{
				// Make a random step sound on this sectors material
				int stepsnd = General.random.Next(WALK_STEP_SOUNDS);
				string sound = "step" + stepsnd.ToString(CultureInfo.InvariantCulture) + "mat" + highestsector.Material.ToString(CultureInfo.InvariantCulture) + ".wav";
				DirectSound.PlaySound(sound, state.pos, 0.3f);

				// Update step time
				stepsoundtime = SharedGeneral.currenttime + nextsounddelay;
			}
		}

		// This plays a fall sound
		private void PlayFallSound(int nextsounddelay)
		{
			// In screen?
			if(sector.VisualSector.InScreen)
			{
				// Make a random step sound on this sectors material
				string sound = "fallmat" + highestsector.Material.ToString(CultureInfo.InvariantCulture) + ".wav";
                DirectSound.PlaySound(sound, state.pos, 0.5f);

				// Update step time
				stepsoundtime = SharedGeneral.currenttime + nextsounddelay;
			}
		}

		#endregion

		#region ================== Processing

		// This processes movements
		public void ProcessMovement()
		{
			Sidedef crossline;

			// Make changes to Z velocity
			onfloor = PerformZChanges();

			// Decelerate
			if(this.IsLocal || this.IsDead)
			{
				// Dead or alive?
				if(this.IsDead)
				{
					// Low deceleration
					state.vel /= 1f + Consts.DEAD_DECELERATION;
				}
				else
				{
					// Normal deceleration
					if(onfloor)
						state.vel /= 1f + Consts.WALK_DECELERATION;
					else
						state.vel /= 1f + Consts.AIR_DECELERATION;
				}

				// Decelerate push vector
				pushvec /= 1f + Consts.PUSH_DECELERATION;
			}
			// For remote players
			else if(!this.IsLocal)
			{
				// Decelerate only Z
				if(onfloor)
					state.vel.z /= 1f + Consts.WALK_DECELERATION;
				else
					state.vel.z /= 1f + Consts.AIR_DECELERATION;
			}

			// Apply velocity to position over time and check for collisions
			state.ApplyVelocity(this.IsLocal || dead, this.IsLocal && !dead,
								General.arena.Actors, this, out crossline);

			// Apply effect of crossing lines
			if((crossline != null) && this.IsLocal) ApplyCrossLineEffect(crossline);
		}

		// Process the actor
		public override void Process()
		{
			// Leave when disposed
			if(disposed) return;

			// Process movements
			ProcessMovement();

			// Destroy when in a bottomless sector
			if((sector == HighestSector) && !sector.HasFloor && onfloor)
			{
				// Cannot be on the bottom of a bottomless pit
				this.Dispose();
				return;
			}

			// Apply new position to the actor
			Move(state.pos);

			// Process animations
			legs_ani.Process();
			torso_ani.Process();

			// Process fire effect
			if(fireeffect != null)
			{
				// Process and dispose when intensity is 0
				fireeffect.Intensity -= Consts.TIMESTEP;
				fireeffect.Process();

				// No more fire?
				if(fireeffect.Intensity <= 0)
				{
					// Dispose
					fireeffect.Dispose();
					fireeffect = null;
				}
			}

			// Process lightnings
			foreach(Lightning l in lightnings) l.Process();

			// Create/Destroy Rage
			if(showrage && (rageeffect == null)) rageeffect = new RageEffect(this);
			if(!showrage && (rageeffect != null))
			{
				rageeffect.Dispose();
				rageeffect = null;
			}

			// Set correct rage color
			if(rageeffect != null) rageeffect.LightColor = ragecolor;

			// When dead and torso animation ended
			if(dead && ((torso_ani.CurrentFrameIndex > 26) || torso_ani.Ended))
			{
				// Now dead on the floor, reduce bias
				renderbias = -20f;
				renderpass = 0;
				deadthreshold = true;

				// No more lightning
				RemoveAllLightnings();
			}

			// Dead and time to spawn blood?
			if(dead && (bloodspawntime > 0) && (bloodspawntime < SharedGeneral.currenttime))
			{
				// Spawn floor blood here
				FloorDecal.Spawn(highestsector, pos.x, pos.y, FloorDecal.blooddecals, false, true, false);
				bloodspawntime = 0;
			}

			// Dead and dissolving?
			if(dead && (SharedGeneral.currenttime > dissolvetime))
			{
				// Completely faded away?
				if((SharedGeneral.currenttime - dissolvetime) > DISSOLVE_SPEED)
				{
					// Destroy this decal
					this.Dispose();
					return;
				}
				else
				{
					// Calculate fade
					alpha = 1f - (float)(SharedGeneral.currenttime - dissolvetime) / (float)DISSOLVE_SPEED;
				}
			}
		}

		#endregion

		#region ================== Rendering

		// This renders the shadow
		public override void RenderShadow()
		{
			// Check if in screen
			if(sector.VisualSector.InScreen && !disposed)
			{
				// Highest sector known?
				if(highestsector != null)
				{
					// Render the shadow
					Shadow.RenderAt(pos.x, pos.y, highestsector.CurrentFloor, SHADOW_SIZE,
						Shadow.AlphaAtHeight(highestsector.CurrentFloor, pos.z) * SHADOW_ALPHA_MUL * alpha);
				}
			}
		}

		// This renders the name
		public void RenderName()
		{
			// Name to show?
			if((name != null) && !disposed)
			{
				// Determine text coordinates
				Vector3 apos = General.arena.Projected(new Vector3(pos.x + NAME_OFFSET, pos.y - NAME_OFFSET, pos.z));
				name.Viewport = new RectangleF(apos.X / Direct3D.DisplayWidth, apos.Y / Direct3D.DisplayHeight, 0f, 0f);

				// Render the actor name
				name.Render();
			}
		}

		// This renders the actor
		public override void Render()
		{
			Matrix apos;

			// Set the lightmap from visual sector
			Direct3D.d3dd.SetTexture(1, sector.VisualSector.Lightmap);

			// Apply lightmap matrix
			Direct3D.d3dd.SetTransform(TransformState.Texture1, lightmapoffsets);
			Direct3D.d3dd.SetTransform(TransformState.Texture2, dynlightmapoffsets * General.arena.LightmapMatrix);

			// Check if in screen
			if(sector.VisualSector.InScreen && !disposed)
			{
				// Render nuke sign?
				if(shownuke)
				{
					// Set render mode
					Direct3D.SetDrawMode(DRAWMODE.NADDITIVEALPHA);
					Direct3D.d3dd.SetRenderState(RenderState.ZWriteEnable, false);
					Direct3D.d3dd.SetRenderState(RenderState.TextureFactor, General.ARGB(0.5f, 1f, 1f, 1f));

					// Set matrices
					Direct3D.d3dd.SetTransform(TransformState.Texture0, Matrix.Identity);

					// Set vertices stream
					Direct3D.d3dd.SetStreamSource(0, Shadow.vertices, 0, MVertex.Stride);

					// Set the nuke texture
					Direct3D.d3dd.SetTexture(0, NukeSign.texture.texture);

					// Render nuke sign
					NukeSign.RenderAt(pos.x, pos.y, pos.z);
				}

				// Render ring sign?
				if(showring)
				{
					// Set render mode
					Direct3D.SetDrawMode(DRAWMODE.NADDITIVEALPHA);
					Direct3D.d3dd.SetRenderState(RenderState.ZWriteEnable, false);
					Direct3D.d3dd.SetRenderState(RenderState.TextureFactor, General.TeamColor(team, 0.8f));

					// Set matrices
					Direct3D.d3dd.SetTransform(TransformState.Texture0, Matrix.Identity);

					// Set vertices stream
					Direct3D.d3dd.SetStreamSource(0, Shadow.vertices, 0, MVertex.Stride);

					// Set the ring texture
					Direct3D.d3dd.SetTexture(0, RingSign.texture.texture);

					// Render ring sign
					RingSign.RenderAt(pos.x, pos.y, pos.z);
				}

				// Set render mode
				Direct3D.SetDrawMode(DRAWMODE.NLIGHTMAPALPHA);
				Direct3D.d3dd.SetRenderState(RenderState.ZWriteEnable, false);
				Direct3D.d3dd.SetRenderState(RenderState.TextureFactor, General.ARGB(alpha, 1f, 1f, 1f));

				// Set vertices stream
				Direct3D.d3dd.SetStreamSource(0, Sprite.Vertices, 0, MVertex.Stride);

				// Legs are only shown when not dead
				if(!dead)
				{
					// Position matrix for the legs
					apos = Matrix.Translation(pos.x + LEGS_OFFSET_X, pos.y + LEGS_OFFSET_Y, pos.z + LEGS_OFFSET_Z);
					Direct3D.d3dd.SetTransform(TransformState.World, Matrix.Multiply(legs_scalerotate, apos));
					Direct3D.d3dd.SetTransform(TransformState.Texture0, DirectionCellMatrix(DirFromAngle(aimangle, 0, 16)));

					// Render with the legs animation frame
					Direct3D.d3dd.SetTexture(0, legs_ani.CurrentFrame.texture);
					Direct3D.d3dd.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
				}

				// Different torso Z when dead
				if(!dead)
				{
					// Position matrix for the torso
					apos = Matrix.Translation(pos.x + TORSO_OFFSET_X, pos.y + TORSO_OFFSET_Y, pos.z + TORSO_OFFSET_Z);
				}
				else
				{
					// Position matrix for the torso
					apos = Matrix.Translation(pos.x + TORSO_DEAD_OFFSET_X, pos.y + TORSO_DEAD_OFFSET_Y, pos.z + TORSO_DEAD_OFFSET_Z);
				}

				// Apply matrices
				Direct3D.d3dd.SetTransform(TransformState.World, Matrix.Multiply(torso_scalerotate, apos));
				Direct3D.d3dd.SetTransform(TransformState.Texture0, DirectionCellMatrix(DirFromAngle(aimangle, 0, 16)));

				// Render with the body animation frame
				Direct3D.d3dd.SetTexture(0, torso_ani.CurrentFrame.texture);
				Direct3D.d3dd.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);

				// Reset texture matrix
				Direct3D.d3dd.SetTransform(TransformState.Texture0, Matrix.Identity);

				// Render collision info
				if((state.showcol != null) && Client.showcollisions) ((IClientCollision)state.showcol).Render();
			}
		}

		#endregion
	}
}

