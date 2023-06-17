/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using Vortice.Direct3D9;

namespace CodeImp.Bloodmasters.Launcher
{
	public struct DisplayAdapterItem
	{
		public int ordinal;
		public string description;

		// Constructor
		public DisplayAdapterItem(int adapterIndex, AdapterIdentifier ai)
		{
			ordinal = adapterIndex;
			description = ai.Description;
		}

		// String representation
		public override string ToString()
		{
			return description;
		}
	}
}
