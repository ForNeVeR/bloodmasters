/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using CodeImp.Bloodmasters.LevelMap;

namespace CodeImp.Bloodmasters.Client.Items;

[ClientItem(9004, Visible=false, OnFloor=false)]
public class TeleportParticles : Item
{
    #region ================== Constants

    private const int SPAWN_MIN_INTERVAL = 30;
    private const int SPAWN_RND_INTERVAL = 100;

    #endregion

    #region ================== Variables

    private int nextspawntime;

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public TeleportParticles(Thing t) : base(t)
    {
    }

    #endregion

    #region ================== Processing

    // Processing
    public override void Process()
    {
        // Time to spawn a particle?
        if(nextspawntime < SharedGeneral.currenttime)
        {
            // In screen?
            if(Sector.VisualSector.InScreen)
            {
                // Spawn particle
                General.arena.p_magic.Add(pos + Vector3D.Random(General.random, 3f, 3f, 3f),
                    Vector3D.Random(General.random, 0.04f, 0.04f, 0.5f), -1);

                // Make new spawn time
                nextspawntime = SharedGeneral.currenttime + SPAWN_MIN_INTERVAL + General.random.Next(SPAWN_RND_INTERVAL);
            }
        }

        // Pass control to base class
        base.Process();
    }

    #endregion
}
