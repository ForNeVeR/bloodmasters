/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

namespace CodeImp.Bloodmasters;

public class Linedef
{
    #region ================== Constants

    #endregion

    #region ================== Variables

    private int index;
    private int vstart;				// Start vertex of the line
    private int vend;				// End vertex of the line
    private bool impassable;
    private bool solid;
    private LINEFLAG flags;
    private ACTION action;
    private int[] arg;				// Action arguments (usage depends on action)
    private Sidedef sfront = null;	// Sidedef on the front (right) side of line
    private Sidedef sback = null;	// Sidedef on the back (left) side of the line
    private float length;
    private float lengthsq;
    private float angle;
    private Map map;
    private float nx, ny;

    #endregion

    #region ================== Properties

    public int Index { get { return index; } }
    public Sidedef Front { get { return sfront; } }
    public Sidedef Back { get { return sback; } }
    public float Length { get { return length; } }
    public float Angle { get { return angle; } }
    public int v1 { get { return vstart; } }
    public int v2 { get { return vend; } }
    public ACTION Action { get { return action; } }
    public LINEFLAG Flags { get { return flags; } }
    public bool Solid { get { return solid; } }
    public bool Impassable { get { return impassable; } }
    public int[] Arg { get { return arg; } }
    public float nX { get { return nx; } }
    public float nY { get { return ny; } }

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public Linedef(BinaryReader data, Vector2D[] vertices, Sidedef[] sidedefs, Map map, int index)
    {
        int sf, sb;
        this.map = map;
        this.index = index;

        // Read linedef
        vstart = data.ReadUInt16();
        vend = data.ReadUInt16();
        flags = (LINEFLAG)data.ReadInt16();
        action = (ACTION)data.ReadByte();
        arg = new int[5];
        for(int k = 0; k < 5; k++) arg[k] = data.ReadByte();
        sf = data.ReadUInt16();
        sb = data.ReadUInt16();

        // Get properties from flags
        solid = (flags & LINEFLAG.SOLID) != 0;
        impassable = (flags & LINEFLAG.IMPASSABLE) != 0;

        // Calculate the length
        float dx = vertices[vend].x - vertices[vstart].x;
        float dy = vertices[vend].y - vertices[vstart].y;
        lengthsq = dx * dx + dy * dy;
        length = (float)Math.Sqrt(lengthsq);

        // Calculate normal
        nx = dx / length;
        ny = dy / length;

        // Calculate the angle
        angle = (float)Math.Atan2(dy, dx);

        // Make sidedef references
        if(sf < 65535) sfront = sidedefs[sf];
        if(sb < 65535) sback = sidedefs[sb];

        // Make linedef references on sidedefs
        if(sfront != null) sfront.SetLinedefRef(this, vertices);
        if(sback != null) sback.SetLinedefRef(this, vertices);
    }

    // Destructor
    public void Dispose()
    {
        // Release references
        sfront = null;
        sback = null;
        map = null;
    }

    #endregion

    #region ================== Methods

    // This tests if the line intersects with the given line coordinates
    public bool IntersectLine(float x3, float y3, float x4, float y4)
    {
        float u_ray, u_line;
        return IntersectLine(x3, y3, x4, y4, out u_ray, out u_line);
    }

    // This tests if the line intersects with the given line coordinates
    public bool IntersectLine(float x3, float y3, float x4, float y4, out float u_ray)
    {
        float u_line;
        return IntersectLine(x3, y3, x4, y4, out u_ray, out u_line);
    }

    // This tests if the line intersects with the given line coordinates
    public bool IntersectLine(float x3, float y3, float x4, float y4, out float u_ray, out float u_line)
    {
        // Get line vertices
        Vector2D v1 = map.Vertices[vstart];
        Vector2D v2 = map.Vertices[vend];

        // Calculate divider
        float div = (y4 - y3) * (v2.x - v1.x) - (x4 - x3) * (v2.y - v1.y);

        // Can this be tested?
        if((div > 0.00001f) || (div < -0.00001f))
        {
            // Calculate the intersection distance from the line
            u_line = ((x4 - x3) * (v1.y - y3) - (y4 - y3) * (v1.x - x3)) / div;

            // Calculate the intersection distance from the ray
            u_ray = ((v2.x - v1.x) * (v1.y - y3) - (v2.y - v1.y) * (v1.x - x3)) / div;

            // Return if intersecting
            return (u_ray >= 0.0f) && (u_ray <= 1.0f) && (u_line >= 0.0f) && (u_line <= 1.0f);
        }
        else
        {
            // Unable to detect intersection
            u_line = float.NaN;
            u_ray = float.NaN;
            return false;
        }
    }

    // This tests on which side of the line the given coordinates are
    // returns < 0 for front (right) side, > 0 for back (left) side and 0 if on the line
    public float SideOfLine(float x, float y)
    {
        // Get line vertices
        Vector2D v1 = map.Vertices[vstart];
        Vector2D v2 = map.Vertices[vend];

        // Calculate and return side information
        return (y - v1.y) * (v2.x - v1.x) - (x - v1.x) * (v2.y - v1.y);
    }

    // This returns the squared shortest distance from given coordinates to line
    public float DistanceToLineSq(float x, float y)
    {
        // Get line vertices
        Vector2D v1 = map.Vertices[vstart];
        Vector2D v2 = map.Vertices[vend];

        // Calculate intersection offset
        float u = ((x - v1.x) * (v2.x - v1.x) + (y - v1.y) * (v2.y - v1.y)) / lengthsq;

        // Limit intersection offset to the line
        float lbound = 1f / length;
        float ubound = 1f - lbound;
        if(u < lbound) u = lbound;
        if(u > ubound) u = ubound;

        // Calculate intersection point
        float ix = v1.x + u * (v2.x - v1.x);
        float iy = v1.y + u * (v2.y - v1.y);

        // Return distance between intersection and point
        // which is the shortest distance to the line
        float ldx = x - ix;
        float ldy = y - iy;
        return ldx * ldx + ldy * ldy;
    }

    // This returns the shortest distance from given coordinates to line
    public float DistanceToLine(float x, float y)
    {
        // Return distance
        return (float)Math.Sqrt(DistanceToLineSq(x, y));
    }

    // This returns the offset coordinates on the line nearest to the given coordinates
    public float NearestOnLine(float x, float y)
    {
        // Get line vertices
        Vector2D v1 = map.Vertices[vstart];
        Vector2D v2 = map.Vertices[vend];

        // Calculate and return intersection offset
        return ((x - v1.x) * (v2.x - v1.x) + (y - v1.y) * (v2.y - v1.y)) / (length * length);
    }

    // This returns the coordinates at a specific position on the line
    public Vector2D CoordinatesAt(float u)
    {
        // Get line vertices
        Vector2D v1 = map.Vertices[vstart];
        Vector2D v2 = map.Vertices[vend];

        // Calculate and return intersection offset
        return new Vector2D(v1.x + u * (v2.x - v1.x), v1.y + u * (v2.y - v1.y));
    }

    #endregion
}
