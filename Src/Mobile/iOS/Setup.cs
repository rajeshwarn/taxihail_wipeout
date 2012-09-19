using System;

using System;
using System.Collections.Generic;
using Cirrious.MvvmCross.Application;
using Cirrious.MvvmCross.Dialog.Touch;
using Cirrious.MvvmCross.Touch.Interfaces;
using Cirrious.MvvmCross.Touch.Platform;
using Cirrious.MvvmCross.Binding.Binders;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using Cirrious.MvvmCross.ExtensionMethods;
using MK.Booking.Mobile.Infrastructure.Practices;
using Xamarin.Geolocation;
using apcurium.MK.Booking.Mobile.Data;
using TinyIoC;


namespace apcurium.MK.Booking.Mobile.Client
{
    public class Setup
        : MvxTouchDialogBindingSetup
    {
        public Setup(MvxApplicationDelegate applicationDelegate, IMvxTouchViewPresenter presenter)
            : base(applicationDelegate, presenter)
        {
        }
        
        #region Overrides of MvxBaseSetup
        
        protected override MvxApplication CreateApp()
        {
            var app = new TaxiHailApp();
            return app;
        }
        

//        protected override void FillValueConverters(Cirrious.MvvmCross.Binding.Interfaces.Binders.IMvxValueConverterRegistry registry)
//        {
//            base.FillValueConverters(registry);
//            
//            var filler = new MvxInstanceBasedValueConverterRegistryFiller(registry);
//            filler.AddFieldConverters(typeof(Converters));
//        }
        
		protected override void InitializeApp ()
		{
			base.InitializeApp ();


		}

        protected override void InitializeIoC()
        {
            TinyIoCServiceProviderSetup.Initialize();
			new AppModule().Initialize();
			TinyIoCContainer.Current.Register<Geolocator>(new Geolocator() { DesiredAccuracy = 250 });                        
			TinyIoCContainer.Current.Register<Geolocator>(new Geolocator() { DesiredAccuracy = 10000 }, CoordinatePrecision.BallPark.ToString());
			TinyIoCContainer.Current.Register<Geolocator>(new Geolocator() { DesiredAccuracy = 1000 }, CoordinatePrecision.Coarse.ToString());
			TinyIoCContainer.Current.Register<Geolocator>(new Geolocator() { DesiredAccuracy = 400 }, CoordinatePrecision.Medium.ToString());
        }

#endregion
    }
}
