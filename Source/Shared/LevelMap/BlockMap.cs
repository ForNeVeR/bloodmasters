/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System.Drawing;

namespace Bloodmasters.LevelMap;

public class BlockMap
{
    #region ================== Constants

    private const float BLOCKSIZE = 128f * Map.MAP_SCALE_XY;
    private const int EXPECTED_LINES_PER_TEST = 500;

    #endregion

    #region ================== Variables

    // Map
    private Map map;

    // Blockmap
    private List<Linedef>[,] blocks;
    private readonly int cols;
    private readonly int rows;
    private readonly float ox;
    private readonly float oy;

    #endregion

    #region ================== Properties

    public float OffsetX { get { return ox; } }
    public float OffsetY { get { return oy; } }
    public int Cols { get { return cols; } }
    public int Rows { get { return rows; } }

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public BlockMap(BinaryReader data, Linedef[] linedefs, Map map)
    {
        int ld;

        // Keep reference to map
        this.map = map;

        // Read blockmap header
        ox = (float)data.ReadInt16() * Map.MAP_SCALE_XY;
        oy = (float)data.ReadInt16() * Map.MAP_SCALE_XY;
        cols = data.ReadUInt16();
        rows = data.ReadUInt16();

        // Make blockmap list and an
        // array to keep the offsets
        blocks = new List<Linedef>[cols, rows];
        int[,] offsets = new int[cols, rows];

        // Go for all rows and columns
        // to read the offsets from data
        for(int r = 0; r < rows; r++)
        {
            for(int c = 0; c < cols; c++)
            {
                // Read offset from data
                offsets[c, r] = data.ReadUInt16() * 2;
            }
        }

        // Go for all rows and columns
        // to read the linedefs from data
        for(int r = 0; r < rows; r++)
        {
            for(int c = 0; c < cols; c++)
            {
                // Make a list for the linedefs
                blocks[c, r] = new List<Linedef>();

                // Seek to the offset for this block
                data.BaseStream.Seek(offsets[c, r], SeekOrigin.Begin);

                // First short is always 0
                data.ReadUInt16();

                // Continue reading until result is 65535
                while((ld = data.ReadUInt16()) < 65535)
                {
                    // Add the linedef to list
                    blocks[c, r].Add(linedefs[ld]);
                }
            }
        }

        // Clean up
        data.Close();
    }

    // Destructor
    public void Dispose()
    {
        // Release references
        blocks = null;
        map = null;
    }

    #endregion

    #region ================== Methods

    // This returns the nearby lines for a specific line
    public List<Linedef> GetCollisionLines(float x1, float y1, float x2, float y2)
    {
        bool[] lineadded = new bool[map.Linedefs.Length];
        Vector2D v1, v2;
        float deltax, deltay;
        float posx, posy;
        int row, col;
        int endrow, endcol;
        int dirx, diry;

        // Make list for the lines
        List<Linedef> lines = new List<Linedef>(EXPECTED_LINES_PER_TEST);

        // Adjust coordinates to match blockmap offset
        v1 = new Vector2D(x1 - ox, y1 - oy);
        v2 = new Vector2D(x2 - ox, y2 - oy);

        // Find the starting cell
        col = (int)Math.Floor(v1.x / BLOCKSIZE);
        row = (int)Math.Floor(v1.y / BLOCKSIZE);

        // Add lines from this cell
        if((row > 0) && (row < rows) && (col > 0) && (col < cols))
            AddBlockLines(lines, row, col, ref lineadded);

        // Find the ending cell
        endcol = (int)Math.Floor(v2.x / BLOCKSIZE);
        endrow = (int)Math.Floor(v2.y / BLOCKSIZE);

        // Crossing outside the cell?
        if((row != endrow) || (col != endcol))
        {
            // Calculate current cell edge coordinates
            float cl = col * BLOCKSIZE;
            float cr = (col + 1) * BLOCKSIZE;
            float ct = row * BLOCKSIZE;
            float cb = (row + 1) * BLOCKSIZE;

            // Line directions
            dirx = Math.Sign(v2.x - v1.x);
            diry = Math.Sign(v2.y - v1.y);

            // Determine horizontal direction
            if(dirx >= 0)
            {
                // Calculate offset and delta movement over x
                posx = (cr - v1.x) / (v2.x - v1.x);
                deltax = BLOCKSIZE / (v2.x - v1.x);
            }
            else
            {
                // Calculate offset and delta movement over x
                posx = (v1.x - cl) / (v1.x - v2.x);
                deltax = BLOCKSIZE / (v1.x - v2.x);
            }

            // Determine vertical direction
            if(diry >= 0)
            {
                // Calculate offset and delta movement over y
                posy = (cb - v1.y) / (v2.y - v1.y);
                deltay = BLOCKSIZE / (v2.y - v1.y);
            }
            else
            {
                // Calculate offset and delta movement over y
                posy = (v1.y - ct) / (v1.y - v2.y);
                deltay = BLOCKSIZE / (v1.y - v2.y);
            }

            // Continue while not reached the end
            while((row != endrow) || (col != endcol))
            {
                // Check in which direction to move
                if(posx < posy)
                {
                    // Move horizontally
                    posx += deltax;
                    col += dirx;
                }
                else
                {
                    // Move vertically
                    posy += deltay;
                    row += diry;
                }

                // Add lines from this cell
                if((row > 0) && (row < rows) && (col > 0) && (col < cols))
                    AddBlockLines(lines, row, col, ref lineadded);
            }
        }

        // Return the list
        return lines;
    }

