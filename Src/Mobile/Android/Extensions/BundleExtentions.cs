using System.Collections.Generic;
using Android.OS;
using Cirrious.MvvmCross.Parse.StringDictionary;

namespace apcurium.MK.Booking.Mobile.Client.Extensions
{
	public static class BundleExtentions
	{
		public static IDictionary<string, string> GetNavigationParameters(this BaseBundle sourceBundle)
		{
			if (sourceBundle == null || sourceBundle.ContainsKey("MvxLaunchData"))
			{
				return new Dictionary<string, string>();
			}

			var navigationParamReader = new MvxStringDictionaryParser();

			var launchData = navigationParamReader.Parse(sourceBundle.GetString("MvxLaunchData"));

			return !launchData.ContainsKey("Params") 
				? new Dictionary<string, string>() 
				: navigationParamReader.Parse(launchData["Params"]);
		}
	}
}