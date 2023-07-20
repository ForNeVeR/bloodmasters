/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

/*

Networking architecture
-------------------------------------------------------------

The Connection

- Keeps track of a connection with the remote peer.
- Sends/receives pings with statistics.
- Keeps track of statistics and Message IDs.
- Unreliable messages are sent to the Gateway.
- Reliable messages are sent to the Gateway and
  are scheduled for retransmission.
- Resends reliable message until confirmed.
- Removes reliable messages when confirmed.
- Sends confirmation when reliable message arrives.
- Buffers out-of-band reliable messages.

*/

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using CodeImp;

#if CLIENT
using CodeImp.Bloodmasters.Client;
#elif LAUNCHER
using CodeImp.Bloodmasters.Launcher;
#else
using CodeImp.Bloodmasters.Server;
#endif

namespace CodeImp.Bloodmasters
{
	public class Connection
	{
		#region ================== Constants
		
		// Standard connection timeout
		public const int DEFAULT_TIMEOUT = 10000;
		
		// Ping interval
		public const int PING_INTERVAL = 3000;
		
		// Maximum messages to buffer in and out
		public const int MESSAGE_BUFFER_LIMIT = 1000;
		
		// Resend interval for reliable messages
		public const int RESEND_INTERVAL = 25;
		public const int RESEND_INTERVAL_MULTIPLIER = 2;
		
		#endregion
		
		#region ================== Variables
		
		// References
		private Gateway gateway;
		private IPEndPoint address;
		
		// Messages IDs
		private uint out_id = 0;
		private uint in_id = 0;
		private int randomid;
		
		// State and timeout
		private bool disposed = false;
		private string disconnectreason = "";
		private int timeout;
		
		// Statistics: Ping and packetloss
		private int pingtime;
		private int packs_in = 0;
		private int packs_out = 0;
		private float lappacks_in = 0;
		private float lappacks_out = 0;
		private int lastping = 0;
		private int lastloss = 0;
		private bool measurepings = false;
		private int lastpingtime = 0;
		
		// Statistics: Throughput per second
		private int datameasuretime = 0;
		private bool showdatameasures = false;
		private int datamsg_in = 0;
		private int datamsg_out = 0;
		private int datapacks_in = 0;
		private int datapacks_out = 0;
		private int databytes_in = 0;
		private int databytes_out = 0;
		/*
		private int last_datamsg_in = 0;
		private int last_datamsg_out = 0;
		private int last_datapacks_in = 0;
		private int last_datapacks_out = 0;
		private int last_databytes_in = 0;
		private int last_databytes_out = 0;
		*/
		
		// Messages
		private ArrayList in_reliables;
		private ArrayList out_reliables;
		private ArrayList out_messages;
		
		#endregion
		
		#region ================== Properties
		
		public bool Disposed { get { return disposed; } }
		public bool MeasurePings { get { return measurepings; } set { measurepings = value; } }
		public int LastPing { get { return lastping; } }
		public int LastLoss { get { return lastloss; } }
		public string DisconnectReason { get { return disconnectreason; } }
		public IPEndPoint Address { get { return address; } }
		public int QueueLength { get { return out_reliables.Count; } }
		public int RandomID { get { return randomid; } }
		public int LastPingTime { get { return lastpingtime; } }
		public int DataMessagesIn { get { return lastpingtime; } }
		public bool ShowDataMeasures { get { return showdatameasures; } set { showdatameasures = value; } }
		
		#endregion
		
		#region ================== Constructor / Destructor
		
		// Constructor
		public Connection(Gateway gateway, IPEndPoint addr)
		{
			// Initialize connection
			this.gateway = gateway;
			this.address = addr;
			
			// Create arrays
			in_reliables = new ArrayList(MESSAGE_BUFFER_LIMIT);
			out_reliables = new ArrayList(MESSAGE_BUFFER_LIMIT);
			out_messages = new ArrayList(MESSAGE_BUFFER_LIMIT);
			
			// Make random ID
			Random rnd = new Random();
			randomid = rnd.Next(int.MaxValue);
			
			// Set timeouts
			timeout = General.GetCurrentTime() + DEFAULT_TIMEOUT;
			pingtime = General.GetCurrentTime() + PING_INTERVAL;
		}
		
