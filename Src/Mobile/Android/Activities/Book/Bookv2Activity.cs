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

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
    [Activity(Label = "Bookv2", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class Bookv2Activity : MvxBindingMapActivityView<BookViewModel>, IAddress
    {

        private bool _menuIsShown;
        private int _menuWidth = 400;
        private DecelerateInterpolator _interpolator = new DecelerateInterpolator(0.9f);        
        private TinyMessageSubscriptionToken _subscription;
      
        protected override void OnViewModelSet()
        {
            UnsubscribeOrderConfirmed();
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

            if (ViewModel != null)
            {
                ViewModel.Initialize();
            }

        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            UnsubscribeOrderConfirmed();
        }

     
        private void MainSettingsButtonOnClick(object sender, EventArgs eventArgs)
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
                    
                    _subscription = TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Subscribe<OrderConfirmed>(OnOrderConfirmed);
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
            if (_subscription != null)
            {
                TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Unsubscribe<OrderConfirmed>(_subscription);
                _subscription.Dispose();
                _subscription = null;
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