using System.Collections.Generic;
using Android.OS;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.CrossCore;

namespace apcurium.MK.Booking.Mobile.Client.Extensions
{
    public static class BundleExtentions
    {
        private const string LaunchDataKey = "MvxLaunchData";
        private const string NavigationParametersKey = "Params";


        public static IDictionary<string, string> GetNavigationParameters(this BaseBundle sourceBundle)
        {
            if (sourceBundle == null)
            {
                return new Dictionary<string, string>();
            }

            var extraData = sourceBundle.GetString(LaunchDataKey);
            if (extraData == null)
            {
                return new Dictionary<string, string>();
            }

            var converter = Mvx.Resolve<IMvxNavigationSerializer>();
            var viewModelRequest = converter.Serializer.DeserializeObject<MvxViewModelRequest>(extraData);

            return viewModelRequest.ParameterValues;
        }

        public static IMvxBundle GetNavigationBundle(this BaseBundle sourceBundle)
        {
            var navParams = GetNavigationParameters(sourceBundle);
            return new MvxBundle(navParams);
        }
    }
}