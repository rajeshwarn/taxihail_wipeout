using Android.App;
using Android.Content;
using apcurium.MK.Booking.Mobile.Infrastructure;
using System.Text;
using System;
using Android.OS;
using Cirrious.CrossCore.Droid;

namespace apcurium.MK.Callbox.Mobile.Client.PlatformIntegration
{

    public class PackageInfo : IPackageInfo
    {
		readonly Context _appContext;
        private static string _userAgent = null;
		public PackageInfo(IMvxAndroidGlobals globals)
        {
			_appContext = globals.ApplicationContext;
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

        public string UserAgent
        {
            get{

                
                if (_userAgent == null)
                {
                    try
                    {
                        StringBuilder result = new StringBuilder(64);
                        result.Append("Dalvik/");


                        result.Append(Java.Lang.JavaSystem.GetProperty("java.vm.version")); // such as 1.1.0
                        result.Append(" (Linux; U; Android ");

                        String version = Build.VERSION.Release; // "1.0" or "3.4b5"
                        result.Append(version.Length > 0 ? version : "1.0");

                        // add the model for the release build
                        if ("REL".Equals(Build.VERSION.Codename))
                        {
                            String model = Build.Model;
                            if (model.Length > 0)
                            {
                                result.Append("; ");
                                result.Append(model);
                            }
                        }
                        String id = Build.Id; // "MASTER" or "M4-rc20"
                        if (id.Length > 0)
                        {
                            result.Append(" Build/");
                            result.Append(id);
                        }
                        result.Append(")");
                        _userAgent = result.ToString();

                    }
                    catch
                    {
                        _userAgent = "";
                    }
                }

                return _userAgent;
            }
        }
    }
}