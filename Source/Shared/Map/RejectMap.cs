/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using System.IO;
using System.Collections;
using CodeImp;

namespace CodeImp.Bloodmasters
{
	public class RejectMap
	{
		#region ================== Variables
		
		// Reject table
		private bool[,] reject;
		
		#endregion
		
		#region ================== Constructor / Destructor
		
		// Constructor
		public RejectMap(BinaryReader data, int numsectors)
		{
			int dbit = 8;
			byte dbyte = 0;
			
			// Make reject array
			reject = new bool[numsectors, numsectors];
			
			// Go through the entire reject table
			for(int t = 0; t < numsectors; t++)
			{
				for(int s = 0; s < numsectors; s++)
				{
					// All bits in this byte read?
					if(dbit == 8)
					{
						// Read next byte and reset bit counter
						dbyte = data.ReadByte();
						dbit = 0;
					}
					
					// Fill the reject entry
					reject[s, t] = (dbyte & (1 << dbit)) > 0;
					
					// Next bit
					dbit++;
				}
			}
			
			// Clean up
			data.Close();
		}
		
		// Destructor
		public void Dispose()
		{
			// Clean up
			reject = null;
		}
		
		#endregion
		
		#region ================== Methods
		
		// This consults the reject map for possible visibility
		public bool CanBeVisible(int sector1, int sector2)
		{
			// Return the reject result
			return (reject[sector1, sector2] == false);
		}
		
		// This consults the reject map for invisibility
		public bool IsInvisible(int sector1, int sector2)
		{
			// Return the reject result
			return reject[sector1, sector2];
		}
		
		#endregion
	}
}
