using apcurium.MK.Booking.Mobile.Infrastructure;
using Foundation;

namespace apcurium.MK.Booking.Mobile.Client.Localization
{
    public class Localize : ILocalization
	{
		public static string GetValue (string key)
		{
			return NSBundle.MainBundle.LocalizedString (key, "", "") ?? string.Empty;
		}

	    public string this[string key]
	    {
            get { return GetValue(key); }
	    }

        public bool Exists(string key)
        {
            const string Undefined = "UNDEFINED";
            return NSBundle.MainBundle.LocalizedString(key, Undefined, string.Empty) != Undefined;
        }

		public string CurrentLanguage
		{
			get { return this["LanguageCode"]; }
		}

		public bool IsRightToLeft
		{
			get { return this["LanguageCode"] == "ar"; }
		}
	}
}

