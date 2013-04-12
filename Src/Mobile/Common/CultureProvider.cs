using System;
using apcurium.MK.Common.Configuration;
using TinyIoC;
using System.Globalization;

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
		
		
		public static string CultureInfoString
		{
			get{
				var culture = TinyIoCContainer.Current.Resolve<IConfigurationManager>().GetSetting ( "PriceFormat" );
				if (string.IsNullOrEmpty(culture) )
				{
					return "en-US";
				}
				else
				{
					return culture;                
				}
			}
		}
	}
}

