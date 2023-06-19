/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using System.Numerics;
using SharpDX;
using SharpDX.Direct3D9;
using SharpDX.Mathematics.Interop;

namespace CodeImp.Bloodmasters.Client
{
	public abstract class Debris : VisualObject
	{
		#region ================== Constants

		private const int FADEOUT_DELAY = 10000;
		private const int RANDOM_DELAY = 5000;
		private const float FADEOUT_SPEED = 0.002f;
		private const float GRAVITY = 0.02f;
		private const int ROTATE_MIN_DELAY = 10;
		private const int ROTATE_RANDOM_DELAY = 50;
		private const float RADIUS = 0.4f;
		private const float SHADOW_SIZE = 1.8f;
		private const float SHADOW_ALPHA_MUL = 1f;
		private const float RESIZE_SCALE = 0.03f;
		private const int MAX_DELAY = 10000;
		private const int FIND_SECTOR_INTERLEAVE = 20;

		#endregion

		#region ================== Variables

		// Position/velocity
		protected Vector3D vel;
		protected Sector sector = null;
		private bool disposed = false;
		private int findsectorinterleave;

		// Appearance
		private Sprite sprite = null;
		private Texture texture = null;
		private float size = 3.5f;
		private float fade = 1f;
		private float size_floor = 0f;
		private int rotatespeed;
		private int changedir;
		private int direction;
		private int nextdirtime;
		private RawMatrix texdirmatrix;
		private int fadeouttime = int.MaxValue;
		private bool foudeoutset = false;
		protected bool collisions = true;
		private bool stopped = false;

		#endregion

		#region ================== Properties

		public bool Disposed { get { return disposed; } }
		public float Size { get { return size; } set { size = value; } }
		public bool Stopped { get { return stopped; } }

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public Debris(Vector3D pos, Vector3D vel)
		{
			// Setup position and velocity
			this.pos = pos;
			this.vel = vel;

			// Set up sprite
			sprite = new Sprite(pos, size, true, true);
			sprite.RotateX = (float)Math.PI * 0.7f;

			// Where are we now?
			sector = General.map.GetSubSectorAt(pos.x, pos.y).Sector;
			size_floor = sector.CurrentFloor;

			// Set maximum timeout
			fadeouttime = General.currenttime + MAX_DELAY;

			// Find sector interleave
			findsectorinterleave = General.random.Next(FIND_SECTOR_INTERLEAVE);

			// Set up rotation
			rotatespeed = ROTATE_MIN_DELAY + General.random.Next(ROTATE_RANDOM_DELAY);
			if(General.random.Next(100) < 50) changedir = -1; else changedir = 1;
			direction = General.random.Next(16);
			nextdirtime = General.currenttime + rotatespeed;
			texdirmatrix = DirectionCellMatrix(direction);
		}

		// Disposer
		public override void Dispose()
		{
			// Clean up
			disposed = true;
			base.Dispose();
			GC.SuppressFinalize(this);
		}

		#endregion

		#region ================== Methods

		// This starts a fadeout
		protected void FadeOut()
		{
			if(!foudeoutset)
			{
				// Set fade out time
				fadeouttime = General.currenttime + FADEOUT_DELAY + General.random.Next(RANDOM_DELAY);
				foudeoutset = true;
			}
		}

		// This sets a texture
		protected void SetTexture(Texture t)
		{
			// Apply texture
			texture = t;
		}

		// This stops the movement
		public void StopMoving()
		{
			// Stop moving
			stopped = true;
			vel = new Vector3D(0f, 0f, 0f);
		}

		// This stops the debris from rotating
		public void StopRotating()
		{
			// Stop rotating
			nextdirtime = 0;
		}

		// This finds the highest sector
		public void FindCurrentSector()
		{
			// Sector where we are now
			sector = General.map.GetSubSectorAt(pos.x, pos.y).Sector;
		}

