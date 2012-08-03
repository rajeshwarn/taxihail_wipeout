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

namespace apcurium.MK.Booking.Mobile.Client.Activities
{
    [Activity(Label = "@string/ApplicationName", MainLauncher = true, Theme = "@style/Theme.Splash", NoHistory = true, Icon = "@drawable/icon", ScreenOrientation=Android.Content.PM.ScreenOrientation.Portrait)]
    public class SplashActivity : Activity
    {


        private LocationService _locationService = new LocationService();

        protected override void OnCreate(Bundle bundle)
        {
            InitializeSocialNetwork();
            _locationService.Start();
            base.OnCreate(bundle);

            var sessionValid = TinyIoCContainer.Current.Resolve<IAccountService>().CheckSession();

            ThreadHelper.ExecuteInThread(this, () => TinyIoCContainer.Current.Resolve<IAccountService>().RefreshCache(),false);            

            if (!sessionValid
                || AppContext.Current.LoggedUser == null)           
            {
                StartActivity(typeof(LoginActivity));
            }
            else
            {
                this.RunOnUiThread(() => StartActivity(typeof(MainActivity)));                
            }

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

            var facebook = new FacebookServicesMD("431321630224094", this);
            var twitterService = new TwitterServiceMonoDroid(oauthConfig, this);

            TinyIoCContainer.Current.Register<IFacebookService>(facebook);
            TinyIoCContainer.Current.Register<ITwitterService>(twitterService);

        }
    }
}
