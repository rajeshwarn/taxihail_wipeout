using System;
using Cirrious.MvvmCross.Platform;

namespace MK.Booking.Mobile.Infrastructure.Practices
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

