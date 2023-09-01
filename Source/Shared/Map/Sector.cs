/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System.Drawing;

namespace CodeImp.Bloodmasters;

public abstract class Sector
{
    #region ================== Constants

    public const string NO_FLAT = "f_sky1";
    public const int UPDATE_LIGHTMAP_INTERVAL = 40;
    public const string SOUND_START = "platform_s.wav";
    public const string SOUND_RUN = "platform_r.wav";
    public const string SOUND_END = "platform_e.wav";

    #endregion

    #region ================== Variables

    private int index;
    private float light;
    private int color;
    private SECTOREFFECT effect;
    private int tag;
    private string tfloor;
    private string tceil;
    private bool dynamic;
    protected Map map;

    // Heights as read from the WAD file
    private float hfloor;
    private float hceil;

    // Fake height for ceilings
    private float hfceil = float.NaN;

    // Dynamic floor heights
    private float curfloor;
    private float lowestfloor;
    private float highestfloor;

    // Movement
    private float targetfloor;
    private float changepersec;		// Amount of change to apply per millisecond

    // Boundaries
    protected RectangleF bounds;
    private float boundscalex, boundscaley;

    // Material
    private int material;
    private LIQUID liquidtype;
    private float liquidheight;

    // Subsectors
    private SubSector[] subsectors;

    // Adjacent sectors
    protected Sector[] adjsectors;

    // Items
    private List<object> items = new();

    #endregion

    #region ================== Properties

    // Properties
    public int Index { get { return index; } }
    public SECTOREFFECT Effect { get { return effect; } }
    public int Tag { get { return tag; } }
    public float Light { get { return light; } }
    public int Color { get { return color; } }
    public bool Dynamic { get { return dynamic; } }
    public bool HasCeiling { get { return tceil != Sector.NO_FLAT; } }
    public bool HasFloor { get { return tfloor != Sector.NO_FLAT; } }

    // Heights as read from the WAD file
    public float HeightFloor { get { return hfloor; } }
    public float HeightCeil { get { return hceil; } }

    // Fake height for ceilings
    public float FakeHeightCeil { get { return hfceil; } }

    // Dynamic floor heights
    public float CurrentFloor { get { return curfloor; } }
    public float LowestFloor { get { return lowestfloor; } }
    public float HighestFloor { get { return highestfloor; } }

    // Movement
    public float TargetFloor { get { return targetfloor; } }
    public float ChangeSpeed { get { return changepersec; } }

    // Ceiling and floor texture names
    public string TextureFloor { get { return tfloor; } }
    public string TextureCeil { get { return tceil; } }

    // Boundaries
    public RectangleF Bounds { get { return bounds; } }
    public float BoundsScaleX { get { return boundscalex; } }
    public float BoundsScaleY { get { return boundscaley; } }
    public float X { get { return bounds.X; } }
    public float Y { get { return bounds.Y; } }
    public float Width { get { return bounds.Width; } }
    public float Height { get { return bounds.Height; } }
    public float Top { get { return bounds.Top; } }
    public float Bottom { get { return bounds.Bottom; } }

    // Material
    public int Material { get { if((liquidtype != LIQUID.NONE) && (curfloor <= liquidheight)) return (int)SECTORMATERIAL.LIQUID; else return material; } }
    public LIQUID LiquidType { get { return liquidtype; } set { liquidtype = value; } }
    public float LiquidHeight { get { return liquidheight; } set { liquidheight = value; } }

    // Subsectors
    public SubSector[] Subsectors { get { return subsectors; } }

    // Adjacent sectors
    public Sector[] AdjacentSectors { get { return adjsectors; } }

    // Items
    public List<object> Items { get { return items; } }

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public Sector(BinaryReader data, int index, Map map)
    {
        // Keep references
        this.index = index;
        this.map = map;

        // Read sector
        hfloor = (float)data.ReadInt16() * Map.MAP_SCALE_Z;
        hceil = (float)data.ReadInt16() * Map.MAP_SCALE_Z;
        tfloor = Wad.BytesToString(data.ReadBytes(8)).ToLower();
        tceil = Wad.BytesToString(data.ReadBytes(8)).ToLower();
        light = ((float)data.ReadInt16()) / 255f;
        effect = (SECTOREFFECT)data.ReadInt16();
        tag = data.ReadInt16();

        // Color from light level
        color = System.Drawing.Color.FromArgb(255, (int)(255f * light), (int)(255f * light), (int)(255f * light)).ToArgb();

        // Dynamic heights
        curfloor = hfloor;
        targetfloor = hfloor;
        lowestfloor = hfloor;
        highestfloor = hfloor;

        // Check if dynamic
        dynamic = (effect == SECTOREFFECT.DOOR) ||
                  (effect == SECTOREFFECT.PLATFORMHIGH) ||
                  (effect == SECTOREFFECT.PLATFORMLOW) ||
                  (effect == SECTOREFFECT.TECHDOORFAST) ||
                  (effect == SECTOREFFECT.TECHDOORSLOW);

        // No subsectors yet, they will be added later
        subsectors = new SubSector[0];
    }

    // Destructor
    public virtual void Dispose()
    {
        // Release references
        adjsectors = null;
        subsectors = null;
        map = null;
    }

    #endregion

    #region ================== Methods

    // This sets the surface material
    public void SetSurfaceMaterial(int mat)
    {
        // Set material
        this.material = mat;
    }

    // This makes the sector move
    public void MoveTo(float height, float speed)
    {
        // Set the movement settings
        targetfloor = height;

        // Move instantly?
        if(speed == 0f)
        {
            // Move instantly
            curfloor = targetfloor;

            // Update lighting when on client side
            UpdateLightmaps();
        }
        else
        {
            UpdateClientSounds();

            // Move with given speed
            if(height > curfloor) changepersec = Math.Abs(speed);
            else changepersec = -Math.Abs(speed);
        }
    }

