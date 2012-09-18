using System;
using System.Collections.Generic;
using Cirrious.MvvmCross.Application;
using Cirrious.MvvmCross.Binding.Binders;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using Cirrious.MvvmCross.ExtensionMethods;
using MK.Booking.Mobile.Infrastructure.Practices;
using Cirrious.MvvmCross.Binding.Android;
using Android.Content;
using apcurium.MK.Booking.Mobile.Client.Converters;
using TinyIoC;
using apcurium.MK.Booking.Mobile.AppServices;


namespace apcurium.MK.Booking.Mobile.Client
{
    public class Setup
        : MvxBaseAndroidBindingSetup
    {
        public Setup(Context applicationContext)
            : base(applicationContext)
        {


        }
        
        
        protected override MvxApplication CreateApp()
        {
            var app = new TaxiHailApp();
            return app;
        }
                
        protected override void InitializeIoC()
        {
            TinyIoCServiceProviderSetup.Initialize();
        }

        protected override IEnumerable<Type> ValueConverterHolders
        {
            get { return new[] { typeof(AppConverters) }; }
        }
    }
}