    // This returns the nearby lines for a specific line and radius
    // NOTE: Line and radius are used to find the lines in a square region
    public List<Linedef> GetCollisionLines(float x1, float y1, float x2, float y2, float radius)
    {
        float l, r, b, t;

        // Make region
        if(x1 < x2) { l = x1; r = x2; } else { l = x2; r = x1; }
        if(y1 < y2) { t = y1; b = y2; } else { t = y2; b = y1; }

        // Extend region with radius
        l -= radius;
        r += radius;
        t -= radius;
        b += radius;

        // Return lines in region
        return GetCollisionLines(new RectangleF(l, t, r - l, b - t));
    }

    // This returns the nearby lines for a specific position and radius
    public List<Linedef> GetCollisionLines(float x, float y, float radius)
    {
        // Make region and return lines in region
        return GetCollisionLines(new RectangleF(x - radius, y - radius, radius * 2, radius * 2));
    }

    // This returns the lines for a specific region
    public List<Linedef> GetCollisionLines(RectangleF region)
    {
        bool[] lineadded = new bool[map.Linedefs.Length];
        int c1, c2, r1, r2, c, r;

        // Calculate the blocks overlapping region
        // NOTE: Top and bottom flipped, because Y axis in map is reversed
        c1 = (int)Math.Floor((region.Left - ox) / BLOCKSIZE);
        c2 = (int)Math.Floor((region.Right - ox) / BLOCKSIZE);
        r1 = (int)Math.Floor((region.Top - oy) / BLOCKSIZE);
        r2 = (int)Math.Floor((region.Bottom - oy) / BLOCKSIZE);

        // Clip to blockmap boundaries
        if(c1 < 0) c1 = 0; else if(c1 >= cols) c1 = cols - 1;
        if(c2 < 0) c2 = 0; else if(c2 >= cols) c2 = cols - 1;
        if(r1 < 0) r1 = 0; else if(r1 >= rows) r1 = rows - 1;
        if(r2 < 0) r2 = 0; else if(r2 >= rows) r2 = rows - 1;

        // Make list for the lines
        List<Linedef> lines = new List<Linedef>(EXPECTED_LINES_PER_TEST);

        // Add lines to the list
        for(c = c1; c <= c2; c++)
        for(r = r1; r <= r2; r++)
            AddBlockLines(lines, r, c, ref lineadded);

        // Return the list
        return lines;
    }

    // This adds lines from a block to a list
    private void AddBlockLines(List<Linedef> lines, int r, int c, ref bool[] lineadded)
    {
        // Go for all lines
        foreach(Linedef ld in blocks[c, r])
        {
            // Line not already added?
            if(!lineadded[ld.Index])
            {
                // Add the line to list
                lines.Add(ld);
                lineadded[ld.Index] = true;
            }
        }
    }

    #endregion
}
