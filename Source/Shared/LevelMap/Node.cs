/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

namespace Bloodmasters.LevelMap;

public class Node
{
    #region ================== Constants

    private const uint ISSUBSECTOR = 1u << 31;

    #endregion

    #region ================== Variables

    // References
    Map map;

    // Split line
    private readonly float x1;
    private readonly float y1;
    private readonly float x2;
    private readonly float y2;
    private readonly float dx;
    private readonly float dy;

    // Sides
    private readonly int leftnode = -1;
    private readonly int rightnode = -1;

    // SubSectors
    private SubSector leftssec = null;
    private SubSector rightssec = null;

    #endregion

    #region ================== Properties

    public Node LeftNode { get { if(leftnode > -1) return map.Nodes[leftnode]; else return null; } }
    public Node RightNode { get { if(rightnode > -1) return map.Nodes[rightnode]; else return null; } }
    public SubSector LeftSubSector { get { return leftssec; } }
    public SubSector RightSubSector { get { return rightssec; } }

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public Node(BinaryReader data, SubSector[] subsectors, Map map)
    {
        // Keep references
        this.map = map;

        // Read node
        x1 = (float)data.ReadInt16() * Map.MAP_SCALE_XY;
        y1 = (float)data.ReadInt16() * Map.MAP_SCALE_XY;
        dx = (float)data.ReadInt16() * Map.MAP_SCALE_XY;
        dy = (float)data.ReadInt16() * Map.MAP_SCALE_XY;

        // Skip the left bounding box
        data.ReadInt16();
        data.ReadInt16();
        data.ReadInt16();
        data.ReadInt16();

        // Skip the right bounding box
        data.ReadInt16();
        data.ReadInt16();
        data.ReadInt16();
        data.ReadInt16();

        // Read the sides
        uint left = data.ReadUInt32();
        uint right = data.ReadUInt32();

        // Determine split line end
        x2 = x1 + dx;
        y2 = y1 + dy;

        // Set up left side
        if((left & ISSUBSECTOR) == ISSUBSECTOR)
            leftssec = subsectors[left & ~ISSUBSECTOR];
        else
            leftnode = (int)left;

        // Set up right side
        if((right & ISSUBSECTOR) == ISSUBSECTOR)
            rightssec = subsectors[right & ~ISSUBSECTOR];
        else
            rightnode = (int)right;
    }

    // Destructor
    public void Dispose()
    {
        // Release references
        map = null;
        leftssec = null;
        rightssec = null;
    }

    #endregion

    #region ================== Methods

    // This tests on which side of the split line the given coordinates are
    // returns < 0 for front (right) side, > 0 for back (left) side and 0 if on the line
    public float SideOfLine(float x, float y)
    {
        // Calculate and return side information
        return (y - y1) * dx - (x - x1) * dy;
    }

    #endregion
}
