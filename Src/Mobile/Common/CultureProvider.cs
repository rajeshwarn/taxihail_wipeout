using System;
using System.Globalization;
using TinyIoC;
using apcurium.MK.Common.Configuration;
using Cirrious.CrossCore;
using MK.Common.iOS.Configuration;

namespace apcurium.MK.Booking.Mobile
{
    public class CultureProvider
    {
		public static string FormatTime(DateTime date )
        {
            var formatTime = new CultureInfo( CultureInfoString ).DateTimeFormat.ShortTimePattern;
            string format = "{0:"+formatTime+"}";
            return string.Format(format, date);
        }
        
        public static string FormatDate(DateTime date )
        {
            return date.Date.ToLongDateString();
        }

		public static string FormatCurrency(double amount)
		{
			return amount.ToString("C", CultureInfo.GetCultureInfo(CultureInfoString));
		}
        
		public static double ParseCurrency(string amount)
		{
			if (amount == null) return 0;

			try
			{
				return double.Parse(amount, NumberStyles.Currency, CultureInfo.GetCultureInfo(CultureInfoString));
			}
			catch
			{
				return 0;
			}
		}
        
        public static string CultureInfoString
        {
            get{
				var culture = Mvx.Resolve<IAppSettings>().Data.PriceFormat;
                if (string.IsNullOrEmpty(culture))
                {
                    return "en-US";
                }
                return culture;
            }
        }
    }
}