		// Dispose
		public void Dispose()
		{
			// Clean up
			in_reliables = null;
			out_reliables = null;
			out_messages = null;
			
			// Remove from gateway
			gateway.DestroyConnection(address);
			disposed = true;
			GC.SuppressFinalize(this);
		}
		
		// Disconnect with a reason
		public void Disconnect(string reason)
		{
			// Set disconnect reason
			disconnectreason = reason;
			this.Dispose();
		}
		
		#endregion
		
		#region ================== Messages
		
		// This resets the messages queues
		public void ResetQueue(bool resetcounters)
		{
			// Clear queues
			out_reliables.Clear();
			in_reliables.Clear();
			
			// Reset counters?
			if(resetcounters)
			{
				out_id = 0;
				in_id = 0;
			}
		}
		
		// This creates a new message
		public NetMessage CreateMessage(MsgCmd cmd, bool reliable)
		{
			// Make a new message
			if(reliable) out_id++;
			return new NetMessage(gateway, this, cmd, reliable, out_id);
		}
		
		// This submits a message for sending
		public void SendMessage(NetMessage msg)
		{
			// Do not process when disposed
			if(disposed) return;
			
			// Check if the message is reliable
			if(msg.Reliable)
			{
				// Timestamp the message
				msg.ResendDelay = RESEND_INTERVAL;
				msg.ResendTime = General.GetCurrentTime() + RESEND_INTERVAL;
				
				// Store the message for retransmission
				if(out_reliables.Count < MESSAGE_BUFFER_LIMIT)
					out_reliables.Add(msg);
				else
					Disconnect("Sending messages buffer overflow");
			}
			
			// Send the message now
			SubmitMessage(msg);
		}
		
		// This sends a message to the gateway
		private void SubmitMessage(NetMessage msg)
		{
			// Do not process when disposed
			if(disposed) return;
			
			// Set the send time
			msg.SimSendTime = General.GetCurrentTime() + gateway.SimulatePing / 2;
			
			// Add to outgoing messages
			out_messages.Add(msg);
		}
		
		// This is called when a message is received
		public void ReceiveMessage(NetMessage msg)
		{
			// Update timeout
			int newtimeout = General.GetCurrentTime() + DEFAULT_TIMEOUT;
			if(newtimeout > timeout) timeout = newtimeout;
			
			// Count the message
			datamsg_in++;
			
			// Reliable message?
			if(msg.Reliable)
			{
				// DEBUG:
				gateway.ShowNetworkingInfo("Received " + msg.Command + " message " + msg.ID + " from " + msg.Address + " (" + msg.Length + " bytes)");
				
				// Check if this message is not yet received
				if(!MessageAlreadyReceived(msg.ID))
				{
					// Check if this is a ping message
					if(msg.Command == MsgCmd.PingOrConfirm)
					{
						// Time when ping is received
						lastpingtime = General.GetCurrentTime();
						
						// Check if we must reply
						if(!measurepings)
						{
							// Reply with a ping
							NetMessage ping = msg.Reply(MsgCmd.PingOrConfirm);
							ping.AddData((int)msg.GetInt());
							ping.AddData((int)packs_in);
							ping.AddData((int)packs_out);
							
							// Reset counters
							packs_in = 0;
							packs_out = 0;
							
							// Send the reply
							ping.Send();
						}
						else
						{
							// Read the new values
							int senttime = msg.GetInt();
							float clpacks_in = msg.GetInt();
							float clpacks_out = msg.GetInt();
							
							// Calculate ping and packetloss
							lastping = General.GetCurrentTime() - senttime;
							float arrived = (clpacks_in + lappacks_in) / (clpacks_out + lappacks_out);
							lastloss = (int)((1f - arrived) * 100f);
							
							// Limit ridiculous values
							if(lastping < 0) lastping = 0; else if(lastping > 999) lastping = 999;
							if(lastloss < 0) lastloss = 0; else if(lastloss > 100) lastloss = 100;
						}
					}
					
					// Add message to received buffer
					if(in_reliables.Count < MESSAGE_BUFFER_LIMIT)
						in_reliables.Add(msg);
					else
						Disconnect("Receiving messages buffer overflow");
					
					// Flush adjacent messages to gateway
					FlushAdjacentMessages();
				}
				
				// Send a confirmation back now
				msg.Confirm();
			}
			// Message confirmation?
			else if(msg.Confirmation)
			{
				// DEBUG:
				gateway.ShowNetworkingInfo("Received confirmation " + msg.ID + " from " + msg.Address + " (" + msg.Length + " bytes)");
				
				// Go for all stored reliables
				foreach(NetMessage rm in out_reliables)
				{
					// Message ID matches the confirmation?
					if(rm.ID == msg.ID)
					{
						// Remove from reliables buffer
						out_reliables.Remove(rm);
						break;
					}
				}
			}
			else
			{
				// DEBUG:
				gateway.ShowNetworkingInfo("Received " + msg.Command + " message from " + msg.Address + " (" + msg.Length + " bytes)");
				
				// Send the message to the gateway
				gateway.ReceivedMessage(msg);
			}
		}
		
