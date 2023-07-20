/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using System.Drawing;
using System.Collections;
using CodeImp.Bloodmasters;
using CodeImp;

#if CLIENT
using CodeImp.Bloodmasters.Client;
#endif

namespace CodeImp.Bloodmasters.Server
{
	public abstract class DynamicSector
	{
		// Static stuff
		public static bool sendsectormovements = false;
		
		// Members
		private bool sendsectorupdate = false;
		protected Sector sector;
		
		// Properties
		public bool SendSectorUpdate
		{
			get { return sendsectorupdate; }
			set
			{
				DynamicSector.sendsectormovements |= value;
				sendsectorupdate = value;
			}
		}
		
		// Constructor
		public DynamicSector(Sector sector)
		{
			// Get references
			this.sector = sector;
		}
		
		// Disposer
		public virtual void Dispose()
		{
			// Clean up
			sector = null;
		}
		
		// Processer
		public virtual void Process()
		{
		}
		
		// This adds information for a sector update
		public void AddSectorMovement(NetMessage msg)
		{
			// Add movement info to message
			msg.AddData((int)sector.Index);
			msg.AddData((float)sector.TargetFloor);
			msg.AddData((float)sector.ChangeSpeed);
		}
	}
}
