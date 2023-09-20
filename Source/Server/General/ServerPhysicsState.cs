using CodeImp.Bloodmasters.LevelMap;

namespace CodeImp.Bloodmasters.Server;

public class ServerPhysicsState : PhysicsState
{
    public ServerPhysicsState(Map map) : base(map)
    {
    }

    protected override PlayerCollision CreatePlayerCollision(IPhysicsState plr, Vector3D sv)
    {
        return new ServerPlayerCollision(plr, pos, sv, radius, isplayer);
    }

    protected override WallCollision CreateWallCollision(Linedef ld, Vector3D sv, float stepheight)
    {
        return new ServerWallCollision(ld, pos, sv, radius, height, stepheight, isplayer);
    }

    protected override bool IsClientMode => false;
}
