/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using System.Collections.Generic;
using CodeImp.Bloodmasters.Client.Graphics;
using SharpDX;
using SharpDX.Direct3D9;

namespace CodeImp.Bloodmasters.Client
{
	public class DynamicLight
	{
		#region ================== Constants

		#endregion

		#region ================== Variables

		// Static light templates
		public static TextureResource[] lightimages = new TextureResource[StaticLight.NUM_LIGHT_TEMPLATES];

		// Settings
		public static bool dynamiclights = true;

		// Properties
		private int basecolor;
		private int color;
		private int template;
		private bool visible;
		private float range;
		private Vector3D pos;
		private bool disposed = false;

		#endregion

		#region ================== Properties

		protected int BaseColor { get { return basecolor; } }
		public int Color { get { return color; } set { color = value; } }
		public bool Visible { get { return visible; } set { if(visible != value) { visible = value; } } }
		public Vector3D Position { get { return pos; } set { pos = value; } }
		public bool Disposed { get { return disposed; } }

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public DynamicLight(Vector3D pos, float range, int basecolor, int template)
		{
			// Properties for all lights
			this.pos = pos;
			this.range = range;
			this.basecolor = basecolor;
			this.color = basecolor;
			this.visible = true;
			this.template = template;

			// Check template
			if((template < 0) || (template >= StaticLight.NUM_LIGHT_TEMPLATES)) throw(new Exception("Invalid light template specified (" + template + ")"));

			// Add the light
			if(DynamicLight.dynamiclights) General.arena.DynamicLights.Add(this);
		}

		// Destructor
		public virtual void Dispose()
		{
			// Remove the light
			if(DynamicLight.dynamiclights) General.arena.DynamicLights.Remove(this);
			GC.SuppressFinalize(this);
		}

		#endregion

		#region ================== Processing

		// Process this light
		public virtual void Process() { }

		#endregion

		#region ================== Rendering

		// This renders the light
		public void Render()
		{
			bool[] donevsectors = new bool[General.arena.VisualSectors.Count];
			Matrix lightinworld;
			float sa;
			int sc;

			// Only when using dynamic lights
			if(!DynamicLight.dynamiclights) return;

			// Light visible?
			if(visible)
			{
				// Set drawing mode
				Direct3D.SetDrawMode(DRAWMODE.NLIGHTBLEND);

				// Matrix to position light texture in world coordinates
				lightinworld = Matrix.Identity;
				lightinworld *= Direct3D.MatrixTranslateTx(-pos.x + range, -pos.y + range);
				lightinworld *= Matrix.Scaling(1f / (range * 2f), 1f / (range * 2f), 1f);
				Direct3D.d3dd.SetTransform(TransformState.Texture0, lightinworld);

				// Set the light texture
				//Direct3D.d3dd.SetTexture(0, DynamicLight.lightimages[template].texture);

				// Get all the nearby lines to check for intersection
                List<Linedef> lines = General.map.BlockMap.GetCollisionLines(pos.x, pos.y, range);
				List<Sector> sectors = new List<Sector>(lines.Count * 2);

				// Go for all lines
				foreach(Linedef ld in lines)
				{
					// Touching both sectors
					if(ld.Front != null) sectors.Add(ld.Front.Sector);
					if(ld.Back != null) sectors.Add(ld.Back.Sector);
				}

				// Not touching any sectors?
				// Then do a simple subsector intersection test
				if(sectors.Count == 0) sectors.Add(General.map.GetSubSectorAt(pos.x, pos.y).Sector);

				// Go for all sectors
                foreach(ClientSector s in sectors)
				{
					// Visual Sector not done yet?
					if((s.VisualSector != null) && s.VisualSector.InScreen && !donevsectors[s.VisualSector.Index])
					{
						// Set the light color
						sa = StaticLight.CalculateHeightAlpha(s, pos.z);
						sc = ColorOperator.Scale(color, sa);
						Direct3D.d3dd.SetRenderState(RenderState.TextureFactor, sc);

						// Render the sector geometry
						s.VisualSector.RenderFlat();

						// Done this visual sector
						donevsectors[s.VisualSector.Index] = true;
					}
				}
			}
		}

		#endregion
	}
}
