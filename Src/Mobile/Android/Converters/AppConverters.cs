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
using Cirrious.MvvmCross.Converters.Visibility;
using apcurium.MK.Booking.Mobile.BindingConverter;

namespace apcurium.MK.Booking.Mobile.Client.Converters
{


    public class AppConverters
    {
        public readonly CoordinateConverter CoordinateConverter = new CoordinateConverter();
        public readonly MvxVisibilityConverter Visibility = new MvxVisibilityConverter();
        public readonly MvxInvertedVisibilityConverter InvertedVisibility = new MvxInvertedVisibilityConverter();
        public readonly BoolInverter BoolInverter = new BoolInverter();

    }
}