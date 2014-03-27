using System;
using System.Globalization;
using apcurium.MK.Common.Configuration;
using Cirrious.CrossCore;

namespace apcurium.MK.Booking.Mobile
{
    public class CultureProvider
    {
		public static string FormatTime(DateTime date )
        {
            var formatTime = new CultureInfo( CultureInfoString ).DateTimeFormat.ShortTimePattern;
            var format = "{0:"+formatTime+"}";
            return string.Format(format, date);
        }
        
        public static string FormatDate(DateTime date )
        {
            return date.Date.ToLongDateString();
        }

		public static string FormatCurrency(double amount)
		{
			Console.WriteLine ("amount:" + amount);
			var formattedAmount = amount.ToString ("C", CultureInfo.GetCultureInfo (CultureInfoString));
			Console.WriteLine ("formatted amount:" + formattedAmount);
			return formattedAmount;
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

