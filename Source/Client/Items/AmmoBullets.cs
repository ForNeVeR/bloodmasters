/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using CodeImp.Bloodmasters.LevelMap;

namespace CodeImp.Bloodmasters.Client.Items;

[ClientItem(8001, Sprite="ammo_bullets.tga",
    Description="Chaingun Ammo",
    Sound="pickuphealth.wav")]
public class AmmoBullets : Item
{
    #region ================== Constants

    #endregion

    #region ================== Variables

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public AmmoBullets(Thing t) : base(t)
    {
    }

    #endregion
}
