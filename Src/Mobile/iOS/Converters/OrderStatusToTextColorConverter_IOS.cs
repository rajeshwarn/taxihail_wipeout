using System;
using Cirrious.MvvmCross.Converters;
using MonoTouch.UIKit;

namespace apcurium.MK.Booking.Mobile.Client.Converters
{
	public class OrderStatusToTextColorConverter: MvxBaseValueConverter
	{
		public override object Convert (object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			var darkGray = 	new UIColor(050f/255f, 050f/255f, 050f/255f, 255f/255f);

			var green = 	new UIColor(000f/255f, 102f/255f, 000f/255f, 255f/255f); //Active : 006600
			var orange = 	new UIColor(204f/255f, 102f/255f, 000f/255f, 255f/255f); //Scheduled : cc6600
			var darkRed = 	new UIColor(102f/255f, 000f/255f, 000f/255f, 255f/255f); //Canceled : 660000
			var navyBlue = 	new UIColor(000f/255f, 051f/255f, 153f/255f, 255f/255f); //Completed : 003399

			if (value == null) return darkGray;

			var name = Enum.GetName(value.GetType(), value);
			if (name == "Unknown") return green;
			if (name == "Pending") return orange;
			if (name == "Created") return orange;
			if (name == "Canceled") return darkRed;
			if (name == "Completed") return navyBlue;
			if (name == "Removed") return darkRed;

			throw new ArgumentException ("Unknown Enum member - please update this converter");
		}
	}
}

