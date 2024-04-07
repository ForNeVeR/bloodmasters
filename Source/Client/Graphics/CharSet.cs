/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

// This provides a way to work with character sets which define
// the texture coordinates and character properties of a font.

using System;
using System.Collections;
using System.Drawing;
using System.Text;

namespace Bloodmasters.Client.Graphics;

public sealed class CharSet
{
    #region ================== Variables

    // Because access to the set must be fast,
    // this collection is public.
    private readonly CharInfo[] chars = new CharInfo[256];

    // Color codes
    private byte colorcode_byte;
    private string colorcode_str;

    #endregion

    #region ================== Properties

    public CharInfo[] Chars { get { return chars; } }
    public byte ColorCodeByte { get { return colorcode_byte; } }
    public string ColorCodeString { get { return colorcode_str; } }
    public char ColorCodeChar { get { return colorcode_str.ToCharArray()[0]; } }
    public bool UsesColorCode { get { return (colorcode_str != null) && (colorcode_str != ""); } }

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public CharSet(string filename)
    {
        // Load the character set
        Configuration cfg = new Configuration();
        cfg.LoadConfiguration(filename);

        // Get the charset from configuration
        IDictionary cfgchars = cfg.ReadSetting("chars", new Hashtable());

        // Go for all defined chars
        foreach(DictionaryEntry item in cfgchars)
        {
            // Get the character Hashtable
            IDictionary chr = (IDictionary)item.Value;
            int i = Convert.ToInt32(item.Key);

            // Set up the structure
            // The charater sizes were based on 800x600 resolution
            // so these are now scaled to the current resolution
            chars[i].width = ((float)(int)chr["width"] / 800f) * (float)Direct3D.DisplayWidth;
            chars[i].height = ((float)(int)chr["height"] / 600f) * (float)Direct3D.DisplayHeight;
            chars[i].u1 = (float)chr["u1"];
            chars[i].v1 = (float)chr["v1"];
            chars[i].u2 = (float)chr["u2"];
            chars[i].v2 = (float)chr["v2"];

            // Check for errors
            if((chars[i].u1 < 0f) || (chars[i].u1 > 1f) ||
               (chars[i].v1 < 0f) || (chars[i].v1 > 1f) ||
               (chars[i].u2 < 0f) || (chars[i].u2 > 1f) ||
               (chars[i].v2 < 0f) || (chars[i].v2 > 1f))
            {
                // Throw error for invalid coordinates
                throw(new Exception("Invalid charset coordinates: " + chars[i].u1.ToString("###############0.0###") +
                                    ", " + chars[i].v1.ToString("###############0.0###") +
                                    ", " + chars[i].u2.ToString("###############0.0###") +
                                    ", " + chars[i].v2.ToString("###############0.0###")));
            }
        }

        // Clean up
        cfgchars = null;
        cfg = null;
    }

    #endregion

    #region ================== Public Functions

    // This sets a color code character
    public void SetColorCode(string cc)
    {
        colorcode_str = cc;
        if((cc != null) && (cc != ""))
        {
            byte[] codebytes = Encoding.ASCII.GetBytes(colorcode_str);
            colorcode_byte = codebytes[0];
        }
    }

    // This checks if the given character exists in the charset
    public bool Contains(char c)
    {
        // Convert character to ASCII
        byte[] keybyte = Encoding.ASCII.GetBytes(c.ToString());
        byte b = keybyte[0];

        // Check if the character has been set
        if((chars[b].width > 0) || (chars[b].height > 0))
        {
            // Return success
            return true;
        }
        else
        {
            // Return failure
            return false;
        }
    }

    // This calculates the size of a text string at a given scale
    public SizeF GetTextSize(string text, float scale)
    {
        bool colorcodeval = false;

        // Size items
        float sizex = 0, sizey = 0;

        // Get the ASCII bytes for the text
        byte[] btext = Encoding.ASCII.GetBytes(text);

        // Go for all chars in text to calculate final text size
        foreach(byte b in btext)
        {
            // Color code?
            if((b == colorcode_byte) && UsesColorCode)
            {
                // Skip this and next byte
                colorcodeval = true;
            }
            // Color code value?
            else if(colorcodeval)
            {
                // Skip this too
                colorcodeval = false;
            }
            else
            {
                // Add to the final size
                sizex += chars[b].width * scale;
                sizey = chars[b].height * scale;
            }
        }

        // Return size
        return new SizeF(sizex, sizey);
    }

    #endregion
}

// This structure defines character properties
public struct CharInfo
{
    // Variables
    public float width;
    public float height;
    public float u1;
    public float v1;
    public float u2;
    public float v2;
}
