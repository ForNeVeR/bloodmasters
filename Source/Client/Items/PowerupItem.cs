/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;

namespace CodeImp.Bloodmasters.Client;

[AttributeUsage(AttributeTargets.Class, AllowMultiple=true, Inherited=false)]
public class PowerupItem : Attribute
{
    // Members
    private float r;
    private float g;
    private float b;

    // Properties
    public float R { get { return r; } set { r = value; } }
    public float G { get { return g; } set { g = value; } }
    public float B { get { return b; } set { b = value; } }

    // Constructor
    public PowerupItem()
    {
    }
}
