/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

// Texture resource is a managed Direct3D texture object.
// Use the LoadTextureResource function from the Direct3D class
// to create a texture resource of this type.

using SharpDX.Direct3D9;
using Direct3D = Bloodmasters.Client.Graphics.Direct3D;

namespace Bloodmasters.Client.Resources;

public class TextureResource : ITextureResource
{
    #region ================== Variables

    // Texture and info
    private readonly string filename;
    public Texture texture = null;
    public ImageInformation info;

    #endregion

    #region ================== Properties

    public ImageInformation Info { get { return info; } }
    public Texture Texture { get { return texture; } }

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public TextureResource(string f, Texture t, ImageInformation i)
    {
        // Set the filename and load resource
        filename = f;
        info = i;
        texture = t;
    }

    #endregion

    #region ================== Methods

    // This unloads the resource
    public void Dispose()
    {
        // Remove from cache
        Graphics.Direct3D.RemoveTextureCache(filename);

        // Clean up
        if(texture != null) texture.Dispose();
        texture = null;
    }

    #endregion
}
