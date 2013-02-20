using System;
using Cirrious.MvvmCross.Converters;
using Cirrious.MvvmCross.Interfaces.Localization;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Callbox.Mobile.Client.Converters
{
    public class CoordinateConverter : MvxBaseValueConverter, IMvxServiceConsumer<IMvxTextProvider>
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
      