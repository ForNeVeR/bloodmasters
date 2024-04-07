/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

namespace Bloodmasters.LevelMap;

public class Segment
{
    #region ================== Constants

    private const uint GLVERTEX = 1u << 31;

    #endregion

    #region ================== Variables

    // General
    private readonly int vstart;				// Start vertex of the segment
    private readonly int vend;				// End vertex of the segment
    private Linedef linedef = null; // Linedef on which this segment lies
    private readonly int side;				// Side of linedef where this segment lies (0=front/right 1=back/left)
    private Sidedef sidedef = null;	// Sidedef of segment (-1 when not on a line)

    #endregion

    #region ================== Properties

    public Linedef Linedef { get { return linedef; } }
    public int Side { get { return side; } }
    public Sidedef Sidedef { get { return sidedef; } }
    public int v1 { get { return vstart; } }
    public int v2 { get { return vend; } }

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public Segment(BinaryReader data, Linedef[] linedefs, int numorigverts)
    {
        int ld;
        uint vs, ve;

        // Read segment
        vs = data.ReadUInt32();
        ve = data.ReadUInt32();
        ld = data.ReadUInt16();
        side = data.ReadUInt16();

        // Not interested in partner segment (other side of line)
        data.ReadUInt32();

        // Make correct start vertex index
        vstart = (int)(vs & ~GLVERTEX);
        if((vs & GLVERTEX) == GLVERTEX) vstart += numorigverts;

        // Make correct end vertex index
        vend = (int)(ve & ~GLVERTEX);
        if((ve & GLVERTEX) == GLVERTEX) vend += numorigverts;

        // Segment along a linedef?
        if(ld < 65535)
        {
            // Make reference to linedef
            linedef = linedefs[ld];

            // Determine on which side
            if(side == 0)
                sidedef = linedef.Front;
            else
                sidedef = linedef.Back;
        }
    }

    // Destructor
    public void Dispose()
    {
        // Release references
        linedef = null;
        sidedef = null;
    }

    #endregion

    #region ================== Methods

    #endregion
}
