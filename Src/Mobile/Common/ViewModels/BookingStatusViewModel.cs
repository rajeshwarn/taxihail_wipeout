using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Subjects;
using Cirrious.MvvmCross.Commands;
using Cirrious.MvvmCross.ExtensionMethods;
using Cirrious.MvvmCross.Interfaces.Commands;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using ServiceStack.Text;
using TinyIoC;
using TinyMessenger;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Booking.Mobile.Models;
using apcurium.MK.Common.Configuration;
using System.Globalization;
using System.Reactive.Linq;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using System.Reactive.Disposables;
using apcurium.MK.Booking.Mobile.Extensions;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class BookingStatusViewModel : BaseViewModel,
        IMvxServiceConsumer<IBookingService>,
        IMvxServiceConsumer<ILocationService>
    {
        private IBookingService _bookingService;
        private const string _doneStatus = "wosDONE";
        private const string _loadedStatus = "wosLOADED";
        private ILocationService _geolocator;
        private const int _refreshPeriod = 20 ; //20 sec
        private bool _isThankYouDialogDisplayed = false;
        private bool _hasSeenReminder = false;
        protected readonly CompositeDisposable Subscriptions = new CompositeDisposable ();

        public BookingStatusViewModel (string order, string orderStatus)
        {
            Order = JsonSerializer.DeserializeFromString<Order> (order);
            OrderStatusDetail = JsonSerializer.DeserializeFromString<OrderStatusDetail> (orderStatus);      
            _geolocator = this.GetService<ILocationService> ();
            _hasSeenReminder = false;
        }

		public override void Load ()
        {
			base.Load ();
            ShowRatingButton = true;
            MessengerHub.Subscribe<OrderRated> (OnOrderRated, o => o.Content.Equals (Order.Id));
            _bookingService = this.GetService<IBookingService> ();
            StatusInfoText = string.Format (Resources.GetString ("StatusStatusLabel"), Resources.GetString ("LoadingMessage"));

            Pickup = new BookAddressViewModel (() => Order.PickupAddress, address => Order.PickupAddress = address, _geolocator)
            {
                //Title = Resources.GetString("BookPickupLocationButtonTitle"),
                EmptyAddressPlaceholder = Resources.GetString("BookPickupLocationEmptyPlaceholder")
            };
            Dropoff = new BookAddressViewModel (() => Order.DropOffAddress, address => Order.DropOffAddress = address, _geolocator)
            {
                //Title = Resources.GetString("BookDropoffLocationButtonTitle"),
                EmptyAddressPlaceholder = Resources.GetString("BookDropoffLocationEmptyPlaceholder")
            };

            CenterMap (true);
        }

		public override void Start (bool firstStart)
		{
			base.Start (firstStart);

			Observable.Timer ( TimeSpan.FromSeconds ( 2 ), TimeSpan.FromSeconds (_refreshPeriod)).Select (c => new Unit ())
				.Subscribe (unit => InvokeOnMainThread (RefreshStatus))
					.DisposeWith (Subscriptions);
		}

		public override void Stop ()
		{
			base.Stop ();
            Subscriptions.DisposeAll ();
		}

        private IEnumerable<CoordinateViewModel> _mapCenter { get; set; }

        public IEnumerable<CoordinateViewModel> MapCenter {
            get { return _mapCenter; }
            private set {
                _mapCenter = value;
                FirePropertyChanged (() => MapCenter);
            }
        }

        BookAddressViewModel pickup;

        public BookAddressViewModel Pickup {
            get {
                return pickup;
            }
            set {
                pickup = value;
                FirePropertyChanged (() => Pickup); 
            }
        }

        BookAddressViewModel dropoff;

        public BookAddressViewModel Dropoff {
            get {
                return dropoff;
            }
            set {
                dropoff = value;
                FirePropertyChanged (() => Dropoff); 
            }
        }

        public Address PickupModel {
            get { return Pickup.Model; }
            set {
                Pickup.Model = value;
                FirePropertyChanged (() => PickupModel);
            }
        }

        private string _confirmationNoTxt { get; set; }

        public string ConfirmationNoTxt {
            get {
                return _confirmationNoTxt;
            }
            set {
                _confirmationNoTxt = value;
                FirePropertyChanged (() => ConfirmationNoTxt);
            }
        }

        public bool IsCallButtonVisible {
            get { return !bool.Parse (TinyIoCContainer.Current.Resolve<IConfigurationManager> ().GetSetting ("Client.HideCallDispatchButton")); }
            private set{}
        }

        public bool IsCallTaxiVisible
        {
            get { return IsDriverInfoAvailable && OrderStatusDetail.DriverInfos.MobilePhone.HasValue (); }
        }


        public IMvxCommand CallTaxi
        {
            get { return GetCommand(() =>
                                        {
                                            if (!string.IsNullOrEmpty(OrderStatusDetail.DriverInfos.MobilePhone))
                                            {
                                                PhoneService.Call(OrderStatusDetail.DriverInfos.MobilePhone);
                                            }
                                            else
                                            {
                                                MessageService.ShowMessage(Resources.GetString("NoPhoneNumberTitle"), Resources.GetString("NoPhoneNumberMessage"));
                                            }
                                        }); }
        }

        public bool IsDriverInfoAvailable
        {
            get { return ( (OrderStatusDetail.IBSStatusId == "wosASSIGNED" ) || (OrderStatusDetail.IBSStatusId == "wosARRIVED") ) && ( OrderStatusDetail.DriverInfos.VehicleRegistration.HasValue() || OrderStatusDetail.DriverInfos.LastName.HasValue() || OrderStatusDetail.DriverInfos.FirstName.HasValue()); }
        }

        private string _statusInfoText { get; set; }

        public string StatusInfoText {
            get { return _statusInfoText; }
            set {
                _statusInfoText = value;
                FirePropertyChanged (() => StatusInfoText);
            }
        }

        private void OnOrderRated (OrderRated msg)
        {
            IsRated = true;
        }

        public bool IsRated{ get; set; }

        private Order _order;

        public Order Order {
            get { return _order; }
            set {
                _order = value;
                FirePropertyChanged (() => Order);
            }
        }

        private OrderStatusDetail _orderStatusDetail;

        public OrderStatusDetail OrderStatusDetail {
            get { return _orderStatusDetail; }
            set {
                _orderStatusDetail = value;
                FirePropertyChanged (() => OrderStatusDetail);
                FirePropertyChanged (() => IsDriverInfoAvailable);
                FirePropertyChanged (() => IsCallTaxiVisible);

            }
        }

        public bool CloseScreenWhenCompleted {
            get;
            set;
        }


        

        private bool _showRatingButton;

        public bool ShowRatingButton {
            get { 
                if (!TinyIoCContainer.Current.Resolve<IAppSettings> ().RatingEnabled) {
                    return false;
                } else {
                    return _showRatingButton;
                }
            }
            set { 
                _showRatingButton = value;
                FirePropertyChanged (() => ShowRatingButton);
            }
        }

        public BookingStatusViewModel ()
        {
            ShowRatingButton = true;
            SetStatusText (Resources.GetString ("LoadingMessage"));
            if (OrderStatusDetail.IBSOrderId.HasValue) {
                ConfirmationNoTxt = string.Format (Resources.GetString ("StatusDescription"), OrderStatusDetail.IBSOrderId.Value);
            }
        }

        private void HideRatingButton (OrderRated orderRated)
        {
            ShowRatingButton = false;
            ShowThankYouDialog ();
        }

        private void SetStatusText (string message)
        {
            this.StatusInfoText = string.Format (Resources.GetString ("StatusStatusLabel"), message);
        }

        private void AddReminder (OrderStatusDetail status)
        {

            if ( ( status != null )
                && ( status.IBSStatusId != null )
                && status.IBSStatusId.Equals("wosSCHED") 
			    && !_hasSeenReminder
				&& this.PhoneService.CanUseCalendarAPI())
            {
                this._hasSeenReminder = true;
                InvokeOnMainThread (() => 
                {
                    MessageService.ShowMessage (Resources.GetString ("AddReminderTitle"), Resources.GetString ("AddReminderMessage"), Resources.GetString ("YesButton"), () => 
                    {
                        var applicationName = TinyIoC.TinyIoCContainer.Current.Resolve<IAppSettings>().ApplicationName;
                        this.PhoneService.AddEventToCalendarAndReminder(string.Format(Resources.GetString("ReminderTitle"),applicationName), 
                                                                        string.Format(Resources.GetString("ReminderDetails"),Order.PickupAddress.FullAddress, FormatTime (Order.PickupDate),FormatDate(Order.PickupDate)), 
                                                                        Order.PickupAddress.FullAddress, 
                                                                        Order.PickupDate, 
                                                                        Order.PickupDate.AddMinutes(-15));
                    }, Resources.GetString("NoButton"), () => 
                    {
                    });
                });
            }
        }

        private string FormatTime(DateTime pickupDate )
        {
            var formatTime = new CultureInfo( CultureInfoString ).DateTimeFormat.ShortTimePattern;
            string format = "{0:"+formatTime+"}";
            return string.Format(format, pickupDate);
        }

        private string FormatDate(DateTime pickupDate )
        {
            return pickupDate.Date.ToLongDateString();
        }

        
        public string CultureInfoString
        {
            get{
                var culture = TinyIoCContainer.Current.Resolve<IConfigurationManager>().GetSetting ( "PriceFormat" );
                if ( culture.IsNullOrEmpty() )
                {
                    return "en-US";
                }
                else
                {
                    return culture;                
                }
            }
        }

        private void RefreshStatus ()
        {

            try {
                var status = TinyIoCContainer.Current.Resolve<IBookingService> ().GetOrderStatus (Order.Id);
                var isDone = TinyIoCContainer.Current.Resolve<IBookingService> ().IsStatusDone (status.IBSStatusId);
#if DEBUG
				//isDone = true;
#endif
                AddReminder(status);

                if (status != null) {
                    StatusInfoText = status.IBSStatusDescription;                        
                    this.OrderStatusDetail = status;


                    CenterMap (true);
                    if (OrderStatusDetail.IBSOrderId.HasValue) {
                        ConfirmationNoTxt = string.Format (Resources.GetString ("StatusDescription"), OrderStatusDetail.IBSOrderId.Value);
                    }
                    if (isDone) {
                        if (!_isThankYouDialogDisplayed) {
                            _isThankYouDialogDisplayed = true;
                            InvokeOnMainThread (ShowThankYouDialog);
                        }
                    }
                }
            } catch (Exception ex) {
                TinyIoCContainer.Current.Resolve<ILogger> ().LogError (ex);
            }
        }

        private void ShowThankYouDialog ()
        {
            string stringNeutral = null;
            Action actionNeutral = null;
            TinyMessageSubscriptionToken orderRatedToken = null;
            if (ShowRatingButton) {
                stringNeutral = Resources.GetString ("RateBtn");
                actionNeutral = () =>
                {
                    if ((Common.Extensions.GuidExtensions.HasValue (Order.Id))) {
                        Order.Id = Order.Id;
                        orderRatedToken = TinyIoCContainer.Current.Resolve<ITinyMessengerHub> ()
                                                            .Subscribe<OrderRated> (HideRatingButton);
                        NavigateToRatingPage.Execute ();
                    }
                };
            }
            var settings = TinyIoCContainer.Current.Resolve<IAppSettings> ();
            MessageService.ShowMessage (Resources.GetString ("View_BookingStatus_ThankYouTitle"),
                String.Format (Resources.GetString ("View_BookingStatus_ThankYouMessage"), settings.ApplicationName),
                Resources.GetString ("ReturnBookingScreen"), () =>
            {
                if (orderRatedToken != null) {
                    TinyIoCContainer.Current.Resolve<ITinyMessengerHub> ()
                                                            .Unsubscribe<OrderRated> (orderRatedToken);
                }
                this.Close ();
            },
                stringNeutral, actionNeutral
            );
        }

        private void CenterMap (bool changeZoom)
        {
            
            if (OrderStatusDetail.VehicleLatitude.HasValue && OrderStatusDetail.VehicleLongitude.HasValue) {
                MapCenter = new CoordinateViewModel[] {
                    new CoordinateViewModel { Coordinate = new Coordinate { Latitude = Pickup.Model.Latitude, Longitude = Pickup.Model.Longitude }, Zoom = changeZoom ? ZoomLevel.Close : ZoomLevel.DontChange } , 
                    new CoordinateViewModel { Coordinate = new Coordinate { Latitude = OrderStatusDetail.VehicleLatitude.GetValueOrDefault(), Longitude = OrderStatusDetail.VehicleLongitude.GetValueOrDefault() }, Zoom = ZoomLevel.DontChange }
                };
            } else {
                MapCenter = new CoordinateViewModel[] { new CoordinateViewModel { Coordinate = new Coordinate { Latitude = Pickup.Model.Latitude, Longitude = Pickup.Model.Longitude }, Zoom = changeZoom ? ZoomLevel.Close : ZoomLevel.DontChange } };
            }
        }

        public IMvxCommand NavigateToRatingPage {
            get {
                return GetCommand (() =>
                {
                    RequestNavigate<BookRatingViewModel> (new { orderId = Order.Id.ToString (), canRate = true.ToString (CultureInfo.InvariantCulture), isFromStatus = true.ToString (CultureInfo.InvariantCulture) });
                });
            }
        }

        public IMvxCommand NewRide {
            get {
                return GetCommand (() =>
                {

                    MessageService.ShowMessage (Resources.GetString ("StatusNewRideButton"), Resources.GetString ("StatusConfirmNewBooking"), Resources.GetString ("YesButton"), () =>
                    {
                        _bookingService.ClearLastOrder ();
                        RequestNavigate<BookViewModel> (clearTop: true);
                    },
                    Resources.GetString ("NoButton"), () => { });   
                                        
                });
            }
        }

        public IMvxCommand CancelOrder {
            get {
                return GetCommand (() =>
                {
                    if ((OrderStatusDetail.IBSStatusId == _doneStatus) || (OrderStatusDetail.IBSStatusId == _loadedStatus)) {
                        MessageService.ShowMessage (Resources.GetString ("CannotCancelOrderTitle"), Resources.GetString ("CannotCancelOrderMessage"));
                        return;
                    }

                    MessageService.ShowMessage ("", Resources.GetString ("StatusConfirmCancelRide"), Resources.GetString ("YesButton"), ()=>
                    {
                        Task.Factory.SafeStartNew ( () =>
                                                   {
                        try 
                        {

                            MessageService.ShowProgress (true);

                            var isSuccess = TinyIoCContainer.Current.Resolve<IBookingService> ().CancelOrder (Order.Id);      
                            if (isSuccess) 
                            {
                                MessengerHub.Publish (new OrderCanceled (this, Order, null));
                                TinyIoCContainer.Current.Resolve<ICacheService>().Clear("LastOrderId");
                                RequestNavigate<BookViewModel> (clearTop: true);
                            } 
                            else 
                            {
                                MessageService.ShowMessage (Resources.GetString ("StatusConfirmCancelRideErrorTitle"), Resources.GetString ("StatusConfirmCancelRideError"));
                            }
                        } 
                        finally 
                        {
                            MessageService.ShowProgress (false);
                        }     
                        });
                    },Resources.GetString ("NoButton"), () => { });
                });
            }
        }

        public IMvxCommand CallCompany {
            get {
                return GetCommand (() =>
                {
                    Action call = () => {
                        PhoneService.Call (Settings.PhoneNumber (Order.Settings.ProviderId.Value)); };
                    MessageService.ShowMessage (string.Empty, 
                                               Settings.PhoneNumberDisplay (Order.Settings.ProviderId.Value), 
                                               Resources.GetString ("CallButton"), 
                                               call, Resources.GetString ("CancelBoutton"), 
                                               () => {});                    
                });
            }
        }
    }
}
