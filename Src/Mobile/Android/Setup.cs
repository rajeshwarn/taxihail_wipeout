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
using apcurium.MK.Booking.Mobile.Client.PlatformIntegration;
using apcurium.MK.Booking.Mobile.Settings;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.Client.Diagnostic;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.Client.Cache;
using Xamarin.Contacts;
using Xamarin.Geolocation;
using apcurium.MK.Booking.Mobile.Data;
using SocialNetworks.Services.OAuth;
using SocialNetworks.Services.MonoDroid;
using apcurium.MK.Booking.Mobile.Client.Activities.Account;
using SocialNetworks.Services;


namespace apcurium.MK.Booking.Mobile.Client
{
    public class Setup
        : MvxBaseAndroidBindingSetup
    {
        public Setup(Context applicationContext)
            : base(applicationContext)
        {

           


        }

        protected override void InitializeAdditionalPlatformServices()
        {
            base.InitializeAdditionalPlatformServices();


            TinyIoCContainer.Current.Register<IMessageService>(new MessageService(this.ApplicationContext));
			TinyIoCContainer.Current.Register<IAddressBookService>(new AddressBookService());
            TinyIoCContainer.Current.Register<IPackageInfo>(new PackageInfo(this.ApplicationContext));
            TinyIoCContainer.Current.Register<IAppSettings>(new AppSettings());
            TinyIoCContainer.Current.Register<IAppContext>(new AppContext(this.ApplicationContext));
            TinyIoCContainer.Current.Register<IAppResource>(new ResourceManager(this.ApplicationContext));
            TinyIoCContainer.Current.Register<ILogger, LoggerImpl>();
            TinyIoCContainer.Current.Register<IErrorHandler, ErrorHandler>();


            TinyIoCContainer.Current.Register<ICacheService>(new CacheService());
            TinyIoCContainer.Current.Register<AddressBook>(new AddressBook(this.ApplicationContext));

            TinyIoCContainer.Current.Register<Geolocator>(new Geolocator(this.ApplicationContext) { DesiredAccuracy = 1000 });
            TinyIoCContainer.Current.Register<Geolocator>(new Geolocator(this.ApplicationContext) { DesiredAccuracy = 10000 }, CoordinatePrecision.BallPark.ToString());
            TinyIoCContainer.Current.Register<Geolocator>(new Geolocator(this.ApplicationContext) { DesiredAccuracy = 1000 }, CoordinatePrecision.Coarse.ToString());
            TinyIoCContainer.Current.Register<Geolocator>(new Geolocator(this.ApplicationContext) { DesiredAccuracy = 900 }, CoordinatePrecision.Medium.ToString());
            InitializeSocialNetwork();
        }


        public void InitializeSocialNetwork()
        {



            var settings = TinyIoCContainer.Current.Resolve<IAppSettings>();



            var oauthConfig = new OAuthConfig
            {

                ConsumerKey = settings.TwitterConsumerKey,
                Callback = settings.TwitterCallback,
                ConsumerSecret = settings.TwitterConsumerSecret,
                RequestTokenUrl = settings.TwitterRequestTokenUrl,
                AccessTokenUrl = settings.TwitterAccessTokenUrl,
                AuthorizeUrl = settings.TwitterAuthorizeUrl
            };
            
            TinyIoCContainer.Current.Register<IFacebookService>((c, p) => new FacebookServicesMD(c.Resolve<IAppSettings>().FacebookAppId, LoginActivity.TopInstance ));
            TinyIoCContainer.Current.Register<ITwitterService>( (c,p)=> new TwitterServiceMonoDroid( oauthConfig, LoginActivity.TopInstance ) );

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
