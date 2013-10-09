using Android.App;
using Android.Content;
using apcurium.MK.Booking.Mobile.Infrastructure;

namespace apcurium.MK.Callbox.Mobile.Client.PlatformIntegration
{

    public class PackageInfo : IPackageInfo
    {
        private Context _appContext;
        public PackageInfo(Context appContext)
        {
            _appContext = appContext;
        }

        public string Platform
        {
            get
            {
                return "Android";
            }
        }


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