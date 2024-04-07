/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;

namespace Bloodmasters.Client.Items;

[AttributeUsage(AttributeTargets.Class, AllowMultiple=true, Inherited=false)]
public class ClientItem : EntityAttribute
{
    // Members
    private readonly int thingid;
    private string defaultsprite = "";
    private string description = "";
    private bool visible = true;
    private bool bob = false;
    private bool temporary = false;
    private string sound = "";
    private bool onfloor = true;
    private float spriteoffset = 0f;

    // Properties
    public int ThingID { get { return thingid; } }
    public string Sprite { get { return defaultsprite; } set { defaultsprite = value; } }
    public string Description { get { return description; } set { description = value; } }
    public bool Visible { get { return visible; } set { visible = value; } }
    public bool Bob { get { return bob; } set { bob = value; } }
    public bool Temporary { get { return temporary; } set { temporary = value; } }
    public bool OnFloor { get { return onfloor; } set { onfloor = value; } }
    public string Sound { get { return sound; } set { sound = value; } }
    public float SpriteOffset { get { return spriteoffset; } set { spriteoffset = value; } }

    // Constructor
    public ClientItem(int thingid)
    {
        // Keep the thing ID
        this.thingid = thingid;
    }
}
