using System;
using Cirrious.MvvmCross.Converters;
using Cirrious.MvvmCross.ExtensionMethods;
using Cirrious.MvvmCross.Interfaces.Localization;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;

namespace apcurium.MK.Booking.Mobile.Client.Converters
{
    public class CoordinateConverter : MvxBaseValueConverter, IMvxServiceConsumer<IMvxTextProvider>
    {
        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
      