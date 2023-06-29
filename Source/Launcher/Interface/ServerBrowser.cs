/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace CodeImp.Bloodmasters.Launcher
{
	public class ServerBrowser
	{
		#region ================== Variables

		// Events
		public delegate void FilteredListChanged();
		public event FilteredListChanged OnFilteredListChanged;

		// Filter
		private string filtertitle = "";
		private bool filterfull = false;
		private bool filterempty = false;
		private int filtertype = 0;
		private string filtermap = "";

		// Networking
		private Gateway gateway;
		private ArrayList addresses = new ArrayList();
		private int nextaddress;
		private int nexttime;
		private int queryinterval;
		private Thread processthread;

		// Masterservers
		private int mastertimeout;
		private string[] master_urls = {"http://www.bloodmasters.com/bloodmasterslist.php",
										"http://server.codeimp.com/bloodmasterslist.php"};

		// Servers
		private Hashtable allitems = new Hashtable();

		#endregion

		#region ================== Properties

		// Filter
		public string FilterTitle { get { return filtertitle; } set { filtertitle = value; OnFilteredListChanged(); } }
		public bool FilterFull { get { return filterfull; } set { filterfull = value; OnFilteredListChanged(); } }
		public bool FilterEmpty { get { return filterempty; } set { filterempty = value; OnFilteredListChanged(); } }
		public int FilterType { get { return filtertype; } set { filtertype = value; OnFilteredListChanged(); } }
		public string FilterMap { get { return filtermap; } set { filtermap = value; OnFilteredListChanged(); } }

		// Other
		public bool Running { get { return (processthread != null); } }

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public ServerBrowser()
		{
			// Read masterserver query timeout
			mastertimeout = General.config.ReadSetting("mastertimeout", 5000);

			// Clients will query the first masterserver in the list first
			// and try the second when the first one fails and so on.
			// Uncomment this to let the client try a random masterserver
			// first and continue from that point on.
			//SortMastersList(General.random.Next(master_urls.Length));
		}

		#endregion

		#region ================== Methods

		// This makes a new list with masterserver urls
		// in order to query them
		private void SortMastersList(int topindex)
		{
			int i, k = 0;

			// Make a new list with everything at
			// topindex at the beginning
			string[] newlist = new string[master_urls.Length];
			for(i = topindex; i < master_urls.Length; i++) newlist[k++] = master_urls[i];
			for(i = 0; i < topindex; i++) newlist[k++] = master_urls[i];

			// And use the new list as the masters list
			master_urls = newlist;
		}

		// Get an item by address
		public GamesListItem GetItemByAddress(string addr)
		{
			return (GamesListItem)allitems[addr];
		}

		// This tests if the given item passes the filter
		private bool VisibleFilteredItem(GamesListItem item)
		{
			// Filter by title?
			if(filtertitle.Trim() != "")
			{
				// Skip if title does not contain filtertitle
				if(item.Title.ToLower().IndexOf(filtertitle.ToLower()) == -1) return false;
			}

			// Filter full
			if(!filterfull && item.IsFull) return false;

			// Filter empty
			if(!filterempty && item.IsEmpty) return false;

			// Filter by gametype?
			if(filtertype > 0)
			{
				// Skip if gametype does not match filtertype
				if((int)item.GameType != (filtertype - 1)) return false;
			}

			// Filter by map name?
			if(filtermap.Trim() != "")
			{
				// Skip if mapname does not contain filtermap
				if(item.MapName.ToLower().IndexOf(filtermap.ToLower()) == -1) return false;
			}

			// Item visible
			return true;
		}

		// This returns a list of filtered servers
		public Hashtable GetFilteredList()
		{
			// Any items at all?
			if(allitems != null)
			{
				// Make arraylist
				Hashtable filtered = new Hashtable(allitems.Count);

				// Go for all items
				foreach(DictionaryEntry de in allitems)
				{
					// Get the item
					GamesListItem item = (GamesListItem)de.Value;

					// Not filtered out, then add to the list
					if((item != null) && (item.Address != null) && VisibleFilteredItem(item))
						filtered.Add(item.Address.ToString(), item);
				}

				// Return list of items
				return filtered;
			}
			else
			{
				// No items
				return new Hashtable();
			}
		}

		// This sends a query message to the specified address
		public void QueryServer(IPEndPoint addr)
		{
			// Make sure query is running
			if(!Running) RunQuery();

			// Query the server
			NetMessage query = gateway.CreateMessage(addr, MsgCmd.ServerInfo);
			query.AddData(General.GetCurrentTime());
			query.Send();
			query = null;
		}

		// This starts a new query sequence
		public string StartNewQuery()
		{
			string line;
			HttpWebResponse resp = null;
			string result = "";

			// Stop any previous queries
			StopQuery();

			// Clear addresses
			addresses.Clear();
			nextaddress = 0;

			// Go for all master servers
			for(int i = 0; i < master_urls.Length; i++)
			{
				// Query the masterserver
				HttpWebRequest query = (HttpWebRequest)WebRequest.Create(master_urls[i]);
				query.Timeout = mastertimeout;
				try { resp = (HttpWebResponse)query.GetResponse(); } catch(Exception) {}

				// Success?
				if(resp != null)
				{
					// Get the result
					Stream body = resp.GetResponseStream();
					StreamReader readbody = new StreamReader(body, Encoding.UTF8);

					// Read all lines
					while((line = readbody.ReadLine()) != null)
					{
						// Anything on this line?
						if(line.Trim() != "")
						{
							try
							{
								// Split IP and Port
								string[] addr = line.Trim().Split(':');

								// Make the IPEndPoint
								IPEndPoint target = new IPEndPoint(IPAddress.Parse(addr[0]),
											int.Parse(addr[1], CultureInfo.InvariantCulture));

								// Add to list
								addresses.Add(target);
							}
							catch(Exception) { }
						}
					}

					// Done
					readbody.Close();
					resp.Close();

					// Run query
					RunQuery();

					// This masterserver worked, no need to try other masterservers.
					// Resort the list so this one will be at the top.
					if(i > 0) SortMastersList(i);
					result = "";
					break;
				}
				else
				{
					// Timeout
					result = "servers list request timed out";
				}
			}

			// Return result
			return result;
		}

		// This runs a query sequence
		public void RunQuery()
		{
			int port = 0;

			// Stop any previous queries
			StopQuery();

			// Determine port number
			if(General.config.ReadSetting("fixedclientport", false))
				port = General.config.ReadSetting("clientport", 0);

			// Create a gateway
			gateway = new Gateway(port, 0, 0);

			// Start processing thread
			processthread = new Thread(new ThreadStart(Process));
			processthread.Name = "Process";
			processthread.Start();

			// Schedule querying
			queryinterval = (int)(1000f / (float)General.config.ReadSetting("queryspeed", 50));
			nexttime = SharedGeneral.GetCurrentTime();
			nextaddress = 0;
		}

		// This stops a query
		public void StopQuery()
		{
			// Thread running?
			if(processthread != null)
			{
				// Stop and wait for it to finish
				processthread.Interrupt();
				processthread.Join();
				processthread = null;
			}

			// Gateway still open?
			if(gateway != null)
			{
				// Dispose gateway
				gateway.Dispose();
				gateway = null;
			}
		}

		// This does the background stuff
		private void Process()
		{
			NetMessage msg;
			GamesListItem item;
			IPEndPoint addr;

			// Continue
			while(true)
			{
				// Process incoming data
				gateway.Process();

				// Get the next message
				msg = gateway.GetNextMessage();
				if(msg != null)
				{
					// Is this server info?
					if(msg.Command == MsgCmd.ServerInfo)
					{
						// Already listed?
						if(allitems.Contains(msg.Address.ToString()))
						{
							// Update this item
							item = (GamesListItem)allitems[msg.Address.ToString()];
							item.Update(msg);
						}
						else
						{
							// Make a new item
							item = new GamesListItem(this);
							allitems.Add(msg.Address.ToString(), item);
							item.Update(msg);
						}

						// Raise event when item is visible
						if(VisibleFilteredItem(item)) OnFilteredListChanged();
					}

					// Clean up
					item = null;
					msg = null;
				}

				// Idle or leave when interrupted
				try { Thread.Sleep(4); } catch(Exception) { return; }

				// Time to send a query?
				if((nexttime <= General.GetCurrentTime()) && (nextaddress < addresses.Count))
				{
					try
					{
						// Get the address
						addr = (IPEndPoint)addresses[nextaddress];

						// Send query message
						QueryServer(addr);
					}
					catch(Exception e) { if(e is ThreadInterruptedException) return; }

					// Next address
					nextaddress++;
					nexttime += queryinterval;
				}
			}
		}

		#endregion
	}
}
