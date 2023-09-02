/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using System.Drawing;
using System.Globalization;
using CodeImp.Bloodmasters;
using CodeImp;

namespace CodeImp.Bloodmasters.Client;

public class LocalMove
{
    // Variables
    private int movetime;
    private float walkangle;

    // Constructor
    public LocalMove(int movetime, float walkangle)
    {
        // Set the variables
        this.movetime = movetime;
        this.walkangle = walkangle;
    }

    // This checks if the move is outdated
    public bool CorrectMove(int basetime)
    {
        return (basetime < movetime);
    }

    // This applies the move to an actor
    public void ApplyTo(Actor actor)
    {
        float walkpower, walklimit;
        Vector2D vel2d;

        // Moving at all?
        if(walkangle > -1f)
        {
            // Check if the actor can move
            if(actor.State.vel.Length() < Consts.MAX_WALK_LENGTH)
            {
                // Determine walking power
                if(!actor.IsOnFloor) walkpower = Consts.AIRWALK_LENGTH;
                else walkpower = Consts.WALK_LENGTH;

                // Determine walking limit
                if(General.localclient.Powerup != POWERUP.SPEED) walklimit = Consts.MAX_WALK_LENGTH;
                else walklimit = Consts.MAX_SPEED_WALK_LENGTH;

                // Add to walk velocity
                actor.State.vel.x += (float)Math.Sin(walkangle) * walkpower;
                actor.State.vel.y += (float)Math.Cos(walkangle) * walkpower;

                // Scale to match walking length
                vel2d = actor.State.vel;
                if(vel2d.Length() > walklimit)
                    vel2d.MakeLength(walklimit);
                actor.State.vel.Apply2D(vel2d);

                // Apply push vector
                actor.State.vel += (Vector3D)actor.PushVector;
            }
        }
    }
}
