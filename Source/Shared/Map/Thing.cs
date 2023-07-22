/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

namespace CodeImp.Bloodmasters
{
	public class Thing
	{
		#region ================== Constants

		#endregion

		#region ================== Variables

		private int index;
		private float x;
		private float y;
		private float z;
		private int type;
		private float angle;
		private int tag;
		private THINGFLAG flags;
		private ACTION action;			// Thing action (usage depends on thing type)
		private int[] arg;				// Thing arguments (usage depends on thing type or action)
		private Sector sector = null;
		private Map map;

		#endregion

		#region ================== Properties

		public int Index { get { return index; } }
		public int Type { get { return type; } }
		public float X { get { return x; } }
		public float Y { get { return y; } }
		public float Z { get { return z; } }
		public float Angle { get { return angle; } }
		public int Tag { get { return tag; } }
		public THINGFLAG Flags { get { return flags; } }
		public ACTION Action { get { return action; } }
		public int[] Arg { get { return arg; } }
		public Sector Sector{ get { return sector; } }

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public Thing(BinaryReader data, int index, Map map)
		{
			// Read thing
			this.map = map;
			this.index = index;
			tag = data.ReadInt16();
			x = (float)data.ReadInt16() * Map.MAP_SCALE_XY;
			y = (float)data.ReadInt16() * Map.MAP_SCALE_XY;
			z = (float)data.ReadInt16() * Map.MAP_SCALE_Z;
			angle = (float)data.ReadInt16() / (360f / ((float)Math.PI * 2f));
			type = data.ReadInt16();
			flags = (THINGFLAG)data.ReadUInt16();
			action = (ACTION)data.ReadByte();
			arg = new int[5];
			for(int k = 0; k < 5; k++) arg[k] = data.ReadByte();
		}

		// Destructor
		public void Dispose()
		{
			// Clean up
			map = null;
			sector = null;
		}

		#endregion

		#region ================== Methods

		// This determines the sector where the thing is in
		public void DetermineSector()
		{
			Sidedef s;
			Linedef l;

			// Get nearest linedef
			l = map.GetNearestLine(x, y);

			// Determine side of line
			float side = l.SideOfLine(x, y);
			if(side < 0) s = l.Front; else s = l.Back;

			// Determine sector
			if(s != null) sector = s.Sector;
		}


		#endregion
	}
}
