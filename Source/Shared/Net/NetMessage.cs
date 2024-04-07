/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

/*

Networking architecture
-------------------------------------------------------------

Messages:

2 bytes ushort	Length (used to combine and split messages)
1 byte			Command
4 bytes uint	PacketID (when reliable or confirming)
# bytes			Data

One packet can contain one or more messages. Use the length
field to combine/split messages to/from packets.


Special Commands:

- Command 128 is a ping (reliable)
- Command 0 is a confirmation (includes a PacketID)

- For all other commands, the MSB (0x80) indicates if
  the message is sent reliable. If the MSB is set, the
  message will contain a PacketID.

Example:
Command 2 = Unreliable message 2
Command 130 = Reliable message 2

*/

using System.Net;
using System.Text;

namespace Bloodmasters.Net;

public class NetMessage
{
    #region ================== Constants

    // Reliable flag
    private const int RELIABLE = 0x80;

    #endregion

    #region ================== Variables

    // Connection
    private Connection conn = null;
    private Gateway gateway;
    private readonly IPEndPoint address;
    private bool disposed = false;

    // Packet
    private readonly int cmd;
    private uint id = 0;
    private MemoryStream data = null;

    // Reader/writer
    private BinaryReader readdata = null;
    private BinaryWriter writedata = null;

    // Encoding for strings
    private readonly Encoding encoding = Encoding.ASCII;

    // Reliable transmission and ping simulation
    private int resendtime;
    private int resenddelay;
    private int simsendtime;

    #endregion

    #region ================== Properties

    public int SimSendTime { get { return simsendtime; } set { simsendtime = value; } }
    public int ResendTime { get { return resendtime; } set { resendtime = value; } }
    public int ResendDelay { get { return resenddelay; } set { resenddelay = value; } }
    public bool Inbound { get { return readdata != null; } }
    public bool Outbound { get { return readdata == null; } }
    public Connection Connection { get { return conn; } }
    public IPEndPoint Address { get { return address; } }
    public bool EndOfMessage { get { return (data.Position == data.Length); } }
    public int DataLength { get { return (int)data.Length; } }
    public MsgCmd Command { get { return (MsgCmd)(cmd & ~RELIABLE); }	}
    public uint ID { get { return id; } }
    public bool Reliable { get { return (cmd & RELIABLE) > 0; } }
    public bool Confirmation { get { return (cmd == 0); } }
    public bool Disposed { get { return disposed; } }

    // Total message length
    public int Length
    {
        get
        {
            if((cmd == 0) || ((cmd & RELIABLE) > 0))
                return (int)data.Length + 7;
            else
                return (int)data.Length + 3;
        }
    }

    #endregion

    #region ================== Constructor

    // Constructor for incoming message
    // - Connection may be null for connectionless messages.
    // - The packet data must include the 2 length bytes.
    public NetMessage(Gateway gateway, IPEndPoint addr, Connection conn, byte[] packet)
    {
        int headerlen = 3;
        int messagelen;

        // Keep references
        this.conn = conn;
        this.gateway = gateway;
        this.address = addr;

        // Test for data
        if(packet.Length < headerlen) throw(new Exception("Packet data too small"));

        // Make streams from packet data
        data = new MemoryStream(packet, false);
        readdata = new BinaryReader(data, encoding);

        // Read the header
        messagelen = unchecked((ushort)IPAddress.NetworkToHostOrder(unchecked((short)readdata.ReadUInt16())));
        cmd = readdata.ReadByte();

        // Compatability with older version
        messagelen = ((messagelen << 8) & 0x0000FF00) | ((messagelen >> 8) & 0x000000FF);

        // Make sure all data is here
        if(packet.Length < messagelen) throw(new Exception("Packet is missing data"));

        // Reliable or confirming?
        if((cmd == 0) || ((cmd & RELIABLE) > 0))
        {
            // There is more data in the header!
            headerlen += 4;

            // Test for data
            if(packet.Length < headerlen) throw(new Exception("Packet data too small"));

            // Read more header data
            id = unchecked((uint)IPAddress.NetworkToHostOrder(unchecked((int)readdata.ReadUInt32())));
        }

        // Close streams
        readdata.Close();
        data.Close();
        readdata = null;
        data = null;

        // Make new stream from data only
        data = new MemoryStream(packet, headerlen, packet.Length - headerlen, false);
        readdata = new BinaryReader(data, encoding);
    }

    // Constructor for new message with connection
    public NetMessage(Gateway gateway, Connection conn, MsgCmd command, bool reliable, uint id)
    {
        // Keep references
        this.conn = conn;
        this.gateway = gateway;
        this.address = conn.Address;

        // Making reliable message?
        if(reliable)
        {
            // Make the command and id
            this.cmd = (int)command | RELIABLE;
            this.id = id;
        }
        else
        {
            // Make the command
            this.cmd = (int)command & ~RELIABLE;
        }

        // Make streams from new packet
        data = new MemoryStream();
        writedata = new BinaryWriter(data, encoding);
    }

