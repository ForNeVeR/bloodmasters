/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using CodeImp;

namespace CodeImp.Bloodmasters
{
	public class NetMessageComparer : IComparer
	{
		bool reversed = false;
		
		// Constructor
		public NetMessageComparer(bool reversed)
		{
			// Apply settings
			this.reversed = reversed;
		}
		
		// Compare two NetMessages
		public int Compare(object x, object y)
		{
			// Get proper message objects
			NetMessage m1 = (NetMessage)x;
			NetMessage m2 = (NetMessage)y;
			
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
}
