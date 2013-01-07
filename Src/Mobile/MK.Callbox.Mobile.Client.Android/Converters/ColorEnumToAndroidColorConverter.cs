using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.Converters;
using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Booking.Mobile.BindingConverter
{
    public class ColorEnumToAndroidColorConverter : MvxBaseValueConverter
    {
        public  object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((ColorEnum)value)
            {
                case ColorEnum.Black:
                    return "@color/black";
                    break;
                case ColorEnum.Green:
                    return "@color/green";
                    break;
                    case ColorEnum.Red:
                    return "@color/red";
                    break;
                    case ColorEnum.LightGray:
                    return "@color/lightgray";
            }
            return "@color/black";
        }

        public  object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}