    // Constructor for new connectionless message
    public NetMessage(Gateway gateway, IPEndPoint addr, MsgCmd command)
    {
        // Keep references
        this.conn = null;
        this.gateway = gateway;
        this.address = addr;

        // Make the command
        cmd = (int)command & ~RELIABLE;

        // Make streams from new packet
        data = new MemoryStream();
        writedata = new BinaryWriter(data, encoding);
    }

    // Disposer
    public void Dispose()
    {
        // Close streams
        if(readdata != null) readdata.Close();
        if(writedata != null) writedata.Close();
        if(data != null) data.Close();

        // Clean up
        conn = null;
        gateway = null;
        readdata = null;
        writedata = null;
        data = null;
        disposed = true;
        GC.SuppressFinalize(this);
    }

    #endregion

    #region ================== Read / Write

    // Writes to the packet data
    public void AddData(byte val) { writedata.Write(val); }
    public void AddData(int val) { writedata.Write(IPAddress.HostToNetworkOrder(val)); }
    public void AddData(uint val) { writedata.Write(unchecked((uint)IPAddress.HostToNetworkOrder(unchecked((int)val)))); }
    public void AddData(short val) { writedata.Write(IPAddress.HostToNetworkOrder(val)); }
    public void AddData(ushort val) { writedata.Write(unchecked((ushort)IPAddress.HostToNetworkOrder(unchecked((short)val)))); }
    public void AddData(float val) { writedata.Write(val); }
    public void AddData(string val) { writedata.Write(val); }
    public void AddData(bool val) { writedata.Write(val); }

    // Reads from the packet data
    public byte GetByte() { return readdata.ReadByte(); }
    public int GetInt() { return IPAddress.NetworkToHostOrder(readdata.ReadInt32()); }
    public uint GetUInt() { return unchecked((uint)IPAddress.NetworkToHostOrder(unchecked((int)readdata.ReadUInt32()))); }
    public short GetShort() { return IPAddress.NetworkToHostOrder(readdata.ReadInt16()); }
    public ushort GetUShort() { return unchecked((ushort)IPAddress.NetworkToHostOrder(unchecked((short)readdata.ReadUInt16()))); }
    public float GetFloat() { return readdata.ReadSingle(); }
    public string GetString() { return readdata.ReadString(); }
    public bool GetBool() { return readdata.ReadBoolean(); }

    #endregion

    #region ================== Methods

    // This submits the message
    public void Send()
    {
        // Send over connection?
        if(conn != null)
        {
            // Send message over connection
            conn.SendMessage(this);
        }
        else
        {
            // Send message over gateway
            gateway.SendMessage(this);
        }
    }

    // This creates the message data
    public byte[] GetMessageData()
    {
        MemoryStream mpacket;
        BinaryWriter bpacket;
        int messagelen;
        bool includeid;

        // Ensure all data is written
        writedata.Flush();

        // Determine message size
        includeid = (cmd == 0) || ((cmd & RELIABLE) > 0);
        if(includeid) messagelen = (int)data.Length + 7;
        else messagelen = (int)data.Length + 3;

        // Compatability with older version
        messagelen = ((messagelen << 8) & 0x0000FF00) | ((messagelen >> 8) & 0x000000FF);

        // Make packet data
        mpacket = new MemoryStream();
        bpacket = new BinaryWriter(mpacket, encoding);
        bpacket.Write(unchecked((ushort)IPAddress.HostToNetworkOrder(unchecked((short)messagelen))));
        bpacket.Write((byte)cmd);
        if(includeid) bpacket.Write(unchecked((uint)IPAddress.HostToNetworkOrder(unchecked((int)id))));
        bpacket.Flush();
        data.WriteTo(mpacket);
        mpacket.Flush();

        // Get packet data
        byte[] result = mpacket.ToArray();

        // Clean up
        bpacket.Close();
        mpacket.Close();
        bpacket = null;
        mpacket = null;

        // Return result
        return result;
    }

    // This sends a confirmation for this packet
    public void Confirm()
    {
        // Send a confirmation message
        NetMessage msg = new NetMessage(gateway, address, MsgCmd.PingOrConfirm);
        msg.id = this.id;
        msg.Send();
    }

    // This makes a message to reply with
    public NetMessage Reply(MsgCmd command)
    {
        // Connection known?
        if(conn != null)
        {
            // Make a reply message with connection
            return conn.CreateMessage(command, Reliable);
        }
        else
        {
            // Make a conectionless reply message
            NetMessage msg = new NetMessage(gateway, address, command);
            return msg;
        }
    }

    #endregion
}
