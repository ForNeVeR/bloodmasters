/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using CodeImp.Bloodmasters.Map;

namespace CodeImp.Bloodmasters.Client.Items;

[ClientItem(8004, Sprite="ammo_grenades.tga",
    Description="Grenades",
    Sound="pickuphealth.wav")]
public class AmmoGrenades : Item
{
    #region ================== Constants

    #endregion

    #region ================== Variables

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public AmmoGrenades(Thing t) : base(t)
    {
    }

    #endregion
}