		// This checks if a specific ID is already received
		private bool MessageAlreadyReceived(uint id)
		{
			// Check if this message is not yet received
			if(id > in_id)
			{
				// Check if not in the buffered list either
				foreach(NetMessage m in in_reliables)
				{
					// Return true if already in the buffer
					if(id == m.ID) return true;
				}
				
				// Not yet received
				return false;
			}
			else
			{
				// Already received
				return true;
			}
		}
		
		// This flushes adjacent messages from the receive buffer
		private void FlushAdjacentMessages()
		{
			bool adjacentfound;
			
			do
			{
				// We have not found an adjacent message yet
				adjacentfound = false;
				
				// Find an adjacent message
				foreach(NetMessage m in in_reliables)
				{
					// Is this one adjacent to the last one?
					if(m.ID == (in_id + 1))
					{
						// Send the message to the gateway
						gateway.ReceivedMessage(m);
						
						// Remove from buffer
						in_reliables.Remove(m);
						
						// Increase counter
						adjacentfound = true;
						in_id++;
						break;
					}
				}
			}
			// Continue while we have adjacent message
			while(adjacentfound);
		}
		
		// This sets a custom timeout for the connection
		public void SetTimeout(int millisec)
		{
			// Set timeout
			timeout = General.GetCurrentTime() + millisec;
		}
		
		#endregion
		
		#region ================== Packets
		
