/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using System.Drawing;
using System.Globalization;
using System.Collections;
using CodeImp.Bloodmasters;
using CodeImp;

namespace CodeImp.Bloodmasters.Client
{
	public class ClientComparer : IComparer
	{
		// Comparer method
		public int Compare(object x, object y)
		{
			int teama, teamb;
			int scorea, scoreb;
			int fragsa, fragsb;
			
			// Get client objects
			Client a = (Client)x;
			Client b = (Client)y;
			
			// Make team/spectator index
			teama = Scoreboard.GetSectionIndex(a);
			teamb = Scoreboard.GetSectionIndex(b);
			
			// Make reversed values
			scorea = 9999 - a.Score;
			scoreb = 9999 - b.Score;
			fragsa = 9999 - a.Frags;
			fragsb = 9999 - b.Frags;
			
			// Make comparable strings
			string sa = teama.ToString() + "_" + scorea.ToString("0000000") + "_" + fragsa.ToString("0000") + "_" + a.Deaths.ToString("0000") + "_" + a.Name + "_" + a.ID.ToString("00");
			string sb = teamb.ToString() + "_" + scoreb.ToString("0000000") + "_" + fragsb.ToString("0000") + "_" + b.Deaths.ToString("0000") + "_" + b.Name + "_" + b.ID.ToString("00");
			
			// Compare and return result
			return string.Compare(sa, sb);
		}
	}
}
