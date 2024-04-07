/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

namespace Bloodmasters.Net;

public class NetMessageComparer : IComparer<NetMessage>
{
    readonly bool reversed = false;

    // Constructor
    public NetMessageComparer(bool reversed)
    {
        // Apply settings
        this.reversed = reversed;
    }

    // Compare two NetMessages
    public int Compare(NetMessage m1, NetMessage m2)
    {
        // Check if sorting reversed
        if(reversed)
        {
            // Compare difference in size
            if(m1.Length < m2.Length) return 1;
            else if(m1.Length == m2.Length) return 0;
            else return -1;
        }
        else
        {
            // Compare difference in size
            if(m1.Length > m2.Length) return 1;
            else if(m1.Length == m2.Length) return 0;
            else return -1;
        }
    }
}
