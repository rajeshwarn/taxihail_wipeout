using apcurium.MK.Booking.Mobile.Infrastructure;
using Foundation;
using UIKit;
using TinyIoC;
using Cirrious.CrossCore.Core;
using apcurium.MK.Booking.Mobile.Client.Helper;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{
    public class PackageInfo : IPackageInfo
    {
        private static string _userAgent;

        public string Platform
        {
            get { return "iOS"; }
        }

		public string PlatformDetails
		{
			get
			{
				return string.Format ("{0} {1}",
					HardwareInfo.Version,
					UIDevice.CurrentDevice.SystemVersion);
			}
		}

        public string Version
        {
            get
            {                
                var nsVersion = NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleVersion");
                var ver = nsVersion.ToString();
                return ver;         
            }
        }


        public string UserAgent
        {
            get
            {
                if ( _userAgent == null )
                {
                    _userAgent = "";
                    //TODO: This will probably not work on the first call;
					TinyIoCContainer.Current.Resolve<IMvxMainThreadDispatcher>().RequestMainThreadAction(() =>
                    {
                        try
                        {
                            var webView = new UIWebView();
                            _userAgent = webView.EvaluateJavascript("navigator.userAgent");
                        }
                        catch
                        {
                            _userAgent = "";
                        }
                    });
                }
                return _userAgent;
            }
        }
    }
}
