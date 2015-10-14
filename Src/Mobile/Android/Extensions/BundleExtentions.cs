using System.Collections.Generic;
using Android.OS;
using Cirrious.MvvmCross.Parse.StringDictionary;

namespace apcurium.MK.Booking.Mobile.Client.Extensions
{
	public static class BundleExtentions
	{
		private const string LaunchDataKey = "MvxLaunchData";
		private const string NavigationParametersKey = "Params";


		public static IDictionary<string, string> GetNavigationParameters(this BaseBundle sourceBundle)
		{
			if (sourceBundle == null || sourceBundle.ContainsKey(LaunchDataKey))
			{
				return new Dictionary<string, string>();
			}

			var navigationParamReader = new MvxStringDictionaryParser();

			var launchData = navigationParamReader.Parse(sourceBundle.GetString(LaunchDataKey));

			return !launchData.ContainsKey(NavigationParametersKey) 
				? new Dictionary<string, string>()
				: navigationParamReader.Parse(launchData[NavigationParametersKey]);
		}
	}
}