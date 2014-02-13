using apcurium.MK.Booking.Mobile.Infrastructure;
using MonoTouch.Foundation;

namespace apcurium.MK.Booking.Mobile.Client.Localization
{
    public class Localize : ILocalization
	{
		public static string GetValue (string key)
		{
			var localizedValue = NSBundle.MainBundle.LocalizedString (key, "", "");
			return localizedValue ?? string.Empty;
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
	}
}

