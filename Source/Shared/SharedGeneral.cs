using System.Runtime.InteropServices;

namespace CodeImp.Bloodmasters;

public class SharedGeneral
{
    // Clock
    public static long timefrequency = -1;
    public static double timescale;
    public static int realtime;				// Real time of processing
    public static int currenttime;			// Current time of this frame
    public static int accumulator;			// Buffer for delta time
    public static int previoustime;			// Previous frame time

    [DllImport("kernel32.dll")] public static extern short QueryPerformanceCounter(ref long x);

    // This returns the time in milliseconds
    public static int GetCurrentTime()
    {
        // TODO: Cross-platform implementation
        long timecount = 0;

        // High resolution clock available?
        if(timefrequency != -1)
        {
            // Get the high resolution count
            QueryPerformanceCounter(ref timecount);

            // Calculate high resolution time in milliseconds
            //return (int)(((double)timecount / (double)timefrequency) * 1000d);
            return (int)((double)timecount * timescale);
        }
        else
        {
            // Use standard clock
            return Environment.TickCount;
        }
    }
}
