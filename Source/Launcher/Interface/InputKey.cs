using System.Windows.Forms;

namespace Bloodmasters.Launcher.Interface;

public struct InputKey
{
    public int keycode;

    // This returns a name for a key
    public static string GetKeyName(int k)
    {
        if(k == (int)EXTRAKEYS.MScrollUp) return ((EXTRAKEYS)k).ToString();
        if(k == (int)EXTRAKEYS.MScrollDown) return ((EXTRAKEYS)k).ToString();
        return ((Keys)k).ToString();
    }

    // This returns the name
    public override string ToString()
    {
        return InputKey.GetKeyName(keycode);
    }
}
