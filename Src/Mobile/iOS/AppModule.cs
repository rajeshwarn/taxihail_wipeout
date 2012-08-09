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
                ConsumerKey = "CIi418tFp0c4DIj8tQKEw",
                Callback = "http://www.apcurium.com/oauth",
                ConsumerSecret = "wtHZHvigOaKaXjHQT3MjdKZ8aICOa6toNcJlbfWX54",
                RequestTokenUrl = "https://api.twitter.com/oauth/request_token",
                AccessTokenUrl = "https://twitter.com/oauth/access_token",
                AuthorizeUrl = "https://twitter.com/oauth/authorize"
            };

            var facebook = new FacebookServiceMT("405057926196041");
            var twitterService = new TwitterServiceMonoTouch(oauthConfig, ()=> AppContext.Current.Window.RootViewController.PresentedViewController == null ? AppContext.Current.Window.RootViewController : AppContext.Current.Window.RootViewController.PresentedViewController.ModalViewController != null ? AppContext.Current.Window.RootViewController.PresentedViewController.ModalViewController : AppContext.Current.Window.RootViewController.PresentedViewController);

            TinyIoCContainer.Current.Register<IFacebookService>(facebook);
            TinyIoCContainer.Current.Register<ITwitterService>(twitterService);

        }

        #endregion
    }
}

