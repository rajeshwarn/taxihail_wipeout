using System;
using System.Globalization;
using apcurium.MK.Common.Configuration;
using Cirrious.CrossCore;
using apcurium.MK.Booking.Mobile.Infrastructure;

namespace apcurium.MK.Booking.Mobile
{
    public class CultureProvider
    {
		public static string FormatTime(DateTime date )
        {
			var formatTime = CultureInfo.DateTimeFormat.ShortTimePattern;
            var format = "{0:"+formatTime+"}";
            return string.Format(format, date);
        }
        
        public static string FormatDate(DateTime date )
        {
            return date.Date.ToLongDateString();
        }

		public static string FormatCurrency(double amount)
		{
			var stringFormattedAsACurrency = string.Format (CultureInfo, Mvx.Resolve<ILocalization> () ["CurrencyPriceFormat"], amount);
			return stringFormattedAsACurrency.Replace (CultureInfo.NumberFormat.NumberDecimalSeparator, CultureInfo.NumberFormat.CurrencyDecimalSeparator);
		}

		public static CultureInfo CultureInfo
        {
            get
			{
				var culture = Mvx.Resolve<IAppSettings>().Data.PriceFormat;
                if (string.IsNullOrEmpty(culture))
                {
					culture = "en-US";
                }
				return CultureInfo.GetCultureInfo(culture);
            }
        }
    }
}

