using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Linq;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Booking.Mobile.PresentationHints;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using ServiceStack.Text;
using apcurium.MK.Booking.Maps;
using System.Net;
using ServiceStack.ServiceClient.Web;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class BookingStatusViewModel : PageViewModel
    {
		private readonly IOrderWorkflowService _orderWorkflowService;
		private readonly IPhoneService _phoneService;
		private readonly IBookingService _bookingService;
		private readonly IPaymentService _paymentService;
		private readonly IAccountService _accountService;
		private readonly IVehicleService _vehicleService;

	    public BookingStatusViewModel(IOrderWorkflowService orderWorkflowService,
			IPhoneService phoneService,
			IBookingService bookingService,
			IPaymentService paymentService,
			IAccountService accountService,
			IVehicleService vehicleService
		)
		{
			_orderWorkflowService = orderWorkflowService;
			_phoneService = phoneService;
			_bookingService = bookingService;
			_paymentService = paymentService;
			_accountService = accountService;
			_vehicleService = vehicleService;
		}

		private int _refreshPeriod = 5; //in seconds
		private bool _waitingToNavigateAfterTimeOut;

		public void Init(string order, string orderStatus)
		{
			Order = JsonSerializer.DeserializeFromString<Order> (order);
			OrderStatusDetail = JsonSerializer.DeserializeFromString<OrderStatusDetail> (orderStatus);
            DisplayOrderNumber();
			IsCancelButtonVisible = true;			
			_waitingToNavigateAfterTimeOut = false;
		}
	
		public override void OnViewLoaded ()
        {
			base.OnViewLoaded ();

			StatusInfoText = string.Format(this.Services().Localize["Processing"]);

            CenterMap ();			
        }

		public override void OnViewStarted (bool firstStart = false)
		{
			base.OnViewStarted (firstStart);

			_refreshPeriod = Settings.OrderStatus.ClientPollingInterval;
            
			Observable.Timer(TimeSpan.FromSeconds(4), TimeSpan.FromSeconds (_refreshPeriod))
				.ObserveOn(SynchronizationContext.Current)
				.Subscribe (_ => RefreshStatus())
				.DisposeWith (Subscriptions);
		}
		
		protected readonly CompositeDisposable Subscriptions = new CompositeDisposable ();
		public override void OnViewStopped ()
		{
			base.OnViewStopped ();
            Subscriptions.DisposeAll ();
		}

		#region Bindings
		private IEnumerable<CoordinateViewModel> _mapCenter;
		public IEnumerable<CoordinateViewModel> MapCenter {
			get { return _mapCenter; }
			private set {
				_mapCenter = value;
				RaisePropertyChanged ();
			}
		}

		bool _isPayButtonVisible;
		public bool IsPayButtonVisible {
			get{
				return _isPayButtonVisible;
			} set{
				_isPayButtonVisible = value;
				RaisePropertyChanged (); 
			}
		}

		bool _isCancelButtonVisible;
		public bool IsCancelButtonVisible{
			get{
				return _isCancelButtonVisible;
			} set{
				_isCancelButtonVisible = value;
				RaisePropertyChanged (); 
			}
		}
		
        bool _isResendButtonVisible;
        public bool IsResendButtonVisible{
            get{
                return _isResendButtonVisible;
            } set{
                _isResendButtonVisible = value;
				RaisePropertyChanged (); 
            }
        }

        private string _confirmationNoTxt;
		public string ConfirmationNoTxt {
			get {
				return _confirmationNoTxt;
			}
			set {
				_confirmationNoTxt = value;
				RaisePropertyChanged ();
			}
		}

        public bool IsCallTaxiVisible
        {
            get 
			{
				return Settings.ShowCallDriver 
					&& IsDriverInfoAvailable 
					&& OrderStatusDetail.DriverInfos.MobilePhone.HasValue (); 
			}
        }

        public bool IsDriverInfoAvailable
        {
            get {
				bool showVehicleInformation = Settings.ShowVehicleInformation;
				bool isOrderStatusValid = OrderStatusDetail.IBSStatusId == VehicleStatuses.Common.Assigned
					|| OrderStatusDetail.IBSStatusId == VehicleStatuses.Common.Arrived
					|| OrderStatusDetail.IBSStatusId == VehicleStatuses.Common.Loaded;
				bool hasDriverInformation = OrderStatusDetail.DriverInfos.VehicleRegistration.HasValue ()
					|| OrderStatusDetail.DriverInfos.LastName.HasValue ()
					|| OrderStatusDetail.DriverInfos.FirstName.HasValue ();

				return showVehicleInformation && isOrderStatusValid && hasDriverInformation;
			}
        }

		public bool CompanyHidden
		{
			get { return string.IsNullOrWhiteSpace(OrderStatusDetail.CompanyName) || !IsDriverInfoAvailable; }
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
		public string StatusInfoText
        {
			get { return _statusInfoText; }
			set
            {
				_statusInfoText = value;
				RaisePropertyChanged();
			}
		}

		private Order _order;
		public Order Order
        {
			get { return _order; }
			set
            {
				_order = value;
				RaisePropertyChanged();
			}
		}
		
		private OrderStatusDetail _orderStatusDetail;
		public OrderStatusDetail OrderStatusDetail
        {
			get { return _orderStatusDetail; }
			set {
				_orderStatusDetail = value;
				RaisePropertyChanged (() => OrderStatusDetail);
				RaisePropertyChanged (() => CompanyHidden);
				RaisePropertyChanged (() => VehicleDriverHidden);
				RaisePropertyChanged (() => VehicleLicenceHidden);
				RaisePropertyChanged (() => VehicleTypeHidden);
				RaisePropertyChanged (() => VehicleMakeHidden);
				RaisePropertyChanged (() => VehicleModelHidden);
				RaisePropertyChanged (() => VehicleColorHidden);
				RaisePropertyChanged (() => IsDriverInfoAvailable);
				RaisePropertyChanged (() => IsCallTaxiVisible);
			}
		}

		private string ConvertToValidPhoneNumberIfNecessary(string phoneNumber)
		{
			return phoneNumber.Length == 11
				? phoneNumber
				: string.Concat("1", phoneNumber);
		}

		public ICommand CallTaxi
        {
            get 
			{ 
				return this.GetCommand(() =>
                {
					if (!string.IsNullOrEmpty(OrderStatusDetail.DriverInfos.MobilePhone))
                    {
						if(Settings.CallDriverUsingProxy)
						{
							var driver = ConvertToValidPhoneNumberIfNecessary(OrderStatusDetail.DriverInfos.MobilePhone);
							var passenger = ConvertToValidPhoneNumberIfNecessary(Order.Settings.Phone);

							var proxyUrl = Settings.CallDriverUsingProxyUrl;
							var request = WebRequest.Create(string.Format(proxyUrl, driver, passenger));
							request.GetResponseAsync();

							this.Services().Message.ShowMessage(this.Services().Localize["GenericTitle"], this.Services().Localize["CallDriverUsingProxyMessage"]);
						}
						else
						{
							_phoneService.Call(OrderStatusDetail.DriverInfos.MobilePhone);
						}
                    }
                    else
                    {
                        this.Services().Message.ShowMessage(this.Services().Localize["NoPhoneNumberTitle"], this.Services().Localize["NoPhoneNumberMessage"]);
                    }
                }); 
			}
        }

		#endregion

        private bool HasSeenReminderPrompt( Guid orderId )
        {
            var hasSeen = this.Services().Cache.Get<string>("OrderReminderWasSeen." + orderId);
            return !string.IsNullOrEmpty(hasSeen);
        }

        private void SetHasSeenReminderPrompt( Guid orderId )
        {
            this.Services().Cache.Set("OrderReminderWasSeen." + orderId, true.ToString());                     
        }

        private void AddReminder (OrderStatusDetail status)
        {
            if (!HasSeenReminderPrompt(status.OrderId )
				&& _phoneService.CanUseCalendarAPI())
            {
                SetHasSeenReminderPrompt(status.OrderId);
                InvokeOnMainThread(() => this.Services().Message.ShowMessage(
                    this.Services().Localize["AddReminderTitle"], 
                    this.Services().Localize["AddReminderMessage"],
                    this.Services().Localize["YesButton"],
					() => _phoneService.AddEventToCalendarAndReminder(
						string.Format(this.Services().Localize["ReminderTitle"], Settings.TaxiHail.ApplicationName), 
                        string.Format(this.Services().Localize["ReminderDetails"], Order.PickupAddress.FullAddress, CultureProvider.FormatTime(Order.PickupDate), CultureProvider.FormatDate(Order.PickupDate)),						              									 
                    Order.PickupAddress.FullAddress, 
                    Order.PickupDate,
                    Order.PickupDate.AddHours(-2)), 
                    this.Services().Localize["NoButton"], () => { }));
            }
        }

		private string _vehicleNumber;
        private bool _isCurrentlyPairing;
		private bool _isDispatchPopupVisible;
        private bool _isContactingNextCompany;
	    private int? _currentIbsOrderId; 

		private bool _refreshStatusIsExecuting;
		private async void RefreshStatus()
        {

            try 
			{
				if(_refreshStatusIsExecuting)
				{
					return;
				}

				Logger.LogMessage ("RefreshStatus starts");
				_refreshStatusIsExecuting = true;

				var status = await _bookingService.GetOrderStatusAsync(Order.Id);
				if(status.VehicleNumber != null)
				{
					_vehicleNumber = status.VehicleNumber;
				}
				else
                {
					status.VehicleNumber = _vehicleNumber;
				}

                if (_isContactingNextCompany && status.IBSOrderId == _currentIbsOrderId)
                {
                    // Don't update status when we're contacting a new dispatch company (switch)
                    return;
                }

                _currentIbsOrderId = status.IBSOrderId;
                _isContactingNextCompany = false;

                SwitchDispatchCompanyIfNecessary(status);

				var isDone = _bookingService.IsStatusDone(status.IBSStatusId);

				if(status.IBSStatusId.HasValue() && status.IBSStatusId.Equals(VehicleStatuses.Common.Scheduled))
				{
					AddReminder(status);
				}

#if DEBUG
                //status.IBSStatusId = VehicleStatuses.Common.Arrived;
#endif
                IsPayButtonVisible = false;

				var statusInfoText = status.IBSStatusDescription;

				if(Settings.ShowEta 
					&& status.IBSStatusId.SoftEqual(VehicleStatuses.Common.Assigned) 
					&& status.VehicleNumber.HasValue()
					&& status.VehicleLatitude.HasValue
					&& status.VehicleLongitude.HasValue)
				{
					Direction d =  await _vehicleService.GetEtaBetweenCoordinates(status.VehicleLatitude.Value, status.VehicleLongitude.Value, Order.PickupAddress.Latitude, Order.PickupAddress.Longitude);
					statusInfoText += " " + FormatEta(d);						
				}

				StatusInfoText = statusInfoText;
                OrderStatusDetail = status;

                CenterMap ();

				var isLoaded = status.IBSStatusId.Equals(VehicleStatuses.Common.Loaded) 
					|| status.IBSStatusId.Equals(VehicleStatuses.Common.Done);
				var paymentSettings = await _paymentService.GetPaymentSettings();
                if (isLoaded 
					&& ((paymentSettings.AutomaticPayment && !paymentSettings.AutomaticPaymentPairing)
						|| paymentSettings.PaymentMode == PaymentMethod.RideLinqCmt)
					&& _accountService.CurrentAccount.DefaultCreditCard != null)
				{
					var isPaired = await _bookingService.IsPaired(Order.Id);
                    var pairState = this.Services().Cache.Get<string>("PairState" + Order.Id);
					var isPairBypass = (pairState == PairingState.Failed) || (pairState == PairingState.Canceled) || (pairState == PairingState.Unpaired);
					if (!isPaired && !_isCurrentlyPairing && !isPairBypass)
					{
						_isCurrentlyPairing = true;
                        GoToPairScreen();
						return;
					}
				}

                UpdatePayCancelButtons(status.IBSStatusId);

                DisplayOrderNumber();

                if (isDone) 
                {
                    this.Services().MessengerHub.Publish(new OrderStatusChanged(this, status.OrderId, OrderStatus.Completed, null));
					GoToSummary();
                }

				if (_bookingService.IsStatusTimedOut(status.IBSStatusId))
                {
                    GoToBookingScreen();
                }
            } 
			catch (Exception ex) 
			{
                Logger.LogError (ex);
            }
			finally
			{			
				Logger.LogMessage ("RefreshStatus ends");
				_refreshStatusIsExecuting = false;			
			}
        }

	    private void SwitchDispatchCompanyIfNecessary(OrderStatusDetail status)
	    {
            if (status.Status == OrderStatus.TimedOut)
            {
                bool alwayAcceptSwitch;
                bool.TryParse(this.Services().Cache.Get<string>("TaxiHailNetworkTimeOutAlwayAccept"), out alwayAcceptSwitch);

                if (status.NextDispatchCompanyKey != null && alwayAcceptSwitch)
                {
                    // Switch without user input
                    SwitchCompany(status);
                }
                else if (status.NextDispatchCompanyKey != null && !_isDispatchPopupVisible && !alwayAcceptSwitch)
                {
                    _isDispatchPopupVisible = true;

                    this.Services().Message.ShowMessage(
                        this.Services().Localize["TaxiHailNetworkTimeOutPopupTitle"],
                        string.Format(this.Services().Localize["TaxiHailNetworkTimeOutPopupMessage"], status.NextDispatchCompanyName),
                        this.Services().Localize["TaxiHailNetworkTimeOutPopupAccept"],
                            () => SwitchCompany(status),
                        this.Services().Localize["TaxiHailNetworkTimeOutPopupRefuse"],
                            () =>
                            {
                                if (status.Status.Equals(OrderStatus.TimedOut))
                                {
                                    _bookingService.IgnoreDispatchCompanySwitch(status.OrderId);
                                    _isDispatchPopupVisible = false;
                                }
                            },
                        this.Services().Localize["TaxiHailNetworkTimeOutPopupAlways"],
                            () =>
                            {
                                this.Services().Cache.Set("TaxiHailNetworkTimeOutAlwayAccept", "true");
                                SwitchCompany(status);
                            });
                }
            }
	    }

	    private async void SwitchCompany(OrderStatusDetail status)
	    {
	        if (status.Status != OrderStatus.TimedOut)
	        {
	            return;
	        }

	        _isDispatchPopupVisible = false;
            _isContactingNextCompany = true;

            try
            {
                var orderStatusDetail = await _bookingService.SwitchOrderToNextDispatchCompany(
                    status.OrderId,
                    status.NextDispatchCompanyKey,
                    status.NextDispatchCompanyName);
                OrderStatusDetail = orderStatusDetail;

                StatusInfoText = string.Format(
                    this.Services().Localize["NetworkContactingNextDispatchDescription"],
                    status.NextDispatchCompanyName);
            }
            catch (WebServiceException ex)
            {
                _isContactingNextCompany = false;
                this.Services().Message.ShowMessage(
                    this.Services().Localize["TaxiHailNetworkTimeOutErrorTitle"],
                    ex.ErrorMessage);
            }
	    }

	    private void DisplayOrderNumber()
	    {
	        if (OrderStatusDetail.IBSOrderId.HasValue)
	        {
	            ConfirmationNoTxt =
                    string.Format(this.Services().Localize["StatusDescription"], OrderStatusDetail.IBSOrderId.Value);
	        }
	    }

	    string FormatEta(Direction direction)
		{
			if (!direction.IsValidEta())
			{
				return string.Empty;
			}

			var durationUnit = direction.Duration <= 1 ? this.Services ().Localize ["EtaDurationUnit"] : this.Services ().Localize ["EtaDurationUnitPlural"];
			return string.Format (this.Services ().Localize ["StatusEta"], direction.FormattedDistance, direction.Duration, durationUnit);
		}

		private async void UpdatePayCancelButtons(string statusId)
		{
			var paymentSettings = await _paymentService.GetPaymentSettings();
		    var isOrderAlreadyPaid = _paymentService.GetPaymentFromCache(Order.Id).HasValue;

            IsPayButtonVisible = ShouldDisplayPayButton(statusId, isOrderAlreadyPaid, paymentSettings);

            IsCancelButtonVisible = _bookingService.IsOrderCancellable(statusId);

            IsResendButtonVisible = isOrderAlreadyPaid
                                && !paymentSettings.AutomaticPayment
								&& paymentSettings.PaymentMode != PaymentMethod.RideLinqCmt
                                && (paymentSettings.IsPayInTaxiEnabled || paymentSettings.PayPalClientSettings.IsEnabled);

			// Unpair button is only available for RideLinqCMT
			if (paymentSettings.PaymentMode == PaymentMethod.RideLinqCmt) 
			{
				var isPaired = await _bookingService.IsPaired(Order.Id);
				IsUnpairButtonVisible = !paymentSettings.AutomaticPayment && isPaired;
			} 
			else 
			{
				IsUnpairButtonVisible = false;
			}
		}

	    private bool ShouldDisplayPayButton(string statusId, bool isOrderAlreadyPaid, ClientPaymentSettings paymentSettings)
	    {
	        bool payingByChargeAccount = Order.Settings.ChargeTypeId == ChargeTypes.Account.Id;
            bool payingByChargeAccountCardOnFile = payingByChargeAccount && OrderStatusDetail.IsChargeAccountPaymentWithCardOnFile;
            bool passengersInCar = statusId == VehicleStatuses.Common.Loaded || statusId == VehicleStatuses.Common.Done;
            bool hasCardOnFile = paymentSettings.IsPayInTaxiEnabled	&& _accountService.CurrentAccount.DefaultCreditCard != null;

	        return !Settings.HidePayNowButtonDuringRide
                && !paymentSettings.AutomaticPayment
                && !isOrderAlreadyPaid
                && (!payingByChargeAccount || payingByChargeAccountCardOnFile)
	            && passengersInCar
                && (hasCardOnFile || paymentSettings.PayPalClientSettings.IsEnabled) // Can pay by card or PayPal
	            && !IsUnpairButtonVisible                                            // Unpair visible (pair button is pay button in pairing situations) 
	            && OrderStatusDetail.CompanyKey == null;                             // Not dispatched to another company
	    }

		public void GoToSummary()
		{
			Logger.LogMessage ("GoToSummary");
			ShowViewModelAndRemoveFromHistory<RideSummaryViewModel> (
				new {
					order = Order.ToJson(),
					orderStatus = OrderStatusDetail.ToJson()
				}.ToStringDictionary());
		}

        public async void GoToBookingScreen()
		{
            if (!_waitingToNavigateAfterTimeOut)
            {
				_waitingToNavigateAfterTimeOut = true;
				await Task.Delay (TimeSpan.FromSeconds (10));
				_bookingService.ClearLastOrder();
				ShowViewModelAndRemoveFromHistory<HomeViewModel>(new { locateUser =  true });
            }
        }

        public void GoToPairScreen()
        {
			ShowViewModel<ConfirmPairViewModel>(new 
			{
				order = Order.ToJson(),
				orderStatus = OrderStatusDetail.ToJson()
			}.ToStringDictionary());
        }

        private void CenterMap ()
        {   
			if (Order == null) 
			{
				return;
			}

			var showPickupOnlyStatus = new [] { VehicleStatuses.Common.Waiting, VehicleStatuses.Common.Timeout, VehicleStatuses.Common.Scheduled };
			var showPickupAndVehicleStatus = new [] { VehicleStatuses.Common.Assigned, VehicleStatuses.Common.Arrived, VehicleStatuses.Common.NoShow };
			var showVehicleAndDropOffStatus = new [] { VehicleStatuses.Common.Loaded, VehicleStatuses.Common.MeterOffNotPayed, VehicleStatuses.Common.Done };

			// should show nothing but pickup
			if (OrderStatusDetail == null
				|| showPickupOnlyStatus.Contains(OrderStatusDetail.IBSStatusId)
				|| (!OrderStatusDetail.VehicleLatitude.HasValue && !OrderStatusDetail.VehicleLongitude.HasValue))
			{
				var pickup = CoordinateViewModel.Create(Order.PickupAddress.Latitude, Order.PickupAddress.Longitude, true);
				MapCenter = new[] { pickup };
				return;
			}

			// should show pickup and vehicle
			if (showPickupAndVehicleStatus.Contains(OrderStatusDetail.IBSStatusId))
			{
				var pickup = CoordinateViewModel.Create(Order.PickupAddress.Latitude, Order.PickupAddress.Longitude, true);
				var vehicle = CoordinateViewModel.Create(OrderStatusDetail.VehicleLatitude.Value, OrderStatusDetail.VehicleLongitude.Value);
				MapCenter = new[] { pickup, vehicle };
				return;
			}

			// should show vehicle and dropoff (if available)
			if (showVehicleAndDropOffStatus.Contains(OrderStatusDetail.IBSStatusId))
			{
				var vehicle = CoordinateViewModel.Create(OrderStatusDetail.VehicleLatitude.Value, OrderStatusDetail.VehicleLongitude.Value, true);

				if (Order.DropOffAddress != null
					&& Order.DropOffAddress.HasValidCoordinate ())
				{
					var dropOff = CoordinateViewModel.Create(Order.DropOffAddress.Latitude, Order.DropOffAddress.Longitude, true);
					MapCenter = new[] { vehicle, dropOff };
				}
				else
				{
					MapCenter = new[] { vehicle };
				}

				return;
			}
        }

		#region Commands

		public ICommand NewRide
        {
            get {
                return this.GetCommand(() => this.Services().Message.ShowMessage(
                    this.Services().Localize["StatusNewRideButton"], 
                    this.Services().Localize["StatusConfirmNewBooking"],
                    this.Services().Localize["YesButton"], 
                    () => { 
						_bookingService.ClearLastOrder();
						ShowViewModel<HomeViewModel> (new { locateUser =  true });
                    },
					this.Services().Localize["NoButton"], delegate {}));
            }
        }

		public ICommand CancelOrder
        {
            get {
                return this.GetCommand (() =>
                {
				    if ((OrderStatusDetail.IBSStatusId == VehicleStatuses.Common.Done) || (OrderStatusDetail.IBSStatusId == VehicleStatuses.Common.Loaded)) {
                        this.Services().Message.ShowMessage(this.Services().Localize["CannotCancelOrderTitle"], this.Services().Localize["CannotCancelOrderMessage"]);
                        return;
                    }

                    this.Services().Message.ShowMessage(
                        "", 
                        this.Services().Localize["StatusConfirmCancelRide"],
                        this.Services().Localize["YesButton"], 
						async () =>
                        {
							var isSuccess = false;
							using(this.Services().Message.ShowProgress())
							{
								isSuccess = await _bookingService.CancelOrder(Order.Id); 
							}
                            if (isSuccess) 
                            {
								this.Services().Analytics.LogEvent("BookCancelled");
								_bookingService.ClearLastOrder();                                
                                ShowViewModelAndRemoveFromHistory<HomeViewModel> (new { locateUser =  true });
                            } 
                            else 
                            {
                                this.Services().Message.ShowMessage(this.Services().Localize["StatusConfirmCancelRideErrorTitle"], this.Services().Localize["StatusConfirmCancelRideError"]);
                            }
                        },
                        this.Services().Localize["NoButton"], () => { });
                });
            }
        }

		public ICommand PayForOrderCommand
        {
            get
            {
                return this.GetCommand(async () =>
                {
#if DEBUG
#else
					if(string.IsNullOrWhiteSpace(OrderStatusDetail.VehicleNumber)){
							this.Services().Message.ShowMessage(this.Services().Localize["VehicleNumberErrorTitle"], 
								this.Services().Localize["VehicleNumberErrorMessage"]);
						return;
					}
#endif
					IsPayButtonVisible = false;
					this.Services().Analytics.LogEvent("PayButtonTapped");

					var paymentSettings = await _paymentService.GetPaymentSettings();
					if ((paymentSettings.AutomaticPayment && !paymentSettings.AutomaticPaymentPairing)
						|| paymentSettings.PaymentMode == PaymentMethod.RideLinqCmt)
                    {
                        GoToPairScreen();
                    }
                    else
                    {
						ShowViewModel<ConfirmCarNumberViewModel>(
	                        new
	                        {
	                            order = Order.ToJson(),
	                            orderStatus = OrderStatusDetail.ToJson()
	                        });
                    }
                });
            }
        }

		public ICommand CallCompany
        {
            get {
                return this.GetCommand (() =>
                {
                    this.Services().Message.ShowMessage(string.Empty,
												Settings.DefaultPhoneNumberDisplay,
                                               this.Services().Localize["CallButton"],
											   () => _phoneService.Call(Settings.DefaultPhoneNumber),
                                               this.Services().Localize["Cancel"], 
                                               () => {});                    
                });
            }
        }

		public ICommand ResendConfirmationToDriver
		{
			get
			{
				return this.GetCommand(() =>
					{
						if (_paymentService.GetPaymentFromCache(Order.Id).HasValue)
						{
							_paymentService.ResendConfirmationToDriver(Order.Id);
						}
					});
			}
		}

		public ICommand Unpair
		{
			get
			{
				return this.GetCommand(async () =>
					{
						using(this.Services().Message.ShowProgress())
						{
							var response = await _paymentService.Unpair(Order.Id);

							if(response.IsSuccessful)
							{
								this.Services().Cache.Set("PairState" + Order.Id, PairingState.Unpaired);
								RefreshStatus();
							}
							else
							{
								this.Services().Message.ShowMessage(this.Services().Localize["CmtRideLinqErrorTitle"], this.Services().Localize["CmtRideLinqUnpairErrorMessage"]);
							}
						}
					});
			}
		}

		public ICommand PrepareNewOrder
        {
			get
			{
				return this.GetCommand(async () =>{
					_bookingService.ClearLastOrder();
                    var address = await _orderWorkflowService.SetAddressToUserLocation();
                    if (address.HasValidCoordinate())
                    {
                        ChangePresentation(new ZoomToStreetLevelPresentationHint(address.Latitude, address.Longitude));
                    }
				});
			}
        }

        bool _isUnpairButtonVisible;
        public bool IsUnpairButtonVisible
        {
            get
            {
                return _isUnpairButtonVisible;
            }
            set
            {
                _isUnpairButtonVisible = value;
				RaisePropertyChanged();
            }
        }

	    #endregion
    }
}
