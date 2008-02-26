/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using System.IO;
using System.Threading;
using System.Collections;
using System.Drawing;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.ComponentModel;
using System.Windows.Forms;
using CodeImp.Bloodmasters;
using CodeImp;

namespace CodeImp.Bloodmasters.Launcher
{
	internal sealed class Direct3D
	{
		#region ================== Constants
		
		// Validation?
		private const bool HARDWARE_VALIDATION = true;
		private const string NVPERFHUD_ADAPTER = "NVIDIA NVPerfHUD";
		
		#endregion
		
		#region ================== Variables
		
		// Devices
		private static AdapterInformation adapter;
		
		// Current settings
		private static DisplayMode displaymode;
		private static bool displaywindowed;
		private static bool displaysyncrefresh;
		private static int displayfsaa;
		private static int displaygamma;
		
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
		public static DisplayMode DisplayMode { get { return displaymode; } set { displaymode = value; } }
		
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
			foreach(AdapterInformation a in Manager.Adapters)
			{
				// Is this the NVPerfHud adapter?
				if(string.Compare(a.Information.Description, NVPERFHUD_ADAPTER, true) == 0)
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
				foreach(AdapterInformation a in Manager.Adapters)
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
						if(a.Adapter == Manager.Adapters.Default.Adapter)
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
			foreach(AdapterInformation a in Manager.Adapters)
			{
				// Validate adapter
				if(ValidateDevice(a.Adapter) == null)
				{
					// Add to the list
					list.Items.Add(new DisplayAdapterItem(a));
					if(adapter.Adapter == a.Adapter) list.SelectedIndex = list.Items.Count - 1;
				}
			}
		}
		
		// This fills a list with all display modes for a specific adapter
		// and selectes the given resolution
		public static void FillResolutionsList(ComboBox list, int ad, int w, int h, int depth, int rate)
		{
			ArrayList newitems = new ArrayList(Manager.Adapters[ad].SupportedDisplayModes.Count);
			
			// Clear the list
			list.Items.Clear();
			
			// Enumerate all display modes
			foreach(DisplayMode d in Manager.Adapters[ad].SupportedDisplayModes)
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
		}
		
