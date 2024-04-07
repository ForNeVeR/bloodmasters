/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using Bloodmasters.LevelMap;

namespace Bloodmasters;

public abstract class WallCollision : Collision
{
    // Members
    protected Linedef line;
    protected bool offending;
    private readonly bool crossing;
    protected float objradius;
    private readonly Sidedef startside;
    protected float objheight;

    // Elements for calculations
    protected Vector2D objpos, objvec, tstart, tend, objcp, linenorm, tint;
    protected float renderheight;
    protected float stepheight;
    protected bool objisplayer;

    // Public properties
    public bool IsCrossing { get { return crossing; } }
    public Sidedef CrossSide { get { if(startside != null) return startside.OtherSide; else return null; } }
    //public Linedef Line { get { return line; } }

    /*
        Optimization planning:
        1: Cache values that can be cached, and cache them as early as possible.
        2: Do not calculate anything we do not need to determine collision
           instead, put it in seperate methods.
    */

    // Constructor
    public WallCollision(Linedef ld, Vector3D objpos, Vector2D objvec, float objradius, float objheight, float stepheight, bool objisplayer)
    {
        float ldcp, rtcp, objveclen;
        Vector2D linecp, vectonewpos;
        bool otherside;
        bool floorblocks = false;
        bool ceilblocks = false;

        GC.SuppressFinalize(this);

        // Keep references
        this.line = ld;
        this.objpos = objpos;
        this.objvec = objvec;
        this.objradius = objradius;
        this.renderheight = objpos.z;
        this.stepheight = stepheight;
        this.objisplayer = objisplayer;
        this.objheight = objheight;

        // References available?
        if((ld.Front != null) && (ld.Back != null))
        {
            // Determine if floor is blocking
            floorblocks = ((objpos.z + stepheight) < ld.Front.Sector.CurrentFloor) ||
                          ((objpos.z + stepheight) < ld.Back.Sector.CurrentFloor);

            // Determine if there is a ceiling
            if(ld.Front.Sector.HasCeiling || ld.Back.Sector.HasCeiling)
            {
                // Ceiling might block
                if(objpos.z < ld.Front.Sector.FakeHeightCeil)
                    ceilblocks = (objpos.z + objheight) > ld.Front.Sector.HeightCeil;
                if(objpos.z < ld.Back.Sector.FakeHeightCeil)
                    ceilblocks |= (objpos.z + objheight) > ld.Back.Sector.HeightCeil;
            }
            else
            {
                // No ceiling
                ceilblocks = false;
            }
        }
        else
        {
            // End of the map always blocks
            floorblocks = true;
            ceilblocks = true;
        }

        // Check if this line can collide
        if((ld.Impassable && objisplayer) || floorblocks || ceilblocks || (ld.Action != 0))
        {
            // Check if the object crosses the line
            float side1 = ld.SideOfLine(objpos.x, objpos.y);
            float side2 = ld.SideOfLine(objpos.x + objvec.x, objpos.y + objvec.y);
            otherside = ((side1 >= 0f) && (side2 <= 0f)) || ((side1 <= 0f) && (side2 >= 0f));

            // Calculate distances from object to the line
            float dist1 = ld.DistanceToLine(objpos.x, objpos.y);
            float dist2 = ld.DistanceToLine(objpos.x + objvec.x, objpos.y + objvec.y);

            // Check if the object is offending the line
            if((dist2 < dist1) || otherside)
            {
                // Check on which side of the line we are
                if(side2 <= 0f) startside = ld.Front; else startside = ld.Back;

                // Keep the collision side
                this.collideobj = startside;

                // Check if really touching the line
                if(otherside) crossing = ld.IntersectLine(objpos.x, objpos.y, objpos.x + objvec.x, objpos.y + objvec.y);

                // Determine collision point on the line
                ldcp = ld.NearestOnLine(objpos.x, objpos.y);
                if(ldcp < 0f) ldcp = 0f; else if(ldcp > 1f) ldcp = 1f;
                linecp = ld.CoordinatesAt(ldcp);

                // Calculate line normal from object to line
                linenorm = linecp - this.objpos;
                linenorm.Normalize();

                // Determine closest point at object to the line
                objcp = this.objpos + linenorm * objradius;

                // Start position of reversed trajectory
                // NOTE: Same as linecp??
                tstart = ld.CoordinatesAt(ldcp);

                // End position of reversed trajectory
                tend = tstart - objvec;

                // Length of object velocity
                // (also length of reversed trajectory)
                objveclen = objvec.Length();

                // Calculate nearest point on reversed trajectory
                rtcp = ((objcp.x - tstart.x) * (tend.x - tstart.x) + (objcp.y - tstart.y) * (tend.y - tstart.y)) / (objveclen * objveclen);
                if(rtcp < 0f) rtcp = 0f; else if(rtcp > 1f) rtcp = 1f;
                tint = tstart + rtcp * (tend - tstart);

                // Check if distance at the intersection
                // is close enough for collision
                Vector2D tid = tint - this.objpos;
                if(tid.Length() <= objradius)
                {
                    // Calculate closest position near wall
                    newobjpos = this.objpos + linecp - objcp;
                    vectonewpos = newobjpos - this.objpos;

                    // Will collide!
                    collide = (ld.Impassable && objisplayer) || floorblocks || ceilblocks;
                    offending = true;
                    distance = vectonewpos.Length();
                }
                else
                {
                    // No collision yet
                    collide = false;
                    offending = true;
                    distance = 10000f + dist1;
                }
            }
            else
            {
                // No collision
                collide = false;
                offending = false;
                distance = 20000f + dist1;
            }
        }
        else
        {
            // No collision
            collide = false;
            offending = false;
            distance = 30000f;
        }
    }

    // Response vectors
    public override Vector2D GetBounceVector()
    {
        // Calculate bouncing vector
        return Vector2D.Reflect(objvec, new Vector2D(line.nY, line.nX));
    }

    // Response vectors
    public override Vector2D GetSlideVector()
    {
        // Calculate sliding vector
        objslidevec = newobjpos - tstart;
        objslidevec.Normalize();
        objslidevec *= Vector2D.DotProduct(objslidevec, objvec);
        objslidevec = objvec - objslidevec;

        // Make sure sliding vector is zero when near zero
        if(objslidevec.Length() < 0.01f) objslidevec = new Vector2D(0f, 0f);

        return objslidevec;
    }
}
