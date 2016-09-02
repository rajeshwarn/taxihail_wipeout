using System;
using apcurium.MK.Common.Entity;
using System.Collections.Generic;
using System.Linq;
using Cirrious.CrossCore.Converters;

namespace apcurium.MK.Booking.Mobile.BindingConverter
{
	public class CreditCardNumberToTypeImageConverter : MvxValueConverter
	{
		private const string Visa = "Visa";
		private const string MasterCard = "MasterCard";
		private const string Amex = "Amex";
		private const string CreditCardGeneric = "Credit Card Generic";
		private const string VisaElectron = "Visa Electron";
		private const string Discover = "Discover";
		private List<ListItem> _creditCardCompanies;

		public CreditCardNumberToTypeImageConverter()
		{
			_creditCardCompanies = new List<ListItem>
				{
					new ListItem {Display = Visa, Image = "visa"},
					new ListItem {Display = MasterCard, Image = "mastercard"},
					new ListItem {Display = Amex, Image = "amex"},
					new ListItem {Display = VisaElectron, Image = "visa_electron"},
					new ListItem {Display = Discover, Image =  "discover"},
					new ListItem {Display = CreditCardGeneric, Image =  "credit_card_generic"}
				};
		}

		public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			var type = _creditCardCompanies.FirstOrDefault(x=>x.Display == value.ToString());
			return type.Image;
		}
	}
}

