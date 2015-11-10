using Android.App;
using Android.Content;
using Android.OS;
using apcurium.MK.Booking.Mobile.Infrastructure;
using Java.Lang;
using String = System.String;
using StringBuilder = System.Text.StringBuilder;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{
    public class PackageInfo : IPackageInfo
    {
        private static string _userAgent;
        private readonly Context _appContext;

        public PackageInfo(Context appContext)
        {
            _appContext = appContext;
        }
			
        public string Platform
        {
            get { return "Android"; }
        }

		public string PlatformDetails
		{
			get
			{
				return string.Format ("{0} {1} {2}",
					Android.OS.Build.VERSION.Release,
					Android.OS.Build.Manufacturer,
					Android.OS.Build.Model);
			}
		}

        public string Version
        {
            get
            {
                var pInfo = Application.Context.PackageManager.GetPackageInfo(_appContext.ApplicationInfo.PackageName, 0);
                return pInfo.VersionName;
            }
        }

        public string UserAgent
        {
            get
            {
                if (_userAgent == null)
                {
                    try
                    {
                        var result = new StringBuilder(64);
                        result.Append("Dalvik/");


                        result.Append(JavaSystem.GetProperty("java.vm.version")); // such as 1.1.0
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