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

            ThreadHelper.ExecuteInThread(this, () => TinyIoCContainer.Current.Resolve<IAccountService>().RefreshCache(AppContext.Current.LoggedUser != null), false);

            if (AppContext.Current.LoggedUser == null)
            {
                StartActivity(typeof(LoginActivity));
            }
            else
            {
                this.RunOnUiThread(() => StartActivity(typeof(MainActivity)));
            }

        }

		protected override void OnDestroy ()
		{
			base.OnDestroy ();
		}

        public static Activity TopActivity{ get; set; }

        public static FacebookServicesMD _fb;
        public FacebookServicesMD GetFacebookService()
        {
            if ( _fb  == null )
            {
				//android key hash: Jbi/meELm8i7T47pKA4HZBN5NUk=
				_fb = new FacebookServicesMD("134284363380764", GetTopActivity());
//                _fb = new FacebookServicesMD("431321630224094", GetTopActivity());
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
            OAuthConfig oauthConfig = new OAuthConfig
            {
                ConsumerKey = "3nNkJ5EcI7yyi56ifLSAA",
                Callback = "http://www.taxihail.com/oauth",
                ConsumerSecret = "Th6nCDTgPiI3JPwHxgm8fQheMaLczUeHHG5liHGZRqs",
                RequestTokenUrl = "https://api.twitter.com/oauth/request_token",
                AccessTokenUrl = "https://twitter.com/oauth/access_token",
                AuthorizeUrl = "https://twitter.com/oauth/authorize"
            };

            //var facebook = new FacebookServicesMD("134284363380764", this);
            var twitterService = new TwitterServiceMonoDroid(oauthConfig, this);

            TinyIoCContainer.Current.Register<IFacebookService>((c, p) => GetFacebookService());
            TinyIoCContainer.Current.Register<ITwitterService>(twitterService);

        }
    }

}
