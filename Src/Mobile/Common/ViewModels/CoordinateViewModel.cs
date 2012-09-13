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

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class CoordinateViewModel
    {
        private double _latitude;
        private double _longitude;

        public CoordinateViewModel(double latitude, double longitude, string title)
        {
            _latitude = latitude;
            _longitude = longitude;
        }


        

    }
}