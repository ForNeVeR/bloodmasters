/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using System.Drawing;
using Bloodmasters.Client.Graphics;
using Bloodmasters.Client.Lights;
using Bloodmasters.LevelMap;

namespace Bloodmasters.Client.Items;

public class Powerup : Item
{
    #region ================== Constants

    private const int LIGHT_TEMPLATE = 3;
    private const float LIGHT_RANGE = 8f;
    private const int PARTICLE_SPAWN_DELAY = 300;

    #endregion

    #region ================== Variables

    // Light
    private readonly DynamicLight light;
    private readonly int color;
    private int particletime;

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public Powerup(Thing t) : base(t)
    {
        // Check if class has a PowerupItem attribute
        if(Attribute.IsDefined(this.GetType(), typeof(PowerupItem), false))
        {
            // Get item attribute
            PowerupItem attr = (PowerupItem)Attribute.GetCustomAttribute(this.GetType(), typeof(PowerupItem), false);

            // Set the color from attribute
            color = General.ARGB(1f, attr.R, attr.G, attr.B);
        }

        // Make the light
        int lcolor = ColorOperator.AdjustContrast(color, 0.6f);
        light = new DynamicLight(pos, LIGHT_RANGE, lcolor, LIGHT_TEMPLATE);

        // Set particle time
        particletime = SharedGeneral.currenttime;
    }

    // Diposer
    public override void Dispose()
    {
        // Clean up
        light.Dispose();

        // Dispose base class
        base.Dispose ();
    }

    #endregion

    #region ================== Methods

    // Process
    public override void Process()
    {
        // Let the base class process
        base.Process();

        // Spawn particles?
        if((particletime < SharedGeneral.currenttime) && !this.IsTaken)
        {
            // Spawn particle
            General.arena.p_magic.Add(this.pos + Vector3D.Random(General.random, 1f, 1f, 0f) + new Vector3D(0f, 0f, 2f),
                Vector3D.Random(General.random, 0.02f, 0.02f, 0.4f), color);

            // Set new particle time
            particletime = SharedGeneral.currenttime + PARTICLE_SPAWN_DELAY;
        }
    }

    // Item is taken
    public override void Take(Client clnt)
    {
        // Raise event in base class
        base.Take(clnt);

        // Display item description
        General.hud.ShowItemMessage(this.Description);

        // Make bright color for particles
        int pcolor = ColorOperator.Scale(color, 2f);

        // Spawn particles
        for(int i = 0; i < 10; i++)
            General.arena.p_magic.Add(this.pos + Vector3D.Random(General.random, 1f, 1f, 1f),
                Vector3D.Random(General.random, 0.02f, 0.02f, 0.4f), pcolor);

        // Spawn particles
        for(int i = 0; i < 6; i++)
            General.arena.p_magic.Add(this.pos + Vector3D.Random(General.random, 1f, 1f, 1f),
                Vector3D.Random(General.random, 0.01f, 0.01f, 0.5f), Color.White.ToArgb());

        // Turn the light off
        light.Visible = false;
    }

    // Item respawns
    public override void Respawn(bool playsound)
    {
        // Raise event in base class
        base.Respawn(playsound);

        // Turn the light on
        light.Visible = true;
    }

    #endregion
}
