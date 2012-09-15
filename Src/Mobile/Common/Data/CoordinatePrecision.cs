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
    public enum CoordinatePrecision
    {
        Fine = 100,
        Medium = 1000,
        Coarse = 400,
        BallPark = 10000,
    }
}