/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using SharpDX.Direct3D9;

namespace CodeImp.Bloodmasters.Client.Resources;

public interface ITextureResource
{
    Texture Texture { get; }
    ImageInformation Info { get; }
}
