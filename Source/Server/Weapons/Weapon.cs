/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using System.Globalization;
using System.Reflection;
#if CLIENT
#endif

namespace CodeImp.Bloodmasters.Server
{
	public abstract class Weapon
	{
		#region ================== Constants

		#endregion

		#region ================== Variables

		// Weapon properties
		protected WEAPON weaponid;
		protected int refiredelay;
		protected string description;
		protected Client client;
		protected AMMO ammotype;
		protected int initialammo;
		protected int useammo;

		// Weapon status
		protected int refiretime;

		#endregion

		#region ================== Properties

		public WEAPON WeaponID { get { return weaponid; } }
		public Client Client { get { return client; } }
		public int RefireDelay { get { return refiredelay; } }
		public string Description { get { return description; } }
		public AMMO AmmoType { get { return ammotype; } }
		public int InitialAmmo { get { return initialammo; } }
		public int UseAmmo { get { return useammo; } }

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public Weapon(Client client)
		{
			// Keep references
			this.client = client;

			// Check if class has a WeaponInfo attribute
			if(Attribute.IsDefined(this.GetType(), typeof(WeaponInfo), false))
			{
				// Get weapon attributes
				WeaponInfo attr = (WeaponInfo)Attribute.GetCustomAttribute(this.GetType(), typeof(WeaponInfo), false);

				// Copy settings from attribute
				weaponid = attr.WeaponID;
				description = attr.Description;
				refiredelay = attr.RefireDelay;
				ammotype = attr.AmmoType;
				initialammo = attr.InitialAmmo;
				useammo = attr.UseAmmo;
			}
		}

		// Dispose
		public virtual void Dispose()
		{
			// Release trigger
			this.Released();

			// Clean up
			client = null;
		}

		#endregion

		#region ================== Methods

		// This creates a weapon by weapon number
		public static Weapon CreateFromID(Client client, WEAPON weaponid)
		{
			// Go for all types in this assembly
			Assembly asm = Assembly.GetExecutingAssembly();
			Type[] asmtypes = asm.GetTypes();
			foreach(Type tp in asmtypes)
			{
				// Check if this type is a class
				if(tp.IsClass && !tp.IsAbstract && !tp.IsArray)
				{
					// Check if class has a WeaponInfo attribute
					if(Attribute.IsDefined(tp, typeof(WeaponInfo), false))
					{
						// Get weapon attribute
						WeaponInfo attr = (WeaponInfo)Attribute.GetCustomAttribute(tp, typeof(WeaponInfo), false);

						// This the weapon we're looking for?
						if(attr.WeaponID == weaponid)
						{
							try
							{
								// Create object from this weapon
								object[] args = new object[1];
								args[0] = client;
								return (Weapon)asm.CreateInstance(tp.FullName, false, BindingFlags.Default,
													null, args, CultureInfo.CurrentCulture, new object[0]);
							}
							// Catch errors
							catch(TargetInvocationException e)
							{
								// Throw the actual exception
								throw(e.InnerException);
							}
						}
					}
				}
			}

			// Nothing found!
			return null;
		}

		// This is called when the weapon (re)fires
		protected abstract void ShootOnce();

		// Call this hwen the trigger is released
		public virtual void Released()
		{
		}

		// Call this when the firing the weapon
		// Returns True when shot, but returns false when out of ammo
		// or when the weapon is still reloading. Use CanShoot to check
		// only if there is enough ammo to fire again.
		public virtual bool Trigger()
		{
			// Check if weapon can fire
			if(refiretime < SharedGeneral.currenttime)
			{
				// Check if the client has ammo
				if(client.UseAmmo(ammotype, useammo))
				{
					// Set the new refire time
					refiretime = SharedGeneral.currenttime + refiredelay;

					// Weapon is shooting
					ShootOnce();
					return true;
				}
			}

			// Cant fire right now
			return false;
		}

		// This checks if there is enough ammo to fire
		public bool CanShoot()
		{
			// Check if the client has ammo
			return client.CheckAmmo(ammotype, useammo);
		}

		// This checks if a weapon is idle and can be switched
		public virtual bool IsIdle()
		{
			// Return true when done reloading
			return (refiretime < SharedGeneral.currenttime);
		}

		#endregion
	}
}
