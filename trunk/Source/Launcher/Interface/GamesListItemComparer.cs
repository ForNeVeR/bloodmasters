/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using CodeImp.Bloodmasters;
using CodeImp;

namespace CodeImp.Bloodmasters.Launcher
{
	public class GamesListItemComparer : IComparer
	{
		// Members
		int subitemindex;
		bool ascending;
		
		// Constructor
		public GamesListItemComparer(int subitemindex, bool ascending)
		{
			// Keep settings
			this.subitemindex = subitemindex;
			this.ascending = ascending;
		}
		
		// Comparer
		public int Compare(object x, object y)
		{
			// Get proper objects
			ListViewItem a = (ListViewItem)x;
			ListViewItem b = (ListViewItem)y;
			
			// Compare subitems
			int strcmp = String.Compare(a.SubItems[subitemindex].Text,
								b.SubItems[subitemindex].Text, true);
			
			// Return proper result
			if(ascending) return strcmp; else return -strcmp;
		}
	}
}
