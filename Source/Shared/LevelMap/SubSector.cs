/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System.Drawing;

namespace CodeImp.Bloodmasters.LevelMap;

public class SubSector
{
    #region ================== Constants

    #endregion

    #region ================== Variables

    // General
    private Sector sector = null;	// Sector in which this subsector is
    private Segment[] segs;			// Segments in subsector

    // Boundaries
    private RectangleF bounds;

    #endregion

    #region ================== Properties

    public Sector Sector { get { return sector; } }
    public Segment[] Segments { get { return segs; } }

    // Boundaries
    public RectangleF Bounds { get { return bounds; } }
    public float X { get { return bounds.X; } }
    public float Y { get { return bounds.Y; } }
    public float Width { get { return bounds.Width; } }
    public float Height { get { return bounds.Height; } }
    public float Top { get { return bounds.Top; } }
    public float Bottom { get { return bounds.Bottom; } }

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public SubSector(BinaryReader data, Segment[] segments, Vector2D[] vertices)
    {
        int numsegs;
        int firstseg;
        float bl = 0;
        float bt = 0;
        float br = 0;
        float bb = 0;

        // Read subsector info from data
        numsegs = (int)data.ReadUInt32();
        firstseg = (int)data.ReadUInt32();

        // Make segments array
        segs = new Segment[numsegs];

        // Go for all segments
        for(int i = 0; i < numsegs; i++)
        {
            // Make reference to segment
            segs[i] = segments[firstseg + i];

            // Take sector from segment when along a linedef
            if(segs[i].Sidedef != null) sector = segs[i].Sidedef.Sector;

            // Get the last vertex
            Vector2D v = vertices[segs[i].v2];

            // First segment?
            if(i == 0)
            {
                // Boundary from segment
                bl = v.x;
                br = v.x;
                bt = v.y;
                bb = v.y;
            }
            else
            {
                // Extend boundary with segment
                bl = Math.Min(bl, v.x);
                br = Math.Max(br, v.x);
                bt = Math.Min(bt, v.y);
                bb = Math.Max(bb, v.y);
            }
        }

        // Make boundary rectangle
        bounds = new RectangleF(bl, bt, br - bl, bb - bt);

        // Make subsector reference at sector if sector is know
        if(sector != null) sector.AddSubSectorRef(this);
    }

    // Destructor
    public void Dispose()
    {
        // Release references
        sector = null;
        segs = null;
    }

    #endregion

    #region ================== Methods

    #endregion
}
