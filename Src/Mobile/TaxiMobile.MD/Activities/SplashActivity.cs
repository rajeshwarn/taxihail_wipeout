using Android.App;
using Android.Content.PM;
using Android.OS;
using TaxiMobile.Activities.Account;

namespace TaxiMobile.Activities
{
    [Activity(Label = "@string/ApplicationName", MainLauncher = true, Theme = "@style/Theme.Splash", NoHistory = true, Icon = "@drawable/icon", ScreenOrientation=ScreenOrientation.Portrait)]
    public class SplashActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            if (AppContext.Current.LoggedUser == null)           
            //if(true)
            {
                StartActivity(typeof(LoginActivity));
            }
            else
            {
                this.RunOnUiThread(() => StartActivity(typeof(MainActivity)));                
            }

        }
    }
}
