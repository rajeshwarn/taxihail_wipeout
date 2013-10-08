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
        private Context _appContext;
        public PackageInfo(Context appContext)
        {
            _appContext = appContext;
        }

        #region IPackageInfo implementation

        public string Platform
        {
            get
            {
                return "Android";
            }
        }

        #endregion

        public string Version
        {
            get
            {
                var pInfo = Application.Context.PackageManager.GetPackageInfo(_appContext.ApplicationInfo.PackageName , 0);
                return pInfo.VersionName;
            }
        }


    }
}