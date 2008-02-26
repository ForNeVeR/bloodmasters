/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Diagnostics;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using CodeImp.Bloodmasters;
using CodeImp;

namespace CodeImp.Bloodmasters.Launcher
{
	public struct DisplayAdapterItem
	{
		public int ordinal;
		public string description;
		
		// Constructor
		public DisplayAdapterItem(AdapterInformation ai)
		{
			ordinal = ai.Adapter;
			description = ai.Information.Description;
		}
		
		// String representation
		public override string ToString()
		{
			return description;
		}
	}
}
