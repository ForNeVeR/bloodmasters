using CodeImp.Bloodmasters.Client.Graphics;

namespace CodeImp.Bloodmasters.Client;

public class ClientPhysicsState : PhysicsState
{
    public ClientPhysicsState(Map map) : base(map)
    {
    }

    protected override bool IsClientMode => true;

    protected override PlayerCollision CreatePlayerCollision(IPhysicsState plr, Vector3D sv)
    {
        return new ClientPlayerCollision(plr, pos, sv, radius, isplayer);
    }

    protected override WallCollision CreateWallCollision(Linedef ld, Vector3D sv, float stepheight)
    {
        // Make new collision with updated parameters
        return new ClientWallCollision(ld, pos, sv, radius, height, stepheight, isplayer);
    }
}
