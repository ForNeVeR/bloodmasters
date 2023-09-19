/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;

namespace CodeImp.Bloodmasters.Client.Graphics;

public abstract class VisualObject : IComparable
{
    #region ================== Variables

    protected Vector3D pos;
    protected float renderbias = 0f;
    protected int renderpass = 1;

    #endregion

    #region ================== Properties

    public Vector3D Position { get { return pos; } }
    public int RenderPass { get { return renderpass; } }

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public VisualObject()
    {
        // Add to the sorted list
        if(General.arena != null) General.arena.AddVisualObject(this);
    }

    // This destroys the object
    public virtual void Dispose()
    {
        // Remove from sorted list
        General.arena.RemoveVisualObject(this);
        GC.SuppressFinalize(this);
    }

    #endregion

    #region ================== Methods

    // This processes the object
    public virtual void Process() { }

    // This renders the object
    public virtual void Render() { }

    // This renders the object sahdow
    public virtual void RenderShadow() { }

    // This compares coordinates
    public static int Compare(Vector3D v1, float renderbias1, Vector3D v2, float renderbias2)
    {
        // Calculate comparision values
        float c1 = (v1.x - v1.y) + v1.z + renderbias1;
        float c2 = (v2.x - v2.y) + v2.z + renderbias2;

        // Return result
        if(c1 == c2) return 0;
        else if(c1 > c2) return 1;
        else return -1;
    }

    // This compares the objects coordinates to
    // another objects coordinates
    public int CompareTo(object obj)
    {
        // Get the proper object reference
        VisualObject o2 = (VisualObject)obj;

        // Compare and return result
        return VisualObject.Compare(this.pos, this.renderbias, o2.pos, o2.renderbias);
    }

    #endregion
}