		// Processes the debris and disposes it when decayed
		public override void Process()
		{
			object hitobj = null;
			float resizeextra, ur = 2f, ul = 2f;
			Vector3D newpos = new Vector3D();
			Vector3D hitpos = new Vector3D();

			// Not disposed already?
			if(!disposed)
			{
				// Not stopped?
				if(!stopped)
				{
					// Check if colliding
					newpos = pos + vel;
					if(General.map.FindRayMapCollision(pos, newpos, ref hitpos, ref hitobj, ref ur, ref ul))
					{
						// Raise collide event
						Collide(hitobj);
						if(disposed) return;
					}
					else
					{
						// Apply new position
						pos = newpos;
					}

					// Apply gravity
					vel.z -= GRAVITY;

					// Outside the map?
					if(!General.map.WithinBoundaries(pos.x, pos.y))
					{
						// Dispose
						this.Dispose();
						return;
					}

					// Find highest sector?
					if(++findsectorinterleave == FIND_SECTOR_INTERLEAVE)
					{
						// Find current sector now
						FindCurrentSector();

						// Reset interleave
						findsectorinterleave = 0;
					}

					// Underneath a floor?
					if(sector.CurrentFloor > (pos.z - 1f))
					{
						// Collision
						Collide(sector);
						if(disposed) return;
					}

					// Above a ceiling?
					if(sector.HasCeiling && (pos.z > sector.HeightCeil) &&
											(pos.z < sector.FakeHeightCeil))
					{
						// Collision
						Collide(sector);
						if(disposed) return;
					}
				}
				else
				{
					// Stay on the floor
					pos.z = sector.CurrentFloor;
				}
			}

			// Not disposed already?
			if(!disposed)
			{
				// Time to rotate?
				if((nextdirtime > 0) && (nextdirtime < General.currenttime))
				{
					// Rotate now
					direction += changedir;
					if(direction < 0) direction = 15;
					if(direction > 15) direction = 0;

					// Make texture matrix for this direction
					texdirmatrix = DirectionCellMatrix(direction);

					// Next rotation time
					nextdirtime += rotatespeed;
				}
			}

			// Fade out?
			if(General.currenttime > fadeouttime) fade -= FADEOUT_SPEED;
			if(fade <= 0f) this.Dispose();

			// Not disposed already?
			if(!disposed)
			{
				// Detemrine resize scale
				resizeextra = (pos.z - size_floor) * RESIZE_SCALE;
				if(resizeextra < 0f) resizeextra = 0f;
				if(resizeextra > 0.6f) resizeextra = 0.6f;

				// Update the sprite
				sprite.Size = size + resizeextra;
				sprite.Position = pos + new Vector3D(0f, 0f, 0.4f);
				sprite.Update();
				sector = sprite.Sector;
			}
		}

		// This makes a texture matrix for a given direction number
		private Matrix DirectionCellMatrix(int dirnumber)
		{
            Matrix cell;

			// Determine cell x and y
			float cellx = dirnumber % 4;
			float celly = dirnumber / 4;

			// Make the matrix for the cell
			cell = Matrix.Identity;
			cell *= Matrix.Scaling(0.25f, 0.25f, 1f);
			cell *= Direct3D.MatrixTranslateTx(cellx * 0.25f, celly * 0.25f);

			// Return result
			return cell;
		}

		// Called when colliding
		public virtual void Collide(object hitobj)
		{
		}

		// This renders a shadow
		public override void RenderShadow()
		{
			// Within the map?
			if((sector != null) && !disposed)
			{
				// Check if in screen
				if(sector.VisualSector.InScreen)
				{
					// Render the shadow
					Shadow.RenderAt(pos.x, pos.y, sector.CurrentFloor, SHADOW_SIZE,
						Shadow.AlphaAtHeight(sector.CurrentFloor, pos.z + (pos.z - sector.CurrentFloor) * 0.5f) * SHADOW_ALPHA_MUL * fade);
				}
			}
		}

		// This renders the debris
		public override void Render()
		{
			// Within the map?
			if((sector != null) && !disposed)
			{
				// Check if in screen
				if(sector.VisualSector.InScreen)
				{
					// Set render mode
					Direct3D.SetDrawMode(DRAWMODE.NLIGHTMAPALPHA);
					Direct3D.d3dd.SetRenderState(RenderState.TextureFactor, General.ARGB(fade, 1f, 1f, 1f));

					// Set texture
					Direct3D.d3dd.SetTexture(0, texture);
					Direct3D.d3dd.SetTexture(1, sector.VisualSector.Lightmap);
					Direct3D.d3dd.SetTransform(TransformState.Texture0, texdirmatrix);

					// Render the sprite
					sprite.Render();
				}
			}
		}

		#endregion
	}
}
