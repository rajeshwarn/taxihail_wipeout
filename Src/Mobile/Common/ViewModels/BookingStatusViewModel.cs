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
using apcurium.MK.Common;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using apcurium.MK.Common.Configuration.Impl;
using System.Threading;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class BookingStatusViewModel : BaseViewModel, IMvxServiceConsumer<IBookingService>
    {
		private readonly IBookingService _bookingService;
        private int _refreshPeriod = 5; //in seconds
        private bool _hasSeenReminder = false;
        private bool _waitingToNavigateAfterTimeOut = false;

		public BookingStatusViewModel (string order, string orderStatus)
		{
			Order = JsonSerializer.DeserializeFromString<Order> (order);
			OrderStatusDetail = JsonSerializer.DeserializeFromString<OrderStatusDetail> (orderStatus);      
            IsCancelButtonVisible = true;
			_hasSeenReminder = false;
            _waitingToNavigateAfterTimeOut = false;
			_bookingService = this.GetService<IBookingService>();
		}
	
		public override void Load ()
        {
			base.Load ();

			StatusInfoText = Str.GetStatusInfoText(Str.LoadingMessage);

            Pickup = new BookAddressViewModel (() => Order.PickupAddress, address => Order.PickupAddress = address)
            {
				EmptyAddressPlaceholder = Str.BookPickupLocationEmptyPlaceholder
            };

            Dropoff = new BookAddressViewModel (() => Order.DropOffAddress, address => Order.DropOffAddress = address)
            {
				EmptyAddressPlaceholder = Str.BookPickupLocationEmptyPlaceholder
            };

            CenterMap ();
        }

		public override void Start (bool firstStart)
		{
			base.Start (firstStart);

            var periodInSettings = TinyIoCContainer.Current.Resolve<IConfigurationManager> ().GetSetting ("Client.OrderStatus.ClientPollingInterval");
            int periodInSettingsValue;
            if(int.TryParse(periodInSettings, out periodInSettingsValue))
            {
                _refreshPeriod = periodInSettingsValue;
            }

            Task.Factory.StartNew (() =>
            {
                Thread.Sleep( 1000 );     
                InvokeOnMainThread(() => RefreshStatus());
            });

#if MONOTOUCH
			Observable.IntervalSafe( TimeSpan.FromSeconds (_refreshPeriod))
#else
			Observable.Interval( TimeSpan.FromSeconds (_refreshPeriod))
#endif
				.Subscribe (unit => InvokeOnMainThread (RefreshStatus))
				.DisposeWith (Subscriptions);

		}
		
		protected readonly CompositeDisposable Subscriptions = new CompositeDisposable ();
		public override void Stop ()
		{
			base.Stop ();
            Subscriptions.DisposeAll ();
		}

		#region Bindings
		private IEnumerable<CoordinateViewModel> _mapCenter;
		public IEnumerable<CoordinateViewModel> MapCenter {
			get { return _mapCenter; }
			private set {
				_mapCenter = value;
				FirePropertyChanged (() => MapCenter);
			}
		}
		
		BookAddressViewModel _pickupViewModel;
		public BookAddressViewModel Pickup {
			get {
				return _pickupViewModel;
			}
			set {
				_pickupViewModel = value;
				FirePropertyChanged (() => Pickup); 
			}
		}
		
		BookAddressViewModel dropoffViewModel;
		public BookAddressViewModel Dropoff {
			get {
				return dropoffViewModel;
			}
			set {
				dropoffViewModel = value;
				FirePropertyChanged (() => Dropoff); 
			}
		}

		bool _isPayButtonVisible;
		public bool IsPayButtonVisible {
			get{
				return _isPayButtonVisible;
			} set{
				_isPayButtonVisible = value;
				FirePropertyChanged (() => IsPayButtonVisible); 
			}
		}

		bool _isCancelButtonVisible;
		public bool IsCancelButtonVisible{
			get{
				return _isCancelButtonVisible;
			} set{
				_isCancelButtonVisible = value;
				FirePropertyChanged (() => IsCancelButtonVisible); 
			}
		}
		
        bool _isResendButtonVisible;
        public bool IsResendButtonVisible{
            get{
                return _isResendButtonVisible;
            } set{
                _isResendButtonVisible = value;
                FirePropertyChanged (() => IsResendButtonVisible); 
            }
        }

        private string _confirmationNoTxt;
		public string ConfirmationNoTxt {
			get {
				return _confirmationNoTxt;
			}
			set {
				_confirmationNoTxt = value;
				FirePropertyChanged (() => ConfirmationNoTxt);
			}
		}
        public bool IsCallTaxiVisible
        {
            get { 
                    var showCallDriver = Config.GetSetting<bool>("Client.ShowCallDriver", false);
                return showCallDriver && IsDriverInfoAvailable && OrderStatusDetail.DriverInfos.MobilePhone.HasValue (); }
        }

        public bool IsDriverInfoAvailable
        {
            get { 
                var showVehicleInformation = Config.GetSetting<bool>("Client.ShowVehicleInformation", true);

                return showVehicleInformation && ( (OrderStatusDetail.IBSStatusId == VehicleStatuses.Common.Assigned) || (OrderStatusDetail.IBSStatusId == VehicleStatuses.Common.Arrived) ) 
                && ( OrderStatusDetail.DriverInfos.VehicleRegistration.HasValue() || OrderStatusDetail.DriverInfos.LastName.HasValue() || OrderStatusDetail.DriverInfos.FirstName.HasValue()); }
        }
		
		public bool IsCallButtonVisible {
            get { return !bool.Parse (Config.GetSetting ("Client.HideCallDispatchButton")); }
		}

		public bool VehicleDriverHidden
		{
			get { return string.IsNullOrWhiteSpace(OrderStatusDetail.DriverInfos.FullName) || !IsDriverInfoAvailable; }
		}
		public bool VehicleLicenceHidden
		{
			get { return string.IsNullOrWhiteSpace(OrderStatusDetail.DriverInfos.VehicleRegistration) || !IsDriverInfoAvailable; }
		}
		public bool VehicleTypeHidden
		{
			get { return string.IsNullOrWhiteSpace(OrderStatusDetail.DriverInfos.VehicleType) || !IsDriverInfoAvailable; }
		}
		public bool VehicleMakeHidden
		{
			get { return string.IsNullOrWhiteSpace(OrderStatusDetail.DriverInfos.VehicleMake) || !IsDriverInfoAvailable; }
		}
		public bool VehicleModelHidden
		{
			get { return string.IsNullOrWhiteSpace(OrderStatusDetail.DriverInfos.VehicleModel) || !IsDriverInfoAvailable; }
		}
		public bool VehicleColorHidden
		{
			get { return string.IsNullOrWhiteSpace(OrderStatusDetail.DriverInfos.VehicleColor) || !IsDriverInfoAvailable; }
		}

		private string _statusInfoText { get; set; }
		public string StatusInfoText {
			get { return _statusInfoText; }
			set {
				_statusInfoText = value;
				FirePropertyChanged (() => StatusInfoText);
			}
		}

		public Address PickupModel {
			get { return Pickup.Model; }
			set {
				Pickup.Model = value;
				FirePropertyChanged (() => PickupModel);
			}
		}

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
				FirePropertyChanged (() => VehicleDriverHidden);
				FirePropertyChanged (() => VehicleLicenceHidden);
				FirePropertyChanged (() => VehicleTypeHidden);
				FirePropertyChanged (() => VehicleMakeHidden);
				FirePropertyChanged (() => VehicleModelHidden);
				FirePropertyChanged (() => VehicleColorHidden);
                FirePropertyChanged (() => IsDriverInfoAvailable);
                FirePropertyChanged (() => IsCallTaxiVisible);
			}
		}

        public IMvxCommand CallTaxi
        {
            get { 
				return GetCommand(() =>
                {
                    if (!string.IsNullOrEmpty(OrderStatusDetail.DriverInfos.MobilePhone))
                    {
						MessageService.ShowMessage (string.Empty, 
						                            OrderStatusDetail.DriverInfos.MobilePhone, 
						                            Str.CallButtonText, 
						                            () => PhoneService.Call (OrderStatusDetail.DriverInfos.MobilePhone),
						                            Str.CancelButtonText, 
						                            () => {});   
                    }
                    else
                    {
                        MessageService.ShowMessage(Resources.GetString("NoPhoneNumberTitle"), Resources.GetString("NoPhoneNumberMessage"));
                    }
                }); 
			}
        }

		#endregion

        private void AddReminder (OrderStatusDetail status)
        {
			if (!_hasSeenReminder
				&& this.PhoneService.CanUseCalendarAPI())
            {
                this._hasSeenReminder = true;
                InvokeOnMainThread (() => 
                {
					MessageService.ShowMessage (Str.AddReminderTitle, Str.AddReminderMessage, Str.YesButtonText, () => 
                    {
						this.PhoneService.AddEventToCalendarAndReminder(Str.ReminderTitle, 
						                                                Str.GetReminderDetails(Order.PickupAddress.FullAddress, Order.PickupDate),						              									 
                                                                        Order.PickupAddress.FullAddress, 
                                                                        Order.PickupDate, 
                                                                        Order.PickupDate.AddHours(-2));
                    }, Str.NoButtonText, () => { });
                });
            }
        }
		        
		string vehicleNumber = null;
        private void RefreshStatus ()
        {
            try {
                var status = BookingService.GetOrderStatus (Order.Id);
				if(status.VehicleNumber != null)
				{
					vehicleNumber = status.VehicleNumber;
				}
				else{
					status.VehicleNumber = vehicleNumber;
				}

				var isDone = BookingService.IsStatusDone (status.IBSStatusId);

                if(status.IBSStatusId.HasValue() && status.IBSStatusId.Equals(VehicleStatuses.Common.Scheduled) )
				{
					AddReminder(status);
				}

#if DEBUG
                //status.IBSStatusId = VehicleStatuses.Common.Arrived;
#endif
                IsPayButtonVisible = false;
                if (status != null) {
                    StatusInfoText = status.IBSStatusDescription;                        
                    this.OrderStatusDetail = status;

                    CenterMap ();

					UpdatePayCancelButtons(status.IBSStatusId);

                    if (OrderStatusDetail.IBSOrderId.HasValue) {
						ConfirmationNoTxt = Str.GetStatusDescription(OrderStatusDetail.IBSOrderId.Value+"");
                    }

                    if (isDone) 
					{
						GoToSummary();
                    }

                    if (BookingService.IsStatusTimedOut (status.IBSStatusId))
                    {
                        GoToBookingScreen();
                    }
                }
            } catch (Exception ex) {
                Logger.LogError (ex);
            }
        }

		void UpdatePayCancelButtons (string statusId)
		{
            var setting = ConfigurationManager.GetPaymentSettings ();
            var isPayEnabled = setting.IsPayInTaxiEnabled || setting.PayPalClientSettings.IsEnabled;

			IsPayButtonVisible =  (statusId == VehicleStatuses.Common.Done
								||statusId == VehicleStatuses.Common.Loaded) 
                                && (isPayEnabled && !PaymentService.GetPaymentFromCache(Order.Id).HasValue);
			
            IsCancelButtonVisible = statusId == VehicleStatuses.Common.Assigned 
                                || statusId == VehicleStatuses.Common.Waiting 
                                || statusId == VehicleStatuses.Common.Arrived;

            IsResendButtonVisible = isPayEnabled && PaymentService.GetPaymentFromCache(Order.Id).HasValue;
		}

		public void GoToSummary(){

			RequestNavigate<RideSummaryViewModel> (new {
				order = Order.ToJson(),
				orderStatus = OrderStatusDetail.ToJson()
			}.ToStringDictionary());
			RequestClose (this);
		}

        public void GoToBookingScreen(){
            if (!_waitingToNavigateAfterTimeOut)
            {
#if MONOTOUCH
                Observable.IntervalSafe(TimeSpan.FromSeconds(10))
#else
                Observable.Interval( TimeSpan.FromSeconds (10))
#endif
                    .Subscribe(unit => InvokeOnMainThread(() => {
                        _bookingService.ClearLastOrder();
                        _waitingToNavigateAfterTimeOut = true;
                        RequestNavigate<BookViewModel>(clearTop: true);
                        RequestClose(this);
                    }));
            }
        }

        private void CenterMap ()
        {            
			var pickup = CoordinateViewModel.Create(Pickup.Model.Latitude, Pickup.Model.Longitude, true);
            if (OrderStatusDetail.IBSStatusId != VehicleStatuses.Common.Waiting && OrderStatusDetail.VehicleLatitude.HasValue && OrderStatusDetail.VehicleLongitude.HasValue) 
			{
                MapCenter = new CoordinateViewModel[] 
				{ 
					pickup,
					CoordinateViewModel.Create(OrderStatusDetail.VehicleLatitude.Value, OrderStatusDetail.VehicleLongitude.Value)                   
                };
            } else {
                MapCenter = new CoordinateViewModel[] { pickup };
            }
        }

		#region Commands

        public IMvxCommand NewRide {
            get {
                return GetCommand (() =>
                {
                    MessageService.ShowMessage (Str.StatusNewRideButtonText, Str.StatusConfirmNewBooking, Str.YesButtonText, () =>
                    {
                        BookingService.ClearLastOrder ();
                        RequestNavigate<BookViewModel> (clearTop: true);
                    },
                    Str.NoButtonText, NoAction);                    
                });
            }
        }

        public IMvxCommand CancelOrder {
            get {
                return GetCommand (() =>
                {
					if ((OrderStatusDetail.IBSStatusId == VehicleStatuses.Common.Done) || (OrderStatusDetail.IBSStatusId == VehicleStatuses.Common.Loaded)) {
                        MessageService.ShowMessage (Str.CannotCancelOrderTitle, Str.CannotCancelOrderMessage);
                        return;
                    }

                    MessageService.ShowMessage ("", Str.StatusConfirmCancelRide, Str.YesButtonText, ()=>
                    {
                        Task.Factory.SafeStartNew ( () =>
                        {
	                        try 
	                        {
	                            MessageService.ShowProgress (true);

	                            var isSuccess = BookingService.CancelOrder (Order.Id);      
	                            if (isSuccess) 
	                            {
									_bookingService.ClearLastOrder();
	                                RequestNavigate<BookViewModel> (clearTop: true);
	                            } 
	                            else 
	                            {
	                                MessageService.ShowMessage (Str.StatusConfirmCancelRideErrorTitle, Str.StatusConfirmCancelRideError);
	                            }
	                        } 
	                        finally 
	                        {
	                            MessageService.ShowProgress (false);
	                        }     
                        });
                    },Str.NoButtonText, () => { });
                });
            }
        }

		public IMvxCommand PayForOrderCommand 
		{
			get {
				return GetCommand (() =>
					{ 
#if DEBUG
#else
                        if(string.IsNullOrWhiteSpace(OrderStatusDetail.VehicleNumber)){
                            MessageService.ShowMessage(Resources.GetString("VehicleNumberErrorTitle"), Resources.GetString("VehicleNumberErrorMessage"));
                            return;
                       }
#endif

						RequestNavigate<ConfirmCarNumberViewModel>(
						new 
						{ 
							order = Order.ToJson(),
							orderStatus = OrderStatusDetail.ToJson()
						}, false, MvxRequestedBy.UserAction);
					});
			}
		}

        public IMvxCommand CallCompany {
            get {
                return GetCommand (() =>
                {                    
                    MessageService.ShowMessage (string.Empty, 
                                                Config.GetSetting( "DefaultPhoneNumberDisplay" ),
                                               Str.CallButtonText, 
                                                () => PhoneService.Call ( Config.GetSetting( "DefaultPhoneNumber" )),
						                       Str.CancelButtonText, 
                                               () => {});                    
                });
            }
        }

       public IMvxCommand ResendConfirmationToDriver {
            get {
                return GetCommand (() =>
                                   {
                    if( PaymentService.GetPaymentFromCache(Order.Id).HasValue )
                    {
                        var formattedAmount = CultureProvider.FormatCurrency(PaymentService.GetPaymentFromCache(Order.Id).Value ); 
                        VehicleClient.SendMessageToDriver(OrderStatusDetail.VehicleNumber, Str.GetPaymentConfirmationMessageToDriver(formattedAmount));
                    }
                });
            }
        }
		#endregion
    }
}
