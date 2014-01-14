using System;
using Android.Graphics;
using Cirrious.CrossCore.Converters;

namespace apcurium.MK.Booking.Mobile.Client.Converters
{
    public class OrderStatusToTextColorConverter: MvxValueConverter
	{
		public override object Convert (object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
            var darkGray = new Color(50, 50, 50, 255);

            var green = new Color(0, 102, 0, 255); //Active : 006600
            var orange = new Color(204, 102, 0, 255); //Scheduled : cc6600
            var darkRed = new Color(102, 0, 0, 255); //Canceled : 660000
            var navyBlue = new Color(0, 51, 153, 255); //Completed : 003399

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

