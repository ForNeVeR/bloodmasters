/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using Bloodmasters.LevelMap;

namespace Bloodmasters.Server;

public class Flag : Item
{
    #region ================== Constants

    // Time when flag auto-returns
    private const int AUTO_RETURN_DELAY = 20000;

    #endregion

    #region ================== Variables

    // Original location
    private Vector3D origpos;

    // Team info
    protected TEAM thisteam;
    protected TEAM otherteam;

    // Auto-return time
    private int returntime;

    #endregion

    #region ================== Properties

    public TEAM ThisTeam { get { return thisteam; } }
    public TEAM OtherTeam { get { return otherteam; } }

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public Flag(Thing t) : base(t)
    {
        // Keep original position
        this.origpos = this.Position;

        // If this is not a CTF game, remove the flags
        if(Host.Instance.Server.GameType != GAMETYPE.CTF) this.Temporary = true;
    }

    #endregion

    #region ================== Control

    // This is called when the item is being touched by a player
    public override void Pickup(Client c)
    {
        // Do what you have to do
        base.Pickup(c);

        // Check if this client can steal the flag
        if(c.Team == otherteam)
        {
            // Carry the flag
            this.Attach(c);
        }
        else
        {
            // Check if flag is not at original position
            Vector3D dpos = this.Position - this.origpos;
            if(dpos.LengthSq() > 0.01f)
            {
                // Return the flag
                Host.Instance.Server.BroadcastReturnFlag(c, this);
                this.Return();
            }
            else
            {
                // Check if client is carrying a flag
                // (that can only be the opponent flag, we
                // already checked the player's team above)
                if(c.Carrying is Flag)
                {
                    // Get the other flag
                    Flag carryflag = (Flag)c.Carrying;

                    // SCORE!
                    Host.Instance.Server.BroadcastScoreFlag(c, carryflag);
                    carryflag.Return();
                    c.AddToScore(1);
                }
            }
        }
    }

    // When processed
    public override void Process()
    {
        // Process base
        base.Process();

        // Check if attached
        if(this.IsAttached)
        {
            // Check if owner is alive
            if(this.Owner.IsAlive)
            {
                // Move flag with client
                Vector3D pos = this.Owner.State.pos;
                this.Move(pos.x, pos.y, pos.z);
            }
            else
            {
                // Detach flag
                this.Detach();
            }
        }
        else
        {
            // Check if flag is not at original position
            Vector3D dpos = this.Position - this.origpos;
            if(dpos.LengthSq() > 0.01f)
            {
                // Time to return the flag?
                if(returntime < SharedGeneral.currenttime)
                {
                    // Auto-return the flag
                    Host.Instance.Server.BroadcastReturnFlag(null, this);
                    this.Return();
                }
            }
        }
    }

    // This detaches the flag
    public override void Detach()
    {
        // Detach flag
        base.Detach();
        returntime = SharedGeneral.currenttime + AUTO_RETURN_DELAY;
    }

    // This returns the flag
    private void Return()
    {
        // Return
        base.Detach();
        this.Move(origpos.x, origpos.y, origpos.z);
    }

    // When respawn is called
    public override void Respawn()
    {
        // Return the flags
        this.Return();
    }

    #endregion
}
