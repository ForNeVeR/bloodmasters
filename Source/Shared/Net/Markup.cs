using System.Text;

namespace CodeImp.Bloodmasters.Net;

public static class Markup
{
    // This trims the last color code from a string
    public static string TrimColorCodes(string str)
    {
        // Remove all color code signs from the end of the string
        return str.TrimEnd(Consts.COLOR_CODE_SIGN.ToCharArray());
    }

    // This strips color codes from a string
    public static string StripColorCodes(string str)
    {
        StringBuilder result = new StringBuilder(str.Length);

        // Split the string by color code
        string[] pieces = str.Split(Consts.COLOR_CODE_SIGN.ToCharArray());

        // Go for all pieces and append them
        result.Append(pieces[0]);
        for(int i = 1; i < pieces.Length; i++)
        {
            // Not an empty string?
            if(pieces[i] != "")
            {
                // Append everything except the first character
                result.Append(pieces[i].Substring(1));
            }
        }

        // Return final string
        return result.ToString();
    }
}
