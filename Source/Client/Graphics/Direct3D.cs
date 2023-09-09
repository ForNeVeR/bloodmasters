/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

// The Direct3D class contains functions for Direct3D related
// functionality which are used throughout this engine. Bla.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Direct3D9;
using SharpDX.Mathematics.Interop;
using Color = System.Drawing.Color;
using Rectangle = System.Drawing.Rectangle;

namespace CodeImp.Bloodmasters.Client;

internal sealed class Direct3D
{
    #region ================== Constants

    // Validation?
    private const bool HARDWARE_VALIDATION = true;
    private const string NVPERFHUD_ADAPTER = "NVIDIA NVPerfHUD";

    #endregion

    #region ================== Variables

    private static Direct3DEx _direct3D;

    // Devices
    public static Device d3dd;
    public static Surface backbuffer;
    public static Surface depthbuffer;
    private static Form rendertarget;
    private static AdapterInformation adapter;

    // Current settings
    private static DisplayModeEx displaymode;
    private static bool displaywindowed;
    private static bool displaysyncrefresh;
    private static int displayfsaa;
    private static PresentParameters displaypp;
    private static Format lightmapformat;
    private static int displaygamma;
    public static bool hightextures;
    private static RawViewport displayviewport;
    private static Rectangle screencliprect;

    // Resources
    private static Dictionary<string, Resource> resources;
    private static Dictionary<string, TextureResource> textures;
    private static int resourceid = 0;

    // Stateblocks
    private static DRAWMODE lastdrawmode = DRAWMODE.UNDEFINED;
    private static StateBlock sb_tlmodalpha;		// Used for TnL alpha blending with texture and argument
    private static StateBlock sb_nalpha;			// Used for standard alpha blending
    private static StateBlock sb_nadditivealpha;	// Used for additive alpha blending
    private static StateBlock sb_nlightmap;			// Used for rendering with a lightmap
    private static StateBlock sb_nlightmapalpha;	// Used for rendering objects
    private static StateBlock sb_tllightblend;		// Used for lightmap blending
    private static StateBlock sb_tllightdraw;		// Used for lightmap building
    private static StateBlock sb_nlines;			// Used for rendering lines
    private static StateBlock sb_pnormal;			// Used for normal particles
    private static StateBlock sb_padditive;			// Used for additive particles
    private static StateBlock sb_nlightblend;		// Used for lightmap blending

    #endregion

    #region ================== Properties

    // Display mode settings
    public static int DisplayAdapter { get { return adapter.Adapter; } }
    public static int DisplayWidth { get { return displaymode.Width; } set { displaymode.Width = value; } }
    public static int DisplayHeight { get { return displaymode.Height; } set { displaymode.Height = value; } }
    public static int DisplayFormat { get { return (int)displaymode.Format; } set { displaymode.Format = (Format)value; } }
    public static int DisplayRefreshRate { get { return displaymode.RefreshRate; } set { displaymode.RefreshRate = value; } }
    public static bool DisplayWindowed { get { return displaywindowed; } set { displaywindowed = value; } }
    public static bool DisplaySyncRefresh { get { return displaysyncrefresh; } set { displaysyncrefresh = value; } }
    public static int DisplayFSAA { get { return displayfsaa; } set { displayfsaa = value; } }
    public static int DisplayGamma { get { return displaygamma; } set { displaygamma = value; } }
    public static DisplayModeEx DisplayMode { get { return displaymode; } set { displaymode = value; } }
    public static Format LightmapFormat { get { return lightmapformat; } }
    public static RawViewport DisplayViewport { get { return displayviewport; } }
    public static Rectangle ScreenClipRectangle { get { return screencliprect; } }

    #endregion

    #region ================== Enumerating

    // This finds and selects a valid adapter if the
    // current adapter is not valid
    // Returns null when no valid adapter was found.
    public static string SelectValidAdapter()
    {
        string error;
        string result = null;

        // Check if the NVPerfHud adapter exists
        foreach(AdapterInformation a in _direct3D.Adapters)
        {
            // Is this the NVPerfHud adapter?
            if(string.Compare(a.Details.Description, NVPERFHUD_ADAPTER, StringComparison.OrdinalIgnoreCase) == 0)
            {
                // Select this adapter
                adapter = a;
                return null;
            }
        }

        // Check if the current adapter is invalid
        if(ValidateDevice(adapter.Adapter) != null)
        {
            // Go for all adapters to find a valid adapter
            foreach(AdapterInformation a in _direct3D.Adapters)
            {
                // Select adapter if valid
                error = ValidateDevice(a.Adapter);
                if(error == null)
                {
                    // Valid one found
                    adapter = a;
                    return null;
                }
                else
                {
                    // If this is the default adapter
                    if(a.Adapter == 0)
                    {
                        // return this error as result
                        result = error;
                    }
                }
            }
        }
        else
        {
            // Adapter is valid
            return null;
        }

        // No valid adapter found
        return result;
    }

    // This returns the bitdepth for the given format
    public static int GetBitDepth(Format f)
    {
        switch(f)
        {
            case Format.A1R5G5B5:
            case Format.A4R4G4B4:
            case Format.A8R3G3B2:
            case Format.L6V5U5:
            case Format.R5G6B5:
            case Format.X1R5G5B5:
            case Format.X4R4G4B4:
                return 16;

            case Format.A8R8G8B8:
            case Format.Q8W8V8U8:
            case Format.X8L8V8U8:
            case Format.X8R8G8B8:
                return 32;

            case Format.R3G3B2:
                return 8;

            case Format.R8G8B8:
                return 24;

            default:
                return 0;
        }
    }

    // This selectes an adapter by its index
    public static void SelectAdapter(int index)
    {
        // Check if there is such an adapter
        if(index < _direct3D.AdapterCount)
        {
            // Select this adapter
            adapter = _direct3D.Adapters[index];
        }
        else
        {
            // Select the default adapter
            adapter = _direct3D.Adapters[0];
        }

        // Write setting to configuration
        General.config.WriteSetting("displaydriver", adapter);
    }

    // This returns a specific display mode
    public static DisplayModeEx GetDisplayMode(int index)
    {
        int counter = 0;

        // Enumerate all display modes
        foreach(DisplayModeEx d in GetAdapterDisplayModes(adapter))
        {
            // Return settings if this the mode to use
            if(index == counter) return d;

            // Next mode
            counter++;
        }

        // Nothing
        return new DisplayModeEx();
    }
    #endregion

    #region ================== Capabilities Validation

    // This finds the closest matching display mode
    private static bool FindDisplayMode(DisplayModeEx mode, bool windowed, int fsaa)
    {
        // In case windowed is true, the display format
        // must be set to the current format
        if(windowed) mode.Format = adapter.CurrentDisplayMode.Format;

        // Go for all display modes to find the one specified
        var allmodes = GetAdapterDisplayModes(adapter);
        foreach(DisplayModeEx dm in allmodes)
        {
            // Check if this is the same mode
            if((dm.Width == mode.Width) &&
               (dm.Height == mode.Height) &&
               (dm.Format == mode.Format) &&
               (dm.RefreshRate == mode.RefreshRate))
            {
                // Check if this format is supported
                if(ValidateDisplayMode(dm, windowed))
                {
                    // Set display mode and return success
                    mode = dm;
                    return true;
                }
            }
        }

        // If the exact mode could not be found,
        // try searching again, but disregard the refreshrate.
        // Go for all display modes to find a matching mode
        allmodes = GetAdapterDisplayModes(adapter);
        foreach(DisplayModeEx dm in allmodes)
        {
            // Check if this is the same mode
            if((dm.Width == mode.Width) &&
               (dm.Height == mode.Height) &&
               (dm.Format == mode.Format))
            {
                // Check if this format is supported
                if(ValidateDisplayMode(dm, windowed))
                {
                    // Set display mode and return success
                    mode = dm;
                    return true;
                }
            }
        }

        // If the mode can still not be found,
        // try searching again, but disregard refreshrate and format.
        // Go for all display modes to find a matching mode
        allmodes = GetAdapterDisplayModes(adapter);
        foreach(DisplayModeEx dm in allmodes)
        {
            // Check if this is the same mode
            if((dm.Width == mode.Width) &&
               (dm.Height == mode.Height))
            {
                // Check if this format is supported
                if(ValidateDisplayMode(dm, windowed))
                {
                    // Set display mode and return success
                    mode = dm;
                    return true;
                }
            }
        }

        // Still no matching display mode found?
        // Then just pick the first valid one.
        // Go for all display modes to find the first valid mode
        allmodes = GetAdapterDisplayModes(adapter);
        foreach(DisplayModeEx dm in allmodes)
        {
            // Check if this format is supported
            if(ValidateDisplayMode(dm, windowed))
            {
                // Set display mode and return success
                mode = dm;
                return true;
            }
        }

        // No valid mode found
        return false;
    }

    // This tests if the given display mode is supported and
    // if it supports everything this engine needs
    public static bool ValidateDisplayMode(DisplayModeEx mode, bool windowed)
    {
        // The resolution must be at least 640x480
        if((mode.Width < 640) || (mode.Height < 480)) return false;

        var direct3d = _direct3D;
        // Test if the display format is supported by the device
        var result = direct3d.CheckDeviceType(adapter.Adapter, DeviceType.Hardware,
            mode.Format, mode.Format, windowed);
        if(!result) return false;

        // Test if we can create surfaces of display format
        result = direct3d.CheckDeviceFormat(adapter.Adapter, DeviceType.Hardware, mode.Format,
            0, ResourceType.Surface, mode.Format);
        if(!result) return false;

        // Test if we can create rendertarget textures of display format
        result = direct3d.CheckDeviceFormat(adapter.Adapter, DeviceType.Hardware, mode.Format,
            Usage.RenderTarget, ResourceType.Texture, mode.Format);
        if(!result) return false;

        // Everything seems to be supported
        return true;
    }

