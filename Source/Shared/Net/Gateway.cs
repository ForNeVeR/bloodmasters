/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

/*

Networking architecture
-------------------------------------------------------------

The Gateway

- Buffers messages for sending.
- Buffers arrived messages.
- Combines buffered messages into a packet and sends it.
- Splits incoming packets into messages and delivers them.
- Maintains a collection of connections.
- Buffers packets for ping simulation.
- Drops packets for packetloss simulation.

*/

using System.Collections;
using System.Net;
using System.Net.Sockets;

namespace CodeImp.Bloodmasters
{
	public abstract class Gateway
	{
		#region ================== Constants

		// Increase this when significant version changes
		// have been made to keep outdated clients from connecting
		public const int PROTOCOL_VERSION = 29;

		// Set this to output networking information
		public const bool SHOW_NETWORKING_INFO = false;

		// This is the number of packets per connection to create per frame
		public const int CLIENT_PACKETS_PER_FRAME = 1;

		#endregion

		#region ================== Variables

		// Input/output socket
		private Socket socket;

		// All connections
		private Hashtable connections;

		// Incoming messages
		private Queue in_messages;

		// Simulation
		private int simping = 0;
		private int simloss = 0;
		private Queue sim_messages;

		#endregion

		#region ================== Properties

		public int SimulatePing
		{
			get { return simping; }

			set
			{
				if(value < 0) throw(new ArgumentException("Invalid value specified for ping simulation."));
				simping = value;
			}
		}

		public int SimulateLoss
		{
			get { return simloss; }

			set
			{
				if((value > 100) || (value < 0)) throw(new ArgumentException("Invalid value specified for packetloss simulation."));
				simloss = value;
			}
		}

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public Gateway(int port, int simping, int simloss)
		{
			// Create input/output socket
			socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			socket.Blocking = false;

			// Create endpoint to bind to
			socket.Bind(new IPEndPoint(IPAddress.Any, port));
			ShowNetworkingInfo("Gateway bound to port " + port);

			// Create arrays
			in_messages = new Queue();
			connections = new Hashtable();
			sim_messages = new Queue();

			// Set lag simulation
			this.SimulatePing = simping;
			this.SimulateLoss = simloss;
		}

		// Dispose
		public void Dispose()
		{
			// Dispose all connections
			Connection[] cc = new Connection[connections.Count];
			connections.Values.CopyTo(cc, 0);
			foreach(Connection c in cc) c.Dispose();

			// Clean up
			try { socket.Close(); } catch(Exception) { }
			socket = null;
			connections.Clear();
			connections = null;
			in_messages = null;
			sim_messages = null;
			GC.SuppressFinalize(this);
			ShowNetworkingInfo("Gateway disposed");
		}

		#endregion

		#region ================== Connections

		// This makes a connection for a specific address
		public Connection CreateConnection(IPEndPoint addr)
		{
			// Connection already made?
			if(connections.Contains(addr.ToString()))
			{
				// Return existing connection
				return (Connection)connections[addr.ToString()];
			}
			else
			{
				// Make connection
				ShowNetworkingInfo("Creating connection for " + addr);
				Connection c = new Connection(this, addr);
				connections.Add(addr.ToString(), c);
				return c;
			}
		}

		// This destroys a connection
		public void DestroyConnection(IPEndPoint addr)
		{
			// Connection exists?
			if(connections.Contains(addr.ToString()))
			{
				// Destroy the connection
				ShowNetworkingInfo("Disposing connection for " + addr);
				Connection c = (Connection)connections[addr.ToString()];
				connections.Remove(addr.ToString());
				c.Dispose();
			}
		}

		// This returns a connection for a specified address
		// Returns null when no connection is open for the address
		public Connection FindConnection(IPEndPoint addr)
		{
			// Connection exists?
			if(connections.Contains(addr.ToString()))
			{
				// Return the connection
				return (Connection)connections[addr.ToString()];
			}
			else
			{
				// No such connection
				return null;
			}
		}

		#endregion

		#region ================== Messages

		// This submits a message for sending
		public void SendMessage(NetMessage msg)
		{
			// TODO: Implement packing algorythm
			// to combine and sort messages into packets

			// DEBUG:
			if(!msg.Reliable)
			{
				if(msg.Confirmation)
					ShowNetworkingInfo("Sending confirmation " + msg.ID + " to " + msg.Address + " (" + msg.Length + " bytes)");
				else
					ShowNetworkingInfo("Sending " + msg.Command + " message to " + msg.Address + " (" + msg.Length + " bytes)");
			}
			else
			{
				ShowNetworkingInfo("Sending " + msg.Command + " message " + msg.ID + " to " + msg.Address + " (" + msg.Length + " bytes)");
			}

			// Send immediately
			try { socket.SendTo(msg.GetMessageData(), (EndPoint)msg.Address); }
			catch(SocketException) { }
		}

		// This gets a message
		// Returns null when no more messages available
		public NetMessage GetNextMessage()
		{
			// Check for messages
			if(in_messages.Count > 0)
			{
				// Return message
				return (NetMessage)in_messages.Dequeue();
			}
			else
			{
				// No new messages
				return null;
			}
		}

