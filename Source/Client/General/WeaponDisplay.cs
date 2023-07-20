using System;
using System.Drawing;
using System.Globalization;
using SharpDX.Direct3D9;

namespace CodeImp.Bloodmasters.Client
{
	public class WeaponDisplay : IDisposable
	{
		#region ================== Constants

		private const int STAY_TIME = 600;
		private const float FADE_OUT_SPEED = 0.05f;
		private const float SPACING = 0f;
		private const float BOX_WIDTH = 0.09f;
		private const float BOX_HEIGHT = 0.06f;
		private const float BOX_TOP = 0.79f;
		private const float SEL_WIDTH = 0.09f;
		private const float SEL_HEIGHT = 0.06f;
		private const float SEL_TOP = 0.79f;
		private const float AMMO_OFFSET = 0f; //-0.01f;

		#endregion

		#region ================== Variables

		// Weapon textures
		private TextureResource[] weaponicons = new TextureResource[(int)WEAPON.TOTAL_WEAPONS];

		// Ammo texts
		private TextResource[] ammotexts = new TextResource[(int)WEAPON.TOTAL_WEAPONS];

		// Icon vertices
		private TLVertex[][] iconvertices = new TLVertex[(int)WEAPON.TOTAL_WEAPONS][];

		// Selection
		private TLVertex[] selection;
		private TextureResource seltexture;

		// Appearance
		private float alpha;
		private int disappeartime;

		// Updating
		private bool updateammo = true;

		#endregion

		#region ================== Properties

		#endregion

		#region ================== Constructor / Disposer

		// Constructor
		public WeaponDisplay()
		{
			string tempfile;
			int i;

			// Weapon textures
			for(i = 0; i < (int)WEAPON.TOTAL_WEAPONS; i++)
			{
				int weaponnum = i + 1;
				tempfile = ArchiveManager.ExtractFile("General.rar/weapon" + weaponnum.ToString(CultureInfo.InvariantCulture) + "icon.tga");
				weaponicons[i] = Direct3D.LoadTexture(tempfile, true);
			}

			// Selection texture
			tempfile = ArchiveManager.ExtractFile("General.rar/white.bmp");
			seltexture = Direct3D.LoadTexture(tempfile, true);
		}

		// Disposer
		public void Dispose()
		{
		}

		#endregion

		#region ================== Methods

		// This updates geometry
		public void UpdateWeaponSet()
		{
			TLVertex[] box;
			float x = 0;
			float totalwidth;
			int weapons = 0;

			// Local client required
			if(General.localclient == null) return;

			// Count the weapons
			foreach(Weapon w in General.localclient.AllWeapons) if(w != null) weapons++;

			// Calculate total width of the bar
			totalwidth = ((float)weapons * BOX_WIDTH) + ((float)(weapons - 1) * SPACING);

			// Determine start position
			x = (1f - totalwidth) * 0.5f;

			// Make the geometry and ammo texts
			for(int i = 0; i < weapons; i++)
			{
				// Setup box
				box = Direct3D.TLRect(x * Direct3D.DisplayWidth,
				                      BOX_TOP * Direct3D.DisplayHeight,
				                      (x + BOX_WIDTH) * Direct3D.DisplayWidth,
				                      (BOX_TOP + BOX_HEIGHT) * Direct3D.DisplayHeight,
				                      128f, 64f);

				// Put in array
				iconvertices[i] = box;

				// Setup ammo text
				ammotexts[i] = Direct3D.CreateTextResource(General.charset_shaded);
				ammotexts[i].Texture = General.font_shaded.texture;
				ammotexts[i].HorizontalAlign = TextAlignX.Center;
				ammotexts[i].VerticalAlign = TextAlignY.Top;
				ammotexts[i].Viewport = new RectangleF(x, BOX_TOP + BOX_HEIGHT + AMMO_OFFSET, BOX_WIDTH, 1f);
				ammotexts[i].Colors = TextResource.color_brighttext;
				ammotexts[i].Scale = 0.4f;

				// Next
				x += BOX_WIDTH + SPACING;
			}

			// This also requires an update of ammo and selection
			UpdateAmmo();
			UpdateSelection();
		}