    // This tests if a device supports the needed features
    // Returns null when valid
    private static string ValidateDevice(int ad)
    {
        string result = null;
        string prefix = "Your video device does not support ";

        // Validate hardware?
        if(HARDWARE_VALIDATION)
        {
            try
            {
                // Get device caps
                var dc = _direct3D.GetDeviceCaps(ad, DeviceType.Hardware);

                // Here we go, the whole list of device requirements
                if(!dc.DestinationBlendCaps.HasFlag(BlendCaps.InverseSourceAlpha)) result = prefix + "Desination InverseSourceAlpha blending.";
                if(!dc.DestinationBlendCaps.HasFlag(BlendCaps.One)) result = prefix + "Desination One blending.";
                //if(!dc.DeviceCaps.SupportsTransformedVertexVideoMemory) result = prefix + "TransformedVertex in video memory.";
                if(!dc.DeviceCaps.HasFlag(DeviceCaps.TLVertexSystemMemory)) result = prefix + "TransformedVertex in system memory.";
                if(!dc.DeviceCaps2.HasFlag(DeviceCaps2.StreamOffset)) result = prefix + "a stream offset.";
                if(!(dc.MaxSimultaneousTextures >= 2)) result = prefix + "multitexture rendering.";
                if(!(dc.MaxStreams >= 1)) result = prefix + "streams.";
                if(!(dc.MaxStreamStride >= 32)) result = prefix + "32 byte streams.";
                if(!(dc.MaxTextureBlendStages >= 2)) result = prefix + "multiple texture blending stages.";
                if(!(dc.MaxTextureHeight >= 1024)) result = prefix + "1024 texture height.";
                if(!(dc.MaxTextureWidth >= 1024)) result = prefix + "1024 texture width.";
                if(!dc.PrimitiveMiscCaps.HasFlag(PrimitiveMiscCaps.CullCW)) result = prefix + "clockwise culling.";
                if(!dc.PrimitiveMiscCaps.HasFlag(PrimitiveMiscCaps.CullCCW)) result = prefix + "counterclockwise culling.";
                if(!dc.PrimitiveMiscCaps.HasFlag(PrimitiveMiscCaps.CullNone)) result = prefix + "rendering without culling.";
                if(!dc.ShadeCaps.HasFlag(ShadeCaps.ColorGouraudRgb)) result = prefix + "gouraud shading.";
                if(!dc.ShadeCaps.HasFlag(ShadeCaps.AlphaGouraudBlend)) result = prefix + "alpha gouraud shading.";
                if(!dc.SourceBlendCaps.HasFlag(BlendCaps.SourceAlpha)) result = prefix + "Source SourceAlpha blending.";
                if(!dc.SourceBlendCaps.HasFlag(BlendCaps.One)) result = prefix + "Source One blending.";
                //if(!dc.TextureAddressCaps.SupportsClamp) result = prefix + "clamped texture addressing.";
                //if(!dc.TextureAddressCaps.SupportsMirror) result = prefix + "mirrored texture addressing.";
                if(!dc.TextureAddressCaps.HasFlag(TextureAddressCaps.Wrap)) result = prefix + "wrapped texture addressing.";
                if(!dc.TextureCaps.HasFlag(TextureCaps.Alpha)) result = prefix + "texture alpha channels.";
                if(!dc.TextureOperationCaps.HasFlag(TextureOperationCaps.SelectArg1)) result = prefix + "SelectArg1 texture operation.";
                if(!dc.TextureOperationCaps.HasFlag(TextureOperationCaps.Modulate)) result = prefix + "Modulate texture operation.";
            }
            // When exception was thrown, this device is not valid
            catch(Exception) { result = "Unexpected problem while validating the video device."; }
        }

        // Return result
        return result;
    }

    // This chooses the best lightmap texture format
    private static void ChooseLightmapFormat()
    {
        // Rendertarget textures of X8R8G8B8 format?
        var result = _direct3D.CheckDeviceFormat(adapter.Adapter, DeviceType.Hardware, displaymode.Format,
            Usage.RenderTarget, ResourceType.Texture, Format.X8R8G8B8);
        if(result)
        {
            // Use X8R8G8B8 format for lightmaps
            lightmapformat = Format.X8R8G8B8;
        }
        else
        {
            // Then use the display format
            lightmapformat = displaymode.Format;
        }

        // Pascal: The R5G6B5 format seems of higher contast than X8R8G8B8
        // Until a solution to this problem is know, do not use other formats
        /*
        // High quality lightmaps?
        if(StaticLight.highlightmaps)
        {
            // Use display format
            lightmapformat = displaymode.Format;
        }
        else
        {
            // Rendertarget textures of L8 format?
            Manager.CheckDeviceFormat(adapter.Adapter, DeviceType.Hardware, displaymode.Format,
                        Usage.RenderTarget, ResourceType.Textures, Format.L8, out result);
            if(result == 0)
            {
                // Use L8 format for lightmaps
                lightmapformat = Format.L8;
            }
            else
            {
                // Rendertarget textures of R5G6B5 format?
                Manager.CheckDeviceFormat(adapter.Adapter, DeviceType.Hardware, displaymode.Format,
                            Usage.RenderTarget, ResourceType.Textures, Format.R5G6B5, out result);
                if(result == 0)
                {
                    // Use R5G6B5 format for lightmaps
                    lightmapformat = Format.R5G6B5;
                }
                else
                {
                    // Then use the display format
                    lightmapformat = displaymode.Format;
                }
            }
        }
        */
    }

    #endregion

    #region ================== Initialization, Reset and Termination

    // This does very early DirectX initialization
    // Calling this function will throw an Exception when DirectX is not installed
    public static void InitDX()
    {
        // Initialize variables
        _direct3D = new Direct3DEx();
        adapter = _direct3D.Adapters[0];
        displaymode = new DisplayModeEx();
    }

    public static void DeinitDirectX()
    {
        _direct3D.Dispose();
        _direct3D = null;
    }

    // This sets up renderstates
    private static void SetupRenderstates()
    {
        // Global renderstates
        d3dd.SetRenderState(RenderState.Ambient, Color.White.ToArgb());
        d3dd.SetRenderState(RenderState.AmbientMaterialSource, ColorSource.Material);
        d3dd.SetRenderState(RenderState.BlendOperation, BlendOperation.Add);
        d3dd.SetRenderState(RenderState.ColorVertex, true);
        d3dd.SetRenderState(RenderState.DiffuseMaterialSource, ColorSource.Color1);
        d3dd.SetRenderState(RenderState.FillMode, FillMode.Solid);
        d3dd.SetRenderState(RenderState.FogEnable, false);
        d3dd.SetRenderState(RenderState.Lighting, false);
        d3dd.SetRenderState(RenderState.LocalViewer, false);
        d3dd.SetRenderState(RenderState.NormalizeNormals, false);
        d3dd.SetRenderState(RenderState.ShadeMode, ShadeMode.Gouraud);
        d3dd.SetRenderState(RenderState.SpecularEnable, false);
        d3dd.SetRenderState(RenderState.StencilEnable, false);
        d3dd.SetRenderState(RenderState.PointSpriteEnable, false);

        // Texture filters
        d3dd.SetSamplerState(0, SamplerState.MagFilter, (int)TextureFilter.Linear);
        d3dd.SetSamplerState(0, SamplerState.MinFilter, (int)TextureFilter.Linear);
        d3dd.SetSamplerState(0, SamplerState.MipFilter, (int)TextureFilter.Linear);

        // Lightmap filters
        d3dd.SetSamplerState(1, SamplerState.AddressU, (int)TextureAddress.Clamp);
        d3dd.SetSamplerState(1, SamplerState.AddressV, (int)TextureAddress.Clamp);
        d3dd.SetSamplerState(1, SamplerState.AddressW, (int)TextureAddress.Clamp);
        d3dd.SetSamplerState(2, SamplerState.AddressU, (int)TextureAddress.Clamp);
        d3dd.SetSamplerState(2, SamplerState.AddressV, (int)TextureAddress.Clamp);
        d3dd.SetSamplerState(2, SamplerState.AddressW, (int)TextureAddress.Clamp);
        d3dd.SetSamplerState(1, SamplerState.MagFilter, (int)TextureFilter.Linear);
        d3dd.SetSamplerState(1, SamplerState.MinFilter, (int)TextureFilter.Linear);
        d3dd.SetSamplerState(1, SamplerState.MipFilter, (int)TextureFilter.Linear);
        d3dd.SetSamplerState(2, SamplerState.MagFilter, (int)TextureFilter.Linear);
        d3dd.SetSamplerState(2, SamplerState.MinFilter, (int)TextureFilter.Linear);
        d3dd.SetSamplerState(2, SamplerState.MipFilter, (int)TextureFilter.Linear);

        // Global material
        var m = new Material();
        m.Ambient = Color4.White;
        m.Diffuse = Color4.White;
        m.Specular = Color4.White;
        d3dd.Material = m;

        // ===== NORMAL LINES STATEBLOCK
        d3dd.BeginStateBlock();
        d3dd.VertexFormat = LVertex.Format;
        d3dd.SetRenderState(RenderState.AlphaBlendEnable, false);
        d3dd.SetRenderState(RenderState.ZEnable, false);
        d3dd.SetRenderState(RenderState.ZWriteEnable, false);
        d3dd.SetRenderState(RenderState.Clipping, true);
        d3dd.SetRenderState(RenderState.CullMode, Cull.None);

        // First texture stage
        d3dd.SetTextureStageState(0, TextureStage.ColorOperation, TextureOperation.SelectArg1);
        d3dd.SetTextureStageState(0, TextureStage.ColorArg1, TextureArgument.Diffuse);
        d3dd.SetTextureStageState(0, TextureStage.ColorArg2, TextureArgument.Diffuse);
        d3dd.SetTextureStageState(0, TextureStage.ResultArg, TextureArgument.Current);

        // No more further stages
        d3dd.SetTextureStageState(1, TextureStage.ColorOperation, TextureOperation.Disable);

        // First alpha stage
        d3dd.SetTextureStageState(0, TextureStage.AlphaOperation, TextureOperation.SelectArg1);

        // No further stages
        d3dd.SetTextureStageState(1, TextureStage.AlphaOperation, TextureOperation.Disable);

        sb_nlines = d3dd.EndStateBlock();

        // ===== NORMAL ALPHA STATEBLOCK
        d3dd.BeginStateBlock();
        d3dd.VertexFormat = MVertex.Format;
        d3dd.SetRenderState(RenderState.DitherEnable, true);
        d3dd.SetRenderState(RenderState.AlphaBlendEnable, true);
        d3dd.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
        d3dd.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha);
        d3dd.SetRenderState(RenderState.ZEnable, true);
        d3dd.SetRenderState(RenderState.ZWriteEnable, true);
        d3dd.SetRenderState(RenderState.Clipping, true);
        d3dd.SetRenderState(RenderState.CullMode, Cull.None);

