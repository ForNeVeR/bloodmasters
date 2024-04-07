/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using Bloodmasters.LevelMap;

namespace Bloodmasters.Server.Items;

public abstract class Item
{
    #region ================== Constants

    public const float ITEM_RADIUS = 0.5f;

    #endregion

    #region ================== Variables

    // References
    private Sector sector = null;

    // Identification
    private string key;
    public static int uniquekeyindex = 0;

    // Other properties
    private bool temporary = false;
    protected float radius = ITEM_RADIUS;
    private bool onfloor = true;

    // Respawn
    private bool taken = false;
    private int respawntime;
    private int respawndelay = 1000;
    private bool willrespawn = true;

    // Attach
    private bool attached = false;
    private Client owner = null;

    // Coordinates
    private Vector3D pos;

    #endregion

    #region ================== Properties

    public string Key { get { return key; } }
    public bool IsTaken { get { return taken; } }
    public bool IsAttached { get { return attached; } }
    public Sector Sector { get { return sector; } }
    public bool Temporary { get { return temporary; } set { temporary = value; } }
    public Vector3D Position { get { return pos; } }
    public float Radius { get { return radius; } }
    public Client Owner { get { return owner; } }

    public int RespawnDelay
    {
        get
        {
            if(willrespawn)
                return respawntime - SharedGeneral.currenttime;
            else
                return Consts.NEVER_RESPAWN_TIME;
        }

        set
        {
            willrespawn = (value > 0);
            respawntime = SharedGeneral.currenttime + value;
        }
    }

    #endregion

    #region ================== Constructor / Destructor

    // Constructor from Thing
    public Item(Thing t)
    {
        // Create key from thing index
        key = "T" + t.Index;

        // From thing
        Initialize(t.X, t.Y, t.Sector.HeightFloor + t.Z);
    }

    // Constructor
    public Item(float ix, float iy, float iz)
    {
        // Create a unique key
        key = "U" + uniquekeyindex++;

        // From coordinates
        Initialize(ix, iy, iz);
    }

    // This initializes the item
    private void Initialize(float ix, float iy, float iz)
    {
        // Check if class has a ServerItem attribute
        if(Attribute.IsDefined(this.GetType(), typeof(ServerItem), false))
        {
            // Get item attribute
            ServerItem attr = (ServerItem)Attribute.GetCustomAttribute(this.GetType(), typeof(ServerItem), false);

            // Copy settings from attribute
            respawndelay = attr.RespawnTime;
            temporary = attr.Temporary;
            onfloor = attr.OnFloor;
        }

        // Move into position
        this.Move(ix, iy, iz);
    }

    // Dispose
    public void Dispose()
    {
        // Clean up
        sector = null;
    }

    #endregion

    #region ================== Control

    // This attaches the item to a client
    public virtual void Attach(Client c)
    {
        // Attach item to client
        attached = true;
        owner = c;
        owner.Carrying = this;

        // Broadcast item pickup signal
        Host.Instance.Server.BroadcastItemPickup(c, this, true);
    }

    // This detaches the item from a client
    public virtual void Detach()
    {
        // Detach item from client
        if(owner != null) owner.Carrying = null;
        attached = false;
        owner = null;
    }

    // Use only this function to move the item
    public void Move(float nx, float ny, float nz)
    {
        // Find the new sector
        Sector newsec = Host.Instance.Server.map.GetSubSectorAt(nx, ny).Sector;
        if(newsec != sector)
        {
            // Sector changes!
            if(sector != null) sector.RemoveItem(this);
            newsec.AddItem(this);
            sector = newsec;
        }

        // Apply new coordinates
        pos = new Vector3D(nx, ny, nz);
    }

    // This takes the item away and schedules it for respawn
    public virtual void Take(Client c)
    {
        // Detach is attached
        if(this.IsAttached) this.Detach();

        // Take item and set new respawn time
        taken = true;
        respawntime = SharedGeneral.currenttime + respawndelay;
        willrespawn = (respawndelay > 0);
        owner = c;

        // Broadcast item pickup signal
        Host.Instance.Server.BroadcastItemPickup(c, this, false);
    }

    // This respawns an item
    public virtual void Respawn()
    {
        // No longer taken
        taken = false;
        owner = null;
    }

    // This is called when the item is being picked up
    public virtual void Pickup(Client c)
    {
        // To be implemented by items that can be picked up
        // Implementation must call Take() or Attach() to take the item!
    }

    // Do I still have to explain what this is for?
    public virtual void Process()
    {
        // Time to respawn?
        if(taken && willrespawn && (respawntime < SharedGeneral.currenttime)) Respawn();

        // Drop to floor?
        if(onfloor) pos.z = sector.CurrentFloor;
    }

    #endregion
}
