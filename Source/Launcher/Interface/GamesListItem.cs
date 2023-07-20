/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System.Drawing;
using System.Net;
using System.Windows.Forms;

namespace CodeImp.Bloodmasters.Launcher
{
	public class GamesListItem
	{
		#region ================== Constants

		// Number of subitems to create in a ListViewItem
		private const int NUM_SUB_ITEMS = 9;

		// Milliseconds at which ping is completely colored red
		private const float PING_RED = 250;

		// Brightness of ping colors (0 = full bright, 255 = black)
		private const float PING_BRIGHTNESS = 125;

		#endregion

		#region ================== Variables

		// Members
		private IPEndPoint address;
		private bool changed;
		private int revision;
		private ServerBrowser browser;

		// Standard info
		private string title;
		private string mapname;
		private string website;
		private int maxplayers;
		private int maxclients;
		private int numplayers;
		private int numclients;
		private GAMETYPE gametype;
		private bool locked;
		private int ping;
		private int protocol;
		private IPRangeInfo cinfo;
		private int flagindex;

		// Extended info
		private int fraglimit;
		private int timelimit;
		private bool joinsmallest;
		private string[] playername;
		private TEAM[] playerteam;
		private bool[] playerspect;
		private int[] playerping;
		private string builddesc;

		#endregion

		#region ================== Properties

		public IPEndPoint Address { get { return address; } }
		public string Title { get { return title; } }
		public string Website { get { return website; } }
		public string MapName { get { return mapname; } }
		public int Players { get { return numplayers; } }
		public int Clients { get { return numclients; } }
		public int MaxPlayers { get { return maxplayers; } }
		public int MaxClients { get { return maxclients; } }
		public GAMETYPE GameType { get { return gametype; } }
		public bool Locked { get { return locked; } }
		public int Ping { get { return ping; } }
		public int Protocol { get { return protocol; } }
		public bool IsFull { get { return (numplayers >= maxplayers); } }
		public bool IsEmpty { get { return (numclients == 0); } }
		public int Fraglimit { get { return fraglimit; } }
		public int Timelimit { get { return timelimit; } }
		public bool JoinSmallest { get { return joinsmallest; } }
		public string[] PlayerName { get { return playername; } }
		public TEAM[] PlayerTeam { get { return playerteam; } }
		public bool[] PlayerSpectator { get { return playerspect; } }
		public int[] PlayerPing { get { return playerping; } }
		public bool Changed { get { return changed; } }
		public int Revision { get { return revision; } }
		public string BuildDescription { get { return builddesc; } }

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public GamesListItem(ServerBrowser browser)
		{
			// New
			this.changed = true;
			this.revision = 0;
			this.browser = browser;
		}

		#endregion

		#region ================== Methods

		// This sends a new query to the server
		public void Refresh()
		{
			// Send new query to this address
			browser.QueryServer(address);
		}

		// This updates a ListViewItem
		public void UpdateListViewItem(ListViewItem item)
		{
			// Incorrect protocol?
			if(protocol != Gateway.PROTOCOL_VERSION)
			{
				// Show red icon
				item.StateImageIndex = 1;
			}
			// Locked server?
			else if(locked)
			{
				// Show locked icon
				item.StateImageIndex = 0;
			}
			else
			{
				// No icon
				item.StateImageIndex = -1;
			}

			// Show server info
			item.Text = title;
			item.ImageIndex = flagindex;
			item.SubItems[1].ForeColor = MakePingColor((float)ping);
			item.SubItems[1].Text = ping + "ms";
			item.SubItems[2].Text = numplayers + " / " + maxplayers;
			item.SubItems[3].Text = numclients + " / " + maxclients;
			item.SubItems[4].Text = gametype.ToString();
			item.SubItems[5].Text = mapname;

			// Check if the map exists and apply color
			if(General.mainwindow.CheckMapExists(mapname))
				item.SubItems[5].ForeColor = SystemColors.WindowText;
			else
				item.SubItems[5].ForeColor = Color.FromArgb(200, 0, 0);

			// These subitems are used for sorting
			item.SubItems[6].Text = ping.ToString("0000");
			item.SubItems[7].Text = numplayers.ToString("0000");
			item.SubItems[8].Text = numclients.ToString("0000");

			// For reference
			item.SubItems[9].Text = address.ToString();

			// Assume updated
			changed = false;
		}

		// This makes a ListViewItem
		public void NewListViewItem(ListView listview)
		{
			// Make new item
			ListViewItem item = new ListViewItem();

			// Make all subitems
			for(int i = 0; i < NUM_SUB_ITEMS; i++) item.SubItems.Add("");

			// Subitems can have own styles
			item.UseItemStyleForSubItems = false;

			// Update item
			UpdateListViewItem(item);

			// Add to the list
			listview.Items.Add(item);
		}

		// This updates this item with info from a message
		public void Update(NetMessage msg)
		{
			// Keep address
			address = msg.Address;

			// Read all standard server info
			int timesig = msg.GetInt();
			title = msg.GetString();
			locked = msg.GetBool();
			website = msg.GetString();
			maxclients = msg.GetByte();
			maxplayers = msg.GetByte();
			gametype = (GAMETYPE)msg.GetByte();
			mapname = msg.GetString();
			numclients = msg.GetByte();
			numplayers = msg.GetByte();
			protocol = msg.GetByte();
			cinfo = General.ip2country.LookupIP(msg.Address.Address.ToString());
			flagindex = General.mainwindow.GetFlagIconIndex(cinfo.ccode1);

			// Check if the protocol is correct
			// Different protocols may have different
			// extended information
			if(protocol == Gateway.PROTOCOL_VERSION)
			{
				// Read all extended server info
				fraglimit = msg.GetShort();
				timelimit = msg.GetShort();
				joinsmallest = msg.GetBool();

				// Make arrays
				playername = new string[numclients];
				playerteam = new TEAM[numclients];
				playerspect = new bool[numclients];
				playerping = new int[numclients];

				// Read all players info
				for(int i = 0; i < numclients; i++)
				{
					// Player info
					playername[i] = msg.GetString();
					playerteam[i] = (TEAM)msg.GetByte();
					playerspect[i] = msg.GetBool();
					playerping[i] = msg.GetShort();
				}

				// Read build description
				if(!msg.EndOfMessage) builddesc = msg.GetString();
			}

			// Calculate ping
			ping = SharedGeneral.GetCurrentTime() - timesig;
			if(ping > 999) ping = 999;

			// Changed
			changed = true;
			revision++;
		}

		// This makes a color for the given ping
		public static Color MakePingColor(float ping)
		{
			const float halfred = PING_RED / 2f;
			float r, g;

			// Ping lower than half the "red ping"?
			if(ping < halfred)
			{
				// Make green-yellow ping color
				r = ping / halfred * 255f;
				g = 255f;
			}
			else
			{
				// Make yellow-red ping color
				r = 255f;
				g = 255f - (ping - halfred) / halfred * 255f;
			}

			// Apply ping color brightness
			r -= PING_BRIGHTNESS / 1.5f;
			g -= PING_BRIGHTNESS / 1.0f;

			// Clip color to boundaries
			if(r < 0f) r = 0f; else if(r > 255f) r = 255f;
			if(g < 0f) g = 0f; else if(g > 255f) g = 255f;

			// Return color
			return Color.FromArgb((int)r, (int)g, 0);
		}

		#endregion
	}
}
