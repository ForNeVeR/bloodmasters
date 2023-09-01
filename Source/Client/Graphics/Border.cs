/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using SharpDX.Direct3D9;

namespace CodeImp.Bloodmasters.Client;

public class Border
{
    #region ================== Constants

    #endregion

    #region ================== Variables

    // Coordinates
    private float left;
    private float top;
    private float right;
    private float bottom;

    // Visibility
    private bool visible;

    // Background color
    private int color;
    private int modcolor;

    // Geometry
    private TLVertex[] vertices;

    // Texture
    private TextureResource texture;

    #endregion

    #region ================== Properties

    public float Left { get { return left; } }
    public float Right { get { return right; } }
    public float Top { get { return top; } }
    public float Bottom { get { return bottom; } }
    public bool Visible { get { return visible; } set { visible = value; } }
    public int ModulateColor { get { return modcolor; } set { modcolor = value; } }
    public TextureResource Texture { get { return texture; } set { texture = value; } }

    public int Color
    {
        get
        {
            return this.color;
        }

        set
        {
            this.color = value;
            this.Position(left, top, right, bottom);
        }
    }

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public Border(int c)
    {
        // Color
        this.color = c;
        this.modcolor = -1;
        this.Visible = true;
    }

    // Disposer
    public void Dispose()
    {
        // Clean up
        texture = null;
        vertices = null;
        GC.SuppressFinalize(this);
    }

    #endregion

    #region ================== Methods

    // This (re)builds the window
    public void Position(float l, float t, float r, float b)
    {
        // Set the coordinates
        this.left = l;
        this.top = t;
        this.right = r;
        this.bottom = b;

        // Make vertices
        vertices = Direct3D.TLRect(left * (float)Direct3D.DisplayWidth,
            top * (float)Direct3D.DisplayHeight,
            right * (float)Direct3D.DisplayWidth,
            bottom * (float)Direct3D.DisplayHeight, color);
    }

    // Rendering
    public void Render()
    {
        // Visible?
        if(visible)
        {
            // Set renderstates
            Direct3D.SetDrawMode(DRAWMODE.TLMODALPHA);
            Direct3D.d3dd.SetRenderState(RenderState.TextureFactor, modcolor);
            if(texture != null) Direct3D.d3dd.SetTexture(0, texture.texture);
            else Direct3D.d3dd.SetTexture(0, null);

            // Render vertices
            Direct3D.d3dd.DrawUserPrimitives(PrimitiveType.TriangleStrip, 2, vertices);
        }
    }

    #endregion
}
