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
        
        public CoordinateViewModel()
        {
           
        }

        public double Latitude{ get; set; }

        public double Longitude { get; set; }
        

    }
}