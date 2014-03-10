using System;
using TinyIoC;
using apcurium.MK.Common.Configuration;
using Android.Content;

namespace apcurium.MK.Booking.Mobile.Client.Helpers
{
    public static class LayoutHelper
    {
        public static int GetLayoutForView(int defaultLayoutId, Context context)
        {
            var defaultLayoutName = context.Resources.GetResourceEntryName(defaultLayoutId);
            var customLayoutName = string.Format("{0}_{1}", defaultLayoutName, TinyIoCContainer.Current.Resolve<IAppSettings>().Data.ApplicationName.Replace(" ", "").ToLower());

            int layoutId = context.Resources.GetIdentifier(customLayoutName, "layout", context.PackageName);
            if (layoutId == 0)
            {
                layoutId = defaultLayoutId;
            }

            return layoutId;
        }
    }
}

