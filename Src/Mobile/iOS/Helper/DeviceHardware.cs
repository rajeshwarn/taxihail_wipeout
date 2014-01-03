using System;
using System.Runtime.InteropServices;
using MonoTouch;
using MonoTouch.UIKit;

namespace apcurium.MK.Booking.Mobile.Client.Helper
{

// make sure to add a 'using System.Runtime.InteropServices;' line to your file  
	public class DeviceHardware
	{
		public const string HardwareProperty = "hw.machine";

		public enum HardwareVersion
		{
// ReSharper disable InconsistentNaming
			iPhone,
			iPhone3G,
			iPhone3GS,
			iPhone4,
			iPod1G,
			iPod2G,
			iPod3G,
			iPod4G,
			iPad,
			iPhoneSimulator,
			iPhone4Simulator,
			iPadSimulator,
			Unknown
		}
// ReSharper restore InconsistentNaming

		// Changing the constant to "/usr/bin/libSystem.dylib" allows this P/Invoke to work on Mac OS X  
		// Using "hw.model" as property gives Macintosh model, "hw.machine" kernel arch (ppc, ppc64, i386, x86_64)  
		[DllImport( Constants.SystemLibrary )]
		// name of the property  
		// output  
		// IntPtr.Zero  
		// IntPtr.Zero  
			// 0  
		static internal extern int sysctlbyname ( [MarshalAs( UnmanagedType.LPStr )] string property, IntPtr output, IntPtr oldLen, IntPtr newp, uint newlen );

		public static HardwareVersion Version
		{
			get
			{
				// get the length of the string that will be returned  
				var pLen = Marshal.AllocHGlobal ( sizeof( int ) );
				sysctlbyname ( HardwareProperty, IntPtr.Zero, pLen, IntPtr.Zero, 0 );
				
				var length = Marshal.ReadInt32 ( pLen );
				
				// check to see if we got a length  
				if ( length == 0 )
				{
					Marshal.FreeHGlobal ( pLen );
					return HardwareVersion.Unknown;
				}
				
				// get the hardware string  
				var pStr = Marshal.AllocHGlobal ( length );
				sysctlbyname ( HardwareProperty, pStr, pLen, IntPtr.Zero, 0 );
				
				// convert the native string into a C# string  
				var hardwareStr = Marshal.PtrToStringAnsi ( pStr );
				HardwareVersion ret;
				
				// determine which hardware we are running  
				if ( hardwareStr == "iPhone1,1" )
					ret = HardwareVersion.iPhone;
				else if ( hardwareStr == "iPhone1,2" )
					ret = HardwareVersion.iPhone3G;
				else if ( hardwareStr == "iPhone2,1" )
					ret = HardwareVersion.iPhone3GS;
				else if ( hardwareStr == "iPhone3,1" )
					ret = HardwareVersion.iPhone4;
				else if ( hardwareStr == "iPad1,1" )
					ret = HardwareVersion.iPad;
				else if ( hardwareStr == "iPod1,1" )
					ret = HardwareVersion.iPod1G;
				else if ( hardwareStr == "iPod2,1" )
					ret = HardwareVersion.iPod2G;
				else if ( hardwareStr == "iPod3,1" )
					ret = HardwareVersion.iPod3G;
				else if ( hardwareStr == "iPod4,1" )
					ret = HardwareVersion.iPod3G;
				else if ( hardwareStr == "i386" || hardwareStr == "x86_64" )
				{
					if ( UIDevice.CurrentDevice.Model.Contains ( "iPhone" ) )
// ReSharper disable CompareOfFloatsByEqualityOperator
						ret = UIScreen.MainScreen.Bounds.Height * UIScreen.MainScreen.Scale == 960 || UIScreen.MainScreen.Bounds.Width * UIScreen.MainScreen.Scale == 960 ? HardwareVersion.iPhone4Simulator : HardwareVersion.iPhoneSimulator;
// ReSharper restore CompareOfFloatsByEqualityOperator
					else
						ret = HardwareVersion.iPadSimulator;
				}
				else
					ret = HardwareVersion.Unknown;
				
				// cleanup  
				Marshal.FreeHGlobal ( pLen );
				Marshal.FreeHGlobal ( pStr );
				
				return ret;
			}
		}
	}
}
