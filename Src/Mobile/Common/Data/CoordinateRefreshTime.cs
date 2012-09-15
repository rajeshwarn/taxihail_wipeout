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

namespace apcurium.MK.Booking.Mobile.Data
{
    public enum CoordinateRefreshTime
    {
        Recently = 2, //Less than 2 minutes
        NotRecently = 15, //Less than 10 minutes
        ALongTimeAgo = 1000, //More than 10 minutes
        
    }
}