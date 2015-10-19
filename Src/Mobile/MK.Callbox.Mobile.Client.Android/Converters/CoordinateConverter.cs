using System;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.CrossCore.Converters;

namespace apcurium.MK.Callbox.Mobile.Client.Converters
{
    public class CoordinateConverter : MvxValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return new CoordinateViewModel();
        }

        public override object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
      