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
using apcurium.MK.Booking.Mobile.Data;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.Client.PlatformIntegration;
using apcurium.MK.Booking.Mobile.Settings;
using apcurium.MK.Common.Diagnostic;
using SocialNetworks.Services.OAuth;
using SocialNetworks.Services.MonoTouch;
using SocialNetworks.Services;
using apcurium.MK.Booking.Mobile.Client.Converters;
using MonoTouch.CoreLocation;


namespace apcurium.MK.Booking.Mobile.Client
{
    public class Setup
        : MvxTouchDialogBindingSetup
    {
        IMvxTouchViewPresenter _presenter;

        public Setup(MvxApplicationDelegate applicationDelegate, IMvxTouchViewPresenter presenter)
            : base(applicationDelegate, presenter)
        {
            _presenter = presenter;
        }
        
        #region Overrides of MvxBaseSetup
        
        protected override MvxApplication CreateApp()
        {
            var app = new TaxiHailApp();
            return app;
        }
        
		protected override void InitializeApp ()
		{
			base.InitializeApp ();
		}

		protected override IEnumerable<System.Type> ValueConverterHolders {
			get {
				return new[] { typeof(AppConverters) };
			}
		}

        protected override void InitializeIoC()
        {
            TinyIoCServiceProviderSetup.Initialize();

            var locationService = new LocationService( );
            locationService.Initialize();
            TinyIoCContainer.Current.Register<ILocationService>(locationService );

//            TinyIoCContainer.Current.Register<ILocationService>(new Geolocator() { DesiredAccuracy = 5000 });                        
//
//            TinyIoCContainer.Current.Register<Geolocator>(new Geolocator() { DesiredAccuracy = 5000 });                        
//			TinyIoCContainer.Current.Register<Geolocator>(new Geolocator() { DesiredAccuracy = 10000 }, CoordinatePrecision.BallPark.ToString());
//			TinyIoCContainer.Current.Register<Geolocator>(new Geolocator() { DesiredAccuracy = 5000 }, CoordinatePrecision.Coarse.ToString());
//            TinyIoCContainer.Current.Register<Geolocator>(new Geolocator() { DesiredAccuracy = 5000 }, CoordinatePrecision.Medium.ToString());
//
			TinyIoCContainer.Current.Register<IAddressBookService>(new AddressBookService());
			TinyIoCContainer.Current.Register<IMessageService>(new MessageService());
            TinyIoCContainer.Current.Register<IAppSettings>(new AppSettings());
            TinyIoCContainer.Current.Register<IPackageInfo>(new PackageInfo());
            TinyIoCContainer.Current.Register<IMvxTouchViewPresenter>(_presenter);
            
            TinyIoCContainer.Current.Register<IAppContext>(AppContext.Current);            
            TinyIoCContainer.Current.Register<IAppResource, Resources>();
            TinyIoCContainer.Current.Register<ILogger, LoggerWrapper>();
            TinyIoCContainer.Current.Register<IErrorHandler, ErrorHandler>();            
            TinyIoCContainer.Current.Register<ICacheService>(new CacheService());
            TinyIoCContainer.Current.Register<IPhoneService, PhoneService>();
            InitializeSocialNetwork();
        }

        private void InitializeSocialNetwork()
        {
            var settings = TinyIoCContainer.Current.Resolve<IAppSettings>();
            
            
            var oauthConfig = new OAuthConfig
            {
                
                ConsumerKey =  settings.TwitterConsumerKey,
                Callback = settings.TwitterCallback,
                ConsumerSecret = settings.TwitterConsumerSecret,
                RequestTokenUrl = settings.TwitterRequestTokenUrl,
                AccessTokenUrl = settings.TwitterAccessTokenUrl,
                AuthorizeUrl = settings.TwitterAuthorizeUrl 
            };
            
            
            var facebook = new FacebookServiceMT(settings.FacebookAppId );// "134284363380764");
            var twitterService = new TwitterServiceMonoTouch(oauthConfig, ()=> AppContext.Current.Window.RootViewController.PresentedViewController == null ? AppContext.Current.Window.RootViewController : AppContext.Current.Window.RootViewController.PresentedViewController.ModalViewController != null ? AppContext.Current.Window.RootViewController.PresentedViewController.ModalViewController : AppContext.Current.Window.RootViewController.PresentedViewController);
            
            TinyIoCContainer.Current.Register<IFacebookService>(facebook);
            TinyIoCContainer.Current.Register<ITwitterService>(twitterService);
            
        }
#endregion
    }
}
