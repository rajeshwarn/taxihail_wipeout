using Android.Content;
using apcurium.MK.Common.Configuration;
using TinyIoC;

namespace apcurium.MK.Booking.Mobile.Client.Helpers
{
    public static class LayoutHelper
    {
        public static int GetLayoutForView(int defaultLayoutId, Context context)
        {
            var defaultLayoutName = context.Resources.GetResourceEntryName(defaultLayoutId);
            var customLayoutName = string.Format("{0}_{1}", defaultLayoutName, TinyIoCContainer.Current.Resolve<IAppSettings>().Data.TaxiHail.ApplicationName.Replace(" ", "").ToLower());

            int layoutId = context.Resources.GetIdentifier(customLayoutName, "layout", context.PackageName);
            if (layoutId == 0)
            {
                layoutId = defaultLayoutId;
            }

            return layoutId;
        }
    }
}

