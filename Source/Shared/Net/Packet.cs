/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

/*

Networking architecture
-------------------------------------------------------------

Packet:

Multiple messages combined and converted to a single data
stream, ready for transmitting over the network.

*/

using System.Net;

namespace CodeImp.Bloodmasters.Net;

public class Packet
{
    #region ================== Constants

    // Maximum optimal size in bytes
    private const int MAX_OPTIMAL_SIZE = 300;

    #endregion

    #region ================== Variables

    // Target/source address
    private readonly IPEndPoint address;

    // Packet data
    private MemoryStream data = null;

    #endregion

    #region ================== Properties

    public IPEndPoint Address { get { return address; } }
    public int Length { get { return (int)data.Length; } }
    public int OptimalSpaceLeft { get { return MAX_OPTIMAL_SIZE - (int)data.Length; } }

    #endregion

    #region ================== Constructor

    // Constructor for new packet
    public Packet(IPEndPoint addr)
    {
        // Set the address
        this.address = addr;

        // Make data stream
        data = new MemoryStream(MAX_OPTIMAL_SIZE);
    }

    // Constructor for new packet
    public Packet(IPEndPoint addr, byte[] newdata)
    {
        // Set the address
        this.address = addr;

        // Make data stream
        data = new MemoryStream(MAX_OPTIMAL_SIZE);

        // Append the new data
        AppendData(newdata);
    }

    // Disposer
    public void Dispose()
    {
        // Clean up
        if(data != null) data.Close();
        data = null;
        GC.SuppressFinalize(this);
    }

    #endregion

    #region ================== Methods

    // This appends data to the stream
    public void AppendData(byte[] newdata)
    {
        // Write data to stream
        data.Write(newdata, 0, newdata.Length);
    }

    // This returns all stream data
    public byte[] GetData()
    {
        // Return stream data
        return data.ToArray();
    }

    #endregion
}
