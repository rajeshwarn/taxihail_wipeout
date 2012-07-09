using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using TaxiMobileApp;
using TaxiMobile.Helpers;
using Microsoft.Practices.ServiceLocation;
using TaxiMobile.Activities.Account;
using TaxiMobile.Activities.Book;

namespace TaxiMobile.Activities
{
    [Activity(Label = "@string/ApplicationName", MainLauncher = true, Theme = "@style/Theme.Splash", NoHistory = true, Icon = "@drawable/icon", ScreenOrientation=Android.Content.PM.ScreenOrientation.Portrait)]
    public class SplashActivity : Activity
    {


        private LocationService _locationService = new LocationService();

        protected override void OnCreate(Bundle bundle)
        {
            _locationService.Start();
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
