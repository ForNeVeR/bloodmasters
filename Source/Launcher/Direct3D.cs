/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Bloodmasters.Launcher.Interface;
using SharpDX.Direct3D9;


namespace Bloodmasters.Launcher;

internal sealed class Direct3D
{
    #region ================== Constants

    // Validation?
    private const bool HARDWARE_VALIDATION = true;
    private const string NVPERFHUD_ADAPTER = "NVIDIA NVPerfHUD";

    #endregion

    #region ================== Variables

    private static Direct3DEx _direct3D9Ex;

    // Devices
    private static int adapterIndex;

    // Current settings
    private static DisplayModeEx displaymode;
    private static bool displaywindowed;
    private static bool displaysyncrefresh;
    private static int displayfsaa;
    private static int displaygamma;

    #endregion

    #region ================== Properties

    // Display mode settings
    public static int DisplayWidth { get { return displaymode.Width; } set { displaymode.Width = value; } }
    public static int DisplayHeight { get { return displaymode.Height; } set { displaymode.Height = value; } }
    public static int DisplayFormat { get { return (int)displaymode.Format; } set { displaymode.Format = (Format)value; } }
    public static int DisplayRefreshRate { get { return displaymode.RefreshRate; } set { displaymode.RefreshRate = value; } }
    public static bool DisplayWindowed { get { return displaywindowed; } set { displaywindowed = value; } }
    public static bool DisplaySyncRefresh { get { return displaysyncrefresh; } set { displaysyncrefresh = value; } }
    public static int DisplayFSAA { get { return displayfsaa; } set { displayfsaa = value; } }
    public static int DisplayGamma { get { return displaygamma; } set { displaygamma = value; } }
    public static DisplayModeEx DisplayMode { get { return displaymode; } set { displaymode = value; } }

    #endregion

    #region ================== Enumerating

