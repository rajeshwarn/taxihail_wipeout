using System;
using Cirrious.MvvmCross.Platform;

namespace apcurium.MK.Booking.Mobile.Mvx
{
     public static class TinyIoCServiceProviderSetup
    {
        public static void Initialize()
        {
            var ioc = new TinyIoCProvider();
            MvxServiceProviderSetup.Initialize(ioc);
        }

        public static void Initialize(Type serviceProviderType)
        {
            var ioc = new TinyIoCProvider();
            MvxServiceProviderSetup.Initialize(serviceProviderType, ioc);
        }
    }
}

