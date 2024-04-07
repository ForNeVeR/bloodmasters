/********************************************************************\
*                                                                   *
*  WAD class by Pascal vd Heiden, www.codeimp.com				    *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

// This is not a complete WAD file manager, it only provides
// a way to read lumps from a WAD file. Thats all I need for now :)

using System;
using System.IO;
using System.Text;

namespace Bloodmasters;

public class Wad
{
    // This makes a string from fixed byte array
    public static string BytesToString(byte[] bytes)
    {
        int length = bytes.Length;

        // Find the first null byte
        for(int i = 0; i < bytes.Length; i++)
        {
            // End of string?
            if(bytes[i] == 0)
            {
                // End of string
                length = i;
                break;
            }
        }

        // Make the string
        return Encoding.ASCII.GetString(bytes, 0, length);
    }

    // This returns a file stream for the given lump name
    public static BinaryReader ReadLump(string wadfile, string lumpname)
    {
        int i;

        // Open the WAD file
        FileStream f = File.OpenRead(wadfile);
        BinaryReader bf = new BinaryReader(f);

        // Get the number of lumps and the offset to
        // the lump metadata
        f.Seek(4, SeekOrigin.Begin);
        int numlumps = bf.ReadInt32();
        int mdoffset = bf.ReadInt32();

        // Go for all lumps
        f.Seek(mdoffset, SeekOrigin.Begin);
        for(i = 0; i < numlumps; i++)
        {
            // Read lump information
            int lpoffset = bf.ReadInt32();
            int lpsize = bf.ReadInt32();
            byte[] lpname = bf.ReadBytes(8);

            // Check if this is the lump we need
            string lpstrname = Encoding.ASCII.GetString(lpname);
            if(lpstrname.ToLower().StartsWith(lumpname.ToLower()))
            {
                // Copy data to memory
                f.Seek(lpoffset, SeekOrigin.Begin);
                byte[] lpdata = bf.ReadBytes(lpsize);
                MemoryStream ms = new MemoryStream(lpdata, false);

                // Close file
                bf.Close();
                f.Close();

                // Return memory stream wrapped in a binary reader
                return new BinaryReader(ms);
            }
        }

        // Close file
        bf.Close();
        f.Close();

        // Return nothing
        return null;
    }
}
