/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using System.Drawing;
using System.Collections;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using CodeImp.Bloodmasters;
using CodeImp;

namespace CodeImp.Bloodmasters.Client
{
	public abstract class Item : VisualObject
	{
		#region ================== Constants
		
		// Rendering
		private const float ITEM_ANGLE_FLOOR_X = (float)Math.PI * -0.5f;
		private const float ITEM_ANGLE_X = (float)Math.PI * -0.25f;
		private const float ITEM_ANGLE_Z = (float)Math.PI * 0.25f;
		private const float ITEM_SCALE_XY = 0.05f;
		private const float ITEM_SCALE_Z = 0.06f;
		private const float ITEM_BOB_AMOUNT = 0.6f;
		private const float ITEM_BOB_FLOAT = 1f;
		private const float ITEM_BOB_ALPHA_AMOUNT = 0.05f;
		private const float ITEM_SHADOW_ALPHA = 0.7f;
		private const float ITEM_SHADOW_SCALE = 0.02f;
		private const float ITEM_SHADOW_SIZE = 1.4f;
		private const float ITEM_SHADOW_OFFSET_X = -0.2f;
		private const float ITEM_SHADOW_OFFSET_Y = 0.2f;
		
		#endregion
		
		#region ================== Variables
		
		// References
		private Sector sector = null;
		
		// Identification
		private string key;
		public static int uniquekeyindex = 0;
		
		// Size in mappixels
		private float width;
		private float height;
		
		// Other properties
		private bool visible = true;
		private bool bob = true;
		private float boboffset;
		private bool temporary = false;
		private string sound = "";
		private bool onfloor = true;
		private float spriteoffset = 0f;
		
		// Animation
		private Animation animation = null;
		
		// Description
		private string description = "";
		
		// Geometry
		private Matrix spritescalerotate;
		private Matrix lightmapoffsets = Matrix.Identity;
		private Matrix dynlightmapoffsets = Matrix.Identity;
		
		// Respawn
		private bool taken = false;
		private int respawntime;
		private int respawndelay;
		private bool willrespawn = true;
		
		// Attach
		private bool attached = false;
		private Client owner = null;
		
		#endregion
		
		#region ================== Properties
		
		public string Key { get { return key; } }
		public bool Visible { get { return visible; } set { visible = value; } }
		public bool Bob { get { return bob; } set { bob = value; } }
		public bool OnFloor { get { return onfloor; } set { onfloor = value; } }
		public int RespawnDelay { get { return respawndelay; } set { respawndelay = value; } }
		public bool IsTaken { get { return taken; } }
		public bool IsAttached { get { return attached; } }
		public Sector Sector { get { return sector; } }
		public string Description { get { return description; } set { description = value; } }
		public bool Temporary { get { return temporary; } set { temporary = value; } }
		public Client Owner { get { return owner; } }
		
		#endregion
		
		#region ================== Constructor / Destructor
		
		// Constructor from Thing
		public Item(Thing t)
		{
			// Create key from thing index
			key = "T" + t.Index;
			
			// From thing
			Initialize(t.X, t.Y, t.Sector.CurrentFloor + t.Z);
		}
		
		// Constructor
		public Item(float ix, float iy, float iz)
		{
			// Create a unique key
			key = "U" + uniquekeyindex++;
			
			// From coordinates
			Initialize(ix, iy, iz);
		}
		
