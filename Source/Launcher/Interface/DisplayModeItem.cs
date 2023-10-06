/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using CodeImp.Bloodmasters.Launcher.General;
using SharpDX.Direct3D9;


namespace CodeImp.Bloodmasters.Launcher.Interface;

public struct DisplayModeItem : IComparable
{
    public DisplayModeEx mode;

    // Constructor
    public DisplayModeItem(DisplayModeEx m)
    {
        mode = m;
    }

    // String representation
    public override string ToString()
    {
        return mode.Width + " x " + mode.Height + " x " +
               CustomDirect3D.GetBitDepth(mode.Format) + " @ " +
               mode.RefreshRate + " Hz";
    }

    // Hash value
    public override int GetHashCode()
    {
        // Make a value for this mode that can be used for comparing
        return ((mode.Width + mode.Height / 2) << 16) | (CustomDirect3D.GetBitDepth(mode.Format) << 10) | mode.RefreshRate;
    }

    // Compare
    public int CompareTo(object obj)
    {
        // Make comparable values
        int m1 = this.GetHashCode();
        int m2 = ((DisplayModeItem)obj).GetHashCode();
        return m1 - m2;
    }
}
