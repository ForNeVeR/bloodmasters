/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using System.Reflection;
using CodeImp.Bloodmasters.Client.Effects;
using CodeImp.Bloodmasters.Client.Graphics;

namespace CodeImp.Bloodmasters.Client.Projectiles;

public abstract class Projectile : VisualObject
{
    #region ================== Constants

    private const int MAX_PROJECTILES = 20;

    #endregion

    #region ================== Variables

    // Static
    private static Type[] projectiletype;

    // Members
    protected PhysicsState state;
    private readonly PROJECTILE type;
    private readonly string id;
    private bool disposed = false;
    private int source;
    private TEAM team;
    private bool inscreen;

    #endregion

    #region ================== Properties

    public string ID { get { return id; } }
    public bool Disposed { get { return disposed; } }
    public Vector3D Position { get { return state.pos; } }
    public Vector3D Velocity { get { return state.vel; } }
    public int SourceID { get { return source; } set { source = value; } }
    public TEAM Team { get { return team; } set { team = value; } }
    public bool InScreen { get { return inscreen; } }

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public Projectile(string id, Vector3D start, Vector3D vel)
    {
        // Copy ID
        this.id = id;

        // Check if class has a ProjectileInfo attribute
        if(Attribute.IsDefined(this.GetType(), typeof(ProjectileInfo), false))
        {
            // Get ProjectileInfo attributes
            ProjectileInfo attr = (ProjectileInfo)Attribute.GetCustomAttribute(this.GetType(), typeof(ProjectileInfo), false);

            // Copy settings from attribute
            this.type = attr.Type;
        }

        // Copy properties
        state = new ClientPhysicsState(General.map);
        state.Bounce = false;
        state.IsPlayer = false;
        state.Blocking = false;
        state.Radius = 1f;
        state.Friction = 0f;
        state.Redirect = false;
        state.StepUp = false;
        state.pos = start;
        state.vel = vel;
    }

    // Dispose
    public override void Dispose()
    {
        // Clean up
        state.Dispose();

        // Dispose base
        base.Dispose();
        disposed = true;
        GC.SuppressFinalize(this);
    }

    #endregion

    #region ================== Static Methods

    // This initializes the projectiles array
    public static void Initialize()
    {
        // Create the array
        projectiletype = new Type[MAX_PROJECTILES];

        // Go for all types in this assembly
        Assembly asm = Assembly.GetExecutingAssembly();
        Type[] asmtypes = asm.GetTypes();
        foreach(Type tp in asmtypes)
        {
            // Check if this type is a class
            if (!tp.IsClass || tp.IsAbstract || tp.IsArray)
                continue;

            // Check if class has a ProjectileInfo attribute
            if (!Attribute.IsDefined(tp, typeof(ProjectileInfo), false))
                continue;

            // Get ProjectileInfo attribute
            ProjectileInfo attr = (ProjectileInfo)Attribute.GetCustomAttribute(tp, typeof(ProjectileInfo), false);

            // Add projectile type to array
            projectiletype[(int)attr.Type] = tp;
        }
    }

    // This returns a projectile type for the given ID
    public static Type GetProjectileType(PROJECTILE projectileid)
    {
        // Return type
        return projectiletype[(int)projectileid];
    }

    #endregion

    #region ================== Methods

    // This is called when an update is received
    public virtual void Update(Vector3D newpos, Vector3D newvel)
    {
        // Apply position and velocity
        state.pos = newpos;
        state.vel = newvel;
        pos = state.pos;
    }

    // This is called when the projectile is being teleported
    public virtual void TeleportTo(Vector3D oldpos, Vector3D newpos, Vector3D newvel)
    {
        // Spawn teleport effects
        new TeleportEffect(oldpos, TEAM.NONE, true);
        new TeleportEffect(newpos, TEAM.NONE, true);

        // Play teleport sound at both locations
        SoundSystem.PlaySound("teleportsmall.wav", oldpos);
        SoundSystem.PlaySound("teleportsmall.wav", newpos);

        // Apply position and velocity
        state.pos = newpos;
        state.vel = newvel;
        pos = state.pos;
    }

    // This is called when destroyed
    public virtual void Destroy(Vector3D atpos, bool silent, Client hitplayer)
    {
        // Remove me
        General.arena.RemoveProjectile(this);
        this.Dispose();
    }

    // Processes the projectile
    public override void Process()
    {
        // Apply velocity without collision checking
        state.ApplyVelocity(false);

        // Check if the position of this projectile lies within the screen
        inscreen = ((state.pos.x > General.arena.ScreenArea.Left) &&
                    (state.pos.x < General.arena.ScreenArea.Right) &&
                    (state.pos.y > General.arena.ScreenArea.Top) &&
                    (state.pos.y < General.arena.ScreenArea.Bottom));

        // Position this object
        pos = state.pos;
    }

    #endregion
}
