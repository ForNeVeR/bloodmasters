using System;
using SharpDX.Direct3D9;

namespace CodeImp.Bloodmasters.Client.Graphics;

public class ClientPlayerCollision : PlayerCollision, IClientCollision
{
    public ClientPlayerCollision(IPhysicsState pl, Vector3D objpos, Vector2D objvec, float objradius, bool objisplayer) : base(pl, objpos, objvec, objradius, objisplayer)
    {
    }

    public override Collision Update(Vector3D objpos, Vector2D objvec)
    {
        // Make new collision with updated parameters
        return new ClientPlayerCollision(this.player, objpos, objvec, this.objradius, this.objisplayer);
    }

    // This renders the collision information
    public void Render()
    {
    }

    // This makes vertices for a line
    public void RenderLine(Vector2D s, Vector2D e, SharpDX.Color c, bool arrow)
    {
        const float arrowlen = 1.5f;
        const float arrowwidth = 0.15f;
        LVertex[] verts = new LVertex[6];
        Vector2D as1 = e;
        Vector2D as2 = e;
        int primitives = 1;

        float angle = (float)Math.Atan2(-e.y + s.y, e.x - s.x);
        float angle1 = angle + (float)Math.PI * (1.5f + arrowwidth);
        float angle2 = angle + (float)Math.PI * (1.5f - arrowwidth);
        as1.x += (float)Math.Sin(angle1) * arrowlen;
        as1.y += (float)Math.Cos(angle1) * arrowlen;
        as2.x += (float)Math.Sin(angle2) * arrowlen;
        as2.y += (float)Math.Cos(angle2) * arrowlen;

        // Start vertex
        verts[0].color = c.ToArgb();
        verts[0].x = s.x;
        verts[0].y = s.y;
        verts[0].z = renderheight;

        // End vertex
        verts[1].color = c.ToArgb();
        verts[1].x = e.x;
        verts[1].y = e.y;
        verts[1].z = renderheight;

        // Arrow?
        if(arrow)
        {
            // More lines
            primitives = 3;

            // Arrow side 1 start vertex
            verts[2].color = c.ToArgb();
            verts[2].x = e.x;
            verts[2].y = e.y;
            verts[2].z = renderheight;

            // Arrow side 1 end vertex
            verts[3].color = c.ToArgb();
            verts[3].x = as1.x;
            verts[3].y = as1.y;
            verts[3].z = renderheight;

            // Arrow side 2 start vertex
            verts[4].color = c.ToArgb();
            verts[4].x = e.x;
            verts[4].y = e.y;
            verts[4].z = renderheight;

            // Arrow side 2 end vertex
            verts[5].color = c.ToArgb();
            verts[5].x = as2.x;
            verts[5].y = as2.y;
            verts[5].z = renderheight;
        }

        // Draw line
        Direct3D.SetDrawMode(DRAWMODE.NLINES);
        Direct3D.d3dd.DrawUserPrimitives(PrimitiveType.LineList, primitives, verts);
    }
}