		// This fills a list with all supported antialiasing modes
		public static void FillAntialiasingList(ComboBox list, int ad, Format f, bool windowed, int curlevel)
		{
			int result = 0, levels = 0;
			
			// Clear the list
			list.Items.Clear();
			
			// Add "none" to list
			list.Items.Add("Off");
			
			// Check if supported
			if(Manager.CheckDeviceMultiSampleType(ad, DeviceType.Hardware, f,
				windowed, MultiSampleType.NonMaskable, out result, out levels))
			{
				// Check if supported on depth stencil
				if(Manager.CheckDeviceMultiSampleType(ad, DeviceType.Hardware, Format.D16,
							windowed, MultiSampleType.NonMaskable, out result, out levels))
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
			if(index < Manager.Adapters.Count)
			{
				// Select this adapter
				adapter = Manager.Adapters[index];
			}
			else
			{
				// Select the default adapter
				adapter = Manager.Adapters.Default;
			}
			
			// Write setting to configuration
			General.config.WriteSetting("displaydriver", adapter.Adapter);
		}
		
		// This returns a specific display mode
		public static DisplayMode GetDisplayMode(int index)
		{
			int counter = 0;
			
			// Enumerate all display modes
			foreach(DisplayMode d in adapter.SupportedDisplayModes)
			{
				// Return settings if this the mode to use
				if(index == counter) return d;
				
				// Next mode
				counter++;
			}
			
			// Nothing
			return new DisplayMode();
		}
		
		#endregion
		
		#region ================== Capabilities Validation
		
		// This finds the closest matching display mode
		private static bool FindDisplayMode(ref DisplayMode mode, bool windowed, int fsaa)
		{
			DisplayModeCollection allmodes;
			
			// In case windowed is true, the display format
			// must be set to the current format
			if(windowed) mode.Format = adapter.CurrentDisplayMode.Format;
			
			// Go for all display modes to find the one specified
			allmodes = adapter.SupportedDisplayModes;
			foreach(DisplayMode dm in allmodes)
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
			allmodes = adapter.SupportedDisplayModes;
			foreach(DisplayMode dm in allmodes)
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
			allmodes = adapter.SupportedDisplayModes;
			foreach(DisplayMode dm in allmodes)
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
			allmodes = adapter.SupportedDisplayModes;
			foreach(DisplayMode dm in allmodes)
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
		public static bool ValidateDisplayMode(DisplayMode mode, bool windowed)
		{
			int result;
			
			// The resolution must be at least 640x480
			if((mode.Width < 640) || (mode.Height < 480)) return false;
			
			// Test if the display format is supported by the device
			Manager.CheckDeviceType(adapter.Adapter, DeviceType.Hardware,
							mode.Format, mode.Format, windowed, out result);
			if(result != 0) return false;
			
			// Test if we can create surfaces of display format
			Manager.CheckDeviceFormat(adapter.Adapter, DeviceType.Hardware, mode.Format,
									 0, ResourceType.Surface, mode.Format, out result);
			if(result != 0) return false;
			
			// Test if we can create rendertarget textures of display format
			Manager.CheckDeviceFormat(adapter.Adapter, DeviceType.Hardware, mode.Format,
					Usage.RenderTarget, ResourceType.Textures, mode.Format, out result);
			if(result != 0) return false;
			
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
					Caps dc = Manager.GetDeviceCaps(ad, DeviceType.Hardware);
					
					// Here we go, the whole list of device requirements
					if(!dc.DestinationBlendCaps.SupportsInverseSourceAlpha) result = prefix + "Desination InverseSourceAlpha blending.";
					if(!dc.DestinationBlendCaps.SupportsOne) result = prefix + "Desination One blending.";
					//if(!dc.DeviceCaps.SupportsTransformedVertexVideoMemory) result = prefix + "TransformedVertex in video memory.";
					if(!dc.DeviceCaps.SupportsTransformedVertexSystemMemory) result = prefix + "TransformedVertex in system memory.";
					if(!dc.DeviceCaps.SupportsStreamOffset) result = prefix + "a stream offset.";
					if(!(dc.MaxSimultaneousTextures >= 2)) result = prefix + "multitexture rendering.";
					if(!(dc.MaxStreams >= 1)) result = prefix + "streams.";
					if(!(dc.MaxStreamStride >= 32)) result = prefix + "32 byte streams.";
					if(!(dc.MaxTextureBlendStages >= 2)) result = prefix + "multiple texture blending stages.";
					if(!(dc.MaxTextureHeight >= 1024)) result = prefix + "1024 texture height.";
					if(!(dc.MaxTextureWidth >= 1024)) result = prefix + "1024 texture width.";
					if(!dc.PrimitiveMiscCaps.SupportsCullClockwise) result = prefix + "clockwise culling.";
					if(!dc.PrimitiveMiscCaps.SupportsCullCounterClockwise) result = prefix + "counterclockwise culling.";
					if(!dc.PrimitiveMiscCaps.SupportsCullNone) result = prefix + "rendering without culling.";
					if(!dc.ShadeCaps.SupportsColorGouraudRgb) result = prefix + "gouraud shading.";
					if(!dc.ShadeCaps.SupportsAlphaGouraudBlend) result = prefix + "alpha gouraud shading.";
					if(!dc.SourceBlendCaps.SupportsSourceAlpha) result = prefix + "Source SourceAlpha blending.";
					if(!dc.SourceBlendCaps.SupportsOne) result = prefix + "Source One blending.";
					//if(!dc.TextureAddressCaps.SupportsClamp) result = prefix + "clamped texture addressing.";
					//if(!dc.TextureAddressCaps.SupportsMirror) result = prefix + "mirrored texture addressing.";
					if(!dc.TextureAddressCaps.SupportsWrap) result = prefix + "wrapped texture addressing.";
					if(!dc.TextureCaps.SupportsAlpha) result = prefix + "texture alpha channels.";
					if(!dc.TextureOperationCaps.SupportsSelectArgument1) result = prefix + "SelectArg1 texture operation.";
					if(!dc.TextureOperationCaps.SupportsModulate) result = prefix + "Modulate texture operation.";
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
			adapter = Manager.Adapters.Default;
			displaymode = new DisplayMode();
		}
		
		#endregion
	}
}
