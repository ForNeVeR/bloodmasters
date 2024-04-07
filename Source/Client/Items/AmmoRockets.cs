/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using Bloodmasters.LevelMap;

namespace Bloodmasters.Client.Items;

[ClientItem(8003, Sprite="ammo_rockets.tga",
    Description="Rockets",
    Sound="pickuphealth.wav")]
public class AmmoRockets : Item
{
    #region ================== Constants

    #endregion

    #region ================== Variables

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public AmmoRockets(Thing t) : base(t)
    {
    }

    #endregion
}
