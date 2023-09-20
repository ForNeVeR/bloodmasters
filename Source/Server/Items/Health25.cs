/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using CodeImp.Bloodmasters.LevelMap;

namespace CodeImp.Bloodmasters.Server;

[ServerItem(2002, RespawnTime=5000)]
public class Health25 : Item
{
    #region ================== Constants

    #endregion

    #region ================== Variables

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public Health25(Thing t) : base(t)
    {
    }

    #endregion

    #region ================== Control

    // This is calledwhen the item is being touched by a player
    public override void Pickup(Client c)
    {
        // Check if the client needs health
        if(c.Health < 100)
        {
            // Do what you have to do
            base.Pickup(c);

            // Take the item
            this.Take(c);

            // Add 25% health to the client
            c.AddToStatus(25, 100, 0, 100);
        }
    }


    #endregion
}
