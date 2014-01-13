using apcurium.MK.Booking.Mobile.BindingConverter;
using Cirrious.MvvmCross.Plugins.Visibility;

namespace apcurium.MK.Booking.Mobile.Client.Converters
{
    public class AppConverters
    {
        public readonly BoolInverter BoolInverter = new BoolInverter();
        public readonly EmptyToResourceConverter EmptyToResource = new EmptyToResourceConverter();
        public readonly EnumToBoolConverter EnumToBool = new EnumToBoolConverter();

        public readonly HasValueToVisibilityConverter HasValueToVisibilityConverter =
            new HasValueToVisibilityConverter();

		public readonly MvxInvertedVisibilityValueConverter InvertedVisibility = new MvxInvertedVisibilityValueConverter();

        public readonly OrderStatusToTextColorConverter OrderStatusToTextColorConverter =
            new OrderStatusToTextColorConverter();

		public readonly MvxVisibilityValueConverter Visibility = new MvxVisibilityValueConverter();
    }
}