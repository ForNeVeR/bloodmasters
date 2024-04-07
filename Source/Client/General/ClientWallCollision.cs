using System;
using Bloodmasters.Client.Graphics;
using Bloodmasters.LevelMap;
using SharpDX;
using SharpDX.Direct3D9;
using Direct3D = Bloodmasters.Client.Graphics.Direct3D;

namespace Bloodmasters.Client;

public class ClientWallCollision : WallCollision, IClientCollision
{
    public ClientWallCollision(Linedef ld, Vector3D objpos, Vector2D objvec, float objradius, float objheight, float stepheight, bool objisplayer) : base(ld, objpos, objvec, objradius, objheight, stepheight, objisplayer)
    {
    }

    // This makes an updated collision
    public override Collision Update(Vector3D objpos, Vector2D objvec)
    {
        // Make new collision with updated parameters
        return new ClientWallCollision(this.line, objpos, objvec, this.objradius, this.objheight, this.stepheight, this.objisplayer);
    }

    // This renders the collision information
    public void Render()
	{
		const float dotsize = 0.3f;
		Color tc = Color.Blue;
		Vector2D tvec = new Vector2D();

		// Using actual coordinates, dont transform them
		Graphics.Direct3D.d3dd.SetTransform(TransformState.World, Matrix.Identity);

		// Get line vertices
		Vector2D v1 = General.map.Vertices[line.v1];
		Vector2D v2 = General.map.Vertices[line.v2];

		// The linedef
		RenderLine(new Vector2D(v1.x, v1.y), new Vector2D(v2.x, v2.y), Color.Yellow, false);

		// Object vector
		if(objvec.Length() > 0.01f)
		{
			Vector2D ovec = objvec;
			ovec.MakeLength(6f);
			RenderLine(new Vector2D(objpos.x, objpos.y), new Vector2D(objpos.x + ovec.x, objpos.y + ovec.y), Color.Maroon, true);
			RenderLine(new Vector2D(objpos.x, objpos.y), new Vector2D(objpos.x + objvec.x * 20f, objpos.y + objvec.y * 20f), Color.Red, true);
		}

		// Check if offending the line
		// (if not, then these are not even calculated)
		if(offending)
		{
			// Line from object to wall
			RenderLine(new Vector2D(objpos.x, objpos.y), new Vector2D(objpos.x + linenorm.x * 6f, objpos.y + linenorm.y * 6f), Color.LawnGreen, true);

			// Reversed trajectory from wall
			if(collide) tc = Color.SkyBlue;
			tvec.x = tend.x - tstart.x;
			tvec.y = tend.y - tstart.y;
			if(tvec.Length() > 0.01f)
			{
				Vector2D tnvec = tvec;
				tnvec.MakeLength(6f);
				RenderLine(new Vector2D(tstart.x, tstart.y), new Vector2D(tstart.x + tnvec.x, tstart.y + tnvec.y), Color.DarkBlue, true);
				tvec.Scale(12f);
				RenderLine(new Vector2D(tstart.x, tstart.y), new Vector2D(tstart.x + tvec.x, tstart.y + tvec.y), tc, true);
			}

			// Sliding vector
			if(objslidevec.Length() > 0.01f)
			{
				Vector2D svec = objslidevec;
				svec.MakeLength(6f);
				RenderLine(new Vector2D(objpos.x, objpos.y), new Vector2D(objpos.x + svec.x, objpos.y + svec.y), Color.Brown, true);
				RenderLine(new Vector2D(objpos.x, objpos.y), new Vector2D(objpos.x + objslidevec.x * 20f, objpos.y + objslidevec.y * 20f), Color.Orange, true);
			}

			// Bounce vector
			if(objbouncevec.Length() > 0.01f)
			{
				Vector2D bvec = objbouncevec;
				bvec.MakeLength(6f);
				RenderLine(new Vector2D(objpos.x, objpos.y), new Vector2D(objpos.x + bvec.x, objpos.y + bvec.y), Color.Gray, true);
				RenderLine(new Vector2D(objpos.x, objpos.y), new Vector2D(objpos.x + objbouncevec.x * 20f, objpos.y + objbouncevec.y * 20f), Color.WhiteSmoke, true);
			}

			// Collision point on object
			RenderLine(new Vector2D(objcp.x - dotsize, objcp.y - dotsize), new Vector2D(objcp.x + dotsize, objcp.y + dotsize), Color.LightGreen, false);
			RenderLine(new Vector2D(objcp.x + dotsize, objcp.y - dotsize), new Vector2D(objcp.x - dotsize, objcp.y + dotsize), Color.LightGreen, false);

			// Collision point on reversed trajectory
			RenderLine(new Vector2D(tint.x - dotsize, tint.y - dotsize), new Vector2D(tint.x + dotsize, tint.y + dotsize), Color.Magenta, false);
			RenderLine(new Vector2D(tint.x + dotsize, tint.y - dotsize), new Vector2D(tint.x - dotsize, tint.y + dotsize), Color.Magenta, false);
		}
	}

	// This makes vertices for a line
	public void RenderLine(Vector2D s, Vector2D e, Color c, bool arrow)
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
