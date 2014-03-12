using System;
using System.Runtime.InteropServices;

namespace apcurium.MK.Booking.Mobile.Client.Helper
{
    public static class HardwareInfo
    {
        private const string HardwareProperty = "hw.machine";

        [DllImport(MonoTouch.Constants.SystemLibrary)]
        static internal extern int sysctlbyname([MarshalAs(UnmanagedType.LPStr)] string property, IntPtr output, IntPtr oldLen, IntPtr newp, uint newlen);

        static HardwareInfo()
        {
            Version = GetVersion();
        }

        public static readonly string Version;

        private static string GetVersion()
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
                return "Simulator";
            }

            Marshal.FreeHGlobal(pLen);
            Marshal.FreeHGlobal(pStr);

            return hardwareStr;
        }
    }
}