namespace CodeImp.Bloodmasters.Server;

public class ServerWallCollision : WallCollision
{
    public ServerWallCollision(Linedef ld, Vector3D objpos, Vector2D objvec, float objradius, float objheight, float stepheight, bool objisplayer) : base(ld, objpos, objvec, objradius, objheight, stepheight, objisplayer)
    {
    }

    public override Collision Update(Vector3D objpos, Vector2D objvec)
    {
        // Make new collision with updated parameters
        return new ServerWallCollision(this.line, objpos, objvec, this.objradius, this.objheight, this.stepheight, this.objisplayer);
    }
}
