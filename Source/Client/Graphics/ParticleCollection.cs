/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using System.Collections.Generic;
using Bloodmasters.Client.Resources;
using SharpDX;
using SharpDX.Direct3D9;

namespace Bloodmasters.Client.Graphics;

public class ParticleCollection
{
    #region ================== Constants

    // Initial memory to allocate for particles
    public const int INITIAL_PARTICLES_MEMORY = 2000;

    #endregion

    #region ================== Variables

    // The particles flock
    private List<Particle> particles = new(INITIAL_PARTICLES_MEMORY);

    // The texture
    private TextureResource texture;

    // Render method
    private DRAWMODE drawmode;

    // Particle properties
    private float minsize;
    private float randomsize;
    private float minresize;
    private float randomresize;
    private int timeout;
    private int randomtimeout;
    private float gravity;
    private float randombright;
    private bool lightmapped;
    private bool fadein;

    #endregion

    #region ================== Properties

    public DRAWMODE DrawMode { get { return drawmode; } set { drawmode = value; } }
    public TextureResource Texture { get { return texture; } }

    public float MinimumSize { get { return minsize; } set { minsize = value; } }
    public float RandomSize { get { return randomsize; } set { randomsize = value; } }
    public float MinimumResize { get { return minresize; } set { minresize = value; } }
    public float RandomResize { get { return randomresize; } set { randomresize = value; } }
    public int Timeout { get { return timeout; } set { timeout = value; } }
    public int RandomTimeout { get { return randomtimeout; } set { randomtimeout = value; } }
    public float Gravity { get { return gravity; } set { gravity = value; } }
    public float RandomBright { get { return randombright; } set { randombright = value; } }
    public bool Lightmapped { get { return lightmapped; } set { lightmapped = value; } }
    public bool FadeIn { get { return fadein; } set { fadein = value; } }

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public ParticleCollection(string texturefile, DRAWMODE drawmode)
    {
        // Load texture
        string tempfile = ArchiveManager.ExtractFile(texturefile);
        texture = Direct3D.LoadTexture(tempfile, true);
        this.drawmode = drawmode;
    }

    // Disposer
    public void Dispose()
    {
        // Clean up
        foreach(Particle p in particles) p.Dispose();
        particles = null;
        texture.Dispose();
        texture = null;
        GC.SuppressFinalize(this);
    }

    #endregion

    #region ================== Methods

    // This creates a particle
    public void Add(Vector3D pos, Vector3D force, int color)
    {
        Add(pos, force, color, timeout, randomtimeout, minsize, randomsize);
    }

    // This creates a particle
    public void Add(Vector3D pos, Vector3D force, int color, int pmintime, int prndtime)
    {
        Add(pos, force, color, pmintime, prndtime, minsize, randomsize);
    }

    // This creates a particle
    public void Add(Vector3D pos, Vector3D force, int color, int pmintime, int prndtime, float pminsize, float prndsize)
    {
        // Make final color
        float bright = 1f + ((float)General.random.NextDouble() - 0.5f) * randombright;
        int pcolor = ColorOperator.Scale(color, bright);

        // Make random settings
        float psize = pminsize + (float)General.random.NextDouble() * prndsize;
        float presize = minresize + (float)General.random.NextDouble() * randomresize;
        int ptimeout = pmintime + General.random.Next(prndtime);

        // Make the particle
        Particle p = new Particle(pos, force, gravity, pcolor, psize, presize, ptimeout, this, lightmapped, fadein);
        if(!p.Disposed) particles.Add(p);
    }

    // This processes all particles
    public void Process()
    {
        int i = 0;
        Particle p;

        // Go for all particles
        while(i < particles.Count)
        {
            // Process particle
            p = particles[i];
            p.Process();

            // Trash when disposed or move on to the next
            if(p.Disposed) particles.RemoveAt(i); else i++;
        }
    }

    // This renders all particles in this collection
    public void Render()
    {
        // Set render mode
        Direct3D.SetDrawMode(drawmode);

        // No lightmap
        Direct3D.d3dd.SetTexture(1, null);

        // Set the texture and vertices stream
        Direct3D.d3dd.SetTexture(0, texture.texture);
        Direct3D.d3dd.SetStreamSource(0, Sprite.Vertices, 0, MVertex.Stride);
        Direct3D.d3dd.SetTransform(TransformState.Texture0, Matrix.Identity);

        // Render all particles
        foreach(Particle p in particles) p.DrawParticle(true);
    }

    #endregion
}
