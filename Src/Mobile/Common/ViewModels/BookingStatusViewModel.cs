using System;
using System.Collections.Generic;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using ServiceStack.Text;
using apcurium.MK.Booking.Api.Contract.Resources;
using System.Reactive.Linq;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using System.Reactive.Disposables;
using apcurium.MK.Booking.Mobile.Extensions;
using System.Threading.Tasks;
using apcurium.MK.Common;
using System.Threading;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class BookingStatusViewModel : BaseViewModel
    {
		private int _refreshPeriod = 5; //in seconds
	    private bool _waitingToNavigateAfterTimeOut;

		public BookingStatusViewModel (string order, string orderStatus)
		{
			Order = JsonSerializer.DeserializeFromString<Order> (order);
			OrderStatusDetail = JsonSerializer.DeserializeFromString<OrderStatusDetail> (orderStatus);      
            IsCancelButtonVisible = true;			
            _waitingToNavigateAfterTimeOut = false;
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

		public override void Start (bool firstStart = false)
		{
			base.Start (firstStart);

            var periodInSettings = this.Services().Config.GetSetting("Client.OrderStatus.ClientPollingInterval");
            int periodInSettingsValue;
            if(int.TryParse(periodInSettings, out periodInSettingsValue))
            {
                _refreshPeriod = periodInSettingsValue;
            }

            Task.Factory.StartNew (() =>
            {
                Thread.Sleep( 1000 );     
                InvokeOnMainThread(RefreshStatus);
            });

			Observable.Interval( TimeSpan.FromSeconds (_refreshPeriod))
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
		
		BookAddressViewModel _dropoffViewModel;
		public BookAddressViewModel Dropoff {
			get {
				return _dropoffViewModel;
			}
			set {
				_dropoffViewModel = value;
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
                var showCallDriver = this.Services().Config.GetSetting("Client.ShowCallDriver", false);
                return showCallDriver && IsDriverInfoAvailable && OrderStatusDetail.DriverInfos.MobilePhone.HasValue (); }
        }

        public bool IsDriverInfoAvailable
        {
            get {
                var showVehicleInformation = this.Services().Config.GetSetting("Client.ShowVehicleInformation", true);

				return showVehicleInformation && ( (OrderStatusDetail.IbsStatusId == VehicleStatuses.Common.Assigned) || (OrderStatusDetail.IbsStatusId == VehicleStatuses.Common.Arrived) ) 
                && ( OrderStatusDetail.DriverInfos.VehicleRegistration.HasValue() || OrderStatusDetail.DriverInfos.LastName.HasValue() || OrderStatusDetail.DriverInfos.FirstName.HasValue()); }
        }
		
		public bool IsCallButtonVisible {
            get { return !bool.Parse(this.Services().Config.GetSetting("Client.HideCallDispatchButton")); }
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

	    private string _statusInfoText;
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

        public AsyncCommand CallTaxi
        {
            get { 
				return GetCommand(() =>
                {
                    if (!string.IsNullOrEmpty(OrderStatusDetail.DriverInfos.MobilePhone))
                    {
                        this.Services().Message.ShowMessage(string.Empty, 
						                            OrderStatusDetail.DriverInfos.MobilePhone, 
						                            Str.CallButtonText,
                                                    () => this.Services().Phone.Call(OrderStatusDetail.DriverInfos.MobilePhone),
						                            Str.CancelButtonText, 
						                            () => {});   
                    }
                    else
                    {
                        this.Services().Message.ShowMessage(this.Services().Resources.GetString("NoPhoneNumberTitle"), this.Services().Resources.GetString("NoPhoneNumberMessage"));
                    }
                }); 
			}
        }

		#endregion


        private bool HasSeenReminderPrompt( Guid orderId )
        {
            var hasSeen = this.Services().Cache.Get<string>("OrderReminderWasSeen." + orderId.ToString());
            return !string.IsNullOrEmpty(hasSeen);
        }

        private void SetHasSeenReminderPrompt( Guid orderId )
        {
            this.Services().Cache.Set("OrderReminderWasSeen." + orderId.ToString(), true.ToString());                     
        }

	private bool IsCmtRideLinq { get { return ConfigurationManager.GetPaymentSettings().PaymentMode == PaymentMethod.RideLinqCmt; } }
        private void AddReminder (OrderStatusDetail status)
        {
            if (!HasSeenReminderPrompt(status.OrderId )
                && this.Services().Phone.CanUseCalendarAPI())
            {
                SetHasSeenReminderPrompt(status.OrderId);
                InvokeOnMainThread(() => this.Services().Message.ShowMessage(Str.AddReminderTitle, Str.AddReminderMessage, Str.YesButtonText, () => this.Services().Phone.AddEventToCalendarAndReminder(Str.ReminderTitle, 
                    Str.GetReminderDetails(Order.PickupAddress.FullAddress, Order.PickupDate),						              									 
                    Order.PickupAddress.FullAddress, 
                    Order.PickupDate, 
                    Order.PickupDate.AddHours(-2)), Str.NoButtonText, () => { }));
            }
        }
		        
		string _vehicleNumber;
        private void RefreshStatus ()
        {
            try {
                var status = this.Services().Booking.GetOrderStatus(Order.Id);
				if(status.VehicleNumber != null)
				{
					_vehicleNumber = status.VehicleNumber;
				}
				else{
					status.VehicleNumber = _vehicleNumber;
				}

                var isDone = this.Services().Booking.IsStatusDone(status.IbsStatusId);

				if(status.IbsStatusId.HasValue() && status.IbsStatusId.Equals(VehicleStatuses.Common.Scheduled) )
				{
					AddReminder(status);
				}

#if DEBUG
                //status.IBSStatusId = VehicleStatuses.Common.Arrived;
#endif
                IsPayButtonVisible = false;
                StatusInfoText = status.IbsStatusDescription;                        
                OrderStatusDetail = status;

                CenterMap ();

					if(IsCmtRideLinq && AccountService.CurrentAccount.DefaultCreditCard != null)
					{
						var isLoaded = status.IBSStatusId.Equals(VehicleStatuses.Common.Loaded) || status.IBSStatusId.Equals(VehicleStatuses.Common.Done);
						var isPaired = BookingService.IsPaired(Order.Id);
						var pairState = CacheService.Get<string>("CmtRideLinqPairState" + Order.Id.ToString());
						var isPairBypass = (pairState == CmtRideLinqPairingState.Failed) || (pairState == CmtRideLinqPairingState.Canceled) || (pairState == CmtRideLinqPairingState.Unpaired);
						if (isLoaded && !isPaired && !IsCurrentlyPairing && !isPairBypass)
						{
							IsCurrentlyPairing = true;
							GoToCmtPairScreen();
							return;
						}
					}

                if (OrderStatusDetail.IbsOrderId.HasValue) {
                    ConfirmationNoTxt = Str.GetStatusDescription(OrderStatusDetail.IbsOrderId.Value+"");
                }

                if (isDone) 
                {
                    GoToSummary();
                }

                if (this.Services().Booking.IsStatusTimedOut(status.IbsStatusId))
                {
                    GoToBookingScreen();
                }
            } catch (Exception ex) {
                Logger.LogError (ex);
            }
        }

		void UpdatePayCancelButtons (string statusId)
		{
            var setting = this.Services().Config.GetPaymentSettings();
            var isPayEnabled = setting.IsPayInTaxiEnabled || setting.PayPalClientSettings.IsEnabled;

			var isPaired = BookingService.IsPaired(Order.Id);
			IsUnpairButtonVisible = IsCmtRideLinq && isPaired;
			IsPayButtonVisible =  (statusId == VehicleStatuses.Common.Done
								||statusId == VehicleStatuses.Common.Loaded)
                                && (isPayEnabled && !PaymentService.GetPaymentFromCache(Order.Id).HasValue);
			
            IsCancelButtonVisible = statusId == null ||
                                    statusId == VehicleStatuses.Common.Assigned 
                                || statusId == VehicleStatuses.Common.Waiting 
                                || statusId == VehicleStatuses.Common.Arrived
                                || statusId == VehicleStatuses.Common.Scheduled;

			IsResendButtonVisible = isPayEnabled && !IsCmtRideLinq && PaymentService.GetPaymentFromCache(Order.Id).HasValue;
		}

		public void GoToSummary(){

			RequestNavigate<RideSummaryViewModel> (new {
				order = Order.ToJson(),
				orderStatus = OrderStatusDetail.ToJson()
			}.ToStringDictionary());
		}

        public void GoToBookingScreen(){
            if (!_waitingToNavigateAfterTimeOut)
            {
				Observable.Interval( TimeSpan.FromSeconds (10))
				.Subscribe(unit => InvokeOnMainThread(() =>
				{
			this.Services().Booking.ClearLastOrder();
                        _waitingToNavigateAfterTimeOut = true;
                        RequestNavigate<BookViewModel>(clearTop: true);
                        RequestClose(this);
                    }));
            }
        }

        private void CenterMap ()
        {            
			var pickup = CoordinateViewModel.Create(Pickup.Model.Latitude, Pickup.Model.Longitude, true);
			if (OrderStatusDetail.IbsStatusId != VehicleStatuses.Common.Waiting && OrderStatusDetail.VehicleLatitude.HasValue && OrderStatusDetail.VehicleLongitude.HasValue) 
			{
                MapCenter = new[] 
				{ 
					pickup,
					CoordinateViewModel.Create(OrderStatusDetail.VehicleLatitude.Value, OrderStatusDetail.VehicleLongitude.Value)                   
                };
            } else {
                MapCenter = new[] { pickup };
            }
        }

		#region Commands

        public AsyncCommand NewRide
        {
            get {
                return GetCommand(() => this.Services().Message.ShowMessage(Str.StatusNewRideButtonText, Str.StatusConfirmNewBooking, Str.YesButtonText, () =>
                {
                    this.Services().Booking.ClearLastOrder();
                    RequestNavigate<BookViewModel> (clearTop: true);
                },
                    Str.NoButtonText, NoAction));
            }
        }

        public AsyncCommand CancelOrder
        {
            get {
                return GetCommand (() =>
                {
						if ((OrderStatusDetail.IbsStatusId == VehicleStatuses.Common.Done) || (OrderStatusDetail.IbsStatusId == VehicleStatuses.Common.Loaded)) {
                            this.Services().Message.ShowMessage(Str.CannotCancelOrderTitle, Str.CannotCancelOrderMessage);
                        return;
                    }

                        this.Services().Message.ShowMessage("", Str.StatusConfirmCancelRide, Str.YesButtonText, () => Task.Factory.SafeStartNew(() =>
                    {
                        try 
                        {
                            this.Services().Message.ShowProgress(true);

                            var isSuccess = this.Services().Booking.CancelOrder(Order.Id);      
                            if (isSuccess) 
                            {
                                this.Services().Booking.ClearLastOrder();
                                RequestNavigate<BookViewModel> (clearTop: true);
                            } 
                            else 
                            {
                                this.Services().Message.ShowMessage(Str.StatusConfirmCancelRideErrorTitle, Str.StatusConfirmCancelRideError);
                            }
                        } 
                        finally 
                        {
                            this.Services().Message.ShowProgress(false);
                        }     
                    }),Str.NoButtonText, () => { });
                });
            }
        }

        public AsyncCommand PayForOrderCommand 
		{
			get {
				return GetCommand (() => RequestNavigate<ConfirmCarNumberViewModel>(
					if(string.IsNullOrWhiteSpace(OrderStatusDetail.VehicleNumber)){
						MessageService.ShowMessage(Resources.GetString("VehicleNumberErrorTitle"), Resources.GetString("VehicleNumberErrorMessage"));
						return;
					}
					if(IsCmtRideLinq)
					{
						GoToCmtPairScreen();
					}
					else
					{
						RequestNavigate<ConfirmCarNumberViewModel>(
							new
							{
								order = Order.ToJson(),
								orderStatus = OrderStatusDetail.ToJson()
							}, false, MvxRequestedBy.UserAction);
					}
			}
		}

        public AsyncCommand CallCompany
        {
            get {
                return GetCommand (() =>
                {
                    this.Services().Message.ShowMessage(string.Empty,
                                                this.Services().Config.GetSetting("DefaultPhoneNumberDisplay"),
                                               Str.CallButtonText,
                                                () => this.Services().Phone.Call(this.Services().Config.GetSetting("DefaultPhoneNumber")),
						                       Str.CancelButtonText, 
                                               () => {});                    
                });
            }
        }

        public AsyncCommand ResendConfirmationToDriver
        {
            get {
                return GetCommand (() =>
                                   {
                     if (this.Services().Payment.GetPaymentFromCache(Order.Id).HasValue)
                    {
						CacheService.Set("CmtRideLinqPairState" + Order.Id.ToString(), CmtRideLinqPairingState.Unpaired);
						PaymentService.Unpair(Order.Id);
                    }
                });
            }
        }
		#endregion
    }
}
