using System;
using System.Collections.Generic;
using apcurium.MK.Booking.Mobile.Client.Cache;
using apcurium.MK.Booking.Mobile.Client.Diagnostics;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.Mvx_;
using Cirrious.MvvmCross.Touch.Platform;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.Client.PlatformIntegration;
using apcurium.MK.Booking.Mobile.Client.PlatformIntegration.Social;
using apcurium.MK.Booking.Mobile.Settings;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Booking.Mobile.Client.Converters;
using apcurium.MK.Booking.Mobile.Client.Binding;
using Cirrious.MvvmCross.Binding.Bindings.Target.Construction;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.Client.Controls.Binding;
using apcurium.MK.Booking.Mobile.AppServices.Social;
using apcurium.MK.Booking.Mobile.AppServices.Social.OAuth;
using MonoTouch.FacebookConnect;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Views;


namespace apcurium.MK.Booking.Mobile.Client
{
    public class Setup: MvxTouchSetup
    {
        readonly IMvxViewPresenter _presenter;

        public Setup(MvxApplicationDelegate applicationDelegate, IMvxViewPresenter presenter)
            : base(applicationDelegate, presenter)
        {
            _presenter = presenter;
        }
        
        #region Overrides of MvxBaseSetup
        
		protected override IMvxApplication CreateApp()
        {
			return new TaxiHailApp();
        }

		protected override List<Type> ValueConverterHolders
		{
			get
			{
				return new[] { typeof(AppConverters) };
			}
		}

		protected override void FillTargetFactories (IMvxTargetBindingFactoryRegistry registry)
        {
            base.FillTargetFactories (registry);
            registry.RegisterFactory(new MvxSimplePropertyInfoTargetBindingFactory(typeof(MvxUITextViewTargetBinding), typeof(UITextView), "Text"));
            CustomBindingsLoader.Load(registry);
        }

		protected override void InitializeLastChance()
        {
            TinyIoCServiceProviderSetup.Initialize();

            TinyIoCContainer.Current.Register<IAnalyticsService, GoogleAnalyticsService>();

            var locationService = new LocationService( );
            locationService.Start();


            TinyIoCContainer.Current.Register<AbstractLocationService>(locationService );
			TinyIoCContainer.Current.Register<IMessageService>(new MessageService());
            TinyIoCContainer.Current.Register<IAppSettings>(new AppSettings());
            TinyIoCContainer.Current.Register<IPackageInfo>(new PackageInfo());
            TinyIoCContainer.Current.Register(_presenter);

            TinyIoCContainer.Current.Register<IAppResource, Resources>();
            TinyIoCContainer.Current.Register<ILogger, LoggerWrapper>();
            TinyIoCContainer.Current.Register<IErrorHandler, ErrorHandler>();            
            TinyIoCContainer.Current.Register<ICacheService>(new CacheService());
            TinyIoCContainer.Current.Register<ICacheService>(new CacheService("MK.Booking.Application.Cache"), "AppCache");

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
