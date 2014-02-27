using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace apcurium.MK.Booking.Mobile.Client.Helpers
{
    public static class PlatformHelper
    {
        public static int APILevel
        {
            get
            {
                return (int)Android.OS.Build.VERSION.SdkInt;
            }
        }

        public static bool IsAndroid23
        {
            get
            {
                return APILevel <= 10;
            }
        }
    }
}

