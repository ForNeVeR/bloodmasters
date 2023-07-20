/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using System.Collections;
using SharpDX.Direct3D9;

namespace CodeImp.Bloodmasters.Client
{
	public class SectorShadow
	{
		#region ================== Constants

		public const float FALL_ANGLE = (float)Math.PI * 1.2f;
		public const float FALL_LENGTH = 5f;
		public const float OFFSET_X = 0f;
		public const float OFFSET_Y = 0.1f;
		public const float DIFF_MIN = 0f;
		public const float DIFF_ALPHA = 0.04f;

		#endregion

		#region ================== Variables

		private int vertexoffset;
		private Sector frontsector = null;
		private Sector backsector = null;

		#endregion

		#region ================== Properties

		public Sector FrontSector { get { return frontsector; } }
		public Sector BackSector { get { return backsector; } }

		#endregion

		#region ================== Constructor / Destructor

		// Moo!
		public SectorShadow()
		{
			GC.SuppressFinalize(this);
		}

		// This makes a sector shadow from a line
		public bool MakeSectorShadow(ArrayList vertices, VisualSector vs, Linedef l)
		{
			TLVertex v;
			float fall_x, fall_y;

			// Get linedef vertices
			Vector2D v1 = General.map.Vertices[l.v1];
			Vector2D v2 = General.map.Vertices[l.v2];

			// Determine shadow coordinates
			fall_x = (float)Math.Sin(FALL_ANGLE) * FALL_LENGTH;
			fall_y = (float)Math.Cos(FALL_ANGLE) * FALL_LENGTH;

			// Determine other sector
			if(l.SideOfLine(v1.x + fall_x, v1.y + fall_y) < 0)
			{
				// Shadow is on front side
				if(l.Front != null) frontsector = l.Front.Sector;
				if(l.Back != null) backsector = l.Back.Sector;
			}
			else
			{
				// Shadow is on back side
				if(l.Back != null) frontsector = l.Back.Sector;
				if(l.Front != null) backsector = l.Front.Sector;
			}

			// Sectors available?
			if((frontsector != null) && (backsector != null))
			{
				// Keep the offset
				vertexoffset = vertices.Count;

				// Left top
				v = new TLVertex();
				v.x = vs.LightmapScaledX(v1.x + fall_x + OFFSET_X) * (float)vs.LightmapSize;
				v.y = vs.LightmapScaledY(v1.y + fall_y + OFFSET_Y) * (float)vs.LightmapSize;
				v.tu = 0.25f;
				v.tv = 0f;
				v.rhw = 1f;
				vertices.Add(v);

				// Right top
				v = new TLVertex();
				v.x = vs.LightmapScaledX(v2.x + fall_x + OFFSET_X) * (float)vs.LightmapSize;
				v.y = vs.LightmapScaledY(v2.y + fall_y + OFFSET_Y) * (float)vs.LightmapSize;
				v.tu = 0.75f;
				v.tv = 0f;
				v.rhw = 1f;
				vertices.Add(v);

				// Left bottom
				v = new TLVertex();
				v.x = vs.LightmapScaledX(v1.x - fall_x + OFFSET_X) * (float)vs.LightmapSize;
				v.y = vs.LightmapScaledY(v1.y - fall_y + OFFSET_Y) * (float)vs.LightmapSize;
				v.tu = 0.25f;
				v.tv = 1f;
				v.rhw = 1f;
				vertices.Add(v);

				// Right bottom
				v = new TLVertex();
				v.x = vs.LightmapScaledX(v2.x - fall_x + OFFSET_X) * (float)vs.LightmapSize;
				v.y = vs.LightmapScaledY(v2.y - fall_y + OFFSET_Y) * (float)vs.LightmapSize;
				v.tu = 0.75f;
				v.tv = 1f;
				v.rhw = 1f;
				vertices.Add(v);

				// Success
				return true;
			}
			else
			{
				// No back sector
				return false;
			}
		}

		#endregion

		#region ================== Methods

		// This renders the shadow
		public void Render()
		{
			float diff;
			float a;

			// Difference in sectors?
			if(backsector.CurrentFloor != frontsector.CurrentFloor)
			{
				// Determine height difference
				diff = Math.Abs(backsector.CurrentFloor - (frontsector.CurrentFloor + DIFF_MIN));

				// Determine alpha value
				a = diff * DIFF_ALPHA;
				if(a > 1f) a = 1f;

				// Render the shadow
				Direct3D.d3dd.SetRenderState(RenderState.TextureFactor, General.ARGB(a, 1f, 1f, 1f));
				Direct3D.d3dd.DrawPrimitives(PrimitiveType.TriangleStrip, vertexoffset, 2);
			}
		}

		#endregion
	}
}
