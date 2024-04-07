/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

namespace Bloodmasters.Server;

[AttributeUsage(AttributeTargets.Class, AllowMultiple=true, Inherited=false)]
public class ProjectileInfo : EntityAttribute
{
    // Members
    private PROJECTILE type;

    // Properties
    public PROJECTILE Type { get { return type; } }

    // Constructor
    public ProjectileInfo(PROJECTILE type)
    {
        // Keep the weapon number
        this.type = type;
    }
}
