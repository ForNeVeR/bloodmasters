/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

namespace CodeImp.Bloodmasters.Map;

public class Sidedef
{
    #region ================== Constants

    #endregion

    #region ================== Variables

    private int index;
    private float tx;				// Texture X offset
    private float ty;				// Texture Y offset
    private Sector sector;			// Sector on which the sidedef lies
    private Linedef linedef;		// Linedef on which this sidedef lies
    private string tlower;			// Lower texture
    private string tmiddle;			// Middle texture
    private string tupper;			// Upper texture
    private Sidedef otherside;		// Sidedef on the other side of the line
    private float angle;

    #endregion

    #region ================== Properties

    public int Index { get { return index; } }
    public Sector Sector { get { return sector; } }
    public Linedef Linedef { get { return linedef; } }
    public string TextureLower { get { return tlower; } }
    public string TextureMiddle { get { return tmiddle; } }
    public string TextureUpper { get { return tupper; } }
    public float TextureX { get { return tx; } }
    public float TextureY { get { return ty; } }
    public Sidedef OtherSide { get { return otherside; } }
    public float Angle { get { return angle; } }
    public float Length { get { return linedef.Length; } }
    public bool IsFront { get { return linedef.Front == this; } }

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public Sidedef(BinaryReader data, Sector[] sectors, int index)
    {
        this.index = index;

        // Read sidedef
        tx = data.ReadInt16();
        ty = data.ReadInt16();
        tupper = Wad.BytesToString(data.ReadBytes(8)).ToLower();
        tlower = Wad.BytesToString(data.ReadBytes(8)).ToLower();
        tmiddle = Wad.BytesToString(data.ReadBytes(8)).ToLower();
        sector = sectors[data.ReadUInt16()];
    }

    // Destructor
    public void Dispose()
    {
        // Release references
        sector = null;
    }

    #endregion

    #region ================== Methods

    // This makes reference to the linedef
    public void SetLinedefRef(Linedef linedef, Vector2D[] vertices)
    {
        float dx, dy;

        // Make reference
        this.linedef = linedef;

        // Check on which side the sidedef is
        if(linedef.Front == this)
        {
            // Other side is back side
            otherside = linedef.Back;

            // Calculate the angle
            dx = vertices[linedef.v2].x - vertices[linedef.v1].x;
            dy = vertices[linedef.v2].y - vertices[linedef.v1].y;
            angle = (float)Math.Atan2(dy, dx);
        }
        else
        {
            // Other side is front side
            otherside = linedef.Front;

            // Calculate the angle
            dx = vertices[linedef.v1].x - vertices[linedef.v2].x;
            dy = vertices[linedef.v1].y - vertices[linedef.v2].y;
            angle = (float)Math.Atan2(dy, dx);
        }
    }

    #endregion
}
