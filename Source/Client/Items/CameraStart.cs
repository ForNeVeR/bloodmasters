/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using Bloodmasters.LevelMap;

namespace Bloodmasters.Client.Items;

[ClientItem(32000, Temporary=true)]
public class CameraStart : Item
{
    #region ================== Constants

    #endregion

    #region ================== Variables

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public CameraStart(Thing t) : base(t)
    {
        // Move the camera
        General.arena.SetCamera(new Vector2D(t.X, t.Y));
    }

    #endregion
}
