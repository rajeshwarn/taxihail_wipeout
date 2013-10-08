using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using apcurium.MK.Booking.Mobile.Infrastructure;
using MonoTouch.Foundation;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{

    public class PackageInfo : IPackageInfo
    {

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



    }
}
