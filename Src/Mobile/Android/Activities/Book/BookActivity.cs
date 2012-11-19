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
        private bool _menuIsShown;
        private int _menuWidth = 400;
        private DecelerateInterpolator _interpolator = new DecelerateInterpolator(0.9f);
        private TinyMessageSubscriptionToken _orderConfirmedSubscription;
        private TinyMessageSubscriptionToken _bookUsingAddressSubscription;
        
        protected override void OnViewModelSet()
        {
            UnsubscribeOrderConfirmed();
            UnsubscribeBookUsingAddress();

            _bookUsingAddressSubscription = TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Subscribe<BookUsingAddress>(m => BookUsingAddress(m.Content));

            SetContentView(Resource.Layout.View_Book);

            var mainSettingsButton = FindViewById<HeaderedLayout>(Resource.Id.MainLayout).FindViewById<ImageButton>(Resource.Id.ViewNavBarRightButton);
            mainSettingsButton.Click -= MainSettingsButtonOnClick;
            mainSettingsButton.Click += MainSettingsButtonOnClick;


            var headerLayoutMenu = FindViewById<HeaderedLayout>(Resource.Id.HeaderLayoutMenu);
            var titleText = headerLayoutMenu.FindViewById<TextView>(Resource.Id.ViewTitle);
            titleText.Text = GetString(Resource.String.View_BookSettingMenu);

            var menu = FindViewById(Resource.Id.BookSettingsMenu);
            menu.Visibility = ViewStates.Gone;



            _menuWidth = WindowManager.DefaultDisplay.Width - 100;
            _menuIsShown = false;

            FindViewById<ImageButton>(Resource.Id.pickupDateButton).Click -= PickDate_Click;
            FindViewById<ImageButton>(Resource.Id.pickupDateButton).Click += PickDate_Click;

            //Settings 

            FindViewById<Button>(Resource.Id.settingsFavorites).Click -= ShowFavorites_Click;
            FindViewById<Button>(Resource.Id.settingsFavorites).Click += ShowFavorites_Click;

            FindViewById<Button>(Resource.Id.settingsAbout).Click -= About_Click;
            FindViewById<Button>(Resource.Id.settingsAbout).Click += About_Click;

            FindViewById<Button>(Resource.Id.settingsSupport).Click -= ReportProblem_Click;
            FindViewById<Button>(Resource.Id.settingsSupport).Click += ReportProblem_Click;

            FindViewById<Button>(Resource.Id.settingsProfile).Click -= ChangeDefaultRideSettings_Click;
            FindViewById<Button>(Resource.Id.settingsProfile).Click += ChangeDefaultRideSettings_Click;

            FindViewById<Button>(Resource.Id.settingsCallCompany).Click -= CallCie_Click;
            FindViewById<Button>(Resource.Id.settingsCallCompany).Click += CallCie_Click;

        }

        private void BookUsingAddress(Address address)
        {

            ViewModel.InitializeOrder();
            ViewModel.Pickup.SetAddress(address, true);
            ViewModel.Dropoff.ClearAddress();

            ViewModel.ConfirmOrder.Execute();
        }


        void ShowFavorites_Click(object sender, EventArgs e)
        {
            RunOnUiThread(() =>
            {
                Intent i = new Intent(this, typeof(LocationListActivity));

                StartActivity(i);
            });
            ToggleSettingsScreenVisibility();
        }

        private void About_Click(object sender, EventArgs e)
        {
            var intent = new Intent().SetClass(this, typeof(AboutActivity));
            StartActivity(intent);
            ToggleSettingsScreenVisibility();
        }

        private void CallCie_Click(object sender, EventArgs e)
        {
            var currentAccount = TinyIoCContainer.Current.Resolve<IAccountService>().CurrentAccount;
            if (currentAccount.Settings.ProviderId.HasValue)
            {
                RunOnUiThread(() => AlertDialogHelper.Show(this, "", TinyIoCContainer.Current.Resolve<IAppSettings>().PhoneNumberDisplay(currentAccount.Settings.ProviderId.Value), this.GetString(Resource.String.CallButton), CallCie, this.GetString(Resource.String.CancelBoutton), delegate { }));
            }

        }

        private void CallCie(object sender, EventArgs e)
        {
            var currentAccount = TinyIoCContainer.Current.Resolve<IAccountService>().CurrentAccount;
            Intent callIntent = new Intent(Intent.ActionCall, Android.Net.Uri.Parse("tel:" + TinyIoCContainer.Current.Resolve<IAppSettings>().PhoneNumberDisplay(currentAccount.Settings.ProviderId.Value)));
            StartActivity(callIntent);
            ToggleSettingsScreenVisibility();

        }


        private void ReportProblem_Click(object sender, EventArgs e)
        {


            ThreadHelper.ExecuteInThread(this, () =>
            {
                Intent emailIntent = new Intent(Intent.ActionSend);

                emailIntent.SetType("message/rfc822");
                emailIntent.PutExtra(Intent.ExtraEmail, new String[] { TinyIoCContainer.Current.Resolve<IAppSettings>().SupportEmail });
                emailIntent.PutExtra(Intent.ExtraCc, new String[] { AppContext.Current.LoggedInEmail });
                emailIntent.PutExtra(Intent.ExtraSubject, Resources.GetString(Resource.String.TechSupportEmailTitle));

                emailIntent.PutExtra(Intent.ExtraStream, Android.Net.Uri.Parse(@"file:///" + LoggerImpl.LogFilename));
                if (TinyIoCContainer.Current.Resolve<IAppSettings>().ErrorLogEnabled && File.Exists(TinyIoCContainer.Current.Resolve<IAppSettings>().ErrorLog))
                {
                    emailIntent.PutExtra(Intent.ExtraStream, Android.Net.Uri.Parse(TinyIoCContainer.Current.Resolve<IAppSettings>().ErrorLog));
                }
                try
                {
                    StartActivity(Intent.CreateChooser(emailIntent, this.GetString(Resource.String.SendEmail)));
                    LoggerImpl.FlushNextWrite();
                }
                catch (Android.Content.ActivityNotFoundException ex)
                {
                    RunOnUiThread(() => Toast.MakeText(this, Resources.GetString(Resource.String.NoMailClient), ToastLength.Short).Show());
                }
            }, false);
        }

        private void ChangeDefaultRideSettings_Click(object sender, EventArgs e)
        {
            var intent = new Intent().SetClass(this, typeof(RideSettingsActivity));
            StartActivity(intent);
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            UnsubscribeOrderConfirmed();
            UnsubscribeBookUsingAddress();
            
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

        
        private void UnsubscribeBookUsingAddress()
        {
            if (_bookUsingAddressSubscription != null)
            {
                TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Unsubscribe<BookUsingAddress>(_bookUsingAddressSubscription);
                _bookUsingAddressSubscription = null;
            }
        }
        
        private void UnsubscribeOrderConfirmed()
        {
            if (_orderConfirmedSubscription != null)
            {
                TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Unsubscribe<OrderConfirmed>(_orderConfirmedSubscription);

                _orderConfirmedSubscription.Dispose();
                _orderConfirmedSubscription = null;
            }
        }

        private void MainSettingsButtonOnClick(object sender, EventArgs eventArgs)
        {
            ToggleSettingsScreenVisibility();
        }

        private void ToggleSettingsScreenVisibility()
        {
            var mainLayout = FindViewById(Resource.Id.MainLayout);
            mainLayout.ClearAnimation();
            mainLayout.DrawingCacheEnabled = true;

            var menu = FindViewById(Resource.Id.BookSettingsMenu);
            menu.Visibility = _menuIsShown ? ViewStates.Gone : ViewStates.Visible;


            var animation = new SlideAnimation(mainLayout, _menuIsShown ? -(_menuWidth) : 0, _menuIsShown ? 0 : -(_menuWidth), _interpolator);
            animation.Duration = 400;
            mainLayout.StartAnimation(animation);

            _menuIsShown = !_menuIsShown;
        }

        protected override bool IsRouteDisplayed
        {
            get { return true; }
        }

        void PickDate_Click(object sender, EventArgs e)
        {
            UnsubscribeOrderConfirmed();

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

        public override void OnBackPressed()
        {
            if (_menuIsShown)
            {
                ToggleSettingsScreenVisibility();
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

    }
}