using apcurium.MK.Booking.Mobile.Infrastructure;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using TinyIoC;
using Cirrious.CrossCore.Core;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{
    public class PackageInfo : IPackageInfo
    {
        private static string _userAgent;

        public string Platform
        {
            get { return "iOS"; }
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
