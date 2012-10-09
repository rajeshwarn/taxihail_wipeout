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
using Android.Util;

namespace apcurium.MK.Booking.Mobile.Client.Helpers
{
    public static class DrawHelper
    {
        public static  int GetPixels(float dipValue)
        {
            int px = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, dipValue , Application.Context.Resources.DisplayMetrics); // getDisplayMetrics());
            return px;
        }
    }
}