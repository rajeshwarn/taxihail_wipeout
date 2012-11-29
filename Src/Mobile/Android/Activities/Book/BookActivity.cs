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
using apcurium.MK.Booking.Mobile.Client.Activities.Location;
using System.IO;
using apcurium.MK.Booking.Mobile.Client.Diagnostic;
using apcurium.MK.Booking.Mobile.Client.Activities.Setting;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.Client.Controls;
namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
    [Activity(Label = "Book", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class BookActivity : MvxBindingMapActivityView<BookViewModel>
    {
        private int _menuWidth = 400;
        private DecelerateInterpolator _interpolator = new DecelerateInterpolator(0.9f);
        
        protected override void OnViewModelSet()
        {
            SetContentView(Resource.Layout.View_Book);
			FindViewById<TouchMap>(Resource.Id.mapPickup).SetMapCenterPins(FindViewById<ImageView>(Resource.Id.mapPickupCenterPin), FindViewById<ImageView>(Resource.Id.mapDropoffCenterPin));

            var mainSettingsButton = FindViewById<HeaderedLayout>(Resource.Id.MainLayout).FindViewById<ImageButton>(Resource.Id.ViewNavBarRightButton);
            mainSettingsButton.Click -= MainSettingsButtonOnClick;
            mainSettingsButton.Click += MainSettingsButtonOnClick;


            var headerLayoutMenu = FindViewById<HeaderedLayout>(Resource.Id.HeaderLayoutMenu);
            var titleText = headerLayoutMenu.FindViewById<TextView>(Resource.Id.ViewTitle);
            titleText.Text = GetString(Resource.String.View_BookSettingMenu);

            var menu = FindViewById(Resource.Id.BookSettingsMenu);
            menu.Visibility = ViewStates.Gone;
            _menuWidth = WindowManager.DefaultDisplay.Width - 100;

            FindViewById<ImageButton>(Resource.Id.pickupDateButton).Click -= PickDate_Click;
            FindViewById<ImageButton>(Resource.Id.pickupDateButton).Click += PickDate_Click;

            
            
            
            //txt.Typeface = AppFonts.Bold;
            
            


			ViewModel.Panel.PropertyChanged -= HandlePropertyChanged;
			ViewModel.Panel.PropertyChanged += HandlePropertyChanged;
        }

        void HandlePropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "MenuIsOpen") {
				AnimateMenu();
			}
        }

        private void BookUsingAddress(Address address)
        {

            ViewModel.InitializeOrder();
            ViewModel.Pickup.SetAddress(address, true);
            ViewModel.Dropoff.ClearAddress();

            ViewModel.ConfirmOrder.Execute();
        }


        protected override void OnResume()
        {
            base.OnResume();
            apcurium.MK.Booking.Mobile.Client.Activities.Book.LocationService.Instance.Start();
        }
        protected override void OnStop()
        {
            
            base.OnStop();
            apcurium.MK.Booking.Mobile.Client.Activities.Book.LocationService.Instance.Stop();
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

        protected override bool IsRouteDisplayed
        {
            get { return true; }
        }

        void PickDate_Click(object sender, EventArgs e)
        {
            var messengerHub = TinyIoCContainer.Current.Resolve<ITinyMessengerHub>();
            var token = default(TinyMessageSubscriptionToken);
            token = messengerHub.Subscribe<DateTimePicked>(msg =>
            {
                if(token!=null)
                {
                    messengerHub.Unsubscribe<DateTimePicked>(token);
                }
                if (msg.Content.HasValue)
                {
                    ViewModel.Order.PickupDate = msg.Content;
                    ViewModel.PickupDateSelected();
                }
            });

            var intent = new Intent(this, typeof(DateTimePickerActivity));
            if (ViewModel.Order.PickupDate.HasValue)
            {
                intent.PutExtra("SelectedDate", ViewModel.Order.PickupDate.Value.Ticks);
            }
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

        private void ShowStatusActivity(Order data, OrderStatusDetail orderInfo)
        {
            RunOnUiThread(() =>
            {
                var param = new Dictionary<string, object>() {{"order", data}, {"orderInfo", orderInfo}};
                ViewModel.NavigateToOrderStatus.Execute(param);
            });
        }

		protected override void OnDestroy ()
		{
			base.OnDestroy ();
			if (ViewModel != null) {
				ViewModel.Panel.PropertyChanged -= HandlePropertyChanged;
			}
		}

    }
}