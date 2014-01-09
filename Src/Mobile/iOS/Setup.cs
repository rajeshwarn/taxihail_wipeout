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
using apcurium.MK.Booking.Mobile.Client.PlatformIntegration.Social;
using apcurium.MK.Booking.Mobile.Settings;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Booking.Mobile.Client.Converters;
using MonoTouch.CoreLocation;
using Cirrious.MvvmCross.Binding.Touch.Target;
using apcurium.MK.Booking.Mobile.Client.Binding;
using Cirrious.MvvmCross.Binding.Bindings.Target.Construction;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using apcurium.MK.Booking.Mobile.Client.Controls.Binding;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.AppServices.Social;
using apcurium.MK.Booking.Mobile.AppServices.Social.OAuth;
using MonoTouch.FacebookConnect;


namespace apcurium.MK.Booking.Mobile.Client
{
    public class Setup
        : MvxTouchDialogBindingSetup
    {
        IMvxTouchViewPresenter _presenter;
        IDictionary<string, string> _options;

        public Setup(MvxApplicationDelegate applicationDelegate, IMvxTouchViewPresenter presenter, IDictionary<string, string> options)
            : base(applicationDelegate, presenter)
        {
            _presenter = presenter;
            _options = options;
        }
        
        #region Overrides of MvxBaseSetup
        
        protected override MvxApplication CreateApp()
        {
            var app = new TaxiHailApp(_options);
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


        protected override void FillTargetFactories (Cirrious.MvvmCross.Binding.Interfaces.Bindings.Target.Construction.IMvxTargetBindingFactoryRegistry registry)
        {
            base.FillTargetFactories (registry);
            registry.RegisterFactory(new MvxSimplePropertyInfoTargetBindingFactory(typeof(MvxUITextViewTargetBinding), typeof(UITextView), "Text"));
            CustomBindingsLoader.Load(registry);
        }

        protected override void InitializeIoC()
        {
            TinyIoCServiceProviderSetup.Initialize();

            TinyIoCContainer.Current.Register<IAnalyticsService, GoogleAnalyticsService>();

            var locationService = new LocationService( );
            locationService.Start();


            TinyIoCContainer.Current.Register<AbstractLocationService>(locationService );
			TinyIoCContainer.Current.Register<IAddressBookService>(new AddressBookService());
			TinyIoCContainer.Current.Register<IMessageService>(new MessageService());
            TinyIoCContainer.Current.Register<IAppSettings>(new AppSettings());
            TinyIoCContainer.Current.Register<IPackageInfo>(new PackageInfo());
            TinyIoCContainer.Current.Register<IMvxTouchViewPresenter>(_presenter);

            TinyIoCContainer.Current.Register<IAppResource, Resources>();
            TinyIoCContainer.Current.Register<ILogger, LoggerWrapper>();
            TinyIoCContainer.Current.Register<IErrorHandler, ErrorHandler>();            
            TinyIoCContainer.Current.Register<ICacheService>(new CacheService());
            TinyIoCContainer.Current.Register<IAppCacheService>(new AppCacheService());

            TinyIoCContainer.Current.Register<IPhoneService, PhoneService>();
            TinyIoCContainer.Current.Register<IPushNotificationService, PushNotificationService>();

            InitializeSocialNetwork();
        }

        private void InitializeSocialNetwork()
        {
            var settings = TinyIoCContainer.Current.Resolve<IAppSettings>();
			FBSettings.DefaultAppID = settings.FacebookAppId;

			if (FBSession.ActiveSession.State == FBSessionState.CreatedTokenLoaded)
			{
				// If there's one, just open the session silently
				FBSession.OpenActiveSession(new[] {"basic_info", "email"},
					allowLoginUI: false,
					completion:(session, status, error) => {});
			}
			TinyIoCContainer.Current.Register<IFacebookService>(new FacebookService());
            
			var oauthConfig = new OAuthConfig
            {
                
                ConsumerKey =  settings.TwitterConsumerKey,
                Callback = settings.TwitterCallback,
                ConsumerSecret = settings.TwitterConsumerSecret,
                RequestTokenUrl = settings.TwitterRequestTokenUrl,
                AccessTokenUrl = settings.TwitterAccessTokenUrl,
                AuthorizeUrl = settings.TwitterAuthorizeUrl 
            };

			var twitterService = new TwitterService(oauthConfig, ()=> {
                return AppContext.Current.Controller; 
            });
            
			TinyIoCContainer.Current.Register<ITwitterService>(twitterService);
            
        }

#endregion
    }
}
