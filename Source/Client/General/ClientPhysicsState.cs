namespace CodeImp.Bloodmasters.Client;

public class ClientPhysicsState : PhysicsState
{
    public ClientPhysicsState(Map map) : base(map)
    {
    }

    protected override bool IsClientMode => true;
}
