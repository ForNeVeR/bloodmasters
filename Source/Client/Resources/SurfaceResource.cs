/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

// Surface resource is a managed Direct3D surface which
// is automatically reloaded on device reset. Use the
// LoadSurfaceResource function from the Direct3D class to
// create a surface resource of this type.

using System;
using System.Drawing;
using System.IO;
using SharpDX.Direct3D9;
using Direct3D = Bloodmasters.Client.Graphics.Direct3D;

namespace Bloodmasters.Client.Resources;

public sealed class SurfaceResource : Resource
{
    #region ================== Variables

    // This is what this class is all about
    private readonly string resourcefilename = "";
    public Surface surface = null;

    // Memory pool where to store this resource
    private readonly Pool memorypool = Pool.Default;

    // Surface properties
    private int width = 0;
    private int height = 0;

    #endregion

    #region ================== Properties

    public string Filename { get { return resourcefilename; } }
    public int Width { get { return width; } }
    public int Height { get { return height; } }

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public SurfaceResource(string filename, string referencename, Pool pool) : base(referencename)
    {
        // Keep the memory pool
        memorypool = pool;

        // Set the filename and load resource
        resourcefilename = filename;
        this.Load();
    }

    #endregion

    #region ================== Functions

    // This loads the resource from the given filename
    public override void Load()
    {
        // Does the file exist?
        if(File.Exists(resourcefilename))
        {
            // Load the image
            Image img = Image.FromFile(resourcefilename);

            // Set the properties
            width = img.Size.Width;
            height = img.Size.Height;

            // We dont need that image anymore
            img.Dispose();
            img = null;
            GC.Collect();

            // Create the surface
            surface = Surface.CreateOffscreenPlain(Graphics.Direct3D.d3dd, width, height, (Format)Graphics.Direct3D.DisplayFormat, memorypool);

            // Load the file into the surface
            Surface.FromFile(surface, resourcefilename, Filter.None, 0);

            // Inform the base class about this load
            base.Load();
        }
        else
        {
            // Error, file not found
            throw new FileNotFoundException("Cannot find the specified file \"" + resourcefilename + "\"", resourcefilename);
        }
    }

    // This unloads the resource
    public override void Unload()
    {
        // Unload the surface
        if((surface != null) && (surface.IsDisposed == false))
        {
            surface.ReleaseDC(surface.GetDC());
            surface.Dispose();
        }
        surface = null;

        // Inform the base class about this unload
        base.Unload();
    }

    #endregion
}