		// This initializes the item
		private void Initialize(float ix, float iy, float iz)
		{
			Matrix spritescale, spriterotate;
			string anifile = "";
			
			// Check if class has a ClientItem attribute
			if(Attribute.IsDefined(this.GetType(), typeof(ClientItem), false))
			{
				// Get item attribute
				ClientItem attr = (ClientItem)Attribute.GetCustomAttribute(this.GetType(), typeof(ClientItem), false);
				
				// Copy settings from attribute
				anifile = attr.Sprite;
				description = attr.Description;
				visible = attr.Visible;
				bob = attr.Bob;
				temporary = attr.Temporary;
				sound = attr.Sound;
				onfloor = attr.OnFloor;
				spriteoffset = attr.SpriteOffset;
			}
			
			// Other
			boboffset = (float)General.random.NextDouble() * 1000f;
			
			// Create animation
			if(anifile != "")
			{
				if(!Animation.IsLoaded("sprites/" + anifile)) Animation.Load("sprites/" + anifile);
				animation = Animation.CreateFrom("sprites/" + anifile);
				
				// Determine sprite size
				width = animation.CurrentFrame.info.Width * ITEM_SCALE_XY;
				height = animation.CurrentFrame.info.Height * ITEM_SCALE_Z;
			}
			else
			{
				width = 1f;
				height = 1f;
			}
			
			// Scale sprite
			spritescale = Matrix.Scaling(width, 1f, height);
			
			// Rotate sprite
			Matrix rot1 = Matrix.RotationX(ITEM_ANGLE_X);
			Matrix rot2 = Matrix.RotationZ(ITEM_ANGLE_Z);
			spriterotate = Matrix.Multiply(rot1, rot2);
			
			// Combine scale and rotation
			spritescalerotate = Matrix.Multiply(spritescale, spriterotate);
			
			// Move into position
			this.Move(ix, iy, iz);
		}
		
		// Dispose
		public override void Dispose()
		{
			// Clean up
			sector = null;
			
			// Let base class dispose
			base.Dispose();
			GC.SuppressFinalize(this);
		}
		
		#endregion
		
		#region ================== Control
		
		// This attaches the item to a client
		public virtual void Attach(Client c)
		{
			// Attach item to client
			attached = true;
			owner = c;
			owner.Carrying = this;
		}
		
		// This detaches the item from a client
		public virtual void Detach()
		{
			// Detach item from client
			if(owner != null) owner.Carrying = null;
			attached = false;
			owner = null;
		}
		
		// This changes the item animation
		public void ChangeAnimation(string anifile)
		{
			// Change the animation
			animation = Animation.CreateFrom("sprites/" + anifile);
		}
		
		// Use only this function to move the item
		public void Move(float nx, float ny, float nz)
		{
			// Find the new sector
			Sector newsec = General.map.GetSubSectorAt(nx, ny).Sector;
			if(newsec != sector)
			{
				// Sector changes!
				if(sector != null) sector.RemoveItem(this);
				newsec.AddItem(this);
				sector = newsec;
			}
			
			// Apply new coordinates
			pos = new Vector3D(nx, ny, nz);
			
			// Update matrices
			this.Update();
		}
		
		// This updates matrices
		public void Update()
		{
			// Get positions on lightmap
			float lx = sector.VisualSector.LightmapScaledX(pos.x);
			float ly = sector.VisualSector.LightmapScaledY(pos.y);
			
			// Make the lightmap matrix
			lightmapoffsets = Matrix.Identity;
			lightmapoffsets *= Matrix.Scaling(width * sector.VisualSector.LightmapScaleX, 0f, 1f);
			lightmapoffsets *= Matrix.RotationZ(ITEM_ANGLE_Z);
			lightmapoffsets *= Matrix.Scaling(1f, sector.VisualSector.LightmapAspect, 1f);
			lightmapoffsets *= Direct3D.MatrixTranslateTx(lx, ly);
			
			// Make the dynamic lightmap matrix
			dynlightmapoffsets = Direct3D.MatrixTranslateTx(pos.x, pos.y);
		}
		
		// This is called when the item is taken
		public virtual void Take(Client clnt) { }
		
		// This is called when the item is being picked up
		public void Pickup(Client clnt, int delay, bool attach, bool silent)
		{
			// Keep the respawn delay
			respawndelay = delay;
			
			// Play pickup sound if any is assigned
			if((sound != "") && !silent && sector.VisualSector.InScreen) DirectSound.PlaySound(sound, pos);
			
			// Call corresponding method
			if(attach)
			{
				// Attach to client
				this.Attach(clnt);
			}
			else
			{
				// If attached, detach
				if(this.IsAttached) this.Detach();
				
				// Take item and set new respawn time
				taken = true;
				respawntime = General.currenttime + respawndelay;
				willrespawn = (respawndelay != Consts.NEVER_RESPAWN_TIME);
			}
			
			// Call take method
			this.Take(clnt);
		}
		