		// This updates the ammos
		public void UpdateAmmo()
		{
			// Indicate that ammo needs updating
			updateammo = true;
		}

		// This really updates ammo
		private void DoUpdateAmmo()
		{
			int i = 0;

			// Local client required
			if(General.localclient == null) return;

			// Go for all the weapons
			foreach(Weapon w in General.localclient.AllWeapons)
			{
				// Weapon available?
				if(w != null)
				{
					// Apply the ammo text
					ammotexts[i].Text = General.localclient.Ammo[(int)w.AmmoType].ToString();
					i++;
				}
			}

			// Ammo updated
			updateammo = false;
		}

		// This updates the selection
		public void UpdateSelection()
		{
			float x;
			float totalwidth;
			int weapons = 0;
			int selected = 0;
			Weapon selweap;

			// Local client required
			if(General.localclient == null) return;

			// Which weapon to select?
			if(General.localclient.SwitchToWeapon != null)
				selweap = General.localclient.SwitchToWeapon;
			else
				selweap = General.localclient.CurrentWeapon;

			// Go for all the weapons
			foreach(Weapon w in General.localclient.AllWeapons)
			{
				// Weapon available?
				if(w != null)
				{
					// Same as selected?
					if(w == selweap) selected = weapons;

					// Count the weapon
					weapons++;
				}
			}

			// Calculate total width of the bar
			totalwidth = ((float)weapons * BOX_WIDTH) + ((float)(weapons - 1) * SPACING);

			// Calculate selection position
			x = ((1f - totalwidth) * 0.5f) + (selected * (BOX_WIDTH + SPACING));

			// Make the selection box
			selection = Direct3D.TLRect(x * Direct3D.DisplayWidth,
										SEL_TOP * Direct3D.DisplayHeight,
										(x + SEL_WIDTH) * Direct3D.DisplayWidth,
										(SEL_TOP + SEL_HEIGHT) * Direct3D.DisplayHeight,
										32f, 32f);
		}

		// This shows the weapon selection
		public void Show()
		{
			alpha = 1f;
			disappeartime = General.currenttime + STAY_TIME;
		}

		#endregion

		#region ================== Processing

		// Processing
		public void Process()
		{
			// Visible?
			if(alpha > 0f)
			{
				// Fade out?
				if(disappeartime < General.currenttime)
				{
					// Fade out
					alpha -= FADE_OUT_SPEED;
				}
			}
		}

		#endregion

		#region ================== Rendering

		// Rendering
		public void Render()
		{
			int i = 0;
			float c;

			// Local client required
			if(General.localclient == null) return;

			// Visible?
			if(alpha > 0f)
			{
				// Ammo needs updating?
				if(updateammo) DoUpdateAmmo();

				// Set drawing mode
				Direct3D.SetDrawMode(DRAWMODE.TLMODALPHA);

				// Render selection
				Direct3D.d3dd.SetTexture(0, seltexture.texture);
				Direct3D.d3dd.SetRenderState(RenderState.TextureFactor, General.ARGB(alpha * 0.5f, 0.7f, 0.7f, 0.7f));
				Direct3D.d3dd.DrawUserPrimitives(PrimitiveType.TriangleStrip, 2, selection);

				// Go for all the weapons
				foreach(Weapon w in General.localclient.AllWeapons)
				{
					// Weapon available?
					if(w != null)
					{
						// Any ammo in this weapon?
						if(General.localclient.Ammo[(int)w.AmmoType] > 0) c = 1f; else c = 0.4f;

						// Render weapon
						Direct3D.d3dd.SetTexture(0, weaponicons[(int)w.WeaponID].texture);
						Direct3D.d3dd.SetRenderState(RenderState.TextureFactor, General.ARGB(alpha * c, 1f, 1f, 1f));
						Direct3D.d3dd.DrawUserPrimitives(PrimitiveType.TriangleStrip, 2, iconvertices[i]);

						// Render ammo text
						ammotexts[i].Render();

						// Count box index
						i++;
					}
				}
			}
		}

		#endregion
	}
}
