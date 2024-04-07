/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;

namespace Bloodmasters.Client.Graphics;

public abstract class Decal
{
    #region ================== Constants

    // Timing
    private const int RND_STAY_TIME = 5000;
    private const int FADE_TIME = 5000;

    #endregion

    #region ================== Variables

    // Settings
    public static int decaltimeout;
    public static bool showdecals;

    // Position
    protected float x = 0f;
    protected float y = 0f;
    protected float z = 0f;

    // Timing
    protected int fadetime;
    protected int fadecolor;
    protected bool permanent;

    // VisualSector
    protected VisualSector sector;

    #endregion

    #region ================== Properties

    public float X { get { return x; } }
    public float Y { get { return y; } }
    public float Z { get { return z; } }
    public VisualSector VisualSector { get { return sector; } }

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public Decal(bool permanent)
    {
        this.permanent = permanent;

        // Permanent decal?
        if(permanent)
        {
            // Near infinite time
            fadetime = int.MaxValue;
        }
        else
        {
            // Create random fade time
            fadetime = SharedGeneral.currenttime + decaltimeout +
                       General.random.Next(RND_STAY_TIME);
        }

        // Start full bright
        fadecolor = -1;
    }

    // Dispose
    // This can also be called by the decal itsself
    // so it must remove itsself from any wall/floor completely
    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    #endregion

    #region ================== Resource Management

    // This unloads all unstable resources
    public virtual void UnloadResources()
    {
    }

    // This rebuilds unstable resources
    public virtual void ReloadResources()
    {
    }

    #endregion

    #region ================== Processing

    // Process this decal
    public virtual void Process()
    {
        if(!permanent)
        {
            // Time over?
            if(SharedGeneral.currenttime > fadetime)
            {
                // Completely faded away?
                if((SharedGeneral.currenttime - fadetime) > FADE_TIME)
                {
                    // Destroy this decal
                    this.Dispose();
                }
                else
                {
                    // Calculate fade
                    float fc = 1f - (float)(SharedGeneral.currenttime - fadetime) / (float)FADE_TIME;
                    fadecolor = General.ARGB(fc, 1f, 1f, 1f);
                }
            }
        }
    }

    #endregion

    #region ================== Rendering

    // Render the decal
    public abstract void Render();

    #endregion
}
