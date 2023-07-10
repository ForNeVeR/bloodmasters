namespace CodeImp.Bloodmasters.Server;

public class ServerPhysicsState : PhysicsState
{
    public ServerPhysicsState(Bloodmasters.Map map) : base(map)
    {
    }

    protected override bool IsClientMode => false;
}
