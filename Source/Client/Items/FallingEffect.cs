/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using Bloodmasters.Client.Sound;
using Bloodmasters.LevelMap;

namespace Bloodmasters.Client.Items;

[ClientItem(9002, Visible=false, OnFloor=false)]
public class FallingEffect : Item
{
    #region ================== Constants

    #endregion

    #region ================== Variables

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public FallingEffect(Thing t) : base(t)
    {
    }

    #endregion

    #region ================== Processing

    // Processing
    public override void Process()
    {
        // Go for all clients
        foreach(Client c in General.clients)
        {
            // Client playing?
            if((c != null) && (c.Actor != null))
            {
                // Client in this sector and below me?
                if(c.Actor.HighestSector == this.Sector)
                {
                    // Calculate alpha
                    float a = (c.Actor.Position.z - this.Sector.CurrentFloor) / (this.pos.z - this.Sector.CurrentFloor);
                    if(a > 1f) a = 1f; else if(a < 0f) a = 0f;

                    // Scream if not screaming yet
                    if((a < 1f) && !c.Actor.FallSoundPlayed)
                    {
                        // Scream for me baby
                        SoundSystem.PlaySound("falling.wav", c.Actor.Position);
                        c.Actor.FallSoundPlayed = true;
                    }

                    // Remove player name below certain level
                    if(a < 0.8f) c.Actor.Name = "";

                    // Apply brightness to actor
                    c.Actor.Alpha = a;
                }
            }
        }

        // Pass control to base class
        base.Process();
    }

    #endregion
}
