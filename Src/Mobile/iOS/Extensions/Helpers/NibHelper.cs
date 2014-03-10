using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using TinyIoC;
using apcurium.MK.Common.Configuration;

namespace apcurium.MK.Booking.Mobile.Client.Extensions.Helpers
{
    public static class NibHelper
    {
        public static UINib GetNibForView(string defaultNibName)
        {
            var customNibName = string.Format("{0}_{1}", defaultNibName, TinyIoCContainer.Current.Resolve<IAppSettings>().Data.ApplicationName.Replace(" ", ""));

            UINib nib;
            if (NSBundle.MainBundle.PathForResource(customNibName, "nib") != null)
            {
                nib = UINib.FromName(customNibName, null);
            }
            else
            {
                nib = UINib.FromName(defaultNibName, null);
            }

            return nib;
        }
    }
}

