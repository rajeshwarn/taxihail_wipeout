using System;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
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
using Cirrious.MvvmCross.Droid.Views;
using TinyIoC;
using TinyMessenger;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.Client.Animations;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Diagnostic;
using apcurium.MK.Booking.Mobile.Client.Models;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
	[Activity(Label = "Home", Theme = "@style/MainTheme",
        ScreenOrientation = ScreenOrientation.Portrait, ClearTaskOnLaunch = true,
        FinishOnTaskLaunch = true)]
    public class HomeActivity : MvxActivity
    {
        private OrderMapView _touchMap;

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
            var errorCode = GooglePlayServicesUtil.IsGooglePlayServicesAvailable(ApplicationContext);
            if (errorCode == ConnectionResult.ServiceMissing
                || errorCode == ConnectionResult.ServiceVersionUpdateRequired
                || errorCode == ConnectionResult.ServiceDisabled)
            {
                //ViewModel.GooglePlayServicesNotAvailable.Execute();
                var dialog = GooglePlayServicesUtil.GetErrorDialog(errorCode, this, 0);
                dialog.Show();
                dialog.DismissEvent += (s, e) => Finish();
            }
            else
            {
                InitMap();
            }        


            ImageView iv = new ImageView();
        }

        protected override void OnViewModelSet()
        {
            SetContentView(Resource.Layout.View_Home);

            _touchMap = FindViewById<OrderMapView>(Resource.Id.mapPickup);
            _touchMap.SetMapCenterPins(FindViewById<ImageView>(Resource.Id.mapPickupCenterPin), FindViewById<ImageView>(Resource.Id.mapDropoffCenterPin));

            ViewModel.OnViewLoaded();

            FindViewById<OrderMapView>(Resource.Id.mapPickup).PostInvalidateDelayed(100);

            // TODO: either subclass ImageView to create a generic imageView with a resource binding (set->SetImageResource) for Car selection or do everything here in the activity
            // To check when viewmodel will integrate it
        }

        protected override void OnResume()
        {
            base.OnResume();
            TinyIoCContainer.Current.Resolve<AbstractLocationService>().Start();

            var mainLayout = FindViewById(Resource.Id.HomeMainLayout);
            mainLayout.Invalidate();

            _touchMap.PostInvalidateDelayed(100);
            _touchMap.OnResume();
        }

        protected override void OnPause()
        {
            base.OnPause();
	        //_touchMap.Pause();
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

        private void InitMap()
        {
            try
            {
      MapsInitializer.Initialize(this.ApplicationContext);            ;
      _touchMap.ViewTreeObserver.AddOnGlobalLayoutListener(new apcurium.MK.Booking.Mobile.Client.Controls.OrderMapView.LayoutObserverForMap(_touchMap));
            }
            catch (GooglePlayServicesNotAvailableException e)
            {
                Logger.LogError(e);
            }
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