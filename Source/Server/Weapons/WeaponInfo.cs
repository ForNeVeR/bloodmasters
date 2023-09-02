/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

namespace CodeImp.Bloodmasters.Server;

[AttributeUsage(AttributeTargets.Class, AllowMultiple=true, Inherited=false)]
public class WeaponInfo : Attribute
{
    // Members
    private WEAPON weaponid;
    private int refiredelay;
    private AMMO ammotype;
    private int initialammo;
    private int useammo;
    private string description = "";

    // Properties
    public WEAPON WeaponID { get { return weaponid; } }
    public int RefireDelay { get { return refiredelay; } set { refiredelay = value; } }
    public AMMO AmmoType { get { return ammotype; } set { ammotype = value; } }
    public int InitialAmmo { get { return initialammo; } set { initialammo = value; } }
    public int UseAmmo { get { return useammo; } set { useammo = value; } }
    public string Description { get { return description; } set { description = value; } }

    // Constructor
    public WeaponInfo(WEAPON weaponid)
    {
        // Keep the weapon number
        this.weaponid = weaponid;
    }
}
