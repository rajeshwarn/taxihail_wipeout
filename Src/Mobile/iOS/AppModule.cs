using System;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Practices;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Diagnostic;
using SocialNetworks.Services.OAuth;
using SocialNetworks.Services.MonoTouch;
using SocialNetworks.Services;

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
            TinyIoCContainer.Current.Register<IAppSettings>(new AppSettings());
            TinyIoCContainer.Current.Register<IAppContext>(AppContext.Current);

            TinyIoCContainer.Current.Register<IAppResource, Resources>();
            TinyIoCContainer.Current.Register<ILogger, LoggerWrapper>();

            TinyIoCContainer.Current.Register<ICacheService>(new CacheService());

            InitializeSocialNetwork();
            
        }

        private void InitializeSocialNetwork()
        {
            OAuthConfig oauthConfig = new OAuthConfig
            {
                ConsumerKey = "3nNkJ5EcI7yyi56ifLSAA",
                Callback = "http://www.taxihail.com/oauth",
                ConsumerSecret = "Th6nCDTgPiI3JPwHxgm8fQheMaLczUeHHG5liHGZRqs",
                RequestTokenUrl = "https://api.twitter.com/oauth/request_token",
                AccessTokenUrl = "https://twitter.com/oauth/access_token",
                AuthorizeUrl = "https://twitter.com/oauth/authorize"
            };

            var facebook = new FacebookServiceMT("134284363380764");
            var twitterService = new TwitterServiceMonoTouch(oauthConfig, ()=> AppContext.Current.Window.RootViewController.PresentedViewController == null ? AppContext.Current.Window.RootViewController : AppContext.Current.Window.RootViewController.PresentedViewController.ModalViewController != null ? AppContext.Current.Window.RootViewController.PresentedViewController.ModalViewController : AppContext.Current.Window.RootViewController.PresentedViewController);

            TinyIoCContainer.Current.Register<IFacebookService>(facebook);
            TinyIoCContainer.Current.Register<ITwitterService>(twitterService);

        }

        #endregion
    }
}

