/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;

namespace CodeImp.Bloodmasters.Client
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple=true, Inherited=false)]
	public class WeaponInfo : Attribute
	{
		// Members
		private WEAPON weaponid;
		private int refiredelay;
		private string sound = "";
		private string description = "";
		private int useammo;
		private AMMO ammotype;

		// Properties
		public WEAPON WeaponID { get { return weaponid; } }
		public int RefireDelay { get { return refiredelay; } set { refiredelay = value; } }
		public string Description { get { return description; } set { description = value; } }
		public string Sound { get { return sound; } set { sound = value; } }
		public int UseAmmo { get { return useammo; } set { useammo = value; } }
		public AMMO AmmoType { get { return ammotype; } set { ammotype = value; } }

		// Constructor
		public WeaponInfo(WEAPON weaponid)
		{
			// Keep the weapon number
			this.weaponid = weaponid;
		}
	}
}