        // Texture addressing
        d3dd.SetSamplerState(0, SamplerState.AddressU, TextureAddress.Wrap);
        d3dd.SetSamplerState(0, SamplerState.AddressV, TextureAddress.Wrap);
        d3dd.SetSamplerState(0, SamplerState.AddressW, TextureAddress.Wrap);

        // First texture stage
        d3dd.SetTextureStageState(0, TextureStage.ColorOperation, TextureOperation.Modulate);
        d3dd.SetTextureStageState(0, TextureStage.ColorArg1, TextureArgument.Texture);
        d3dd.SetTextureStageState(0, TextureStage.ColorArg2, TextureArgument.Diffuse);
        d3dd.SetTextureStageState(0, TextureStage.ResultArg, TextureArgument.Current);
        d3dd.SetTextureStageState(0, TextureStage.TexCoordIndex, 0);
        d3dd.SetTextureStageState(0, TextureStage.TextureTransformFlags, TextureTransform.Disable);

        // No more further stages
        d3dd.SetTextureStageState(1, TextureStage.ColorOperation, TextureOperation.Disable);

        // First alpha stage
        d3dd.SetTextureStageState(0, TextureStage.AlphaOperation, TextureOperation.Modulate);
        d3dd.SetTextureStageState(0, TextureStage.AlphaArg1, TextureArgument.Texture);
        d3dd.SetTextureStageState(0, TextureStage.AlphaArg2, TextureArgument.TFactor);

        // No further stages
        d3dd.SetTextureStageState(1, TextureStage.AlphaOperation, TextureOperation.Disable);

        sb_nalpha = d3dd.EndStateBlock();

        // ===== ADDITIVE ALPHA STATEBLOCK
        d3dd.BeginStateBlock();
        d3dd.VertexFormat = MVertex.Format;
        d3dd.SetRenderState(RenderState.DitherEnable, true);
        d3dd.SetRenderState(RenderState.AlphaBlendEnable, true);
        d3dd.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
        d3dd.SetRenderState(RenderState.DestinationBlend, Blend.One);
        d3dd.SetRenderState(RenderState.ZEnable, true);
        d3dd.SetRenderState(RenderState.ZWriteEnable, false);
        d3dd.SetRenderState(RenderState.Clipping, true);
        d3dd.SetRenderState(RenderState.CullMode, Cull.None);

        // Texture addressing
        d3dd.SetSamplerState(0, SamplerState.AddressU, TextureAddress.Wrap);
        d3dd.SetSamplerState(0, SamplerState.AddressV, TextureAddress.Wrap);
        d3dd.SetSamplerState(0, SamplerState.AddressW, TextureAddress.Wrap);

        // First texture stage
        d3dd.SetTextureStageState(0, TextureStage.ColorOperation, TextureOperation.Modulate);
        d3dd.SetTextureStageState(0, TextureStage.ColorArg1, TextureArgument.Texture);
        d3dd.SetTextureStageState(0, TextureStage.ColorArg2, TextureArgument.TFactor);
        d3dd.SetTextureStageState(0, TextureStage.ResultArg, TextureArgument.Current);
        d3dd.SetTextureStageState(0, TextureStage.TexCoordIndex, 0);
        d3dd.SetTextureStageState(0, TextureStage.TextureTransformFlags, TextureTransform.Disable);

        // No more further stages
        d3dd.SetTextureStageState(1, TextureStage.ColorOperation, TextureOperation.Disable);

        // First alpha stage
        d3dd.SetTextureStageState(0, TextureStage.AlphaOperation, TextureOperation.Modulate);
        d3dd.SetTextureStageState(0, TextureStage.AlphaArg1, TextureArgument.Texture);
        d3dd.SetTextureStageState(0, TextureStage.AlphaArg2, TextureArgument.TFactor);

        // No further stages
        d3dd.SetTextureStageState(1, TextureStage.AlphaOperation, TextureOperation.Disable);

        sb_nadditivealpha = d3dd.EndStateBlock();

        // ===== DYNAMIC LIGHTMAP BLENDING STATEBLOCK
        d3dd.BeginStateBlock();
        d3dd.VertexFormat = MVertex.Format;
        d3dd.SetRenderState(RenderState.DitherEnable, false);
        d3dd.SetRenderState(RenderState.AlphaBlendEnable, true);
        d3dd.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
        d3dd.SetRenderState(RenderState.DestinationBlend, Blend.One);
        d3dd.SetRenderState(RenderState.ZEnable, false);
        d3dd.SetRenderState(RenderState.ZWriteEnable, false);
        d3dd.SetRenderState(RenderState.Clipping, false);
        d3dd.SetRenderState(RenderState.CullMode, Cull.None);

        // Texture addressing
        d3dd.SetSamplerState(0, SamplerState.AddressU, TextureAddress.Clamp);
        d3dd.SetSamplerState(0, SamplerState.AddressV, TextureAddress.Clamp);
        d3dd.SetSamplerState(0, SamplerState.AddressW, TextureAddress.Clamp);

        // First texture stage
        d3dd.SetTextureStageState(0, TextureStage.ColorOperation, TextureOperation.Modulate);
        d3dd.SetTextureStageState(0, TextureStage.ColorArg1, TextureArgument.Texture);
        d3dd.SetTextureStageState(0, TextureStage.ColorArg2, TextureArgument.TFactor);
        d3dd.SetTextureStageState(0, TextureStage.ResultArg, TextureArgument.Current);
        d3dd.SetTextureStageState(0, TextureStage.TexCoordIndex, 2);
        d3dd.SetTextureStageState(0, TextureStage.TextureTransformFlags, TextureTransform.Count2);

        // No more further stages
        d3dd.SetTextureStageState(1, TextureStage.ColorOperation, TextureOperation.Disable);
        d3dd.SetTextureStageState(1, TextureStage.TextureTransformFlags, TextureTransform.Disable);

        // First alpha stage
        d3dd.SetTextureStageState(0, TextureStage.AlphaOperation, TextureOperation.Modulate);
        d3dd.SetTextureStageState(0, TextureStage.AlphaArg1, TextureArgument.Texture);
        d3dd.SetTextureStageState(0, TextureStage.AlphaArg2, TextureArgument.TFactor);

        // No further stages
        d3dd.SetTextureStageState(1, TextureStage.AlphaOperation, TextureOperation.Disable);

        sb_nlightblend = d3dd.EndStateBlock();

        // ===== LIGHTMAP BLENDING STATEBLOCK
        d3dd.BeginStateBlock();
        d3dd.VertexFormat = TLVertex.Format;
        d3dd.SetRenderState(RenderState.DitherEnable, false);
        d3dd.SetRenderState(RenderState.AlphaBlendEnable, true);
        d3dd.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
        d3dd.SetRenderState(RenderState.DestinationBlend, Blend.One);
        d3dd.SetRenderState(RenderState.ZEnable, false);
        d3dd.SetRenderState(RenderState.ZWriteEnable, false);
        d3dd.SetRenderState(RenderState.Clipping, true);
        d3dd.SetRenderState(RenderState.CullMode, Cull.None);

        // First texture stage
        d3dd.SetTextureStageState(0, TextureStage.ColorOperation, TextureOperation.Modulate);
        d3dd.SetTextureStageState(0, TextureStage.ColorArg1, TextureArgument.Texture);
        d3dd.SetTextureStageState(0, TextureStage.ColorArg2, TextureArgument.Diffuse);
        d3dd.SetTextureStageState(0, TextureStage.ResultArg, TextureArgument.Current);
        d3dd.SetTextureStageState(0, TextureStage.TexCoordIndex, 0);
        d3dd.SetTextureStageState(0, TextureStage.TextureTransformFlags, TextureTransform.Disable);

        // No more further stages
        d3dd.SetTextureStageState(1, TextureStage.ColorOperation, TextureOperation.Disable);
        d3dd.SetTextureStageState(1, TextureStage.TextureTransformFlags, TextureTransform.Disable);

        // First alpha stage
        d3dd.SetTextureStageState(0, TextureStage.AlphaOperation, TextureOperation.Modulate);
        d3dd.SetTextureStageState(0, TextureStage.AlphaArg1, TextureArgument.Texture);
        d3dd.SetTextureStageState(0, TextureStage.AlphaArg2, TextureArgument.Diffuse);

        // No further stages
        d3dd.SetTextureStageState(1, TextureStage.AlphaOperation, TextureOperation.Disable);

        sb_tllightblend = d3dd.EndStateBlock();

