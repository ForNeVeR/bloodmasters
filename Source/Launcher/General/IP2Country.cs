/********************************************************************\
*                                                                   *
*  Configuration class by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using System.IO;
using System.Text;
using System.Globalization;
using System.Collections.Generic;
using System.Net;

namespace CodeImp;

public class IP2Country
{
    #region ================== Variables

    // This holds a list of IP ranges
    private IPRangeInfo[] ipranges;

    #endregion

    #region ================== Properties

    public int NumRanges { get { return ipranges.Length; } }

    #endregion

    #region ================== Constructor / Destructor

    // This creates an empty lookup table
    public IP2Country()
    {
        // Empty array
        ipranges = Array.Empty<IPRangeInfo>();
    }

    // This initializes the lookup table with the given input file
    public IP2Country(string csvfile)
    {
        List<IPRangeInfo> iprangeslist = new List<IPRangeInfo>(100000);
        IPRangeInfo r;
        string line;
        string[] items;

        // Check if the csv file exists
        if(File.Exists(csvfile))
        {
            // Open the csv file
            FileStream csv = File.Open(csvfile, FileMode.Open, FileAccess.Read, FileShare.Read);

            // Create reader
            StreamReader csvReader = new StreamReader(csv, Encoding.ASCII);

            // Read all lines of the file
            while((line = csvReader.ReadLine()) != null)
            {
                // Split the line by its commas
                items = line.Split(',');

                // Create range struct
                r = new IPRangeInfo();
                r.from = Convert.ToInt64(items[0].Replace("\"", ""));
                r.to = Convert.ToInt64(items[1].Replace("\"", ""));
                r.ccode1 = items[2].Replace("\"", "").ToCharArray();
                r.ccode2 = items[3].Replace("\"", "").ToCharArray();
                r.country = items[4].Replace("\"", "");

                // Add struct to array
                iprangeslist.Add(r);
            }

            // Convert the list to IPRangeInfo[]
            ipranges = iprangeslist.ToArray();
        }
        else
        {
            // Empty array
            ipranges = Array.Empty<IPRangeInfo>();

            /*
            // File not found
            throw(new FileNotFoundException("Unable to find the required ip-to-country CSV file."));
            */
        }
    }

    #endregion

    #region ================== Private Methods

    // This makes a proper titled string
    private string ProperCountryName(string name)
    {
        string newname;

        // Make the country name nicely
        newname = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name.ToLower());
        newname = newname.Replace(" And ", " and ");
        newname = newname.Replace(" Of ", " of ");
        newname = newname.Replace(" The ", " the ");
        newname = newname.Replace(" D'", " d'");
        return newname;
    }

    #endregion

    #region ================== Lookup Methods

    // This finds and returns country information for the given IP address
    public IPRangeInfo LookupIP(string ipaddress)
    {
        IPRangeInfo item;
        IPRangeInfo unknown = new IPRangeInfo(0, 0, "", "", "Location unknown");

        // Parse ip address
        IPAddress ip = IPAddress.Parse(ipaddress);

        // Create long value for comparision
        byte[] ipbytes = ip.GetAddressBytes();
        long ipnumber = (long)ipbytes[0] * 16777216L + (long)ipbytes[1] * 65536L + (long)ipbytes[2] * 256L + (long)ipbytes[3];

        // Initialize start points for search
        int mid;
        int left = 0;
        int right = ipranges.Length - 1;

        // Do a binary search for the range that starts with this value
        while(left <= right)
        {
            // Get the middle item
            mid = (left + right) / 2;
            item = ipranges[mid];

            // Compare item and value
            if(ipnumber == item.from)
            {
                // Make the country name nicely
                item.country = ProperCountryName(item.country);

                // Return this range info
                return item;
            }
            else if(ipnumber < item.from) right = mid - 1;
            else left = mid + 1;
        }

        // If no range started with this value, then
        // left is just after the range where it should be.
        // So backtrack a bit until correct range found.
        do
        {
            // Go back
            left--;

            // Check if valid item
            if(left < 0)
            {
                // Return unknown range
                return unknown;
            }

            // Get the item
            item = ipranges[left];
        }
        while(item.from > ipnumber);

        // Check if	the IP is truly in this range
        if(item.to >= ipnumber)
        {
            // Make the country name nicely
            item.country = ProperCountryName(item.country);

            // Return this range info
            return item;
        }
        else
        {
            // Return unknown range
            return unknown;
        }
    }

    #endregion
}
