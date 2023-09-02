/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using System.Drawing;
using CodeImp;

namespace CodeImp.Bloodmasters;

public struct Line2D
{
    #region ================== Members

    // Members
    public Vector2D v1;
    public Vector2D v2;

    #endregion

    #region ================== Properties

    // Properties
    public Vector2D d { get { return v2 - v1; } }
    public float dx { get { return v2.x - v1.x; } }
    public float dy { get { return v2.y - v1.y; } }

    #endregion

    #region ================== Constructors

    // Constructor
    public Line2D(Vector2D v1, Vector2D v2)
    {
        this.v1 = v1;
        this.v2 = v2;
    }

    // Constructor
    public Line2D(Vector2D v1, float dx, float dy)
    {
        this.v1 = v1;
        this.v2 = new Vector2D(v1.x + dx, v1.y + dy);
    }

    #endregion

    #region ================== Static Methods

    // This calculates the length
    public static float Length(float dx, float dy)
    {
        // Calculate and return the length
        return (float)Math.Sqrt(LengthSq(dx, dy));
    }

    // This calculates the square of the length
    public static float LengthSq(float dx, float dy)
    {
        // Calculate and return the length
        return dx * dx + dy * dy;
    }

    // This tests if the line intersects with the given line coordinates
    public static bool IntersectLine(Vector2D v1, Vector2D v2, float x3, float y3, float x4, float y4)
    {
        float u_ray, u_line;
        return IntersectLine(v1, v2, x3, y3, x4, y4, out u_ray, out u_line);
    }

    // This tests if the line intersects with the given line coordinates
    public static bool IntersectLine(Vector2D v1, Vector2D v2, float x3, float y3, float x4, float y4, out float u_ray)
    {
        float u_line;
        return IntersectLine(v1, v2, x3, y3, x4, y4, out u_ray, out u_line);
    }

    // This tests if the line intersects with the given line coordinates
    public static bool IntersectLine(Vector2D v1, Vector2D v2, float x3, float y3, float x4, float y4, out float u_ray, out float u_line)
    {
        // Calculate divider
        float div = (y4 - y3) * (v2.x - v1.x) - (x4 - x3) * (v2.y - v1.y);

        // Can this be tested?
        if((div > 0.000001f) || (div < -0.000001f))
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
    public static float SideOfLine(Vector2D v1, Vector2D v2, Vector2D p)
    {
        // Calculate and return side information
        return (p.y - v1.y) * (v2.x - v1.x) - (p.x - v1.x) * (v2.y - v1.y);
    }

    // This returns the shortest distance from given coordinates to line
    public static float DistanceToLine(Vector2D v1, Vector2D v2, Vector2D p, bool bounded)
    {
        return (float)Math.Sqrt(DistanceToLineSq(v1, v2, p, bounded));
    }

    // This returns the shortest distance from given coordinates to line
    public static float DistanceToLineSq(Vector2D v1, Vector2D v2, Vector2D p, bool bounded)
    {
        // Calculate intersection offset
        float u = ((p.x - v1.x) * (v2.x - v1.x) + (p.y - v1.y) * (v2.y - v1.y)) / LengthSq(v2.x - v1.x, v2.y - v1.y);

        if(bounded)
        {
            // Limit intersection offset to the line
            float lbound = 1f / Length(v2.x - v1.x, v2.y - v1.y);
            float ubound = 1f - lbound;
            if(u < lbound) u = lbound;
            if(u > ubound) u = ubound;
        }

        // Calculate intersection point
        Vector2D i = v1 + u * (v2 - v1);

        // Return distance between intersection and point
        // which is the shortest distance to the line
        float ldx = p.x - i.x;
        float ldy = p.y - i.y;
        return ldx * ldx + ldy * ldy;
    }

    // This returns the offset coordinates on the line nearest to the given coordinates
    public static float NearestOnLine(Vector2D v1, Vector2D v2, Vector2D p)
    {
        // Calculate and return intersection offset
        return ((p.x - v1.x) * (v2.x - v1.x) + (p.y - v1.y) * (v2.y - v1.y)) / LengthSq(v2.x - v1.x, v2.y - v1.y);
    }

    // This returns the coordinates at a specific position on the line
    public static Vector2D CoordinatesAt(Vector2D v1, Vector2D v2, float u)
    {
        // Calculate and return intersection offset
        return new Vector2D(v1.x + u * (v2.x - v1.x), v1.y + u * (v2.y - v1.y));
    }

    #endregion

    #region ================== Methods

    public float Length() { return Line2D.Length(dx, dy); }
    public float LengthSq() { return Line2D.LengthSq(dx, dy); }

    public bool IntersectLine(float x3, float y3, float x4, float y4)
    {
        return Line2D.IntersectLine(v1, v2, x3, y3, x4, y4);
    }

    public bool IntersectLine(float x3, float y3, float x4, float y4, out float u_ray)
    {
        return Line2D.IntersectLine(v1, v2, x3, y3, x4, y4, out u_ray);
    }

    public bool IntersectLine(float x3, float y3, float x4, float y4, out float u_ray, out float u_line)
    {
        return Line2D.IntersectLine(v1, v2, x3, y3, x4, y4, out u_ray, out u_line);
    }

    public float SideOfLine(Vector2D p)
    {
        return Line2D.SideOfLine(v1, v2, p);
    }

    public float DistanceToLine(Vector2D p, bool bounded)
    {
        return Line2D.DistanceToLine(v1, v2, p, bounded);
    }

    public float DistanceToLineSq(Vector2D p, bool bounded)
    {
        return Line2D.DistanceToLineSq(v1, v2, p, bounded);
    }

    public float NearestOnLine(Vector2D p)
    {
        return Line2D.NearestOnLine(v1, v2, p);
    }

    public Vector2D CoordinatesAt(float u)
    {
        return Line2D.CoordinatesAt(v1, v2, u);
    }

    #endregion
}
