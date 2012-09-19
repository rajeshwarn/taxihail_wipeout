using System;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Practices;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Diagnostic;
using SocialNetworks.Services.OAuth;
using SocialNetworks.Services.MonoTouch;
using SocialNetworks.Services;
using apcurium.MK.Booking.Mobile.Settings;
using apcurium.MK.Booking.Mobile.Client.PlatformIntegration;
using apcurium.MK.Booking.Mobile.Navigation;
using apcurium.MK.Booking.Mobile.Data;
using Xamarin.Geolocation;

namespace apcurium.MK.Booking.Mobile.Client
{
    public class AppModule : IModule
    {
        public AppModule()
        {
        }

        #region IModule implementation
        public void Initialize()
        {
			TinyIoCContainer.Current.Register<IMessageService>(new MessageService());
            TinyIoCContainer.Current.Register<INavigationService>(new NavigationService( AppContext.Current.Controller ));
            TinyIoCContainer.Current.Register<IAppSettings>(new AppSettings());
            TinyIoCContainer.Current.Register<IPackageInfo>(new PackageInfo());

            TinyIoCContainer.Current.Register<IAppContext>(AppContext.Current);

            TinyIoCContainer.Current.Register<IAppResource, Resources>();
            TinyIoCContainer.Current.Register<ILogger, LoggerWrapper>();
			TinyIoCContainer.Current.Register<IErrorHandler, ErrorHandler>();

            TinyIoCContainer.Current.Register<ICacheService>(new CacheService());



            InitializeSocialNetwork();
            
        }

        private void InitializeSocialNetwork()
        {
             var settings = TinyIoCContainer.Current.Resolve<IAppSettings>();

            
            OAuthConfig oauthConfig = new OAuthConfig
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