        // ===== LIGHTMAP DRAWING STATEBLOCK
        d3dd.BeginStateBlock();
        d3dd.VertexFormat = TLVertex.Format;
        d3dd.SetRenderState(RenderState.DitherEnable, false);
        d3dd.SetRenderState(RenderState.AlphaBlendEnable, true);
        d3dd.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
        d3dd.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha);
        d3dd.SetRenderState(RenderState.ZEnable, false);
        d3dd.SetRenderState(RenderState.ZWriteEnable, false);
        d3dd.SetRenderState(RenderState.Clipping, true);
        d3dd.SetRenderState(RenderState.CullMode, Cull.None);
        d3dd.SetRenderState(RenderState.TextureFactor, General.ARGB(1f, 1f, 1f, 1f));

        // Texture addressing
        d3dd.SetSamplerState(0, SamplerState.AddressU, TextureAddress.Clamp);
        d3dd.SetSamplerState(0, SamplerState.AddressV, TextureAddress.Clamp);
        d3dd.SetSamplerState(0, SamplerState.AddressW, TextureAddress.Clamp);

        // First texture stage
        d3dd.SetTextureStageState(0, TextureStage.ColorOperation, TextureOperation.SelectArg1);
        d3dd.SetTextureStageState(0, TextureStage.ColorArg1, TextureArgument.Texture);
        d3dd.SetTextureStageState(0, TextureStage.ResultArg, TextureArgument.Current);
        d3dd.SetTextureStageState(0, TextureStage.TexCoordIndex, 0);
        d3dd.SetTextureStageState(0, TextureStage.TextureTransformFlags, TextureTransform.Disable);

        // No more further stages
        d3dd.SetTextureStageState(1, TextureStage.ColorOperation, TextureOperation.Disable);
        d3dd.SetTextureStageState(1, TextureStage.TextureTransformFlags, TextureTransform.Disable);

        // First alpha stage
        d3dd.SetTextureStageState(0, TextureStage.AlphaOperation, TextureOperation.Modulate);
        d3dd.SetTextureStageState(0, TextureStage.AlphaArg1, TextureArgument.Texture);
        d3dd.SetTextureStageState(0, TextureStage.AlphaArg2, TextureArgument.TFactor);

        // No further stages
        d3dd.SetTextureStageState(1, TextureStage.AlphaOperation, TextureOperation.Disable);

        sb_tllightdraw = d3dd.EndStateBlock();

        // ===== NORMAL LIGHTMAP STATEBLOCK
        d3dd.BeginStateBlock();
        d3dd.VertexFormat = MVertex.Format;
        d3dd.SetRenderState(RenderState.DitherEnable, true);
        d3dd.SetRenderState(RenderState.AlphaBlendEnable, false);
        d3dd.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
        d3dd.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha);
        d3dd.SetRenderState(RenderState.ZEnable, true);
        d3dd.SetRenderState(RenderState.ZWriteEnable, true);
        d3dd.SetRenderState(RenderState.Clipping, true);
        d3dd.SetRenderState(RenderState.CullMode, Cull.None);

        // Texture addressing
        d3dd.SetSamplerState(0, SamplerState.AddressU, TextureAddress.Wrap);
        d3dd.SetSamplerState(0, SamplerState.AddressV, TextureAddress.Wrap);
        d3dd.SetSamplerState(0, SamplerState.AddressW, TextureAddress.Wrap);
        d3dd.SetSamplerState(1, SamplerState.AddressU, TextureAddress.Clamp);
        d3dd.SetSamplerState(1, SamplerState.AddressV, TextureAddress.Clamp);
        d3dd.SetSamplerState(1, SamplerState.AddressW, TextureAddress.Clamp);
        d3dd.SetSamplerState(2, SamplerState.AddressU, TextureAddress.Clamp);
        d3dd.SetSamplerState(2, SamplerState.AddressV, TextureAddress.Clamp);
        d3dd.SetSamplerState(2, SamplerState.AddressW, TextureAddress.Clamp);

        // First texture stage
        d3dd.SetTextureStageState(0, TextureStage.ColorOperation, TextureOperation.Modulate);
        d3dd.SetTextureStageState(0, TextureStage.ColorArg1, TextureArgument.Texture);
        d3dd.SetTextureStageState(0, TextureStage.ColorArg2, TextureArgument.Diffuse);
        d3dd.SetTextureStageState(0, TextureStage.ResultArg, TextureArgument.Current);
        d3dd.SetTextureStageState(0, TextureStage.TexCoordIndex, 0);

        // First alpha stage
        d3dd.SetTextureStageState(0, TextureStage.AlphaOperation, TextureOperation.SelectArg1);
        d3dd.SetTextureStageState(0, TextureStage.AlphaArg1, TextureArgument.Texture);

        // Second alpha stage
        d3dd.SetTextureStageState(1, TextureStage.AlphaOperation, TextureOperation.SelectArg1);
        d3dd.SetTextureStageState(1, TextureStage.AlphaArg1, TextureArgument.Current);

        // Only when using dynamic lights
        if(DynamicLight.dynamiclights)
        {
            // Second texture stage
            d3dd.SetTextureStageState(1, TextureStage.ColorOperation, TextureOperation.SelectArg1);
            d3dd.SetTextureStageState(1, TextureStage.ColorArg1, TextureArgument.Texture);
            d3dd.SetTextureStageState(1, TextureStage.ColorArg2, TextureArgument.Current);
            d3dd.SetTextureStageState(1, TextureStage.ResultArg, TextureArgument.Temp);
            d3dd.SetTextureStageState(1, TextureStage.TexCoordIndex, 1);
            d3dd.SetTextureStageState(1, TextureStage.TextureTransformFlags, TextureTransform.Disable);

            // Third texture stage
            d3dd.SetTextureStageState(2, TextureStage.ColorOperation, TextureOperation.Add);
            d3dd.SetTextureStageState(2, TextureStage.ColorArg1, TextureArgument.Texture);
            d3dd.SetTextureStageState(2, TextureStage.ColorArg2, TextureArgument.Temp);
            d3dd.SetTextureStageState(2, TextureStage.ResultArg, TextureArgument.Temp);
            d3dd.SetTextureStageState(2, TextureStage.TexCoordIndex, 2);
            d3dd.SetTextureStageState(2, TextureStage.TextureTransformFlags, TextureTransform.Count2);

            // Fourth texture stage
            d3dd.SetTextureStageState(3, TextureStage.ColorOperation, TextureOperation.Modulate2X);
            d3dd.SetTextureStageState(3, TextureStage.ColorArg1, TextureArgument.Temp);
            d3dd.SetTextureStageState(3, TextureStage.ColorArg2, TextureArgument.Current);
            d3dd.SetTextureStageState(3, TextureStage.ResultArg, TextureArgument.Current);
            d3dd.SetTextureStageState(3, TextureStage.TextureTransformFlags, TextureTransform.Disable);

            // Third alpha stage
            d3dd.SetTextureStageState(2, TextureStage.AlphaOperation, TextureOperation.SelectArg1);
            d3dd.SetTextureStageState(2, TextureStage.AlphaArg1, TextureArgument.Current);

            // Fourth alpha stage
            d3dd.SetTextureStageState(3, TextureStage.AlphaOperation, TextureOperation.SelectArg1);
            d3dd.SetTextureStageState(3, TextureStage.AlphaArg1, TextureArgument.Current);

            // No further stages
            d3dd.SetTextureStageState(4, TextureStage.ColorOperation, TextureOperation.Disable);
            d3dd.SetTextureStageState(4, TextureStage.AlphaOperation, TextureOperation.Disable);
        }
        else
        {
            // Second texture stage
            d3dd.SetTextureStageState(1, TextureStage.ColorOperation, TextureOperation.Modulate2X);
            d3dd.SetTextureStageState(1, TextureStage.ColorArg1, TextureArgument.Texture);
            d3dd.SetTextureStageState(1, TextureStage.ColorArg2, TextureArgument.Current);
            d3dd.SetTextureStageState(1, TextureStage.ResultArg, TextureArgument.Current);
            d3dd.SetTextureStageState(1, TextureStage.TexCoordIndex, 1);
            d3dd.SetTextureStageState(1, TextureStage.TextureTransformFlags, TextureTransform.Disable);

            // No more further stages
            d3dd.SetTextureStageState(2, TextureStage.ColorOperation, TextureOperation.Disable);
            d3dd.SetTextureStageState(2, TextureStage.AlphaOperation, TextureOperation.Disable);
        }

        sb_nlightmap = d3dd.EndStateBlock();

        // ===== LIGHTMAP, ALPHA AND TEXTURE TRANSFORM STATEBLOCK
        d3dd.BeginStateBlock();
        d3dd.VertexFormat = MVertex.Format;
        d3dd.SetRenderState(RenderState.DitherEnable, true);
        d3dd.SetRenderState(RenderState.AlphaBlendEnable, true);
        d3dd.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
        d3dd.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha);
        d3dd.SetRenderState(RenderState.TextureFactor, General.ARGB(1f, 1f, 1f, 1f));
        d3dd.SetRenderState(RenderState.ZEnable, true);
        d3dd.SetRenderState(RenderState.ZWriteEnable, false);
        d3dd.SetRenderState(RenderState.Clipping, true);
        d3dd.SetRenderState(RenderState.CullMode, Cull.None);

        // Texture addressing
        d3dd.SetSamplerState(0, SamplerState.AddressU, TextureAddress.Clamp);
        d3dd.SetSamplerState(0, SamplerState.AddressV, TextureAddress.Clamp);
        d3dd.SetSamplerState(0, SamplerState.AddressW, TextureAddress.Clamp);
        d3dd.SetSamplerState(1, SamplerState.AddressU, TextureAddress.Clamp);
        d3dd.SetSamplerState(1, SamplerState.AddressV, TextureAddress.Clamp);
        d3dd.SetSamplerState(1, SamplerState.AddressW, TextureAddress.Clamp);
        d3dd.SetSamplerState(2, SamplerState.AddressU, TextureAddress.Clamp);
        d3dd.SetSamplerState(2, SamplerState.AddressV, TextureAddress.Clamp);
        d3dd.SetSamplerState(2, SamplerState.AddressW, TextureAddress.Clamp);

        // First texture stage
        d3dd.SetTextureStageState(0, TextureStage.ColorOperation, TextureOperation.Modulate);
        d3dd.SetTextureStageState(0, TextureStage.ColorArg1, TextureArgument.Texture);
        d3dd.SetTextureStageState(0, TextureStage.ColorArg2, TextureArgument.Diffuse);
        d3dd.SetTextureStageState(0, TextureStage.ResultArg, TextureArgument.Current);
        d3dd.SetTextureStageState(0, TextureStage.TexCoordIndex, 0);
        d3dd.SetTextureStageState(0, TextureStage.TextureTransformFlags, TextureTransform.Count2);

        // First alpha stage
        d3dd.SetTextureStageState(0, TextureStage.AlphaOperation, TextureOperation.Modulate);
        d3dd.SetTextureStageState(0, TextureStage.AlphaArg1, TextureArgument.Texture);
        d3dd.SetTextureStageState(0, TextureStage.AlphaArg2, TextureArgument.TFactor);

        // Second alpha stage
        d3dd.SetTextureStageState(1, TextureStage.AlphaOperation, TextureOperation.SelectArg1);
        d3dd.SetTextureStageState(1, TextureStage.AlphaArg1, TextureArgument.Current);

        // Only when using dynamic lights
        if(DynamicLight.dynamiclights)
        {
            // Second texture stage
            d3dd.SetTextureStageState(1, TextureStage.ColorOperation, TextureOperation.SelectArg1);
            d3dd.SetTextureStageState(1, TextureStage.ColorArg1, TextureArgument.Texture);
            d3dd.SetTextureStageState(1, TextureStage.ColorArg2, TextureArgument.Current);
            d3dd.SetTextureStageState(1, TextureStage.ResultArg, TextureArgument.Temp);
            d3dd.SetTextureStageState(1, TextureStage.TexCoordIndex, 1);
            d3dd.SetTextureStageState(1, TextureStage.TextureTransformFlags, TextureTransform.Count2);

            // Third texture stage
            d3dd.SetTextureStageState(2, TextureStage.ColorOperation, TextureOperation.Add);
            d3dd.SetTextureStageState(2, TextureStage.ColorArg1, TextureArgument.Texture);
            d3dd.SetTextureStageState(2, TextureStage.ColorArg2, TextureArgument.Temp);
            d3dd.SetTextureStageState(2, TextureStage.ResultArg, TextureArgument.Temp);
            d3dd.SetTextureStageState(2, TextureStage.TexCoordIndex, 2);
            d3dd.SetTextureStageState(2, TextureStage.TextureTransformFlags, TextureTransform.Count2);

            // Fourth texture stage
            d3dd.SetTextureStageState(3, TextureStage.ColorOperation, TextureOperation.Modulate2X);
            d3dd.SetTextureStageState(3, TextureStage.ColorArg1, TextureArgument.Temp);
            d3dd.SetTextureStageState(3, TextureStage.ColorArg2, TextureArgument.Current);
            d3dd.SetTextureStageState(3, TextureStage.ResultArg, TextureArgument.Current);
            d3dd.SetTextureStageState(3, TextureStage.TextureTransformFlags, TextureTransform.Disable);

            // Third alpha stage
            d3dd.SetTextureStageState(2, TextureStage.AlphaOperation, TextureOperation.SelectArg1);
            d3dd.SetTextureStageState(2, TextureStage.AlphaArg1, TextureArgument.Current);

            // Fourth alpha stage
            d3dd.SetTextureStageState(3, TextureStage.AlphaOperation, TextureOperation.SelectArg1);
            d3dd.SetTextureStageState(3, TextureStage.AlphaArg1, TextureArgument.Current);

            // No more further stages
            d3dd.SetTextureStageState(4, TextureStage.ColorOperation, TextureOperation.Disable);
            d3dd.SetTextureStageState(4, TextureStage.AlphaOperation, TextureOperation.Disable);
        }
        else
        {
            // Second texture stage
            d3dd.SetTextureStageState(1, TextureStage.ColorOperation, TextureOperation.Modulate2X);
            d3dd.SetTextureStageState(1, TextureStage.ColorArg1, TextureArgument.Texture);
            d3dd.SetTextureStageState(1, TextureStage.ColorArg2, TextureArgument.Current);
            d3dd.SetTextureStageState(1, TextureStage.ResultArg, TextureArgument.Current);
            d3dd.SetTextureStageState(1, TextureStage.TexCoordIndex, 1);
            d3dd.SetTextureStageState(1, TextureStage.TextureTransformFlags, TextureTransform.Count2);

            // No more further stages
            d3dd.SetTextureStageState(2, TextureStage.ColorOperation, TextureOperation.Disable);
            d3dd.SetTextureStageState(2, TextureStage.AlphaOperation, TextureOperation.Disable);
        }

        sb_nlightmapalpha = d3dd.EndStateBlock();

        // ===== TNL MODULATE ALPHA STATEBLOCK
        d3dd.BeginStateBlock();
        d3dd.VertexFormat = TLVertex.Format;
        d3dd.SetRenderState(RenderState.DitherEnable, true);
        d3dd.SetRenderState(RenderState.AlphaBlendEnable, true);
        d3dd.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
        d3dd.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha);
        d3dd.SetRenderState(RenderState.TextureFactor, General.ARGB(1f, 1f, 1f, 1f));
        d3dd.SetRenderState(RenderState.ZEnable, false);
        d3dd.SetRenderState(RenderState.ZWriteEnable, false);
        d3dd.SetRenderState(RenderState.Clipping, false);
        d3dd.SetRenderState(RenderState.CullMode, Cull.None);

        // Texture addressing
        d3dd.SetSamplerState(0, SamplerState.AddressU, TextureAddress.Wrap);
        d3dd.SetSamplerState(0, SamplerState.AddressV, TextureAddress.Wrap);
        d3dd.SetSamplerState(0, SamplerState.AddressW, TextureAddress.Wrap);

        // First texture stage
        d3dd.SetTextureStageState(0, TextureStage.ColorOperation, TextureOperation.Modulate);
        d3dd.SetTextureStageState(0, TextureStage.ColorArg1, TextureArgument.Texture);
        d3dd.SetTextureStageState(0, TextureStage.ColorArg2, TextureArgument.Diffuse);
        d3dd.SetTextureStageState(0, TextureStage.ResultArg, TextureArgument.Current);
        d3dd.SetTextureStageState(0, TextureStage.TexCoordIndex, 0);

        // No more further stages
        d3dd.SetTextureStageState(1, TextureStage.ColorOperation, TextureOperation.Disable);

        // First alpha stage
        d3dd.SetTextureStageState(0, TextureStage.AlphaOperation, TextureOperation.Modulate);
        d3dd.SetTextureStageState(0, TextureStage.AlphaArg1, TextureArgument.Texture);
        d3dd.SetTextureStageState(0, TextureStage.AlphaArg2, TextureArgument.TFactor);

        // No further stages
        d3dd.SetTextureStageState(1, TextureStage.AlphaOperation, TextureOperation.Disable);

        sb_tlmodalpha = d3dd.EndStateBlock();

        // ===== NORMAL PARTICLES STATEBLOCK
        d3dd.BeginStateBlock();
        d3dd.VertexFormat = MVertex.Format;
        d3dd.SetRenderState(RenderState.DitherEnable, true);
        d3dd.SetRenderState(RenderState.AlphaBlendEnable, true);
        d3dd.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
        d3dd.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha);
        d3dd.SetRenderState(RenderState.ZEnable, true);
        d3dd.SetRenderState(RenderState.ZWriteEnable, false);
        d3dd.SetRenderState(RenderState.Clipping, false);
        d3dd.SetRenderState(RenderState.CullMode, Cull.None);

        // Texture addressing
        d3dd.SetSamplerState(0, SamplerState.AddressU, TextureAddress.Wrap);
        d3dd.SetSamplerState(0, SamplerState.AddressV, TextureAddress.Wrap);
        d3dd.SetSamplerState(0, SamplerState.AddressW, TextureAddress.Wrap);

        // First texture stage
        d3dd.SetTextureStageState(0, TextureStage.ColorOperation, TextureOperation.Modulate);
        d3dd.SetTextureStageState(0, TextureStage.ColorArg1, TextureArgument.Texture);
        d3dd.SetTextureStageState(0, TextureStage.ColorArg2, TextureArgument.TFactor);
        d3dd.SetTextureStageState(0, TextureStage.ResultArg, TextureArgument.Current);
        d3dd.SetTextureStageState(0, TextureStage.TextureTransformFlags, TextureTransform.Count2);
        d3dd.SetTextureStageState(0, TextureStage.TexCoordIndex, 0);

        // Second texture stage
        d3dd.SetTextureStageState(1, TextureStage.ColorOperation, TextureOperation.Modulate2X);
        d3dd.SetTextureStageState(1, TextureStage.ColorArg1, TextureArgument.Texture);
        d3dd.SetTextureStageState(1, TextureStage.ColorArg2, TextureArgument.Current);
        d3dd.SetTextureStageState(1, TextureStage.ResultArg, TextureArgument.Current);
        d3dd.SetTextureStageState(1, TextureStage.TextureTransformFlags, TextureTransform.Count2);
        d3dd.SetTextureStageState(1, TextureStage.TexCoordIndex, 1);

        // No more further stages
        d3dd.SetTextureStageState(2, TextureStage.ColorOperation, TextureOperation.Disable);

        // First alpha stage
        d3dd.SetTextureStageState(0, TextureStage.AlphaOperation, TextureOperation.Modulate);
        d3dd.SetTextureStageState(0, TextureStage.AlphaArg1, TextureArgument.Texture);
        d3dd.SetTextureStageState(0, TextureStage.AlphaArg2, TextureArgument.TFactor);

        // No further stages
        d3dd.SetTextureStageState(1, TextureStage.AlphaOperation, TextureOperation.Disable);

        // No further stages
        d3dd.SetTextureStageState(2, TextureStage.AlphaOperation, TextureOperation.Disable);

        sb_pnormal = d3dd.EndStateBlock();

        // ===== ADDITIVE PARTICLES STATEBLOCK
        d3dd.BeginStateBlock();
        d3dd.VertexFormat = MVertex.Format;
        d3dd.SetRenderState(RenderState.DitherEnable, true);
        d3dd.SetRenderState(RenderState.AlphaBlendEnable, true);
        d3dd.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
        d3dd.SetRenderState(RenderState.DestinationBlend, Blend.One);
        d3dd.SetRenderState(RenderState.ZEnable, true);
        d3dd.SetRenderState(RenderState.ZWriteEnable, false);
        d3dd.SetRenderState(RenderState.Clipping, false);
        d3dd.SetRenderState(RenderState.CullMode, Cull.None);

        // Texture addressing
        d3dd.SetSamplerState(0, SamplerState.AddressU, TextureAddress.Wrap);
        d3dd.SetSamplerState(0, SamplerState.AddressV, TextureAddress.Wrap);
        d3dd.SetSamplerState(0, SamplerState.AddressW, TextureAddress.Wrap);

        // First texture stage
        d3dd.SetTextureStageState(0, TextureStage.ColorOperation, TextureOperation.Modulate);
        d3dd.SetTextureStageState(0, TextureStage.ColorArg1, TextureArgument.Texture);
        d3dd.SetTextureStageState(0, TextureStage.ColorArg2, TextureArgument.TFactor);
        d3dd.SetTextureStageState(0, TextureStage.ResultArg, TextureArgument.Current);
        d3dd.SetTextureStageState(0, TextureStage.TexCoordIndex, 0);

        // Second texture stage
        d3dd.SetTextureStageState(1, TextureStage.ColorOperation, TextureOperation.Disable);

        // First alpha stage
        d3dd.SetTextureStageState(0, TextureStage.AlphaOperation, TextureOperation.Modulate);
        d3dd.SetTextureStageState(0, TextureStage.AlphaArg1, TextureArgument.Texture);
        d3dd.SetTextureStageState(0, TextureStage.AlphaArg2, TextureArgument.TFactor);

        // No further stages
        d3dd.SetTextureStageState(1, TextureStage.AlphaOperation, TextureOperation.Disable);

        sb_padditive = d3dd.EndStateBlock();
    }

    // This sets the fullscreen gamma
    public static void SetColorCorrection(float gamma)
    {
        float r, g, b;
        GammaRamp ramp = new GammaRamp();
        short[] sr = new short[256];
        short[] sg = new short[256];
        short[] sb = new short[256];
        float brighten = (gamma - 1f) * 10000f;

        // Go for all 256 color shades
        for(int i = 0; i < 256; i++)
        {
            // Create original colors
            r = 257 * i;
            g = 257 * i;
            b = 257 * i;

            // Adjust with gamma
            r = r * gamma + brighten;
            g = g * gamma + brighten;
            b = b * gamma + brighten;

            // Limit colors to max/min
            if(r > 65535) r = 65535; else if(r < 0) r = 0;
            if(g > 65535) g = 65535; else if(g < 0) g = 0;
            if(b > 65535) b = 65535; else if(b < 0) b = 0;

            // Apply colors to the ramps
            sr[i] = unchecked((short)((ushort)r));
            sg[i] = unchecked((short)((ushort)g));
            sb[i] = unchecked((short)((ushort)b));
        }

        // Apply the gamma ramps
        Array.Copy(sr, ramp.Red, sr.Length);
        Array.Copy(sg, ramp.Green, sr.Length);
        Array.Copy(sb, ramp.Blue, sr.Length);
        d3dd.SetGammaRamp(0, ref ramp, calibrate: false);
    }

    // This function unloads and terminates
    public static void Terminate()
    {
        // Unload stateblocks
        try { sb_nalpha.Dispose(); } catch(Exception) {} sb_nalpha = null;
        try { sb_nadditivealpha.Dispose(); } catch(Exception) {} sb_nadditivealpha = null;
        try { sb_tlmodalpha.Dispose(); } catch(Exception) {} sb_tlmodalpha = null;
        try { sb_nlightmap.Dispose(); } catch(Exception) {} sb_nlightmap = null;
        try { sb_nlightmapalpha.Dispose(); } catch(Exception) {} sb_nlightmapalpha = null;
        try { sb_tllightdraw.Dispose(); } catch(Exception) {} sb_tllightdraw = null;
        try { sb_tllightblend.Dispose(); } catch(Exception) {} sb_tllightblend = null;
        try { sb_nlines.Dispose(); } catch(Exception) {} sb_nlines = null;
        try { sb_pnormal.Dispose(); } catch(Exception) {} sb_pnormal = null;
        try { sb_padditive.Dispose(); } catch(Exception) {} sb_padditive = null;
        try { sb_nlightblend.Dispose(); } catch(Exception) {} sb_nlightblend = null;

        // Unload all Direct3D resources
        try { UnloadAllResources(); } catch(Exception) {}

        // Clean up
        try { backbuffer.Dispose(); } catch(Exception) {}
        try { depthbuffer.Dispose(); } catch(Exception) {}
        backbuffer = null;
        depthbuffer = null;
        rendertarget = null;
        resources = null;
        GC.Collect();
        try { d3dd.Dispose(); } catch(Exception) {}
        d3dd = null;
        GC.Collect();
    }

    // This will initialize the Direct3D device
    public static bool Initialize(Form target)
    {
        DeviceType devtype;

        // Indicate that we will manage objects ourself
        //Device.IsUsingEventHandlers = false;

        // Make dictionaries for resources
        resources = new Dictionary<string, Resource>();
        textures = new Dictionary<string, TextureResource>();

        // Set the render target
        rendertarget = target;

        // Keep screen clipping rectangle
        screencliprect = Cursor.Clip;

        // Find the exact or closest matching display mode.
        // This also sets the format to the current
        // display format for windowed mode.
        if(FindDisplayMode(displaymode, displaywindowed, displayfsaa))
        {
            // Choose most appropriate lightmap format
            ChooseLightmapFormat();

            // Create presentation parameters
            displaypp = CreatePresentParameters(displaymode, displaywindowed, displaysyncrefresh, displayfsaa);

            // Adjust rendertarget to display mode
            // This also sets the rendering options depending on the videocard
            AdjustRenderTarget(adapter, displaymode, displaywindowed);

            // Determine device type for compatability with NVPerfHUD
            if(string.Compare(adapter.Details.Description, NVPERFHUD_ADAPTER, true) == 0)
                devtype = DeviceType.Reference;
            else
                devtype = DeviceType.Hardware;

            // Display the window
            rendertarget.Show();
            rendertarget.Activate();

            try
            {
                // Check if this adapter supports TnL
                var d3dcaps = _direct3D.GetDeviceCaps(adapter.Adapter, devtype);
                if(d3dcaps.DeviceCaps.HasFlag(DeviceCaps.HWTransformAndLight))
                {
                    // Initialize with hardware TnL
                    d3dd = new Device(_direct3D, adapter.Adapter, devtype, rendertarget.Handle,
                        CreateFlags.HardwareVertexProcessing, displaypp);
                }
                else
                {
                    // Initialize with software TnL
                    d3dd = new Device(_direct3D, adapter.Adapter, devtype, rendertarget.Handle,
                        CreateFlags.SoftwareVertexProcessing, displaypp);
                }
            }
            catch(Exception)
            {
                // Failed
                MessageBox.Show("Unable to initialize the Direct3D Device. Another application may have taken exclusive mode on this videocard.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Get the backbuffer
            backbuffer = d3dd.GetBackBuffer(0, 0);
            depthbuffer = d3dd.DepthStencilSurface;

            // Keep viewport
            displayviewport = d3dd.Viewport;

            // Apply gamma correction
            SetColorCorrection(1f + (float)displaygamma / 20f);

            // Setup renderstates
            SetupRenderstates();

            // Clear the screen
            ClearScreen();

            // Success
            return true;
        }
        else
        {
            // Unable to find a valid display mode
            MessageBox.Show("Your video card or video driver does not support the required features for this game, or no valid display mode could be found.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
    }

    // This creates presentation parameters for the requested mode
    private static PresentParameters CreatePresentParameters(DisplayModeEx mode, bool windowed, bool syncrefresh, int fsaa)
    {
        // Create the presentation parameters
        PresentParameters d3dpp = new PresentParameters();

        // Backbuffer swap method
        d3dpp.SwapEffect = SwapEffect.Discard;

        // Windowed mode
        d3dpp.Windowed = windowed;

        // Backbuffer and display format
        d3dpp.BackBufferCount = 1;
        d3dpp.BackBufferFormat = mode.Format;
        d3dpp.BackBufferWidth = mode.Width;
        d3dpp.BackBufferHeight = mode.Height;
        d3dpp.EnableAutoDepthStencil = true;
        d3dpp.AutoDepthStencilFormat = Format.D16;
        d3dpp.FullScreenRefreshRateInHz = windowed ? 0 : mode.RefreshRate;

        // Check if using fullscreen antialiasing
        if(fsaa > -1)
        {
            d3dpp.MultiSampleType = MultisampleType.NonMaskable;
            d3dpp.MultiSampleQuality = fsaa;
        }
        else
        {
            d3dpp.MultiSampleType = MultisampleType.None;
        }

        // Check if synchronizing with refreshrate
        if(syncrefresh)
        {
            // Force synchronization with refresh rate
            d3dpp.PresentationInterval = PresentInterval.One;
        }
        else
        {
            // Force immediate frame presentation
            d3dpp.PresentationInterval = PresentInterval.Immediate;
        }

        // Return parameters
        return d3dpp;
    }

    // Adjust the rendertarget to match with the display mode
    private static void AdjustRenderTarget(AdapterInformation ad, DisplayModeEx mode, bool windowed)
    {
        // Get device caps
        var dc = _direct3D.GetDeviceCaps(ad.Adapter, DeviceType.Hardware);

        // Check if displaying in windowed mode
        if(windowed)
        {
            // Resize the rendertarget to match the display mode
            rendertarget.ClientSize = new Size(mode.Width, mode.Height);
            rendertarget.Refresh();
        }
        else
        {
            // Maximize the window
            //rendertarget.WindowState = FormWindowState.Maximized;
            //rendertarget.Refresh();
        }
    }

    // This is to disable the automatic resize reset
    private static void CancelResize(object sender, CancelEventArgs e)
    {
        // Cancel resize event
        e.Cancel = true;
    }

    // This clears both the primary buffer and back buffer black
    public static void ClearScreen()
    {
        // Clear backbuffer black
        d3dd.Clear(ClearFlags.Target | ClearFlags.ZBuffer, new(), 1f, 0);

        // Flip backbuffer with primary buffer
        d3dd.Present();

        // Clear the new backbuffer also
        d3dd.Clear(ClearFlags.Target | ClearFlags.ZBuffer, new(), 1f, 0);
    }

    #endregion

    #region ================== Rendering Loop

    // This sets a specific render mode
    public static void SetDrawMode(DRAWMODE drawmode) { SetDrawMode(drawmode, false); }
    public static void SetDrawMode(DRAWMODE drawmode, bool forceapply)
    {
        // Check if not the same as last mode
        if((drawmode != lastdrawmode) || forceapply)
        {
            // Select the mode
            switch(drawmode)
            {
                // Normal alpha blending
                case DRAWMODE.NALPHA: sb_nalpha.Apply(); break;

                // Additive alpha blending
                case DRAWMODE.NADDITIVEALPHA: sb_nadditivealpha.Apply(); break;

                // Normal alpha blending with modulated alpha argument
                case DRAWMODE.TLMODALPHA: sb_tlmodalpha.Apply(); break;

                // Normal lightmap rendering
                case DRAWMODE.NLIGHTMAP: sb_nlightmap.Apply(); break;

                // Lightmap drawing
                case DRAWMODE.TLLIGHTDRAW: sb_tllightdraw.Apply(); break;

                // Lightmap blending
                case DRAWMODE.TLLIGHTBLEND: sb_tllightblend.Apply(); break;

                // Normal lightmap rendering with alpha blending
                case DRAWMODE.NLIGHTMAPALPHA: sb_nlightmapalpha.Apply(); break;

                // Lines
                case DRAWMODE.NLINES: sb_nlines.Apply(); break;

                // Normal pointsprites
                case DRAWMODE.PNORMAL: sb_pnormal.Apply(); break;

                // Additive pointsprites
                case DRAWMODE.PADDITIVE: sb_padditive.Apply(); break;

                // Dynamic lightmap blending
                case DRAWMODE.NLIGHTBLEND: sb_nlightblend.Apply(); break;
            }

            // This mode is last set
            lastdrawmode = drawmode;
        }
    }

    // This test if rendering is possible and reloads resources when reset
    // Returns false when rendering is not possible, true when everything is fine or reloaded
    public static bool StartRendering()
    {
        // Always apply a new draw mode
        lastdrawmode = DRAWMODE.UNDEFINED;

        // When minimized, do not render anything
        if(rendertarget.WindowState != FormWindowState.Minimized)
        {
            // Test the cooperative level
            var coopresult = d3dd.TestCooperativeLevel();

            // Check if device must be reset
            if(coopresult == (int)ResultCode.DeviceLost)
            {
                // Device is lost and cannot be reset now
                return false;
            }

            // Clear the screen
            d3dd.Clear(ClearFlags.Target | ClearFlags.ZBuffer, new(), 1f, 0);

            // Ready to render
            return true;
        }
        else
        {
            // Minimized, you cannot see anything
            return false;
        }
    }

    // This finishes and displays the rendered scene
    public static void FinishRendering()
    {
        try
        {
            // Display the scene
            d3dd.Present();
        }

        // Errors are not a problem here
        catch(Exception) { }
    }

    // This writes a screenshot
    public static void SaveScreenshot(string filepathname)
    {
        // Save screenshot
        Surface.ToFile(backbuffer, filepathname, ImageFileFormat.Png);
    }

    #endregion

    #region ================== Resource Management

    // Create a surface resource without referencename
    public static SurfaceResource LoadSurfaceResource(string filename, Pool memorypool)
    {
        // Continue making reference names until an unused one is found
        while(resources.ContainsKey(resourceid.ToString())) resourceid = (resourceid + 1) % (int.MaxValue - 1);

        // Load the resource with this as reference name
        return LoadSurfaceResource(filename, resourceid.ToString(), memorypool);
    }

    // Create a surface resource
    public static SurfaceResource LoadSurfaceResource(string filename, string referencename, Pool memorypool)
    {
        // Create the SurfaceResource
        SurfaceResource res = new SurfaceResource(filename, referencename, memorypool);

        // Add resource to collection
        resources.Add(referencename, res);

        // Return the resource
        return res;
    }

    // This creates a texture from file and sets the image information
    public static TextureResource LoadTexture(string filename, bool usecache)
    {
        return LoadTexture(filename, usecache, false, 0, 0);
    }

    // This creates a texture from file and sets the image information
    public static TextureResource LoadTexture(string filename, bool usecache, bool mipmap)
    {
        return LoadTexture(filename, usecache, mipmap, 0, 0);
    }

    // This creates a texture from file
    public static TextureResource LoadTexture(string filename, bool usecache, bool mipmap, int width, int height)
    {
        ImageInformation i = new ImageInformation();
        Texture t;
        int mipmaplevels = 1;
        if(mipmap) mipmaplevels = 2;

        // Check if the file exists
        if(File.Exists(Path.Combine(Paths.BundledResourceDir, filename)))
        {
            // Check if already loaded
            if((textures.TryGetValue(filename, out TextureResource textureResource)) && (usecache == true))
            {
                // Return resource
                return textureResource;
            }
            else
            {
                // Load texture file
                t =  Texture.FromFile(d3dd, Path.Combine(Paths.BundledResourceDir, filename), width, height, mipmaplevels, Usage.None, Format.Unknown,
                    Pool.Default, Filter.Linear | Filter.MirrorU | Filter.MirrorV | Filter.Dither,
                    Filter.Triangle, 0, out i);

                // Make resource
                TextureResource r = new TextureResource(filename, t, i);

                // Add to textures
                if(usecache == true) textures.Add(filename, r);

                // Return resource
                return r;
            }
        }
        else
        {
            // File not found
            throw(new FileNotFoundException("Cannot find the texture file \"" + filename + "\".", filename));
        }
    }

    // This creates a new texture
    public static TextureResource CreateTexture(bool mipmap, int width, int height, Format format)
    {
        ImageInformation i = new ImageInformation();
        Texture t;
        int mipmaplevels = 1;
        if(mipmap) mipmaplevels = 2;

        // Create texture
        t = new Texture(d3dd, width, height, mipmaplevels, Usage.None, format, Pool.Default);

        // Create texture information
        i.Format = format;
        i.Depth = GetBitDepth(format);
        i.Height = height;
        i.Width = width;
        i.MipLevels = mipmaplevels;
        i.ResourceType = ResourceType.Texture;

        // Make resource
        TextureResource r = new TextureResource("__new__", t, i);

        // Return resource
        return r;
    }

    // This removes a specific texture resource from cache
    public static void RemoveTextureCache(string filename)
    {
        // Remove from cache
        textures.Remove(filename);
    }

    // This removes all textures from cache
    public static void FlushTextures()
    {
        // Clear all textures
        textures.Clear();
    }

    // Create a text resource without referencename
    public static TextResource CreateTextResource(CharSet charset)
    {
        // Continue making reference names until an unused one is found
        while(resources.ContainsKey(resourceid.ToString())) resourceid = (resourceid + 1) % (int.MaxValue - 1);

        // Load the resource with this as reference name
        return CreateTextResource(charset, resourceid.ToString());
    }

    // Create a text resource
    public static TextResource CreateTextResource(CharSet charset, string referencename)
    {
        // Create the TextResource
        TextResource res = new TextResource(charset, referencename);

        // Add resource to collection
        resources.Add(referencename, res);

        // Return the resource
        return res;
    }

    // Destroy a resource
    public static void DestroyResource(string referencename)
    {
        // Check if this resource exists
        if(resources.TryGetValue(referencename, out Resource res))
        {
            // Unload the resource
            res.Unload();

            // Remove resource from collection
            resources.Remove(referencename);
        }
    }

    // Find a resource
    public static Resource GetResource(string referencename)
    {
        // Check if this resource exists
        if(resources.TryGetValue(referencename, out Resource res))
        {
            // Return the resource object
            return res;
        }
        else
        {
            // Return nothing
            return null;
        }
    }

    // Unload all resources (but keep the objects)
    private static void UnloadAllResources()
    {
        // Let the arena unload its resources
        if(General.arena != null) General.arena.UnloadResources();
        if(General.console != null) General.console.UnloadResources();
        if(General.chatbox != null) General.chatbox.UnloadResources();
        if(General.scoreboard != null) General.scoreboard.UnloadResources();
        if(General.hud != null) General.hud.UnloadResources();
        if(General.gamemenu != null) General.gamemenu.UnloadResources();

        // Go for all resources
        foreach(Resource res in resources.Values)
        {
            // Unload this resource
            res.Unload();
        }

        // Clean up memory
        GC.Collect();
    }

    // Reload all resources
    private static void ReloadAllResources()
    {
        // Go for all resources
        foreach(Resource res in resources.Values)
        {
            // Reload this resource
            res.Reload();
        }

        // Let the arena rebuild its resources
        if(General.arena != null) General.arena.ReloadResources();
        if(General.console != null) General.console.ReloadResources();
        if(General.chatbox != null) General.chatbox.ReloadResources();
        if(General.scoreboard != null) General.scoreboard.ReloadResources();
        if(General.hud != null) General.hud.ReloadResources();
        if(General.gamemenu != null) General.gamemenu.ReloadResources();
    }

    #endregion

    #region ================== Tools

    private static List<DisplayModeEx> GetAdapterDisplayModes(AdapterInformation a)
    {
        var direct3d = _direct3D;
        var displayModes = new List<DisplayModeEx>();
        foreach (var format in Enum.GetValues<Format>())
        {
            var displayModeFilter = new DisplayModeFilter
            {
                Format = format,
                Size = Unsafe.SizeOf<DisplayModeFilter>(),
            };

            var count = direct3d.GetAdapterModeCountEx(a.Adapter, displayModeFilter);
            for (var i = 0; i < count; ++i)
            {
                var mode = direct3d.EnumerateAdapterModesEx(adapter.Adapter, displayModeFilter, i);
                displayModes.Add(mode);
            }
        }

        return displayModes;
    }

    // This creates a managed texture from a rendertarget
    public static Texture CreateManagedTexture(Texture rt)
    {
        // Get texture information
        SurfaceDescription info = rt.GetLevelDescription(0);

        // Make system memory texture
        Texture s = new Texture(Direct3D.d3dd, info.Width, info.Height, 1,
            Usage.None, info.Format, Pool.SystemMemory);

        // Get surfaces
        Surface rts = rt.GetSurfaceLevel(0);
        Surface ss = s.GetSurfaceLevel(0);

        // Copy data from RT to S
        Direct3D.d3dd.GetRenderTargetData(rts, ss);

        // Copy data from S to T
        var gs = Texture.ToStream(s, ImageFileFormat.Bmp);
        Texture t = Texture.FromStream(Direct3D.d3dd, gs, info.Width, info.Height,
            1, Usage.None, info.Format, Pool.Default,
            Filter.Linear, Filter.Linear, 0);

        // Clean up
        rts.Dispose();
        ss.Dispose();
        s.Dispose();
        gs.Close();
        gs.Dispose();

        // Return new texture
        return t;
    }

    // This creates a translation matrix for 2D texture coordinates
    public static Matrix MatrixTranslateTx(float x, float y)
    {
        var m = Matrix.Identity;
        m.M31 = x;
        m.M32 = y;
        return m;
    }

    // This makes a TL rectangle with texture coordinates and colors
    public static TLVertex[] TLRect(float v1x, float v1y, float v1u, float v1v, int v1c,
        float v2x, float v2y, float v2u, float v2v, int v2c,
        float v3x, float v3y, float v3u, float v3v, int v3c,
        float v4x, float v4y, float v4u, float v4v, int v4c)
    {
        TLVertex[] rect = new TLVertex[4];

        // Lefttop
        rect[0].x = v1x;
        rect[0].y = v1y;
        rect[0].tu = v1u;
        rect[0].tv = v1v;
        rect[0].color = v1c;
        rect[0].rhw = 1f;

        // Righttop
        rect[1].x = v2x;
        rect[1].y = v2y;
        rect[1].tu = v2u;
        rect[1].tv = v2v;
        rect[1].color = v2c;
        rect[1].rhw = 1f;

        // Leftbottom
        rect[2].x = v3x;
        rect[2].y = v3y;
        rect[2].tu = v3u;
        rect[2].tv = v3v;
        rect[2].color = v3c;
        rect[2].rhw = 1f;

        // Rightbottom
        rect[3].x = v4x;
        rect[3].y = v4y;
        rect[3].tu = v4u;
        rect[3].tv = v4v;
        rect[3].color = v4c;
        rect[3].rhw = 1f;

        return rect;
    }

    // This makes a TL rectangle with texture coordinates and a single color
    public static TLVertex[] TLRect(float v1x, float v1y, float v1u, float v1v,
        float v2x, float v2y, float v2u, float v2v,
        float v3x, float v3y, float v3u, float v3v,
        float v4x, float v4y, float v4u, float v4v, int c)
    {
        return TLRect(v1x, v1y, v1u, v1v, c,
            v2x, v2y, v2u, v2v, c,
            v3x, v3y, v3u, v3v, c,
            v4x, v4y, v4u, v4v, c);
    }

    // This makes a TL rectangle with texture coordinates
    public static TLVertex[] TLRect(float v1x, float v1y, float v1u, float v1v,
        float v2x, float v2y, float v2u, float v2v,
        float v3x, float v3y, float v3u, float v3v,
        float v4x, float v4y, float v4u, float v4v)
    {
        return TLRect(v1x, v1y, v1u, v1v, -1,
            v2x, v2y, v2u, v2v, -1,
            v3x, v3y, v3u, v3v, -1,
            v4x, v4y, v4u, v4v, -1);
    }

    // This makes a TL rectangle with texture coordinates
    public static TLVertex[] TLRect(float left, float top, float right, float bottom, float tl, float tt, float tr, float tb)
    {
        return TLRect(left, top, tl, tt, -1,
            right, top, tr, tt, -1,
            left, bottom, tl, tb, -1,
            right, bottom, tr, tb, -1);
    }

    // This makes a TL rectangle with texture coordinates
    public static TLVertex[] TLRect(float left, float top, float right, float bottom, float tw, float th)
    {
        float twu = 1f / tw;
        float thu = 1f / th;
        return TLRect(left, top, twu, thu, -1,
            right, top, 1f - twu, thu, -1,
            left, bottom, twu, 1f - thu, -1,
            right, bottom, 1f - twu, 1f - thu, -1);
    }

    // This makes a TL rectangle
    public static TLVertex[] TLRect(float left, float top, float right, float bottom)
    {
        return TLRect(left, top, 0f, 0f, -1,
            right, top, 1f, 0f, -1,
            left, bottom, 0f, 1f, -1,
            right, bottom, 1f, 1f, -1);
    }

    // This makes a TL rectangle
    public static TLVertex[] TLRect(float left, float top, float right, float bottom, int c)
    {
        return TLRect(left, top, 0f, 0f, c,
            right, top, 1f, 0f, c,
            left, bottom, 0f, 1f, c,
            right, bottom, 1f, 1f, c);
    }

    // This makes a TL rectangle with texture coordinates
    public static TLVertex[] TLRectL(float left, float top, float right, float bottom, float tl, float tt, float tr, float tb)
    {
        TLVertex[] rect = new TLVertex[6];

        // Lefttop
        rect[0].x = left;
        rect[0].y = top;
        rect[0].tu = tl;
        rect[0].tv = tt;
        rect[0].color = -1;
        rect[0].rhw = 1f;

        // Righttop
        rect[1].x = right;
        rect[1].y = top;
        rect[1].tu = tr;
        rect[1].tv = tt;
        rect[1].color = -1;
        rect[1].rhw = 1f;

        // Leftbottom
        rect[2].x = left;
        rect[2].y = bottom;
        rect[2].tu = tl;
        rect[2].tv = tb;
        rect[2].color = -1;
        rect[2].rhw = 1f;

        // Leftbottom
        rect[3].x = left;
        rect[3].y = bottom;
        rect[3].tu = tl;
        rect[3].tv = tb;
        rect[3].color = -1;
        rect[3].rhw = 1f;

        // Righttop
        rect[4].x = right;
        rect[4].y = top;
        rect[4].tu = tr;
        rect[4].tv = tt;
        rect[4].color = -1;
        rect[4].rhw = 1f;

        // Rightbottom
        rect[5].x = right;
        rect[5].y = bottom;
        rect[5].tu = tr;
        rect[5].tv = tb;
        rect[5].color = -1;
        rect[5].rhw = 1f;

        return rect;
    }

    // This makes a Quad
    public static MVertex[] MQuadList(Vector3D v1, float v1u, float v1v,
        Vector3D v2, float v2u, float v2v,
        Vector3D v3, float v3u, float v3v,
        Vector3D v4, float v4u, float v4v)
    {
        MVertex[] rect = new MVertex[6];

        // Lefttop
        rect[0].x = v1.x;
        rect[0].y = v1.y;
        rect[0].z = v1.z;
        rect[0].t1u = v1u;
        rect[0].t1v = v1v;

        // Righttop
        rect[1].x = v2.x;
        rect[1].y = v2.y;
        rect[1].z = v2.z;
        rect[1].t1u = v2u;
        rect[1].t1v = v2v;

        // Leftbottom
        rect[2].x = v3.x;
        rect[2].y = v3.y;
        rect[2].z = v3.z;
        rect[2].t1u = v3u;
        rect[2].t1v = v3v;

        // Leftbottom
        rect[3].x = v3.x;
        rect[3].y = v3.y;
        rect[3].z = v3.z;
        rect[3].t1u = v3u;
        rect[3].t1v = v3v;

        // Righttop
        rect[4].x = v2.x;
        rect[4].y = v2.y;
        rect[4].z = v2.z;
        rect[4].t1u = v2u;
        rect[4].t1v = v2v;

        // Rightbottom
        rect[5].x = v4.x;
        rect[5].y = v4.y;
        rect[5].z = v4.z;
        rect[5].t1u = v4u;
        rect[5].t1v = v4v;

        return rect;
    }

    #endregion
}

// MVertex
public struct MVertex
{
    // Vertex format
    public static readonly VertexFormat Format = VertexFormat.Position | VertexFormat.Texture3 | VertexFormat.Diffuse;
    public static readonly unsafe int Stride = sizeof(MVertex);

    // Members
    public float x;
    public float y;
    public float z;
    public int color;
    public float t1u;
    public float t1v;
    public float t2u;
    public float t2v;
    public float t3u;
    public float t3v;
}

// LVertex
public struct LVertex
{
    // Vertex format
    public static readonly VertexFormat Format = VertexFormat.Position | VertexFormat.Diffuse;
    public static readonly unsafe int Stride = sizeof(LVertex);

    // Members
    public float x;
    public float y;
    public float z;
    public int color;
}

// TLVertex
public struct TLVertex
{
    // Vertex format
    public static readonly VertexFormat Format = VertexFormat.PositionRhw | VertexFormat.Texture1 | VertexFormat.Diffuse;
    public static readonly unsafe int Stride = sizeof(TLVertex);

    // Members
    public float x;
    public float y;
    public float z;
    public float rhw;
    public int color;
    public float tu;
    public float tv;
}

// PVertex
public struct PVertex
{
    // Vertex format
    public static readonly VertexFormat Format = VertexFormat.Position | VertexFormat.PointSize | VertexFormat.Diffuse;
    public static readonly unsafe int Stride = sizeof(PVertex);

    // Members
    public float x;
    public float y;
    public float z;
    public float size;
    public int color;
}
