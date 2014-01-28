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

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
	[Activity(Label = "Book", Theme = "@style/MainTheme",
        ScreenOrientation = ScreenOrientation.Portrait, ClearTaskOnLaunch = true,
        FinishOnTaskLaunch = true)]
    public class BookActivity : BaseBindingActivity<BookViewModel>
    {
        private readonly DecelerateInterpolator _interpolator = new DecelerateInterpolator(0.9f);
        private int _menuWidth = 400;
        private TouchMap _touchMap;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            _touchMap.OnCreate(bundle);

            var errorCode = GooglePlayServicesUtil.IsGooglePlayServicesAvailable(ApplicationContext);
            if (errorCode == ConnectionResult.ServiceMissing
                || errorCode == ConnectionResult.ServiceVersionUpdateRequired
                || errorCode == ConnectionResult.ServiceDisabled)
            {
                ViewModel.GooglePlayServicesNotAvailable.Execute();
                var dialog = GooglePlayServicesUtil.GetErrorDialog(errorCode, this, 0);
                dialog.Show();
                dialog.DismissEvent += (s, e) => Finish();
            }
            else
            {
                InitMap();
            }
        }

        private void InitMap()
        {
            try
            {
		MapsInitializer.Initialize(this.ApplicationContext);			;
		_touchMap.ViewTreeObserver.AddOnGlobalLayoutListener(new LayoutObserverForMap(_touchMap));
            }
            catch (GooglePlayServicesNotAvailableException e)
            {
                Logger.LogError(e);
            }
        }


        protected override void OnViewModelSet()
        {
            SetContentView(Resource.Layout.View_Book);

            _touchMap = FindViewById<TouchMap>(Resource.Id.mapPickup);
            _touchMap.SetMapCenterPins(FindViewById<ImageView>(Resource.Id.mapPickupCenterPin),
                FindViewById<ImageView>(Resource.Id.mapDropoffCenterPin));

            var mainSettingsButton =
                FindViewById<HeaderedLayout>(Resource.Id.MainLayout)
                    .FindViewById<ImageButton>(Resource.Id.ViewNavBarRightButton);
            mainSettingsButton.Click -= MainSettingsButtonOnClick;
            mainSettingsButton.Click += MainSettingsButtonOnClick;

            var bigButton = FindViewById<View>(Resource.Id.BigButtonTransparent);
            bigButton.Click -= MainSettingsButtonOnClick;
            bigButton.Click += MainSettingsButtonOnClick;

            //var mainLayoutMenu = FindViewById<RelativeLayout>(Resource.Id.MainLayoutMenu);
            //var titleText = mainLayoutMenu.FindViewById<TextView>(Resource.Id.ViewTitle);
            //titleText.Text = GetString(Resource.String.View_BookSettingMenu);

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

            ViewModel.OnViewLoaded();

            var _listContainer = FindViewById<ViewGroup>(Resource.Id.listContainer);

            for (int i = 0; i < _listContainer.ChildCount; i++)
            {
                if (_listContainer.GetChildAt(i).GetType() == typeof(Button))
                {
                    try
                    {
                        Button child = (Button)_listContainer.GetChildAt(i);
                        child.SetTypeface(Typeface.Create("sans-serif-light", TypefaceStyle.Normal), TypefaceStyle.Normal);
                    }
                    catch
                    {
                    
                    }
                }
            }

            FindViewById<TouchMap>(Resource.Id.mapPickup).PostInvalidateDelayed(100);
        }

        private void HandleSignOutButtonClick(object sender, EventArgs e)
        {
            ViewModel.Panel.SignOut.Execute();
            // Finish the activity, because clearTop does not seem to be enough in this case
            // Finish is delayed 1sec in order to prevent the application from being terminated
            Observable.Return(Unit.Default).Delay(TimeSpan.FromSeconds(1)).Subscribe(x => { Finish(); });
        }

        private void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "MenuIsOpen")
            {
                AnimateMenu();
            }
        }

        protected override void OnResume()
        {
            base.OnResume();

            TinyIoCContainer.Current.Resolve<AbstractLocationService>().Start();
            ViewModel.CenterMap(true);

            var mainLayout = FindViewById(Resource.Id.MainLayout);
            mainLayout.Invalidate();

            _touchMap.PostInvalidateDelayed(100);
            _touchMap.OnResume();
        }

        protected override void OnPause()
        {
            base.OnPause();

	        _touchMap.Pause();
        }

        protected override void OnDestroy()
        {
            if (ViewModel != null)
            {
                ViewModel.Panel.PropertyChanged -= HandlePropertyChanged;
            }

            base.OnDestroy();

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

            var menu = FindViewById(Resource.Id.BookSettingsMenu);

            var animation = new SlideAnimation(mainLayout, ViewModel.Panel.MenuIsOpen ? 0 : (_menuWidth),
                ViewModel.Panel.MenuIsOpen ? (_menuWidth) : 0, _interpolator);
            animation.Duration = 400;
            animation.AnimationStart +=
                (sender, e) => { if (ViewModel.Panel.MenuIsOpen) menu.Visibility = ViewStates.Visible; };
            animation.AnimationEnd +=
                (sender, e) => { if (!ViewModel.Panel.MenuIsOpen) menu.Visibility = ViewStates.Gone; };

            mainLayout.StartAnimation(animation);
        }

        private void PickDate_Click(object sender, EventArgs e)
        {
            //Close Menu if open
            ViewModel.Panel.MenuIsOpen = false;

            var messengerHub = TinyIoCContainer.Current.Resolve<ITinyMessengerHub>();
            var token = default(TinyMessageSubscriptionToken);
// ReSharper disable once RedundantAssignment
            token = messengerHub.Subscribe<DateTimePicked>(msg =>
            {
// ReSharper disable AccessToModifiedClosure
                if (token != null)

                {
                    messengerHub.Unsubscribe<DateTimePicked>(token);
                }
// ReSharper restore AccessToModifiedClosure

                ViewModel.PickupDateSelectedCommand.Execute(msg.Content);
            });

            var intent = new Intent(this, typeof (DateTimePickerActivity));
            if (ViewModel.Order.PickupDate.HasValue)
            {
                intent.PutExtra("SelectedDate", ViewModel.Order.PickupDate.Value.Ticks);
            }
            intent.PutExtra("UseAmPmFormat", ViewModel.UseAmPmFormat);

            StartActivityForResult(intent, (int) ActivityEnum.DateTimePicked);
        }

        public override void OnBackPressed()
        {
            if (ViewModel.Panel.MenuIsOpen)
            {
                ViewModel.Panel.MenuIsOpen = false;
            }
            else
            {
                base.OnBackPressed();
            }
        }
    }
}