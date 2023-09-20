/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using CodeImp.Bloodmasters.LevelMap;

namespace CodeImp.Bloodmasters.Server;

[ServerItem(2003, RespawnTime=30000)]
public class HealthMega : Item
{
    #region ================== Constants

    #endregion

    #region ================== Variables

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public HealthMega(Thing t) : base(t)
    {
    }

    #endregion

    #region ================== Control

    // This is calledwhen the item is being touched by a player
    public override void Pickup(Client c)
    {
        // Check if the client needs health
        if(c.Health < 200)
        {
            // Do what you have to do
            base.Pickup(c);

            // Take the item
            this.Take(c);

            // Add 25% health to the client
            c.AddToStatus(100, 200, 0, 100);
        }
    }


    #endregion
}
