using System;
using System.Collections.Generic;
using apcurium.MK.Booking.Mobile.Client.Cache;
using apcurium.MK.Booking.Mobile.Client.Diagnostics;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.IoC;
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
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Dialog.Touch;
using MK.Common.iOS.Configuration;


namespace apcurium.MK.Booking.Mobile.Client
{
    public class Setup : MvxTouchDialogSetup
    {
        public Setup(MvxApplicationDelegate applicationDelegate, UIWindow window)
			: base(applicationDelegate, window)
        {
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
				return new List<Type> { typeof(AppConverters) };
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
			base.InitializeLastChance();
            var container = TinyIoCContainer.Current;

            container.Register<IAnalyticsService, GoogleAnalyticsService>();

            var locationService = new LocationService( );
            locationService.Start();

            container.Register<AbstractLocationService>(locationService );
            container.Register<IMessageService, MessageService>();
            container.Register<IPackageInfo>(new PackageInfo());

            container.Register<ILocalization, Localize>();
            container.Register<ILogger, LoggerWrapper>();
            container.Register<IErrorHandler, ErrorHandler>();            
            container.Register<ICacheService>(new CacheService());
            container.Register<ICacheService>(new CacheService("MK.Booking.Application.Cache"), "AppCache");

            container.Register<IPhoneService, PhoneService>();
            container.Register<IPushNotificationService, PushNotificationService>();

            container.Register<IAppSettings>(new AppSettingsService(container.Resolve<ICacheService>(), container.Resolve<ILogger>()));

            InitializeSocialNetwork();
        }

        private void InitializeSocialNetwork()
        {
            var settings = TinyIoCContainer.Current.Resolve<IAppSettings>();
            if (settings.Data.FacebookEnabled)
            {
                FBSettings.DefaultAppID = settings.Data.FacebookAppId;

                if (FBSession.ActiveSession.State == FBSessionState.CreatedTokenLoaded)
                {
                    // If there's one, just open the session silently
                    FBSession.OpenActiveSession(new[] { "basic_info", "email" },
                        allowLoginUI: false,
                        completion: (session, status, error) =>
                        {
                        });
                }

            }
            TinyIoCContainer.Current.Register<IFacebookService>(new FacebookService());

            var oauthConfig = new OAuthConfig();
            if (settings.Data.TwitterEnabled)
            {
                oauthConfig = new OAuthConfig
                {
                
                    ConsumerKey = settings.Data.TwitterConsumerKey,
                    Callback = settings.Data.TwitterCallback,
                    ConsumerSecret = settings.Data.TwitterConsumerSecret,
                    RequestTokenUrl = settings.Data.TwitterRequestTokenUrl,
                    AccessTokenUrl = settings.Data.TwitterAccessTokenUrl,
                    AuthorizeUrl = settings.Data.TwitterAuthorizeUrl 
                };


            }
            var twitterService = new TwitterService(oauthConfig, () => Mvx.Resolve<UINavigationController>());

            TinyIoCContainer.Current.Register<ITwitterService>(twitterService);
            
        }

		protected override Cirrious.MvvmCross.Touch.Views.Presenters.IMvxTouchViewPresenter CreatePresenter()
		{
			return new PhonePresenter(base.ApplicationDelegate, base.Window);
		}

		protected override Cirrious.CrossCore.IoC.IMvxIoCProvider CreateIocProvider()
		{
			return new TinyIoCProvider(TinyIoCContainer.Current);
		}

#endregion
    }
}
