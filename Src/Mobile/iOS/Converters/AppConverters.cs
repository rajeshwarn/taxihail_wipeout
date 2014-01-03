using apcurium.MK.Booking.Mobile.BindingConverter;

namespace apcurium.MK.Booking.Mobile.Client.Converters
{

	public class AppConverters
	{
        public readonly BoolInverter BoolInverter = new BoolInverter();
		public readonly OrderStatusToTextColorConverter OrderStatusToTextColorConverter = new OrderStatusToTextColorConverter();
        public readonly NoValueToTrueConverter NoValueToTrueConverter = new NoValueToTrueConverter();
        public readonly EmptyToResourceConverter EmptyToResource = new EmptyToResourceConverter();
        public readonly PhoneNumberConverter PhoneNumber = new PhoneNumberConverter();
        public readonly EnumToBoolConverter EnumToBool = new EnumToBoolConverter();
	}
}