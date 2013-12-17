using System;
using System.Collections.Generic;
using Android.App;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using SlidingPanel;
using Cirrious.MvvmCross.Binding.Android.Views;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using Android.Content;
using apcurium.MK.Booking.Mobile.Client.Models;
using TinyIoC;
using TinyMessenger;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Client.Activities.GeoLocation;
using System.IO;
using apcurium.MK.Booking.Mobile.Client.Diagnostic;
using apcurium.MK.Booking.Mobile.Client.Activities.Setting;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.Client.Controls;
using System.Reactive.Linq;
using System.Reactive;
using Java.Lang;
using Android.OS;
using Android.Content.PM;
using Android.Gms.Maps;
using Android.Gms.Common;


namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
    [Activity(Label = "Book", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, ClearTaskOnLaunch = true, FinishOnTaskLaunch = true )]
    public class BookActivity : MvxBindingActivityView<BookViewModel>
    {
        private int _menuWidth = 400;
        private DecelerateInterpolator _interpolator = new DecelerateInterpolator(0.9f);
		private TouchMap _touchMap;

		protected override void OnCreate(Bundle bundle)
		{
			try
			{
				MapsInitializer.Initialize(this.ApplicationContext);
			}
			catch (GooglePlayServicesNotAvailableException e)
			{
				Logger.LogError(e);
			}

			base.OnCreate(bundle);
			_touchMap.OnCreate(bundle);
			_touchMap.SetMapReady();

		}



        protected override void OnViewModelSet()
        {
			SetContentView(Resource.Layout.View_Book);

			_touchMap = FindViewById<TouchMap>(Resource.Id.mapPickup);
			_touchMap.SetMapCenterPins(FindViewById<ImageView>(Resource.Id.mapPickupCenterPin), FindViewById<ImageView>(Resource.Id.mapDropoffCenterPin));

            var mainSettingsButton = FindViewById<HeaderedLayout>(Resource.Id.MainLayout).FindViewById<ImageButton>(Resource.Id.ViewNavBarRightButton);
            mainSettingsButton.Click -= MainSettingsButtonOnClick;
            mainSettingsButton.Click += MainSettingsButtonOnClick;

			var bigButton = FindViewById<View>(Resource.Id.BigButtonTransparent);
			bigButton.Click -= MainSettingsButtonOnClick;
			bigButton.Click += MainSettingsButtonOnClick;

            var headerLayoutMenu = FindViewById<HeaderedLayout>(Resource.Id.HeaderLayoutMenu);
            var titleText = headerLayoutMenu.FindViewById<TextView>(Resource.Id.ViewTitle);
            titleText.Text = GetString(Resource.String.View_BookSettingMenu);

            var menu = FindViewById(Resource.Id.BookSettingsMenu);
            menu.Visibility = ViewStates.Gone;
            _menuWidth = WindowManager.DefaultDisplay.Width - 100;

            FindViewById<View>(Resource.Id.pickupDateButton).Click -= PickDate_Click;
            FindViewById<View>(Resource.Id.pickupDateButton).Click += PickDate_Click;

			var signOutButton = FindViewById<View>(Resource.Id.settingsLogout);
			signOutButton.Click -= HandleSignOutButtonClick;
			signOutButton.Click += HandleSignOutButtonClick;

			ViewModel.Panel.PropertyChanged -= HandlePropertyChanged;
			ViewModel.Panel.PropertyChanged += HandlePropertyChanged;

			ViewModel.Load();

            FindViewById<TouchMap>(Resource.Id.mapPickup).PostInvalidateDelayed(100);
        }

        void HandleSignOutButtonClick (object sender, EventArgs e)
        {
			ViewModel.Panel.SignOut.Execute();
			// Finish the activity, because clearTop does not seem to be enough in this case
			// Finish is delayed 1sec in order to prevent the application from being terminated
			Observable.Return(Unit.Default).Delay (TimeSpan.FromSeconds(1)).Subscribe(x=>{
				Finish();
			});
        }

        void HandlePropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "MenuIsOpen") {
				AnimateMenu();
			}
        }

        protected override void OnResume()
        {
            base.OnResume();

            ViewModel.ShowTutorial.Execute();

			TinyIoC.TinyIoCContainer.Current.Resolve<AbstractLocationService>().Start();
            ViewModel.CenterMap(true);

            var mainLayout = FindViewById(Resource.Id.MainLayout);
            mainLayout.Invalidate();

			_touchMap.PostInvalidateDelayed(100);
			_touchMap.OnResume();
        }

		protected override void OnPause()
		{
			base.OnPause();

			_touchMap.OnPause();
		}

        protected override void OnStart ()
        {
            base.OnStart ();
            if(ViewModel != null) ViewModel.Start();
        }

        protected override void OnRestart ()
        {
            base.OnRestart ();
            if(ViewModel != null) ViewModel.Restart();
        }

        protected override void OnStop()
        {            
            base.OnStop();
            if(ViewModel != null) ViewModel.Stop();			
        }

        protected override void OnDestroy ()
        {
            base.OnDestroy ();

            if (ViewModel != null) {
                ViewModel.Unload();
                ViewModel.Panel.PropertyChanged -= HandlePropertyChanged;
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

        private void MainSettingsButtonOnClick(object sender, EventArgs eventArgs)
        {
			ViewModel.Panel.MenuIsOpen = !ViewModel.Panel.MenuIsOpen;
        }

        private void AnimateMenu()
        {
            var mainLayout = FindViewById(Resource.Id.MainLayout);
            mainLayout.ClearAnimation();
            mainLayout.DrawingCacheEnabled = true;

            var menu = FindViewById(Resource.Id.BookSettingsMenu);

			var animation = new SlideAnimation(mainLayout, ViewModel.Panel.MenuIsOpen ? 0: -(_menuWidth), ViewModel.Panel.MenuIsOpen ? -(_menuWidth): 0, _interpolator);
            animation.Duration = 400;
			animation.AnimationStart +=	 (sender, e) => {
				if(ViewModel.Panel.MenuIsOpen) menu.Visibility = ViewStates.Visible;
			};
			animation.AnimationEnd +=	 (sender, e) => {
				if(!ViewModel.Panel.MenuIsOpen) menu.Visibility = ViewStates.Gone;
			};

            mainLayout.StartAnimation(animation);
        }

        void PickDate_Click(object sender, EventArgs e)
        {
			//Close Menu if open
			ViewModel.Panel.MenuIsOpen = false;

            var messengerHub = TinyIoCContainer.Current.Resolve<ITinyMessengerHub>();
            var token = default(TinyMessageSubscriptionToken);
            token = messengerHub.Subscribe<DateTimePicked>(msg =>
            {
                if(token!=null)
                {
                    messengerHub.Unsubscribe<DateTimePicked>(token);
                }

                ViewModel.PickupDateSelectedCommand.Execute(msg.Content);
            });

            var intent = new Intent(this, typeof(DateTimePickerActivity));
            if (ViewModel.Order.PickupDate.HasValue)
            {
                intent.PutExtra("SelectedDate", ViewModel.Order.PickupDate.Value.Ticks);
            }
            intent.PutExtra("UseAmPmFormat", ViewModel.UseAmPmFormat );

            StartActivityForResult(intent, (int)ActivityEnum.DateTimePicked);
        }

        public override void OnBackPressed ()
		{
			if (ViewModel.Panel.MenuIsOpen) {
				ViewModel.Panel.MenuIsOpen = false;
			}
			else
            {
                base.OnBackPressed();
            }
        }

    }
}