		// This respawns the item
		public virtual void Respawn(bool playsound)
		{
			// Play item respawn sound
			if(playsound && sector.VisualSector.InScreen) DirectSound.PlaySound("itemrespawn.wav", pos);
			
			// Make respawn effect
			if(attached || taken) RespawnEffect();
			
			// Show the item
			taken = false;
		}
		
		// This spawns the respawn effect
		protected virtual void RespawnEffect()
		{
			// In screen?
			if(sector.VisualSector.InScreen)
			{
				// Add particles
				for(int i = 0; i < 10; i++)
					General.arena.p_magic.Add(pos + Vector3D.Random(General.random, 1f, 1f, 1f),
						Vector3D.Random(General.random, 0.05f, 0.05f, 0.3f), Color.Gray.ToArgb());
			}
		}
		
		// Do I still have to explain what this is for?
		public override void Process()
		{
			// Process animation
			if(animation != null) animation.Process();
			
			// Time to respawn?
			if(taken && willrespawn && (respawntime < General.currenttime)) Respawn(true);
			
			// Drop to floor?
			if(onfloor) pos.z = sector.CurrentFloor;
		}
		
		#endregion
		
		#region ================== Rendering
		
		// This renders the shadow of the item
		public override void RenderShadow()
		{
			float bobalpha = ITEM_SHADOW_ALPHA;
			float sx, sy;
			
			// Check if in screen
			if(sector.VisualSector.InScreen)
			{
				// Item visible and not taken?
				if(visible && !taken)
				{
					// Determine bob
					if(bob)
					{
						// Bob settings
						bobalpha -= (float)(1f + Math.Sin(General.currenttime * 0.01f + boboffset)) * ITEM_BOB_ALPHA_AMOUNT;
						sx = pos.x;
						sy = pos.y;
					}
					else
					{
						// Normal settings
						sx = pos.x + ITEM_SHADOW_OFFSET_X;
						sy = pos.y + ITEM_SHADOW_OFFSET_Y;
					}
					
					// Render the shadow
					Shadow.RenderAt(sx, sy, sector.CurrentFloor, ITEM_SHADOW_SIZE + ITEM_SHADOW_SCALE * (float)animation.CurrentFrame.info.Width, bobalpha);
				}
			}
		}
		
		// This renders the item
		public override void Render()
		{
			float bobz = pos.z;
			
			// Check if in screen
			if(sector.VisualSector.InScreen)
			{
				// Item visible and not taken?
				if(visible && !taken && (animation != null))
				{
					// Set render mode
					Direct3D.SetDrawMode(DRAWMODE.NLIGHTMAPALPHA);
					Direct3D.d3dd.RenderState.ZBufferWriteEnable = false;
					
					// Not transparent
					Direct3D.d3dd.RenderState.TextureFactor = -1;
					
					// Determine bob Z
					if(bob) bobz += (float)(ITEM_BOB_FLOAT + Math.Sin(General.currenttime * 0.01f + boboffset)) * ITEM_BOB_AMOUNT;
					
					// Position the item
					Matrix apos = Matrix.Translation(pos.x - spriteoffset, pos.y + spriteoffset, bobz + spriteoffset);
					
					// Apply world matrix
					Direct3D.d3dd.Transform.World = Matrix.Multiply(spritescalerotate, apos);
					
					// Apply lightmap matrix
					Direct3D.d3dd.Transform.Texture0 = Matrix.Identity;
					Direct3D.d3dd.Transform.Texture1 = lightmapoffsets;
					Direct3D.d3dd.Transform.Texture2 = dynlightmapoffsets * General.arena.LightmapMatrix;
					
					// Set the item texture and vertices stream
					Direct3D.d3dd.SetTexture(0, animation.CurrentFrame.texture);
					Direct3D.d3dd.SetStreamSource(0, Sprite.Vertices, 0, MVertex.Stride);
					
					// Set the lightmap from visual sector
					Direct3D.d3dd.SetTexture(1, sector.VisualSector.Lightmap);
					
					// Render it!
					Direct3D.d3dd.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
				}
			}
		}
		
		#endregion
	}
}
