using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Infrastructure;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{

    public class PackageInfo : IPackageInfo
    {
        public TaxiMobileApplication App { get; set; }
        public PackageInfo(TaxiMobileApplication app)
        {
            App = app;
        }

        public string Version
        {
            get
            {
                var pInfo = Application.Context.PackageManager.GetPackageInfo(Application.Context.PackageName, 0);
                return pInfo.VersionName;
            }
        }


    }
}