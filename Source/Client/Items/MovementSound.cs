/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using CodeImp.Bloodmasters.Map;

namespace CodeImp.Bloodmasters.Client.Items;

[ClientItem(9999, Temporary=true)]
public class MovementSound : Item
{
    // Constructor
    public MovementSound(Thing t) : base(t)
    {
        // Indicate that this sector must play movement sounds
        ((ClientSector)t.Sector).PlayMovementSound = true;
    }
}
