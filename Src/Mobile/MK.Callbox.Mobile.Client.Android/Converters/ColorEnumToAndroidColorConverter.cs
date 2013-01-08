using System;
using System.Globalization;
using Cirrious.MvvmCross.Converters;
using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Callbox.Mobile.Client.Converters
{
    public class ColorEnumToAndroidColorConverter : MvxBaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((ColorEnum)value)
            {
                case ColorEnum.Black:
                    return Android.Graphics.Color.Red;
                    break;
                case ColorEnum.Green:
                    return Android.Graphics.Color.Green;
                    break;
                    case ColorEnum.Red:
                    return Android.Graphics.Color.Red;
                    break;
                    case ColorEnum.LightGray:
                    return Android.Graphics.Color.Gray;
            }
            return Android.Graphics.Color.Black;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}