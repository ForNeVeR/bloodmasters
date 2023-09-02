/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

namespace CodeImp.Bloodmasters.Client;

[ClientItem(8005, Sprite="ammo_fuel.tga",
    Description="Fuel",
    Sound="pickuphealth.wav")]
public class AmmoFuel : Item
{
    #region ================== Constants

    #endregion

    #region ================== Variables

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public AmmoFuel(Thing t) : base(t)
    {
    }

    #endregion
}
