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
using apcurium.MK.Common;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using ServiceStack.Text;
using apcurium.MK.Booking.Maps;
using System.Net;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Enumeration;
using ServiceStack.ServiceClient.Web;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class BookingStatusViewModel : PageViewModel
    {
		private readonly IOrderWorkflowService _orderWorkflowService;
		private readonly IPhoneService _phoneService;
		private readonly IBookingService _bookingService;
		private readonly IPaymentService _paymentService;
		private readonly IVehicleService _vehicleService;
	    private readonly IMetricsService _metricsService;

	    private int _refreshPeriod = 5;              // in seconds
        private bool _waitingToNavigateAfterTimeOut;
        private string _vehicleNumber;
        private bool _isDispatchPopupVisible;
        private bool _isContactingNextCompany;
        private int? _currentIbsOrderId; 

		private bool _isCmtRideLinq;

	    public BookingStatusViewModel(IOrderWorkflowService orderWorkflowService,
			IPhoneService phoneService,
			IBookingService bookingService,
			IPaymentService paymentService,
			IVehicleService vehicleService,
		    IMetricsService metricsService)
		{
			_orderWorkflowService = orderWorkflowService;
			_phoneService = phoneService;
			_bookingService = bookingService;
			_paymentService = paymentService;
			_vehicleService = vehicleService;
	        _metricsService = metricsService;
		}

		public void Init(string order, string orderStatus)
		{
			GetIsCmtRideLinq();
			Order = JsonSerializer.DeserializeFromString<Order> (order);
			OrderStatusDetail = JsonSerializer.DeserializeFromString<OrderStatusDetail> (orderStatus);
            DisplayOrderNumber();
			IsCancelButtonVisible = false;			
			_waitingToNavigateAfterTimeOut = false;
		}

		private async void GetIsCmtRideLinq()
		{
			try
			{
				var paymentSettings = await _paymentService.GetPaymentSettings();

				_isCmtRideLinq = paymentSettings.PaymentMode == PaymentMethod.RideLinqCmt;

			    RefreshView();
			}
			catch(Exception ex) 
			{
				Logger.LogError(ex);	
			}
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
				.Subscribe(_ => RefreshStatus())
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
		public IEnumerable<CoordinateViewModel> MapCenter
        {
			get { return _mapCenter; }
			private set 
            {
				_mapCenter = value;
				RaisePropertyChanged ();
			}
		}

		bool _isCancelButtonVisible;
		public bool IsCancelButtonVisible
        {
			get { return _isCancelButtonVisible; } 
            set
            {
				_isCancelButtonVisible = value;
				RaisePropertyChanged (); 
			}
		}

        private string _confirmationNoTxt;
		public string ConfirmationNoTxt
        {
			get { return _confirmationNoTxt; }
			set 
            {
				_confirmationNoTxt = value;
				RaisePropertyChanged ();
			}
		}

        public bool IsCallTaxiVisible
        {
            get 
			{
                // don't hide the button if the user's phone number is not 
                // entered because we need to tell the user why it's not there
				return Settings.ShowCallDriver 
					&& IsDriverInfoAvailable 
					&& OrderStatusDetail.DriverInfos.MobilePhone.HasValue ()
					&& (OrderStatusDetail.IBSStatusId == VehicleStatuses.Common.Assigned
						|| OrderStatusDetail.IBSStatusId == VehicleStatuses.Common.Arrived); 
			}
        }

        public bool IsMessageTaxiVisible
        {
            get
            {
                return Settings.ShowMessageDriver
                    && IsDriverInfoAvailable
                    && OrderStatusDetail.VehicleNumber.HasValue()
                    && (OrderStatusDetail.IBSStatusId == VehicleStatuses.Common.Assigned
                        || OrderStatusDetail.IBSStatusId == VehicleStatuses.Common.Arrived);
            }
        }

        public bool IsDriverInfoAvailable
        {
            get 
            {
				var showVehicleInformation = Settings.ShowVehicleInformation;
				var isOrderStatusValid = OrderStatusDetail.IBSStatusId == VehicleStatuses.Common.Assigned
					|| OrderStatusDetail.IBSStatusId == VehicleStatuses.Common.Arrived
					|| OrderStatusDetail.IBSStatusId == VehicleStatuses.Common.Loaded;
				var hasDriverInformation = OrderStatusDetail.DriverInfos.VehicleRegistration.HasValue ()
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
		    get
		    {
		        return IsInformationHidden(OrderStatusDetail.DriverInfos.FullName);
		    }
		}

	    private bool IsInformationHidden(string informationToValidate)
	    {
	        return !IsDriverInfoAvailable
	               || string.IsNullOrWhiteSpace(informationToValidate) 
                   //Needed to hide information sent in error when in eHail auto ridelinq pairing mode.
	               || _isCmtRideLinq;
	    }

	    public bool VehicleLicenceHidden
		{
		    get
		    {
                return IsInformationHidden(OrderStatusDetail.DriverInfos.VehicleRegistration);
		    }
		}

	    public bool VehicleMedallionHidden
	    {
	        get
	        {
	            return !IsDriverInfoAvailable 
                    || string.IsNullOrWhiteSpace(OrderStatusDetail.VehicleNumber) 
                    // Medallion should be hidden when not in eHail mode.
					|| !_isCmtRideLinq;
	        }
	    }

	    public bool VehicleTypeHidden
		{
            get
            {
                return IsInformationHidden(OrderStatusDetail.DriverInfos.VehicleType);
            }
		}
		public bool VehicleMakeHidden
		{
            get
            {
                return IsInformationHidden(OrderStatusDetail.DriverInfos.VehicleMake);
            }
		}
		public bool VehicleModelHidden
		{
            get
            {
                return IsInformationHidden(OrderStatusDetail.DriverInfos.VehicleModel);
            }
		}
		public bool VehicleColorHidden
		{
            get
            {
                return IsInformationHidden(OrderStatusDetail.DriverInfos.VehicleColor);
            }
		}

        public bool DriverPhotoHidden
        {
            get
            {
                return IsInformationHidden(OrderStatusDetail.DriverInfos.DriverPhotoUrl);
            }
        }
        
        public bool CanGoBack
		{
			get
			{
				return // we know from the start it's a scheduled
						(Order.CreatedDate != Order.PickupDate 													
							&& !OrderStatusDetail.IBSStatusId.HasValue())
						// it has the status scheduled
						|| OrderStatusDetail.IBSStatusId.SoftEqual(VehicleStatuses.Common.Scheduled)
						// it is cancelled or no show
						|| (OrderStatusDetail.IBSStatusId.SoftEqual (VehicleStatuses.Common.Cancelled)
							|| OrderStatusDetail.IBSStatusId.SoftEqual (VehicleStatuses.Common.NoShow)
							|| OrderStatusDetail.IBSStatusId.SoftEqual (VehicleStatuses.Common.CancelledDone))
						// there was an error with ibs order creation
						|| (OrderStatusDetail.IBSStatusId.SoftEqual(VehicleStatuses.Unknown.None)
							&& OrderStatusDetail.Status == OrderStatus.Canceled);
			}
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
				RaisePropertyChanged(() => CanGoBack);
			}
		}
		
		private OrderStatusDetail _orderStatusDetail;
		public OrderStatusDetail OrderStatusDetail
        {
			get { return _orderStatusDetail; }
			set
			{
			    _orderStatusDetail = value;
			    RefreshView();
			}
        }

	    private void RefreshView()
	    {
	        RaisePropertyChanged(() => OrderStatusDetail);
	        RaisePropertyChanged(() => CompanyHidden);
	        RaisePropertyChanged(() => VehicleDriverHidden);
	        RaisePropertyChanged(() => VehicleLicenceHidden);
	        RaisePropertyChanged(() => VehicleTypeHidden);
	        RaisePropertyChanged(() => VehicleMakeHidden);
	        RaisePropertyChanged(() => VehicleModelHidden);
	        RaisePropertyChanged(() => VehicleColorHidden);
	        RaisePropertyChanged(() => DriverPhotoHidden);
	        RaisePropertyChanged(() => IsDriverInfoAvailable);
	        RaisePropertyChanged(() => IsCallTaxiVisible);
	        RaisePropertyChanged(() => IsMessageTaxiVisible);
	        RaisePropertyChanged(() => CanGoBack);
	        RaisePropertyChanged(() => VehicleMedallionHidden);
	    }

	    public ICommand CallTaxi
        {
            get 
			{ 
				return this.GetCommand(async () =>
				{
				    var canCallDriver = Order.Settings.Phone.HasValue()
				       && OrderStatusDetail.DriverInfos.MobilePhone.HasValue();

				    if (canCallDriver)
				    {
				        var shouldInitiateCall = false;
				        await this.Services().Message.ShowMessage(
                            this.Services().Localize["GenericTitle"], this.Services().Localize["CallDriverUsingProxyPrompt"],
                            this.Services().Localize["OkButtonText"], () => { shouldInitiateCall = true; },
                            this.Services().Localize["Cancel"], () => { });

				        if (!shouldInitiateCall)
				        {
				            return;
				        }

				        var success = await _bookingService.InitiateCallToDriver(Order.Id);
                        if (success)
				        {
				            this.Services().Message.ShowMessage(
                                this.Services().Localize["GenericTitle"],
				                this.Services().Localize["CallDriverUsingProxyMessage"]);
				        }
				        else
				        {
                            this.Services().Message.ShowMessage(
                                this.Services().Localize["GenericErrorTitle"],
                                this.Services().Localize["CallDriverUsingProxyErrorMessage"]);
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

		private bool CanRefreshStatus(OrderStatusDetail status)
		{
			return status.IBSOrderId.HasValue		// we can exit this loop only if we are assigned an IBSOrderId 
				|| status.IBSStatusId.HasValue();	// or if we get an IBSStatusId
		}

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

				while(!CanRefreshStatus(status))
				{
					Logger.LogMessage ("Waiting for Ibs Order Creation (ibs order id)");
					await Task.Delay(TimeSpan.FromSeconds(1));
					status = await _bookingService.GetOrderStatusAsync(Order.Id);

					if(status.IBSOrderId.HasValue)
					{
						Logger.LogMessage("Received Ibs Order Id: {0}", status.IBSOrderId.Value);
					}
				}

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

				if(status.IBSStatusId.SoftEqual(VehicleStatuses.Common.Scheduled))
				{
					AddReminder(status);
				}
					
				var statusInfoText = status.IBSStatusDescription;

                var isLocalMarket = await _orderWorkflowService.GetAndObserveHashedMarket()
                    .Select(hashedMarket => !hashedMarket.HasValue())
                    .Take(1);

                var isUsingGeoServices = isLocalMarket
                    ? Settings.LocalAvailableVehiclesMode == LocalAvailableVehiclesModes.Geo
                    : Settings.ExternalAvailableVehiclesMode == ExternalAvailableVehiclesModes.Geo;


                if (Settings.ShowEta 
					&& status.IBSStatusId.SoftEqual(VehicleStatuses.Common.Assigned) 
					&& status.VehicleNumber.HasValue()
					&& status.VehicleLatitude.HasValue
					&& status.VehicleLongitude.HasValue)
				{
					long? eta;

					if(isUsingGeoServices)
                    {
						var geoData = await _vehicleService.GetVehiclePositionInfoFromGeo(Order.PickupAddress.Latitude, Order.PickupAddress.Longitude, status.DriverInfos.VehicleRegistration, status.OrderId);
                        eta = geoData.Eta;

						if(geoData.IsPositionValid)
                        {
                            status.VehicleLatitude = geoData.Latitude;
                            status.VehicleLongitude = geoData.Longitude;
                        }
					}
					else
					{
						var direction = await _vehicleService.GetEtaBetweenCoordinates(status.VehicleLatitude.Value, status.VehicleLongitude.Value, Order.PickupAddress.Latitude, Order.PickupAddress.Longitude);

                        // Log original eta value
					    if (direction.IsValidEta())
					    {
                            _metricsService.LogOriginalRideEta(Order.Id, direction.Duration);
					    }

					    eta = direction.Duration;
					}                       

				    statusInfoText += " " + FormatEta(eta);
				}

                // Needed to do this here since cmtGeoService needs the device's location to calculate the Eta and does not have the ability to get the position of a specific vehicle(or a bach of vehicle) without the device location.
				if (isUsingGeoServices &&
					(status.IBSStatusId.SoftEqual(VehicleStatuses.Common.Loaded)
						|| status.IBSStatusId.SoftEqual(VehicleStatuses.Common.Arrived)))
                {
                    //refresh vehicle position on the map from the geo data
                    var geoData = await _vehicleService.GetVehiclePositionInfoFromGeo(Order.PickupAddress.Latitude, Order.PickupAddress.Longitude, status.DriverInfos.VehicleRegistration, Order.Id);
					if(geoData.IsPositionValid)
                    {
                        status.VehicleLatitude = geoData.Latitude;
                        status.VehicleLongitude = geoData.Longitude;
                    }
                }

				StatusInfoText = statusInfoText;
                OrderStatusDetail = status;

                CenterMap ();

				UpdateActionsPossibleOnOrder(status);

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

                if (status.NextDispatchCompanyKey != null
                    && (alwayAcceptSwitch || Settings.Network.AutoConfirmFleetChange))
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

	    string FormatEta(long? eta)
		{
			if (!eta.HasValue)
			{
				return string.Empty;
			}

			var durationUnit = eta.Value <= 1 ? this.Services ().Localize ["EtaDurationUnit"] : this.Services ().Localize ["EtaDurationUnitPlural"];
			return string.Format(this.Services ().Localize ["StatusEta"], eta.Value, durationUnit);
		}

		private async void UpdateActionsPossibleOnOrder(OrderStatusDetail status)
		{
			IsCancelButtonVisible = _bookingService.IsOrderCancellable(status);

		    var arePassengersOnBoard = OrderStatusDetail.IBSStatusId.SoftEqual(VehicleStatuses.Common.Loaded);
            var isUnPairPossible = DateTime.UtcNow <= OrderStatusDetail.UnpairingTimeOut;
		    
            if (arePassengersOnBoard
                && (Order.Settings.ChargeTypeId == ChargeTypes.CardOnFile.Id
                || Order.Settings.ChargeTypeId == ChargeTypes.PayPal.Id)) 
			{
                var isPaired = await _bookingService.IsPaired(Order.Id);

                CanEditAutoTip = isPaired;
			    IsUnpairButtonVisible = isPaired && isUnPairPossible;
			} 
			else
			{
				IsUnpairButtonVisible = false;
			}
		}

		public async void GoToSummary()
		{
			Logger.LogMessage ("GoToSummary");

            ShowViewModelAndRemoveFromHistory<RideSummaryViewModel>(new { orderId = Order.Id});
		}

        public async void GoToBookingScreen()
		{
            if (!_waitingToNavigateAfterTimeOut)
            {
				_waitingToNavigateAfterTimeOut = true;
				await Task.Delay (TimeSpan.FromSeconds (10));
				_bookingService.ClearLastOrder();

                ShowViewModelAndRemoveFromHistory<HomeViewModel>(new { locateUser = true });
            }
        }

        private void CenterMap()
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

                    var confirmationMessage = Settings.WarnForFeesOnCancel
                        && (VehicleStatuses.CanCancelOrderStatus.Contains(OrderStatusDetail.IBSStatusId))
                        ? this.Services().Localize["StatusConfirmCancelRideAndWarnForCancellationFees"]
                        : this.Services().Localize["StatusConfirmCancelRide"];

                    this.Services().Message.ShowMessage(
                        string.Empty,
                        confirmationMessage,
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

		public ICommand CallCompany
        {
            get 
            {
                return this.GetCommand (() => 
                    this.Services().Message.ShowMessage(string.Empty,
                        Settings.DefaultPhoneNumberDisplay,
                        this.Services().Localize["CallButton"],
                        () => _phoneService.Call(Settings.DefaultPhoneNumber),
                        this.Services().Localize["Cancel"], 
                        () => {}));
            }
        }

		public ICommand Unpair
		{
			get
			{
				return this.GetCommand(async () =>
				{
					var message = Order.PromoCode.HasValue()
						? this.Services().Localize["UnpairWarningMessageWithPromo"]
						: this.Services().Localize["UnpairWarningMessage"];

					this.Services().Message.ShowMessage(
						this.Services().Localize["WarningTitle"], 
						message, 
						this.Services().Localize["UnpairWarningCancelButton"],
						async () => {
							var response = await _paymentService.Unpair(Order.Id);

							if(response.IsSuccessful)
							{
								RefreshStatus();
							}
							else
							{
								this.Services().Message.ShowMessage(this.Services().Localize["CmtRideLinqErrorTitle"], this.Services().Localize["UnpairErrorMessage"]);
							}
						},
						this.Services().Localize["Cancel"], () => {});
				});
			}
		}

        public ICommand EditAutoTipCommand
        {
            get
            {
                return this.GetCommand(() =>
                {
                    ShowViewModel<EditAutoTipViewModel>();
                });
            }
        }

	    public ICommand SendMessageToDriverCommand
	    {
	        get
	        {
	            return this.GetCommand(async () =>
	            {
                    var message = await this.Services().Message.ShowPromptDialog(
                        this.Services().Localize["MessageToDriverTitle"],
                        string.Empty,
						() => { return; });

	                var messageSent = await _vehicleService.SendMessageToDriver(message, _vehicleNumber);
	                if (!messageSent)
	                {
                        this.Services().Message.ShowMessage(
                            this.Services().Localize["Error"],
                            this.Services().Localize["SendMessageToDriverErrorMessage"]);
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
            get { return _isUnpairButtonVisible; }
            set
            {
                _isUnpairButtonVisible = value;
				RaisePropertyChanged();
            }
        }

        private bool _canEditAutoTip;
        public bool CanEditAutoTip
        {
            get { return _canEditAutoTip; }
            set
            {
                if (_canEditAutoTip != value)
                {
                    _canEditAutoTip = value;
                    RaisePropertyChanged();
                }
            }
        }

	    #endregion
    }
}
