/********************************************************************\
*                                                                   *
*  Configuration class by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using System.IO;
using System.Text;
using System.Globalization;
using System.Collections;
using System.Collections.Specialized;

namespace CodeImp
{
	// Struct for IP Ranges
	public struct IPRangeInfo
	{
		// Complete constructor
		public IPRangeInfo(long pfrom, long pto, string pccode1,
						   string pccode2, string pcountry)
		{
			from = pfrom;
			to = pto;
			ccode1 = pccode1.ToCharArray();
			ccode2 = pccode2.ToCharArray();
			country = pcountry;
		}
		
		// Members
		public long from;
		public long to;
		public char[] ccode1;
		public char[] ccode2;
		public string country;
	}
}
