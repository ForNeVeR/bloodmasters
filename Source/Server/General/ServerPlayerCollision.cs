namespace Bloodmasters.Server;

public class ServerPlayerCollision : PlayerCollision
{
    public ServerPlayerCollision(IPhysicsState pl, Vector3D objpos, Vector2D objvec, float objradius, bool objisplayer) : base(pl, objpos, objvec, objradius, objisplayer)
    {
    }

    public override Collision Update(Vector3D objpos, Vector2D objvec)
    {
        // Make new collision with updated parameters
        return new ServerPlayerCollision(this.player, objpos, objvec, this.objradius, this.objisplayer);
    }
}
