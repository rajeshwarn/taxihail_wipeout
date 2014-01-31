using System;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Linq;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Common;
using Android.Gms.Maps;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Client.Animations;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Diagnostic;
using apcurium.MK.Booking.Mobile.Client.Models;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Booking.Mobile.ViewModels;
using TinyIoC;
using TinyMessenger;
using Cirrious.MvvmCross.Droid.Views;
using System.Threading.Tasks;
using apcurium.MK.Booking.Mobile.Extensions;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
	[Activity(Label = "Home", Theme = "@style/MainTheme",
        ScreenOrientation = ScreenOrientation.Portrait, ClearTaskOnLaunch = true,
        FinishOnTaskLaunch = true)]
    public class HomeActivity : MvxActivity
    {
        private TouchMap _touchMap;

        public new HomeViewModel ViewModel
		{
			get
			{
				return (HomeViewModel)DataContext;
			}
		}

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            _touchMap.OnCreate(bundle);
            InitMap();        
        }

        private void InitMap()
        {
		    MapsInitializer.Initialize(this.ApplicationContext);
		    _touchMap.ViewTreeObserver.AddOnGlobalLayoutListener(new LayoutObserverForMap(_touchMap));
        }


        protected override void OnViewModelSet()
        {
            SetContentView(Resource.Layout.View_Book);

            _touchMap = FindViewById<TouchMap>(Resource.Id.mapPickup);
            _touchMap.SetMapCenterPins(FindViewById<ImageView>(Resource.Id.mapPickupCenterPin), FindViewById<ImageView>(Resource.Id.mapDropoffCenterPin));

            ViewModel.OnViewLoaded();

            FindViewById<TouchMap>(Resource.Id.mapPickup).PostInvalidateDelayed(100);

        }

        protected override void OnResume()
        {
            base.OnResume();
            TinyIoCContainer.Current.Resolve<AbstractLocationService>().Start();
        }

        protected override void OnPause()
        {
            base.OnPause();
	        _touchMap.Pause();
        }

        protected override void OnStart()
        {
            base.OnStart();
            if (ViewModel != null) ViewModel.Start();
        }

        protected override void OnStop()
        {
            base.OnStop();
            if (ViewModel != null) ViewModel.OnViewStopped();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (ViewModel != null)
            {
                ViewModel.OnViewUnloaded();
            }

            _touchMap.OnDestroy();
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            _touchMap.OnSaveInstanceState(outState);
        }

        public override void OnLowMemory()
        {
            base.OnLowMemory();
            _touchMap.OnLowMemory();
        }
    }
}