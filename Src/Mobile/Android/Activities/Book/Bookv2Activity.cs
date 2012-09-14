using System;
using Android.App;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using SlidingPanel;
using Cirrious.MvvmCross.Binding.Android.Views;
 
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using Android.Content;
using apcurium.MK.Booking.Mobile.Client.Models;
using TinyIoC;
using TinyMessenger;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Client.Activities.Location;
using apcurium.MK.Booking.Mobile.Client.Activities.History;
using System.IO;
using apcurium.MK.Booking.Mobile.Client.Diagnostic;
using apcurium.MK.Booking.Mobile.Client.Activities.Setting;
using apcurium.MK.Booking.Mobile.Infrastructure;
using SocialNetworks.Services;
using apcurium.MK.Booking.Mobile.Client.Activities.Account;
using Android.Content.PM;
namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
    [Activity(Label = "Bookv2", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class Bookv2Activity : MvxBindingMapActivityView<BookViewModel>, IAddress
    {

        private bool _menuIsShown;
        private int _menuWidth = 400;
        private DecelerateInterpolator _interpolator = new DecelerateInterpolator(0.9f);        
        private TinyMessageSubscriptionToken _pickDateTimeSubscription;
        private TinyMessageSubscriptionToken _bookUsingAddressSubscription;
        private TinyMessageSubscriptionToken _rebookSubscription;
        

        protected override void OnViewModelSet()
        {
            UnsubscribeOrderConfirmed();
            UnsubscribeBookUsingAddress();

            _bookUsingAddressSubscription = TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Subscribe<BookUsingAddress>(m => BookUsingAddress(m.Content));
            _rebookSubscription = TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Subscribe<RebookRequested>(m => Rebook(m.Content));

            SetContentView(Resource.Layout.Bookv2);
            
            var bottomLayout = FindViewById<FrameLayout>(Resource.Id.bottomLayout);
            AlphaAnimation alpha = new AlphaAnimation(0.1F, 0.1F);
            alpha.Duration = 0;
            alpha.FillAfter = true;
            bottomLayout.StartAnimation(alpha);
            var mainSettingsButton = FindViewById<ImageButton>(Resource.Id.MainSettingsBtn);
            mainSettingsButton.Click -= MainSettingsButtonOnClick;
            mainSettingsButton.Click += MainSettingsButtonOnClick;
            
            
            
            _menuWidth = WindowManager.DefaultDisplay.Width - 100;
            _menuIsShown = false;
            var mainSettingsLayout = FindViewById<LinearLayout>(Resource.Id.mainSettingsLayout);
            var mainSettingsLayoutHeader = FindViewById<RelativeLayout>(Resource.Id.mainSettingsLayoutHeader);

            FindViewById<Button>(Resource.Id.BookItBtn).Click -= new EventHandler(BookItBtn_Click);
            FindViewById<Button>(Resource.Id.BookItBtn).Click += new EventHandler(BookItBtn_Click);

            FindViewById<ImageButton>(Resource.Id.pickupDateButton).Click -= new EventHandler(PickDate_Click);
            FindViewById<ImageButton>(Resource.Id.pickupDateButton).Click += new EventHandler(PickDate_Click);


            //Settings 
            FindViewById<Button>(Resource.Id.settingsFavorites).Click -= new EventHandler(ShowFavorites_Click);
            FindViewById<Button>(Resource.Id.settingsFavorites).Click += new EventHandler(ShowFavorites_Click);

            FindViewById<Button>(Resource.Id.settingsHistory).Click -= new EventHandler(ShowHistory_Click);
            FindViewById<Button>(Resource.Id.settingsHistory).Click += new EventHandler(ShowHistory_Click);

            FindViewById<Button>(Resource.Id.settingsAbout).Click -= new EventHandler(About_Click);
            FindViewById<Button>(Resource.Id.settingsAbout).Click += new EventHandler(About_Click);


            FindViewById<Button>(Resource.Id.settingsLogout).Click -= new EventHandler(Logout_Click);
            FindViewById<Button>(Resource.Id.settingsLogout).Click += new EventHandler(Logout_Click);

            FindViewById<Button>(Resource.Id.settingsSupport).Click -= new EventHandler(ReportProblem_Click);
            FindViewById<Button>(Resource.Id.settingsSupport).Click += new EventHandler(ReportProblem_Click);

            FindViewById<Button>(Resource.Id.settingsProfile).Click -= new EventHandler(ChangeDefaultRideSettings_Click);
            FindViewById<Button>(Resource.Id.settingsProfile).Click += new EventHandler(ChangeDefaultRideSettings_Click);

            FindViewById<Button>(Resource.Id.settingsCallCompany ).Click -= new EventHandler(CallCie_Click);
            FindViewById<Button>(Resource.Id.settingsCallCompany).Click += new EventHandler(CallCie_Click);


            

            if (ViewModel != null)
            {
                ViewModel.Initialize();
            }

        }

        private void Rebook(Order order)
        {
            ViewModel.Rebook(order);
            BookItBtn_Click(this, EventArgs.Empty);
        }

        


        private void BookUsingAddress(Address address)
        {
            
            ViewModel.Load();
            ViewModel.Pickup.SetAddress(address);
            ViewModel.Dropoff.ClearAddress();
            BookItBtn_Click(this, EventArgs.Empty);
        }


        void ShowFavorites_Click(object sender, EventArgs e)
        {
         

            RunOnUiThread(() =>
            {
                Intent i = new Intent(this, typeof(LocationsActivity));
                
                StartActivity( i);
            });
            ToggleSettingsScreenVisibility();
        }

        void ShowHistory_Click(object sender, EventArgs e)
        {
            RunOnUiThread(() =>
            {
                Intent i = new Intent(this, typeof(HistoryActivity));
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

            if (AppContext.Current.LoggedUser.Settings.ProviderId.HasValue)
            {
                RunOnUiThread(() => AlertDialogHelper.Show(this, "", TinyIoCContainer.Current.Resolve<IAppSettings>().PhoneNumberDisplay(AppContext.Current.LoggedUser.Settings.ProviderId.Value), "Call", CallCie, "Cancel", delegate { }));
            }
            
        }

        private void CallCie(object sender, EventArgs e)
        {

            Intent callIntent = new Intent(Intent.ActionCall, Android.Net.Uri.Parse("tel:" + TinyIoCContainer.Current.Resolve<IAppSettings>().PhoneNumberDisplay(AppContext.Current.LoggedUser.Settings.ProviderId.Value)));
            StartActivity(callIntent);
            ToggleSettingsScreenVisibility();

        }

        private void Logout_Click(object sender, EventArgs e)
        {
            AppContext.Current.SignOut();
            var facebook = TinyIoC.TinyIoCContainer.Current.Resolve<IFacebookService>();
            if (facebook.IsConnected)
            {
                facebook.SetCurrentContext(this);
                facebook.Disconnect();
            }

            var twitterService = TinyIoC.TinyIoCContainer.Current.Resolve<ITwitterService>();
            if (twitterService.IsConnected)
            {
                twitterService.Disconnect();
            }

            TinyIoC.TinyIoCContainer.Current.Resolve<ICacheService>().ClearAll();
            RunOnUiThread(() =>
            {
                Finish();
                StartActivity(typeof(LoginActivity));
            });
        }

        private void ReportProblem_Click(object sender, EventArgs e)
        {


            ThreadHelper.ExecuteInThread(this, () =>
            {
                Intent emailIntent = new Intent(Intent.ActionSend);

                emailIntent.SetType("application/octet-stream");
                emailIntent.PutExtra(Intent.ExtraEmail, new String[] { TinyIoCContainer.Current.Resolve<IAppSettings>().SupportEmail });
                emailIntent.PutExtra(Intent.ExtraCc, new String[] { AppContext.Current.LoggedInEmail });
                emailIntent.PutExtra(Intent.ExtraSubject, Resources.GetString(Resource.String.TechSupportEmailTitle));

                //following line is for test purposes only.  Need to be removed when tested and also remove AboutAssets.txt from Assets (set action to None or remove completely)
                emailIntent.PutExtra(Intent.ExtraStream, Android.Net.Uri.Parse(@"file:///" + LoggerImpl.LogFilename)); // @"file:///android_asset/AboutAssets.txt" ) );

                if (TinyIoCContainer.Current.Resolve<IAppSettings>().ErrorLogEnabled && File.Exists(TinyIoCContainer.Current.Resolve<IAppSettings>().ErrorLog))
                {
                    emailIntent.PutExtra(Intent.ExtraStream, Android.Net.Uri.Parse(TinyIoCContainer.Current.Resolve<IAppSettings>().ErrorLog));
                }
                try
                {
                    StartActivity(Intent.CreateChooser(emailIntent, "Send mail..."));
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

        private void UnsubscribeBookUsingAddress()
        {
            if (_bookUsingAddressSubscription != null)
            {
                TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Unsubscribe<BookUsingAddress>(_bookUsingAddressSubscription);

            }
        }

     
        private void MainSettingsButtonOnClick(object sender, EventArgs eventArgs)
        {
            ToggleSettingsScreenVisibility();
        }

        private void ToggleSettingsScreenVisibility()
        {
            View v2 = FindViewById<FrameLayout>(Resource.Id.scrollinglayout);
            v2.ClearAnimation();
            v2.DrawingCacheEnabled = true;

            if (_menuIsShown)
            {
                SlideAnimation a = new SlideAnimation(v2, -(_menuWidth), 0, _interpolator);
                a.Duration = 400;
                v2.StartAnimation(a);
            }
            else
            {
                SlideAnimation a = new SlideAnimation(v2, 0, -(_menuWidth), _interpolator);
                a.Duration = 400;
                v2.StartAnimation(a);
            }

            _menuIsShown = !_menuIsShown;
        }

    
        public void OnResumeEvent()
        {
            OnResume();
        }


        protected override bool IsRouteDisplayed
        {
            get { return true; }
        }

        void PickDate_Click(object sender, EventArgs e)
        {
            _pickDateTimeSubscription = TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Subscribe<DateTimePicked>(OnDataTimePicked);

            var intent = new Intent(this, typeof(DateTimePickerActivity));
            if (ViewModel.Order.PickupDate.HasValue)
            {
                intent.PutExtra("SelectedDate", ViewModel.Order.PickupDate.Value.Ticks);
            }
            StartActivityForResult(intent, (int)ActivityEnum.DateTimePicked);            
        }

        private void OnDataTimePicked(DateTimePicked picked)
        {
            ViewModel.Order.PickupDate = picked.Content;   
        }

        void BookItBtn_Click(object sender, EventArgs e)
        {
            ConfirmOrder();
        }

        private void ConfirmOrder()
        {
            ThreadHelper.ExecuteInThread(this, () =>
            {

                if ((ViewModel.Order.PickupAddress.FullAddress.IsNullOrEmpty()) || (!ViewModel.Order.PickupAddress.HasValidCoordinate()))
                {
                    RunOnUiThread(() => this.ShowAlert(Resource.String.InvalidBookinInfoTitle, Resource.String.InvalidBookinInfo));
                }
                else
                {
                    
                    _pickDateTimeSubscription = TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Subscribe<OrderConfirmed>(OnOrderConfirmed);
                    RunOnUiThread(() =>
                    {
                        Intent i = new Intent(this, typeof(BookDetailActivity));
                        var serializedInfo = ViewModel.Order.Serialize();
                        i.PutExtra("BookingInfo", serializedInfo);
                        StartActivityForResult(i, (int)ActivityEnum.BookConfirmation);
                    });
                }
            }, true);
        }

        private void StartNewOrder()
        {            
        }

        private void UnsubscribeOrderConfirmed()
        {
            if (_pickDateTimeSubscription != null)
            {
                TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Unsubscribe<OrderConfirmed>(_pickDateTimeSubscription);
                _pickDateTimeSubscription.Dispose();
                _pickDateTimeSubscription = null;
            }
        }

        private void OnOrderConfirmed(OrderConfirmed orderConfirmed)
        {
            CompleteOrder(orderConfirmed.Content);
        }
        private void CompleteOrder(CreateOrder order)
        {
            ThreadHelper.ExecuteInThread(this, () =>
            {
                var service = TinyIoCContainer.Current.Resolve<IBookingService>();
                order.Id = Guid.NewGuid();
                try
                {
                    var orderInfo = service.CreateOrder(order);

                    if (orderInfo.IBSOrderId.HasValue
                        && orderInfo.IBSOrderId > 0)
                    {
                        AppContext.Current.LastOrder = order.Id;

                        var orderCreated = new Order { CreatedDate = DateTime.Now, DropOffAddress = order.DropOffAddress, IBSOrderId = orderInfo.IBSOrderId, Id = order.Id, PickupAddress = order.PickupAddress, Note = order.Note, PickupDate = order.PickupDate.HasValue ? order.PickupDate.Value : DateTime.Now, Settings = order.Settings };

                        ShowStatusActivity(orderCreated, orderInfo);

                        StartNewOrder();
                    }

                }
                catch (Exception ex)
                {
                    RunOnUiThread(() =>
                    {
                        string error = ex.Message;
                        string err = GetString(Resource.String.ErrorCreatingOrderMessage);
                        err += error.HasValue() ? " (" + error + ")" : "";
                        this.ShowAlert(GetString(Resource.String.ErrorCreatingOrderTitle), err);
                    });
                }

            }, true);
        }

        private void ShowStatusActivity(Order data, OrderStatusDetail orderInfo)
        {
            RunOnUiThread(() =>
            {
                Intent i = new Intent(this, typeof(BookingStatusActivity));
                var serialized = data.Serialize();
                i.PutExtra("Order", serialized);

                serialized = orderInfo.Serialize();
                i.PutExtra("OrderStatusDetail", serialized);


                StartActivityForResult(i, 101);
            });
        }


        /*protected override MapView Map
    {
        get { return FindViewById<MapView>(Resource.Id.mapPickup); }
    }

    protected override int TitleResourceId
    {
        get { return Resource.String.PickupMapTitle; }
    }

    protected override Address Location { get; set; }

    protected override bool NeedFindCurrentLocation
    {
        get
        {
            return false;
        }
    }

    protected override bool AddressCanBeEmpty
    {
        get { return true; }
    }

    protected override AutoCompleteTextView Address
    {
        get { return new AutoCompleteTextView(this); }
    }

    protected override Drawable MapPin
    {
        get { return Resources.GetDrawable(Resource.Drawable.pin_green); }
    }*/


       /* protected void InitializeDropDownMenu()
        {
            //Initialize dropdown control

            // Address book intent
            var contactIntent = new Intent(Intent.ActionPick, ContactsContract.CommonDataKinds.StructuredPostal.ContentUri);
            // Favorite address intent
            var locationIntent = new Intent(this, typeof(LocationsActivity));
            locationIntent.PutExtra(NavigationStrings.ParentScreen.ToString(), (int)ParentScreens.BookScreen);
            //gps intent
            var gpsIntent = new Intent(LocationBroadcastReceiver.ACTION_RESP);
            //nearby places intent
            var placesIntent = new Intent(this, typeof(LocationsActivity));
            placesIntent.PutExtra(NavigationStrings.ParentScreen.ToString(), (int)ParentScreens.BookScreen);
            placesIntent.PutExtra(NavigationStrings.LocationType.ToString(), (int)LocationTypes.NearbyPlaces);

            IntentFilter filter = new IntentFilter(LocationBroadcastReceiver.ACTION_RESP);
            filter.AddCategory(Intent.CategoryDefault);
            //var receiver = new LocationBroadcastReceiver(this);
            //RegisterReceiver(receiver, filter);

            //DropDownMenu definition
            var iconActionControl = new IconActionControl(this, "images/arrow-right@2x.png", new List<IconAction>() { 
				new IconAction("images/location-icon@2x.png", gpsIntent, null), 
				new IconAction("images/favorite-icon@2x.png", locationIntent, (int)ActivityEnum.Pickup), 
				new IconAction("images/contacts@2x.png", contactIntent, 42),
				new IconAction("images/nearby-icon@2x.png", placesIntent, (int)ActivityEnum.NearbyPlaces )
			}, false);
            var dropDownControlLayout = FindViewById<LinearLayout>(Resource.Id.linear_iconaction);
            dropDownControlLayout.AddView(iconActionControl);
        }*/
    }
}