using Android.App;
using Android.OS;
using SocialNetworks.Services;
using SocialNetworks.Services.MonoDroid;
using SocialNetworks.Services.OAuth;
using TinyIoC;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Client.Activities.Book;
using apcurium.MK.Booking.Mobile.Client.Activities.Account;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using System;
using apcurium.MK.Booking.Mobile.Infrastructure;

namespace apcurium.MK.Booking.Mobile.Client.Activities
{
    [Activity(Label = "@string/ApplicationName", MainLauncher = true, Theme = "@style/Theme.Splash", NoHistory = true, Icon = "@drawable/icon", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class SplashActivity : Activity
    {


        private LocationService _locationService = new LocationService();

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            InitializeSocialNetwork();

            _locationService.Start();

            ThreadHelper.ExecuteInThread(this, () => TinyIoCContainer.Current.Resolve<IAccountService>().RefreshCache(true), false);

            if (AppContext.Current.LoggedUser == null)
            {
                StartActivity(typeof(LoginActivity));
            }
            else
            {
                this.RunOnUiThread(() => StartActivity(typeof(MainActivity)));
            }

        }


        public static Activity TopActivity{ get; set; }

        public static FacebookServicesMD _fb;
        public FacebookServicesMD GetFacebookService()
        {
            if ( _fb  == null )
            {
                var settings = TinyIoCContainer.Current.Resolve<IAppSettings>();
                _fb = new FacebookServicesMD(settings.FacebookAppId, GetTopActivity());
            }
            return _fb;
        }


        public Activity GetTopActivity()
        {

            return TopActivity;
        }

        private void InitializeSocialNetwork()
        {
            TopActivity = this;

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
            
            var twitterService = new TwitterServiceMonoDroid(oauthConfig, this);

            TinyIoCContainer.Current.Register<IFacebookService>((c, p) => GetFacebookService());
            TinyIoCContainer.Current.Register<ITwitterService>(twitterService);

        }
    }
}
