/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using System.Globalization;
using System.Reflection;
using Bloodmasters.Client.Graphics;
using Bloodmasters.Client.Lights;

namespace Bloodmasters.Client.Weapons;

public abstract class Weapon : VisualObject
{
    #region ================== Constants

    private const float FLARE_OFFSET_X = 1f;
    private const float FLARE_OFFSET_Y = -1f;
    private const float FLARE_OFFSET_Z = 10f;
    private const float FLARE_DELTA_ANGLE = 0.43f;
    private const float FLARE_DISTANCE = 3.8f;

    #endregion

    #region ================== Variables

    // Weapon properties
    protected WEAPON weaponid;
    protected int refiredelay;
    protected string description;
    protected string sound;
    protected Client client;
    protected AMMO ammotype;
    protected int useammo;

    // Weapon status
    protected int refiretime;

    // Other members
    protected DynamicLight light;

    #endregion

    #region ================== Properties

    public WEAPON WeaponID { get { return weaponid; } }
    public Client Client { get { return client; } }
    public int RefireDelay { get { return refiredelay; } }
    public string Description { get { return description; } }
    public AMMO AmmoType { get { return ammotype; } }
    public int UseAmmo { get { return useammo; } }

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public Weapon(Client client)
    {
        // Keep references
        this.client = client;
        this.renderbias = 10f;

        // Check if class has a WeaponInfo attribute
        if(Attribute.IsDefined(this.GetType(), typeof(WeaponInfo), false))
        {
            // Get weapon attributes
            WeaponInfo attr = (WeaponInfo)Attribute.GetCustomAttribute(this.GetType(), typeof(WeaponInfo), false);

            // Copy settings from attribute
            weaponid = attr.WeaponID;
            description = attr.Description;
            refiredelay = attr.RefireDelay;
            sound = attr.Sound;
            useammo = attr.UseAmmo;
            ammotype = attr.AmmoType;
        }

        // Make the dynamic light
        light = new DynamicLight(this.pos, 12f, -1, 3);
        light.Visible = false;
    }

    // Dispose
    public override void Dispose()
    {
        // Clean up
        client = null;
        light.Dispose();

        // Dispose base
        base.Dispose();
        GC.SuppressFinalize(this);
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

    // This determines the flare position
    public static Vector3D GetFlarePosition(Actor a)
    {
        // Make rounded angle of actor
        float rangle = Actor.AngleFromDir(Actor.DirFromAngle(a.AimAngle, 0, 16), 0, 16);
        rangle += FLARE_DELTA_ANGLE * (float)Math.PI;

        // Position flare
        return a.Position +
               new Vector3D(FLARE_OFFSET_X, FLARE_OFFSET_Y, FLARE_OFFSET_Z) +
               Vector3D.FromAnimationAngle(rangle, FLARE_DISTANCE);
    }

    // This is called when the weapon (re)fires
    protected abstract void ShootOnce();

    // Processes the weapon
    public override void Process()
    {
        // Move weapon to player location
        if(client.Actor != null)
        {
            this.pos = client.Actor.Position;
            light.Position = Weapon.GetFlarePosition(client.Actor);
        }
    }

    // Call this hwen the trigger is released
    public virtual void Released()
    {
    }

    // Call this when the trigger is being pulled
    public virtual void Trigger()
    {
        // Check if weapon can fire
        if(refiretime < SharedGeneral.currenttime)
        {
            // Weapon is shooting
            ShootOnce();

            // Set the new refire time
            refiretime = SharedGeneral.currenttime + refiredelay;
        }
    }

    // This checks if a weapon is idle and can be switched
    public virtual bool IsIdle()
    {
        // Return true when done reloading
        return (refiretime < SharedGeneral.currenttime);
    }

    #endregion
}
