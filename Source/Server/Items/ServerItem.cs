/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

namespace Bloodmasters.Server;

[AttributeUsage(AttributeTargets.Class, AllowMultiple=true, Inherited=false)]
public class ServerItem : EntityAttribute
{
    // Members
    private int thingid;
    private int respawntime = 3000;
    private bool temporary = false;
    private bool onfloor = true;

    // Properties
    public int ThingID { get { return thingid; } }
    public int RespawnTime { get { return respawntime; } set { respawntime = value; } }
    public bool Temporary { get { return temporary; } set { temporary = value; } }
    public bool OnFloor { get { return onfloor; } set { onfloor = value; } }

    // Constructor
    public ServerItem(int thingid)
    {
        // Keep the thing ID
        this.thingid = thingid;
    }
}
