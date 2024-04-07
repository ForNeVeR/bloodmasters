namespace Bloodmasters;

public class SharedGeneral
{
    // Clock
    public static long timefrequency = -1;
    public static double timescale;
    public static int realtime;				// Real time of processing
    public static int currenttime;			// Current time of this frame
    public static int accumulator;			// Buffer for delta time
    public static int previoustime;			// Previous frame time

    // This returns the time in milliseconds
    public static int GetCurrentTime()
    {
        // High resolution clock available?
        if(timefrequency != -1)
        {
            // Get the high resolution count
            long timecount = TimeProvider.System.GetTimestamp();

            // Calculate high resolution time in milliseconds
            //return (int)(((double)timecount / (double)timefrequency) * 1000d);
            return (int)(timecount * timescale);
        }
        else
        {
            // Use standard clock
            return Environment.TickCount;
        }
    }
}
