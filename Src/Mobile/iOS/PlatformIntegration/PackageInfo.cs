using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using apcurium.MK.Booking.Mobile.Infrastructure;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{

    public class PackageInfo : IPackageInfo
    {
        private static string _userAgent;
        public PackageInfo()
        {

        }

        public string Platform
        {
            get { return "iOS"; }
        }

        public string Version
        {
            get
            {                
                NSObject nsVersion = NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleVersion");
                string ver = nsVersion.ToString();
                return ver;         
            }
        }


        public string UserAgent
        {
            get
            {
                if ( _userAgent == null )
                {
                    try
                    {
                        var webView = new UIWebView();
                        _userAgent = webView.EvaluateJavascript( "navigator.userAgent" );
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