		// This make packets from the outgoing messages
		// Also performs packetloss simulation
		public ArrayList GetPackets()
		{
			int i = 0;
			int waste, msgspacked = 0;
			Packet packet;
			NetMessage msg;
			
			// Make packets array
			ArrayList packets = new ArrayList(out_messages.Count);
			
			// Sort all messages by size, biggest first
			out_messages.Sort(new NetMessageComparer(true));
			
			// Go for all messages
			while(i < out_messages.Count)
			{
				// Get the current message
				msg = (NetMessage)out_messages[i];
				
				// Check if this message must be sent now
				if(msg.SimSendTime <= General.GetCurrentTime())
				{
					// Reset best find
					waste = int.MaxValue;
					packet = null;
					
					// Go for all existing packages
					foreach(Packet p in packets)
					{
						// Free space in packet to hold message here?
						if(p.OptimalSpaceLeft >= msg.Length)
						{
							// Less waste than previous find?
							if(p.OptimalSpaceLeft - msg.Length < waste)
							{
								// This packet qualifies
								waste = p.OptimalSpaceLeft - msg.Length;
								packet = p;
							}
						}
					}
					
					// No suitable packet found?
					if(packet == null)
					{
						// Allowed to make another packet?
						if(packets.Count < Gateway.CLIENT_PACKETS_PER_FRAME)
						{
							// Create new packet and store the message
							packet = new Packet(address, msg.GetMessageData());
							packets.Add(packet);
							if(out_messages.Count > i) out_messages.RemoveAt(i);
							
							// Count it
							msgspacked++;
							datamsg_out++;
						}
						else
						{
							// Skip message
							i++;
						}
					}
					else
					{
						// Store the message here
						packet.AppendData(msg.GetMessageData());
						if(out_messages.Count > i) out_messages.RemoveAt(i);
						
						// Count it
						msgspacked++;
						datamsg_out++;
					}
				}
				else
				{
					// Skip message
					i++;
				}
			}
			
			// DEBUG:
			if(msgspacked > 0) gateway.ShowNetworkingInfo("Packed " + msgspacked + " messages into " + packets.Count + " packets.");
			
			// Count the number of packets
			packs_out += packets.Count;
			datapacks_out += packets.Count;
			
			// Count the amount of data
			foreach(Packet pp in packets) databytes_out += pp.Length;
			
			// Apply packetloss?
			if(gateway.SimulateLoss > 0)
			{
				// Go for all packets
				for(i = packets.Count - 1; i >= 0; i--)
				{
					// Randomly choose to drop this message
					Random rnd = new Random();
					if(rnd.Next((int)(10000f / (float)gateway.SimulateLoss)) < 100)
					{
						// Remove packet
						packets.RemoveAt(i);
					}
				}
			}
			
			// Return packets
			return packets;
		}
		
		// This notifies the connection a packet has arrived
		public void ReceivedPacket(int bytes)
		{
			// Count the packet
			packs_in++;
			datapacks_in++;
			
			// Count the amount of data
			databytes_in += bytes;
		}
		
		#endregion
		
		#region ================== Processing
		
		// This processes this connection
		public void Process()
		{
			string stats;
			
			// Do not process when disposed
			if(disposed) return;
			
			// Timed out?
			if(timeout < General.GetCurrentTime())
			{
				// Disconnect
				Disconnect("Connection timed out");
				return;
			}
			
			// Time to measure data throughput?
			if(datameasuretime <= General.GetCurrentTime())
			{
				// Do we want to know?
				if(showdatameasures)
				{
					// Show data throughput statistics now
					stats = "NET STATS   IN: " + databytes_in + " B/s   (" + datamsg_in + " msgs / " + datapacks_in + " packs)   ";
					stats += "   OUT: " + databytes_out + " B/s   (" + datamsg_out + " msgs / " + datapacks_out + " packs)";
					gateway.WriteStats(stats);
				}
				
				// Reset statistics
				databytes_in = 0;
				databytes_out = 0;
				datamsg_in = 0;
				datamsg_out = 0;
				datapacks_in = 0;
				datapacks_out = 0;
				
				// Set new measure time
				datameasuretime = General.GetCurrentTime() + 1000;
			}
			
			// Time to send a ping?
			if(measurepings && (pingtime < General.GetCurrentTime()))
			{
				// Send ping message
				NetMessage ping = CreateMessage(MsgCmd.PingOrConfirm, true);
				ping.AddData((int)General.GetCurrentTime());
				ping.Send();
				
				// Reset counters
				lappacks_in = packs_in;
				lappacks_out = packs_out;
				packs_in = 0;
				packs_out = 0;
				
				// Set new ping time
				pingtime = General.GetCurrentTime() + PING_INTERVAL;
			}
			
			// Go for all stored reliables
			foreach(NetMessage msg in out_reliables)
			{
				// Time to resend this message?
				if(msg.ResendTime < General.GetCurrentTime())
				{
					// Send the message now and adjust resend time
					msg.ResendDelay *= RESEND_INTERVAL_MULTIPLIER;
					msg.ResendTime = General.GetCurrentTime() + msg.ResendDelay;
					SubmitMessage(msg);
				}
			}
		}
		
		#endregion
	}
}
