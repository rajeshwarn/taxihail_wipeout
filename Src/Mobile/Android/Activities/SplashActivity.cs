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
using Cirrious.MvvmCross.Android.Views;
using Cirrious.MvvmCross.Interfaces.Views;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using Cirrious.MvvmCross.Views;
using Java.Lang;
using Cirrious.MvvmCross.Interfaces.Platform.Location;
using System.Threading.Tasks;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.AppServices.Impl;

namespace apcurium.MK.Booking.Mobile.Client.Activities
{
    [Activity(Label = "@string/ApplicationName", MainLauncher = true, Theme = "@style/Theme.Splash", NoHistory = true, Icon = "@drawable/icon", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class SplashActivity : MvxBaseSplashScreenActivity
    {

        public SplashActivity()
        {
            
            
        }


       
        
        

   
        protected override void OnResume()
        {
          
            base.OnResume();
            apcurium.MK.Booking.Mobile.Client.Activities.Book.LocationService.Instance.Start();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            GC.Collect();
        }
        

       
    }

}
