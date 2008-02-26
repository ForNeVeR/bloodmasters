/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using System.Drawing;
using System.Globalization;
using System.Collections;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using CodeImp.Bloodmasters;
using CodeImp;

namespace CodeImp.Bloodmasters.Client
{
	public class WindowBorder
	{
		#region ================== Constants
		
		private const float MIDDLE_LENGTH = 1.6f;
		private const float T0 = 0.01f;
		private const float T1 = 0.279f;
		private const float T2 = 0.724f;
		private const float T3 = 1f; //0.996;
		
		#endregion
		
		#region ================== Variables
		
		public static TextureResource texture;
		private VertexBuffer vertices;
		private RectangleF pos;
		private float bsize;
		private int faces;
		
		#endregion
		
		#region ================== Constructor / Destructor
		
		// Constructor
		public WindowBorder(float left, float top, float width, float height, float bordersize)
		{
			// Settings
			this.pos = new RectangleF(left, top, width, height);
			this.bsize = bordersize;
			
			// Make geometry
			CreateGeometry();
		}
		
		// This disposes the object
		public void Dispose()
		{
			// Clean up
			DestroyGeometry();
			GC.SuppressFinalize(this);
		}
		
		#endregion
		
		#region ================== Geometry
		
		// This creates geometry
		public void CreateGeometry()
		{
			ArrayList verts = new ArrayList();
			RectangleF ots, ins;
			int numw, numh, i;
			float patchw, patchh, barsize, blocksize;
			
			// Make sure old stuff is discarded
			DestroyGeometry();
			
			// Calculate bar size 
			barsize = (bsize * MIDDLE_LENGTH) * Direct3D.DisplayWidth;
			blocksize = bsize * Direct3D.DisplayWidth;
			
			// Calculate outside rectangle
			ots = new RectangleF(pos.X * Direct3D.DisplayWidth, pos.Y * Direct3D.DisplayHeight,
							pos.Width * Direct3D.DisplayWidth, pos.Height * Direct3D.DisplayHeight);
			
			// Calculate inside rectangle
			ins = new RectangleF(pos.X * Direct3D.DisplayWidth + blocksize, pos.Y * Direct3D.DisplayHeight + blocksize,
							pos.Width * Direct3D.DisplayWidth - blocksize * 2f, pos.Height * Direct3D.DisplayHeight - blocksize * 2f);
			
			// Calculate number of bars horizontal and vertical
			numw = (int)Math.Floor(ins.Width / barsize);
			numh = (int)Math.Floor(ins.Height / barsize);
			
			// Calculate patch size of bars horizontal and vertical
			patchw = ins.Width  - (ins.Width * numw);
			patchh = ins.Height  - (ins.Height * numh);
			
			// Make left top corner
			verts.AddRange(Direct3D.TLRectL(ots.Left, ots.Top, ins.Left, ins.Top, T0, T0, T1, T1));
			
			// Make right top corner
			verts.AddRange(Direct3D.TLRectL(ins.Right, ots.Top, ots.Right, ins.Top, T2, T0, T3, T1));
			
			// Make left bottom corner
			verts.AddRange(Direct3D.TLRectL(ots.Left, ins.Bottom, ins.Left, ots.Bottom, T0, T2, T1, T3));
			
			// Make right bottom corner
			verts.AddRange(Direct3D.TLRectL(ins.Right, ins.Bottom, ots.Right, ots.Bottom, T2, T2, T3, T3));
			
			// Go for all horizontal bars
			for(i = 0; i < numw; i++)
			{
				// Make upper horizontal bar
				verts.AddRange(Direct3D.TLRectL(ins.Left + (float)i * barsize, ots.Top, ins.Left + (float)(i + 1) * barsize, ins.Top, T1, T0, T2, T1));
				
				// Make lower horizontal bar
				verts.AddRange(Direct3D.TLRectL(ins.Left + (float)i * barsize, ins.Bottom, ins.Left + (float)(i + 1) * barsize, ots.Bottom, T1, T2, T2, T3));
			}
			
			// Make upper horizontal patch
			verts.AddRange(Direct3D.TLRectL(ins.Left + (float)numw * barsize, ots.Top, ins.Right, ins.Top, T1, T0, T2, T1));
			
			// Make lower horizontal patch
			verts.AddRange(Direct3D.TLRectL(ins.Left + (float)numw * barsize, ins.Bottom, ins.Right, ots.Bottom, T1, T2, T2, T3));
			
			// Go for all vertical bars
			for(i = 0; i < numh; i++)
			{
				// Make left vertical bar
				verts.AddRange(Direct3D.TLRectL(ots.Left, ins.Top + (float)i * barsize, ins.Left, ins.Top + (float)(i + 1) * barsize, T0, T1, T1, T2));
				
				// Make right vertical bar
				verts.AddRange(Direct3D.TLRectL(ins.Right, ins.Top + (float)i * barsize, ots.Right, ins.Top + (float)(i + 1) * barsize, T2, T1, T3, T2));
			}
			
			// Make left vertical patch
			verts.AddRange(Direct3D.TLRectL(ots.Left, ins.Top + (float)numh * barsize, ins.Left, ins.Bottom, T0, T1, T1, T2));
			
			// Make right vertical patch
			verts.AddRange(Direct3D.TLRectL(ins.Right, ins.Top + (float)numh * barsize, ots.Right, ins.Bottom, T2, T1, T3, T2));
			
			// Make inner area
			verts.AddRange(Direct3D.TLRectL(ins.Left, ins.Top, ins.Right, ins.Bottom, T1, T1, T2, T2));
			
			// Keep number of traingles
			faces = verts.Count / 3;
			
			// Create vertex buffer
			vertices = new VertexBuffer(typeof(TLVertex), verts.Count, Direct3D.d3dd,
						Usage.WriteOnly, TLVertex.Format, Pool.Default);
			
			// Lock vertex buffer
			TLVertex[] vertsarray = (TLVertex[])vertices.Lock(0, typeof(TLVertex), LockFlags.None, verts.Count);
			
			// Fill the vertex buffer
			verts.CopyTo(vertsarray);
			
			// Done filling the vertex buffer
			vertices.Unlock();
		}
		
		// This cleans up the vertex buffer
		public void DestroyGeometry()
		{
			// Clean up if needed
			if(vertices != null)
			{
				vertices.Dispose();
				vertices = null;
			}
		}
		
		#endregion
		
		#region ================== Rendering
		
		// This renders the window
		public void Render()
		{
			// Render the poly
			Direct3D.SetDrawMode(DRAWMODE.TLMODALPHA);
			Direct3D.d3dd.RenderState.TextureFactor = -1;
			Direct3D.d3dd.SetTexture(0, WindowBorder.texture.texture);
			Direct3D.d3dd.SetStreamSource(0, vertices, 0, TLVertex.Stride);
			Direct3D.d3dd.DrawPrimitives(PrimitiveType.TriangleList, 0, faces);
		}
		
		#endregion
	}
}
