using System;
using System.Runtime.InteropServices;
using MonoTouch;
using MonoTouch.UIKit;

namespace apcurium.MK.Booking.Mobile.Client.Helper
{
    public static class HardwareInfo
    {
        public const string HardwareProperty = "hw.machine";

        [DllImport(MonoTouch.Constants.SystemLibrary)]
        static internal extern int sysctlbyname([MarshalAs(UnmanagedType.LPStr)] string property, IntPtr output, IntPtr oldLen, IntPtr newp, uint newlen);

        public static string Version
        {
            get
            {
                var pLen = Marshal.AllocHGlobal(sizeof(int));
                sysctlbyname(HardwareProperty, IntPtr.Zero, pLen, IntPtr.Zero, 0);

                var length = Marshal.ReadInt32(pLen);

                if (length == 0)
                {
                    Marshal.FreeHGlobal(pLen);

                    return "Unknown";
                }

                var pStr = Marshal.AllocHGlobal(length);
                sysctlbyname(HardwareProperty, pStr, pLen, IntPtr.Zero, 0);

                var hardwareStr = Marshal.PtrToStringAnsi(pStr);

                if (hardwareStr == "i386" || hardwareStr == "x86_64")
                {
                    if (UIDevice.CurrentDevice.Model.Contains("iPhone"))
                        return UIScreen.MainScreen.Bounds.Height * UIScreen.MainScreen.Scale == 960 
                            || UIScreen.MainScreen.Bounds.Width * UIScreen.MainScreen.Scale == 960 
                            ? "iPhone4Simulator"
                            : "iPhoneSimulator";
                    else
                        return "iPadSimulator";
                }

                Marshal.FreeHGlobal(pLen);
                Marshal.FreeHGlobal(pStr);

                return hardwareStr;
            }
        }
    }
}