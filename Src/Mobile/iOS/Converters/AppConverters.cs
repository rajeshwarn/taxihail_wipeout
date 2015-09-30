using apcurium.MK.Booking.Mobile.BindingConverter;

namespace apcurium.MK.Booking.Mobile.Client.Converters
{
	public class AppConverters
	{
        public readonly BoolInverter BoolInverter = new BoolInverter();
		public readonly OrderStatusToImageNameConverter OrderStatusToImageNameConverter = new OrderStatusToImageNameConverter();
        public readonly NoValueToTrueConverter NoValueToTrueConverter = new NoValueToTrueConverter();
        public readonly NoValueToVisibilityConverter NoValueToVisibility = new NoValueToVisibilityConverter();
        public readonly EmptyToResourceConverter EmptyToResource = new EmptyToResourceConverter();
        public readonly PhoneNumberConverter PhoneNumber = new PhoneNumberConverter();
        public readonly EnumToBoolConverter EnumToBool = new EnumToBoolConverter();
		public readonly EnumToBoolConverter EnumToInvertedBool = new EnumToBoolConverter(true);

		public readonly CurrencyFormatConverter CurrencyFormat = new CurrencyFormatConverter();
        public readonly StringFormatConverter StringFormat = new StringFormatConverter();

        public readonly DialCodeConverter DialCodeConverter = new DialCodeConverter();
        public readonly CreditCardNumberToTypeImageConverter CreditCardCompanyImageConverter = new CreditCardNumberToTypeImageConverter();
	}
}