		// This adds a message to the receive buffer
		internal void ReceivedMessage(NetMessage msg)
		{
			// Not a ping or confirm?
			if(msg.Command != MsgCmd.PingOrConfirm)
			{
				// Add to received messages
				if(simping > 0)
				{
					// Add to ping simulation buffer
					msg.SimSendTime = SharedGeneral.GetCurrentTime() + simping / 2;
					sim_messages.Enqueue(msg);
				}
				else
				{
					// Add immediately
					in_messages.Enqueue(msg);
				}
			}
		}

		// This creates a new message
		public NetMessage CreateMessage(IPEndPoint addr, MsgCmd cmd)
		{
			// Make a new message
			return new NetMessage(this, addr, cmd);
		}

		#endregion

		#region ================== Processing

		// This processes networking
		public void Process()
		{
			IPEndPoint addr = new IPEndPoint(IPAddress.Any, 0);
			EndPoint addrep = (EndPoint)addr;
			byte[] buffer = new byte[2048];
			int received;
			bool disconnected;
			Connection conn = null;

			// Continue while data is available
			while(socket.Available > 0)
			{
				// Read a packet from socket
				received = 0;
				try
				{
					received = socket.ReceiveFrom(buffer, ref addrep);
					addr = (IPEndPoint)addrep;
				}
				catch(Exception) { }

				// Apply packetloss?
				if(simloss > 0)
				{
					// Randomly choose to drop this message
					Random rnd = new Random();
					if(rnd.Next((int)(10000f / (float)simloss)) < 100)
					{
						// Drop packet
						received = 0;
					}
				}

				// Received anything?
				if(received > 0)
				{
					try
					{
						// Make stream from entire packet data
						MemoryStream pstream = new MemoryStream(buffer, 0, received, false);
						BinaryReader preader = new BinaryReader(pstream);

						// DEBUG:
						ShowNetworkingInfo("Received packet from " + addr + " (" + received + " bytes)");

						// Find connection with this address
						conn = FindConnection(addr);
						if(conn != null) conn.ReceivedPacket(received);

						// Continue until end of packet reached
						while(pstream.Position < pstream.Length)
						{
							// Read first 2 bytes (message length)
							int msglen = unchecked((ushort)IPAddress.NetworkToHostOrder(unchecked((short)preader.ReadUInt16())));

							// Compatability with older version
							msglen = ((msglen << 8) & 0x0000FF00) | ((msglen >> 8) & 0x000000FF);

							// Read the entire message
							pstream.Seek(-2, SeekOrigin.Current);
							byte[] msgdata = preader.ReadBytes(msglen);

							// Create the message from data
							NetMessage msg = new NetMessage(this, addr, conn, msgdata);

							// Connectionless message?
							if(conn == null)
							{
								// DEBUG:
								ShowNetworkingInfo("Received " + msg.Command + " message from " + msg.Address + " (" + msg.Length + " bytes)");

								// Put message in receive buffer
								ReceivedMessage(msg);
							}
							else
							{
								// Let the connection know it received this message
								msg.Connection.ReceiveMessage(msg);
							}
						}

						// Close streams
						preader.Close();
						pstream.Close();

					}
					catch(Exception e)
					{
						// Something went wrong while handling the packet
						ShowNetworkingInfo(e.GetType().Name + ": " + e.Message);
						ShowNetworkingInfo("Corrupt packet received from " + addr);
					}
				}
			}

			do
			{
				// Go for all connections
				disconnected = false;
				foreach(DictionaryEntry de in connections)
				{
					// Get the connection
					Connection c = (Connection)de.Value;

					// Process connection
					c.Process();

					// Disconnected?
					if(c.Disposed)
					{
						// Remove connection and leave the processing loop
						DestroyConnection(c.Address);
						disconnected = true;
						break;
					}
					else
					{
						// Go for all packets
						ArrayList packets = c.GetPackets();
						foreach(Packet p in packets)
						{
							// DEBUG:
							ShowNetworkingInfo("Sending packet to " + p.Address + " (" + p.Length + " bytes)");

							// Send packet
							try { socket.SendTo(p.GetData(), (EndPoint)p.Address); }
							catch(SocketException) { }
						}
					}
				}
			}
			// Continue until no more connections lost
			while(disconnected);

			// Move messages from ping simulation
			while((sim_messages.Count > 0) &&
			(((NetMessage)sim_messages.Peek()).SimSendTime < SharedGeneral.GetCurrentTime()))
				in_messages.Enqueue(sim_messages.Dequeue());
		}

		#endregion

		#region ================== Debug Output

		// This shows networking info
		public void ShowNetworkingInfo(string info)
		{
			if(SHOW_NETWORKING_INFO) WriteLine("NET: " + info);
		}

		// This writes an output message
        protected abstract void WriteLine(string text);

		// This outputs statistics
		public virtual void WriteStats(string statsmsg)
		{
		}

		#endregion
	}
}