    // This finds and selects a valid adapter if the
    // current adapter is not valid
    // Returns null when a valid adapter was found.
    public static string SelectValidAdapter()
    {
        string error;
        string result = null;

        // Check if the NVPerfHud adapter exists
        for (var i = 0; i < _direct3D9Ex.AdapterCount; ++i)
        {
            var a = _direct3D9Ex.GetAdapterIdentifier(i);

            // Is this the NVPerfHud adapter?
            if(string.Compare(a.Description, NVPERFHUD_ADAPTER, StringComparison.OrdinalIgnoreCase) == 0)
            {
                // Select this adapter
                adapterIndex = i;
                return null;
            }
        }

        // Check if the current adapter is invalid
        if(ValidateDevice(adapterIndex) != null)
        {
            // Go for all adapters to find a valid adapter
            for (var a = 0; a < _direct3D9Ex.AdapterCount; ++a)
            {
                // Select adapter if valid
                error = ValidateDevice(a);
                if(error == null)
                {
                    // Valid one found
                    adapterIndex = a;
                    return null;
                }
                else
                {
                    // If this is the default adapter
                    if(a == 0)
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

    // This fills a list with all available adapters
    public static void FillAdaptersList(ComboBox list)
    {
        // Clear the list
        list.Items.Clear();

        // Enumerate all display adapters
        for (var a = 0; a < _direct3D9Ex.AdapterCount; ++a)
        {
            // Validate adapter
            if(ValidateDevice(a) == null)
            {
                // Add to the list
                list.Items.Add(new DisplayAdapterItem(a, _direct3D9Ex.GetAdapterIdentifier(a)));
                if(adapterIndex == a) list.SelectedIndex = list.Items.Count - 1;
            }
        }
    }

    // This fills a list with all display modes for a specific adapter
    // and selectes the given resolution
    public static void FillResolutionsList(ComboBox list, int ad, int w, int h, int depth, int rate)
    {
        var displayModes = GetAdapterDisplayModes(ad);

        var newitems = new List<DisplayModeItem>(displayModes.Count);

        // Clear the list
        list.Items.Clear();

        // Enumerate all display modes
        foreach(var d in displayModes)
        {
            // Validate resolution
            if(ValidateDisplayMode(d, true) || ValidateDisplayMode(d, false))
            {
                // Add to the list
                newitems.Add(new DisplayModeItem(d));
            }
        }

        // Sort the list
        newitems.Sort();

        // Go for all items to fill the list
        foreach(DisplayModeItem d in newitems)
        {
            // Add to the list
            list.Items.Add(d);
            if((d.mode.Width == w) && (d.mode.Height == h) &&
               ((int)d.mode.Format == depth) && (d.mode.RefreshRate == rate))
                list.SelectedIndex = list.Items.Count - 1;
        }

        //If a suitable resolution was not found
        if (list.SelectedItem is null)
            list.SelectedIndex = list.Items.Count - 1;
    }

    // This fills a list with all supported antialiasing modes
    public static void FillAntialiasingList(ComboBox list, int ad, Format f, bool windowed, int curlevel)
    {
        int levels = 0;

        // Clear the list
        list.Items.Clear();

        // Add "none" to list
        list.Items.Add("Off");

        // Check if supported
        if(_direct3D9Ex.CheckDeviceMultisampleType(ad, DeviceType.Hardware, f,
               windowed, MultisampleType.NonMaskable, out levels))
        {
            // Check if supported on depth stencil
            if(_direct3D9Ex.CheckDeviceMultisampleType(ad, DeviceType.Hardware, Format.D16,
                   windowed, MultisampleType.NonMaskable, out levels))
            {
                // Add levels to the list
                for(int i = 1; i <= levels; i++)
                {
                    // Add to list
                    list.Items.Add("Level " + i);
                    if(i - 1 == curlevel) list.SelectedIndex = list.Items.Count - 1;
                }
            }
        }

        // Nothing selected?
        if(list.SelectedIndex == -1) list.SelectedIndex = 0;
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
        if(index < _direct3D9Ex.AdapterCount)
        {
            // Select this adapter
            adapterIndex = index;
        }
        else
        {
            // Select the default adapter
            adapterIndex = 0;
        }

        // Write setting to configuration
        Program.config.WriteSetting("displaydriver", adapterIndex);
    }

    #endregion

    #region ================== Capabilities Validation

    // This tests if the given display mode is supported and
    // if it supports everything this engine needs
    public static bool ValidateDisplayMode(DisplayModeEx mode, bool windowed)
    {
        // The resolution must be at least 640x480
        if((mode.Width < 640) || (mode.Height < 480)) return false;

        // Test if the display format is supported by the device
        var result = _direct3D9Ex.CheckDeviceType(adapterIndex, DeviceType.Hardware,
            mode.Format, mode.Format, windowed);
        if (!result) return false;

        // Test if we can create surfaces of display format
        result = _direct3D9Ex.CheckDeviceFormat(adapterIndex, DeviceType.Hardware, mode.Format,
            0, ResourceType.Surface, mode.Format);
        if (!result) return false;

        // Test if we can create rendertarget textures of display format
        result = _direct3D9Ex.CheckDeviceFormat(adapterIndex, DeviceType.Hardware, mode.Format,
            Usage.RenderTarget, ResourceType.Texture, mode.Format);
        if (!result) return false;

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
                var dc = _direct3D9Ex.GetDeviceCaps(ad, DeviceType.Hardware);

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

    #endregion

    #region ================== Initialization

    // This does very early DirectX initialization
    // Calling this function will throw an Exception when DirectX is not installed
    public static void InitDX()
    {
        // Initialize variables
        _direct3D9Ex = new Direct3DEx();
        adapterIndex = 0;
        displaymode = new DisplayModeEx();
    }

    public static void DeinitDirectX()
    {
        _direct3D9Ex.Dispose();
        _direct3D9Ex = null;
    }

    #endregion

    private static List<DisplayModeEx> GetAdapterDisplayModes(int adapter)
    {
        var displayModes = new List<DisplayModeEx>();
        foreach (var format in Enum.GetValues<Format>())
        {
            var displayModeFilter = new DisplayModeFilter
            {
                Format = format,
                Size = Unsafe.SizeOf<DisplayModeFilter>(),
            };

            var count = _direct3D9Ex.GetAdapterModeCountEx(adapter, displayModeFilter);
            for (var i = 0; i < count; ++i)
            {
                var mode = _direct3D9Ex.EnumerateAdapterModesEx(adapter, displayModeFilter, i);
                displayModes.Add(mode);
            }
        }

        return displayModes;
    }
}