    protected abstract void UpdateClientSounds();

    // This tests if the given X/Y coordinate is within the sector
    public bool IntersectXY(float x, float y)
    {
        // TODO[#114]: Use point-in-polygon algorithm for this?
        // See http://astronomy.swin.edu.au/~pbourke/geometry/insidepoly/

        // Check if the given coordinates are
        // in this sector and return result.
        return (map.GetSubSectorAt(x, y).Sector == this);
    }

    // This makes a reference to a subsector
    public void AddSubSectorRef(SubSector ss)
    {
        // First subsector?
        if(subsectors.Length == 0)
        {
            // Take bounds from subsector
            bounds = ss.Bounds;
        }
        else
        {
            // Extend boundary with subsector
            bounds = RectangleF.Union(bounds, ss.Bounds);
        }

        // Adjust bounds scalars
        boundscalex = 1f / bounds.Width;
        boundscaley = 1f / bounds.Height;

        // Add the subsector
        SubSector[] newss = new SubSector[subsectors.Length + 1];
        subsectors.CopyTo(newss, 0);
        newss[subsectors.Length] = ss;
        subsectors = newss;
    }

    // This finds all adjacent sectors
    public void FindAdjacentSectors()
    {
        List<Sector> adjs = new List<Sector>();

        // Go for all subsectors
        foreach(SubSector ss in subsectors)
        {
            // Go for all segs in subsector
            foreach(Segment sg in ss.Segments)
            {
                // Segment on a line?
                if(sg.Sidedef != null)
                {
                    // Get sidedef on other side
                    Sidedef os = sg.Sidedef.OtherSide;

                    // Other side found?
                    if(os != null) if(!adjs.Contains(os.Sector)) adjs.Add(os.Sector);
                }
            }
        }

        // Make the array
        adjsectors = adjs.ToArray();
    }

    // This finds the lowest adjacent floor
    // Returns true when changes were made
    public bool FindLowestAdjFloor()
    {
        // Go for all adjacent sectors
        foreach(Sector s in adjsectors)
        {
            // Check if sector floor is lower
            if(s.lowestfloor < this.lowestfloor)
            {
                // Copy lowest floor height
                this.lowestfloor = s.lowestfloor;
                return true;
            }
        }

        // Nothing done
        return false;
    }

    // This finds the highest adjacent floor
    // Returns true when changes were made
    public bool FindHighestAdjFloor()
    {
        // Go for all adjacent sectors
        foreach(Sector s in adjsectors)
        {
            // Check if sector floor is higher
            if(s.highestfloor > this.highestfloor)
            {
                // Copy highest floor height
                this.highestfloor = s.highestfloor;
                return true;
            }
        }

        // Nothing done
        return false;
    }

    // This makes a fake ceiling height for this sector
    // Returns true when the fake ceiling was made
    public bool CreateFakeCeiling()
    {
        // Leave when this sector doesnt need a fake ceiling
        if((float.IsNaN(hfceil) == false) || (tceil == Sector.NO_FLAT)) return false;

        // Go for all adjacent sectors
        foreach(Sector s in adjsectors)
        {
            // Check if sector has a height to copy for fake ceiling
            if(float.IsNaN(s.FakeHeightCeil) == false)
            {
                // Copy sector fake height
                hfceil = s.FakeHeightCeil;
                return true;
            }
            else if(s.TextureCeil == Sector.NO_FLAT)
            {
                // Copy sector height
                hfceil = s.HeightCeil;
                return true;
            }
        }

        // Nothing done
        return false;
    }

    // This returns a scaled X coordinate for the boundaries
    // of this sector with a map coordinate as input
    public float GetBoundsScaledX(float mapx)
    {
        return (mapx - bounds.Left) * boundscalex;
    }

    // This returns a scaled Y coordinate for the boundaries
    // of this sector with a map coordinate as input
    public float GetBoundsScaledY(float mapy)
    {
        return (mapy - bounds.Top) * boundscaley;
    }

    // This adds an item to this sector
    public void AddItem(object i)
    {
        // Add item to sector
        items.Add(i);
    }

    // This removes an item from this sector
    public void RemoveItem(object i)
    {
        // Remove item from sector
        items.Remove(i);
    }

    // This sets to update the lightmap and those of adjacent sectors
    protected abstract void UpdateLightmaps();

    #endregion

    #region ================== Process

    // This processes the sector movement with the given deltatime
    public void Process()
    {
        bool playstopsound = false;

        // Any movement?
        if(changepersec != 0f)
        {
            // Move the floor
            float prevfloor = curfloor;
            curfloor += changepersec;

            UpdateClientLighting();
            DropPlayers();

            // Check the movement direction
            if(changepersec > 0f)
            {
                // Stop the movement if target reached
                if(curfloor > targetfloor)
                {
                    // Stop moving here
                    curfloor = targetfloor;
                    changepersec = 0f;
                    UpdateLightmaps();
                    playstopsound = true;
                }
            }
            else
            {
                // Stop the movement if target reached
                if(curfloor < targetfloor)
                {
                    // Stop moving here
                    curfloor = targetfloor;
                    changepersec = 0f;
                    UpdateLightmaps();
                    playstopsound = true;
                }
            }

            PlaySounds(playstopsound);
        }
    }

    protected abstract void UpdateClientLighting();
    protected abstract void DropPlayers();
    protected abstract void PlaySounds(bool playstopsound);

    #endregion
}
