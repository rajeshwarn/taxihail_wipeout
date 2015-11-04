using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Common;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.Configuration.Impl;
using System.Reactive;
using apcurium.MK.Booking.Mobile.PresentationHints;
using apcurium.MK.Booking.Mobile.ViewModels.Map;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using ServiceStack.ServiceClient.Web;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Booking.Mobile.Infrastructure.DeviceOrientation;
using apcurium.MK.Booking.Mobile.Models;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public sealed class BookingStatusViewModel : BaseViewModel
    {
        private readonly IPhoneService _phoneService;
        private readonly IBookingService _bookingService;
        private readonly IVehicleService _vehicleService;
        private readonly IMetricsService _metricsService;
        private readonly IPaymentService _paymentService;
        private readonly IOrderWorkflowService _orderWorkflowService;
        private readonly ILocationService _locationService;
        private readonly IOrientationService _orientationService;
        private readonly SerialDisposable _subscriptions = new SerialDisposable();

        private int _refreshPeriod = 5; // in seconds
        private string _vehicleNumber;
        private bool _isDispatchPopupVisible;
        private bool _isContactingNextCompany;
        private int? _currentIbsOrderId;
        private bool _canAutoFollowTaxi;
        private bool _autoFollowTaxi;

        private bool _isCmtRideLinq;

        private bool _isStarted;

        private bool _isOrderRefreshing;

        public static WaitingCarLandscapeViewModelParameters WaitingCarLandscapeViewModelParameters { get; set; }

        public BookingStatusViewModel(
            IPhoneService phoneService,
            IBookingService bookingService,
            IVehicleService vehicleService,
            IPaymentService paymentService,
            IMetricsService metricsService,
            IOrderWorkflowService orderWorkflowService,
            IOrientationService orientationService,
            ILocationService locationService)
        {
            _orderWorkflowService = orderWorkflowService;
            _phoneService = phoneService;
            _bookingService = bookingService;
            _paymentService = paymentService;
            _vehicleService = vehicleService;
            _metricsService = metricsService;
            _locationService = locationService;
            _orientationService = orientationService;

            BottomBar = AddChild<BookingStatusBottomBarViewModel>();

            GetIsCmtRideLinq();

            _orientationService.NotifyOrientationChanged += DeviceOrientationChanged;
            _orientationService.Initialize(new[] { DeviceOrientations.Right, DeviceOrientations.Left });
        }

        /// <summary>
        /// eHail
        /// </summary>
        public void StartBookingStatus(Order order, OrderStatusDetail orderStatusDetail)
        {
            if (_isStarted)
            {
                return;
            }
            _isStarted = true;

            Order = order;
            OrderStatusDetail = orderStatusDetail;
            DisplayOrderNumber();

            if (orderStatusDetail.IBSStatusId.HasValue())
            {
                BottomBar.NotifyBookingStatusAppbarChanged();
            }
            else
            {
                BottomBar.PrepareForNewOrder();
            }

            CenterMapOnPinsIfNeeded();

            StatusInfoText = orderStatusDetail.IBSStatusId == null
                ? this.Services().Localize["Processing"]
                : orderStatusDetail.IBSStatusDescription;

            _orderWorkflowService.SetAddresses(order.PickupAddress, order.DropOffAddress);

            _subscriptions.Disposable = GetTimerObservable()
                .ObserveOn(SynchronizationContext.Current)
                .Where(_ => !_isOrderRefreshing)
                .SelectMany(async (_, cancellationToken) =>
                {
                    _isOrderRefreshing = true;
                    await RefreshStatus(cancellationToken);
                    _isOrderRefreshing = false;
                    return Unit.Default;
                })
                .Subscribe(
                    _ => { },
                    ex =>
                    {
                        Logger.LogMessage("An unhandled error occurred in the eHail BookingStatus observable");
                        Logger.LogError(ex);
                    },
                    () => Logger.LogMessage("eHail: BookingStatus Observable triggered OnCompleted")
                );
        }

        /// <summary>
        /// Manual RideLinQ
        /// </summary>
        public void StartBookingStatus(OrderManualRideLinqDetail orderManualRideLinqDetail, bool isRestoringFromBackground = false)
        {
            if (_isStarted)
            {
                return;
            }
            _isStarted = true;

            BottomBar.PrepareForNewOrder();

            if (isRestoringFromBackground)
            {
                ((HomeViewModel)Parent).AutomaticLocateMeAtPickup.ExecuteIfPossible();
            }

            var subscriptions = new CompositeDisposable();

            // Manual RideLinQ status observable
            GetTimerObservable()
                .ObserveOn(SynchronizationContext.Current)
                .SelectMany(_ => GetManualRideLinqDetails())
                .StartWith(orderManualRideLinqDetail)
                .Do(RefreshManualRideLinqDetails)
                .Where(orderDetails => orderDetails != null && (orderDetails.EndTime.HasValue || orderDetails.PairingError.HasValue()))
                .Take(1) // trigger only once
                .SelectMany(async orderDetails =>
                {
                    try
                    {
                        if (orderDetails.PairingError.HasValue())
                        {
                            Logger.LogMessage("A pairing error occurred in manual RideLinQ trip. Going back home...");

                            await GoToHomeScreen();
                        }
                        else
                        {
                            GoToRideSummary(orderDetails.OrderId);
                        }

                        return orderDetails;
                    }
                    catch (Exception ex)
                    {
                        Logger.LogMessage("An error occurred while trying to end a manual RideLinQ trip");
                        Logger.LogError(ex);
                        return null;
                    }
                })
                .Subscribe(_ => { },
                    ex =>
                    {
                        Logger.LogMessage("An unhandled error occurred in the manual RideLinQ BookingStatus observable");
                        Logger.LogError(ex);
                    },
                    () => Logger.LogMessage("Manual RideLinQ: BookingStatus Observable triggered OnCompleted"))
                .DisposeWith(subscriptions);

            var deviceLocationObservable = Observable.Timer(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(2))
                .Where(_ => ManualRideLinqDetail != null && ManualRideLinqDetail.Medallion.HasValue())
                .SelectMany(_ => _locationService.GetUserPosition())
                .Select(pos =>
                {
                    if (pos == null)
                    {
                        return null;
                    }

                    return new PairedTaxiPosition
                    {
                        Latitude = pos.Latitude,
                        Longitude = pos.Longitude
                    };
                });

            var orderId = orderManualRideLinqDetail.OrderId;
            var deviceName = orderManualRideLinqDetail.DeviceName;
            var taxilocationViaGeo = GetAndObserveTaxiLocationViaGeo(orderManualRideLinqDetail.DeviceName, orderId);

            _orderWorkflowService.GetAndObserveIsUsingGeo()
                .DistinctUntilChanged()
                .SelectMany(isUsingGeo => isUsingGeo && deviceName.HasValue()
                    ? taxilocationViaGeo
                    : deviceLocationObservable
                )
                .Where(pos => pos != null)
                .ObserveOn(SynchronizationContext.Current)
                .Do(
                    pos =>
                        UpdatePosition(pos.Latitude, pos.Longitude, orderManualRideLinqDetail.Medallion, pos.Market,
                            CancellationToken.None))
                .Subscribe(_ => CenterMapIfNeeded(), Logger.LogError)
                .DisposeWith(subscriptions);

            _locationService.Start();

            Disposable.Create(_locationService.Stop).DisposeWith(subscriptions);

            _subscriptions.Disposable = subscriptions;

            BottomBar.NotifyBookingStatusAppbarChanged();
        }

        private void StopBookingStatus()
        {
            if (!_isStarted)
            {
                return;
            }

            _subscriptions.Disposable = null;

            _canAutoFollowTaxi = false;
            _autoFollowTaxi = false;

            Order = null;
            OrderStatusDetail = null;

            ManualRideLinqDetail = null;

            TaxiLocation = null;

            _bookingService.ClearLastOrder();
            _orderWorkflowService.PrepareForNewOrder();

            _vehicleService.SetAvailableVehicle(true);

            MapCenter = null;

            StopOrientationServiceIfNeeded();

            _isStarted = false;
        }

        private async void GetIsCmtRideLinq()
        {
            try
            {
                var paymentSettings = await _paymentService.GetPaymentSettings();

                _isCmtRideLinq = paymentSettings.PaymentMode == PaymentMethod.RideLinqCmt;

                RefreshView();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }

        // Method used when we are restoring from background to zoom back to the order's location correctly.
        private void CenterMapOnPinsIfNeeded()
        {
            // Handle case where we are waiting for a taxi.
            if (OrderStatusDetail.IBSStatusId == VehicleStatuses.Common.Waiting)
            {
                ((HomeViewModel)Parent).AutomaticLocateMeAtPickup.ExecuteIfPossible();

                return;
            }

            // Handle case where order is in a state where we should not do anything anyway.
            if (VehicleStatuses.ShowOnMapStatuses.None(status => status == OrderStatusDetail.IBSStatusId))
            {
                return;
            }

            // We have no vehicle positions to display, so don't try to.
            if (!OrderStatusDetail.VehicleLatitude.HasValue || !OrderStatusDetail.VehicleLongitude.HasValue)
            {
                return;
            }

            // Handle case where we have both the taxi and the pickup point.
            if (OrderStatusDetail.IBSStatusId == VehicleStatuses.Common.Assigned || OrderStatusDetail.IBSStatusId == VehicleStatuses.Common.Arrived)
            {
                MapCenter = new[]
				{
					CoordinateViewModel.Create(Order.PickupAddress.Latitude, Order.PickupAddress.Longitude, true),
					CoordinateViewModel.Create(OrderStatusDetail.VehicleLatitude.Value, OrderStatusDetail.VehicleLongitude.Value)
				};

                return;
            }

            // Handle case where the user is in a taxi.

            MapCenter = new[]
			{
				CoordinateViewModel.Create(OrderStatusDetail.VehicleLatitude.Value, OrderStatusDetail.VehicleLongitude.Value, true)
			};

        }

        /// <summary>
        /// Gets and observe the taxilocation via Geo when using Manual Pairing.
        /// </summary>
        /// <remarks>
        /// Must only use this via manual pairing.
        /// 
        /// This observable will also fallback to the device's location if Geo is not available for any reason.
        /// </remarks>
        private IObservable<PairedTaxiPosition> GetAndObserveTaxiLocationViaGeo(string medallion, Guid orderId)
        {
            return _vehicleService.GetAndObserveCurrentTaxiLocation(medallion, orderId)
                .Materialize()
                .SelectMany(async notif =>
                {
                    //Fallback in case of errors from the GeoService call
                    if ((notif.Kind == NotificationKind.OnError) || (notif.Kind == NotificationKind.OnNext && notif.Value == null))
                    {
                        var fallbackPosition = await _locationService.GetUserPosition();

                        if (fallbackPosition == null)
                        {
                            return Notification.CreateOnNext<PairedTaxiPosition>(null);
                        }

                        var value = new PairedTaxiPosition
                        {
                            Latitude = fallbackPosition.Latitude,
                            Longitude = fallbackPosition.Longitude,
                        };

                        return Notification.CreateOnNext(value);
                    }

                    if (notif.Kind == NotificationKind.OnNext && notif.Value != null)
                    {
                        var position = new PairedTaxiPosition
                        {
                            Longitude = notif.Value.Longitude,
                            Latitude = notif.Value.Latitude,
                            Orientation = notif.Value.CompassCourse,
                            Market = notif.Value.Market
                        };

                        return Notification.CreateOnNext(position);
                    }

                    return Notification.CreateOnCompleted<PairedTaxiPosition>();
                })
                .Dematerialize();
        }

        private void UpdatePosition(double latitude, double longitude, string medallion, string market, CancellationToken token, double? compassCourse = null)
        {
            token.ThrowIfCancellationRequested();

            if (TaxiLocation != null && TaxiLocation.Latitude == latitude && TaxiLocation.Longitude == longitude)
            {
                //Nothing to update.
                return;
            }

            if (TaxiLocation == null)
            {
                TaxiLocation = new TaxiLocation
                {
                    Longitude = longitude,
                    Latitude = latitude,
                    VehicleNumber = medallion,
                    CompassCourse = compassCourse,
                    Market = market
                };
            }
            else
            {
                TaxiLocation.Latitude = latitude;
                TaxiLocation.Longitude = longitude;
                TaxiLocation.CompassCourse = compassCourse;

                RaisePropertyChanged(() => TaxiLocation);
            }

        }

        private IObservable<Unit> GetTimerObservable()
        {
            _refreshPeriod = Settings.OrderStatus.ClientPollingInterval;

            return Observable.Timer(TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(_refreshPeriod))
                .Select(_ => Unit.Default);
        }

        private void StopOrientationServiceIfNeeded()
        {
            if (_orientationService.Stop())
            {
                if (WaitingCarLandscapeViewModelParameters != null)
                {
                    WaitingCarLandscapeViewModelParameters.CloseWaitingWindow();
                    WaitingCarLandscapeViewModelParameters = null;
                }
            }
        }

        private async Task<OrderManualRideLinqDetail> GetManualRideLinqDetails()
        {
            try
            {
                var res = await _bookingService.GetTripInfoFromManualRideLinq(ManualRideLinqDetail.OrderId);

                if (res.IsSuccessful)
                {
                    return res.Data;
                }

                Logger.LogMessage("GetTripInfo for ManualRideLinqDetail returned an error: " + res.Message);
                return null;
            }
            catch (Exception ex)
            {
                Logger.LogMessage("An error occurred when trying to get the trip info for ManualRideLinQ.");
                Logger.LogError(ex);

                return null;
            }
        }

        private void RefreshManualRideLinqDetails(OrderManualRideLinqDetail manualRideLinqDetails)
        {
            if (manualRideLinqDetails == null)
            {
                return;
            }

            try
            {
                ManualRideLinqDetail = manualRideLinqDetails;

                ConfirmationNoTxt = string.Format(this.Services().Localize["StatusDescription"], manualRideLinqDetails.TripId);

                var localize = this.Services().Localize;

                if (!_canAutoFollowTaxi)
                {
                    _canAutoFollowTaxi = true;
                    _autoFollowTaxi = true;
                }

                if (manualRideLinqDetails.PairingError.HasValue())
                {
                    StatusInfoText = "{0}".InvariantCultureFormat(localize["ManualRideLinqStatus_PairingError"]);
                }

                StatusInfoText = "{0}".InvariantCultureFormat(localize["OrderStatus_PairingSuccess"]);
            }
            catch (Exception ex)
            {
                Logger.LogMessage("An error occurred while refreshing the manual RideLinQ details");
                Logger.LogError(ex);
            }
        }

        #region Bindings
        public OrderManualRideLinqDetail ManualRideLinqDetail
        {
            get { return _manualRideLinqDetail; }
            set
            {
                _manualRideLinqDetail = value;
                RaisePropertyChanged();
                BottomBar.NotifyBookingStatusAppbarChanged();
            }
        }

        public TaxiLocation TaxiLocation
        {
            get { return _taxiLocation; }
            set
            {
                if (_taxiLocation == value)
                {
                    return;
                }

                _taxiLocation = value;
                RaisePropertyChanged();
            }
        }

        public BookingStatusBottomBarViewModel BottomBar
        {
            get { return _bottomBar; }
            set
            {
                _bottomBar = value;
                RaisePropertyChanged();
            }
        }

        private IEnumerable<CoordinateViewModel> _mapCenter;
        public IEnumerable<CoordinateViewModel> MapCenter
        {
            get { return _mapCenter; }
            private set
            {
                _mapCenter = value;
                RaisePropertyChanged();
            }
        }


        private string _confirmationNoTxt;
        public string ConfirmationNoTxt
        {
            get { return _confirmationNoTxt; }
            set
            {
                _confirmationNoTxt = value;
                RaisePropertyChanged();
                RaisePropertyChanged(() => IsConfirmationNoHidden);
            }
        }

        public bool IsContactTaxiVisible
        {
            get
            {
                return (IsCallTaxiVisible || IsMessageTaxiVisible)
                    && (OrderStatusDetail.IBSStatusId == VehicleStatuses.Common.Assigned
                        || OrderStatusDetail.IBSStatusId == VehicleStatuses.Common.Arrived);
            }
        }

        public bool IsCallTaxiVisible
        {
            get
            {
                if (OrderStatusDetail == null || OrderStatusDetail.DriverInfos == null)
                {
                    return false;
                }

                return Settings.ShowCallDriver
                    && OrderStatusDetail.DriverInfos.MobilePhone.HasValue()
                    && (OrderStatusDetail.IBSStatusId == VehicleStatuses.Common.Assigned
                        || OrderStatusDetail.IBSStatusId == VehicleStatuses.Common.Arrived);
            }
        }

        public bool IsMessageTaxiVisible
        {
            get
            {
                if (OrderStatusDetail == null)
                {
                    return false;
                }

                return Settings.ShowMessageDriver
                    && OrderStatusDetail.VehicleNumber.HasValue()
                    && (OrderStatusDetail.IBSStatusId == VehicleStatuses.Common.Assigned
                        || OrderStatusDetail.IBSStatusId == VehicleStatuses.Common.Arrived);
            }
        }

        public bool IsDriverInfoAvailable
        {
            get
            {
                if (OrderStatusDetail == null)
                {
                    return false;
                }

                var showVehicleInformation = Settings.ShowVehicleInformation;
                var isOrderStatusValid = OrderStatusDetail.IBSStatusId == VehicleStatuses.Common.Assigned
                    || OrderStatusDetail.IBSStatusId == VehicleStatuses.Common.Arrived
                    || OrderStatusDetail.IBSStatusId == VehicleStatuses.Common.Loaded;
                var hasDriverInformation = OrderStatusDetail.DriverInfos.FullVehicleInfo.HasValue()
                    || OrderStatusDetail.DriverInfos.FullName.HasValue();

                return showVehicleInformation && isOrderStatusValid && hasDriverInformation;
            }
        }

        public bool CompanyHidden
        {
            get { return !IsDriverInfoAvailable || string.IsNullOrWhiteSpace(OrderStatusDetail.CompanyName); }
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

        public bool VehicleFullInfoHidden
        {
            get
            {
                return IsInformationHidden(OrderStatusDetail.DriverInfos.FullVehicleInfo);
            }
        }

        public bool DriverPhotoHidden
        {
            get
            {
                return !IsDriverInfoAvailable || string.IsNullOrWhiteSpace(OrderStatusDetail.DriverInfos.DriverPhotoUrl);
            }
        }

        public bool CanGoBack
        {
            get
            {
                if (Order == null || OrderStatusDetail == null)
                {
                    return true;
                }

                return // we know from the start it's a scheduled
                        (Order.CreatedDate != Order.PickupDate
                            && !OrderStatusDetail.IBSStatusId.HasValue())
                    // it has the status scheduled
                        || OrderStatusDetail.IBSStatusId.SoftEqual(VehicleStatuses.Common.Scheduled)
                    // it is cancelled or no show
                        || (OrderStatusDetail.IBSStatusId.SoftEqual(VehicleStatuses.Common.Cancelled)
                            || OrderStatusDetail.IBSStatusId.SoftEqual(VehicleStatuses.Common.NoShow)
                            || OrderStatusDetail.IBSStatusId.SoftEqual(VehicleStatuses.Common.CancelledDone))
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
            RaisePropertyChanged(() => VehicleFullInfoHidden);
            RaisePropertyChanged(() => DriverPhotoHidden);
            RaisePropertyChanged(() => IsDriverInfoAvailable);
            RaisePropertyChanged(() => IsCallTaxiVisible);
            RaisePropertyChanged(() => IsMessageTaxiVisible);
            RaisePropertyChanged(() => IsContactTaxiVisible);
            RaisePropertyChanged(() => IsProgressVisible);
            RaisePropertyChanged(() => CanGoBack);
            RaisePropertyChanged(() => VehicleMedallionHidden);
        }

        public bool IsProgressVisible
        {
            get
            {
                return OrderStatusDetail != null && (string.IsNullOrEmpty(OrderStatusDetail.IBSStatusId)
                    || OrderStatusDetail.IBSStatusId == VehicleStatuses.Common.Waiting);
            }
        }

        public bool IsConfirmationNoHidden
        {
            get { return !ConfirmationNoTxt.HasValue(); }
        }

        public ICommand CallTaxiCommand
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

        private bool HasSeenReminderPrompt(Guid orderId)
        {
            var hasSeen = this.Services().Cache.Get<string>("OrderReminderWasSeen." + orderId);
            return !string.IsNullOrEmpty(hasSeen);
        }

        private void SetHasSeenReminderPrompt(Guid orderId)
        {
            this.Services().Cache.Set("OrderReminderWasSeen." + orderId, true.ToString());
        }

        private void AddReminder(OrderStatusDetail status)
        {
            if (!HasSeenReminderPrompt(status.OrderId)
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

        private BookingStatusBottomBarViewModel _bottomBar;
        private OrderManualRideLinqDetail _manualRideLinqDetail;
        private TaxiLocation _taxiLocation;


        private async Task RefreshStatus(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            try
            {
                var status = await _bookingService.GetOrderStatusAsync(Order.Id);

                if (status == null)
                {
                    Logger.LogMessage("Status for order {0} is not currently available.".InvariantCultureFormat(Order.Id));

                    return;
                }

                while (!CanRefreshStatus(status))
                {
                    Logger.LogMessage("Waiting for Ibs Order Creation (ibs order id)");
                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                    status = await _bookingService.GetOrderStatusAsync(Order.Id);

                    if (status.IBSOrderId.HasValue)
                    {
                        Logger.LogMessage("Received Ibs Order Id: {0}", status.IBSOrderId.Value);
                    }
                }

                cancellationToken.ThrowIfCancellationRequested();

                if (status.VehicleNumber != null)
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

                await SwitchDispatchCompanyIfNecessary(status);

                var isDone = _bookingService.IsStatusDone(status.IBSStatusId);

                if (status.IBSStatusId.SoftEqual(VehicleStatuses.Common.Scheduled))
                {
                    AddReminder(status);
                }

                if (status.IBSStatusId.SoftEqual(VehicleStatuses.Common.Assigned) || status.IBSStatusId.SoftEqual(VehicleStatuses.Common.Arrived))
                {
                    if (_orientationService.Start())
                    {
                        WaitingCarLandscapeViewModelParameters = null;
                    }
                }
                else
                {
                    StopOrientationServiceIfNeeded();
                }

                var statusInfoText = status.IBSStatusDescription;

                var isLocalMarket = await _orderWorkflowService.GetAndObserveHashedMarket()
                    .Select(hashedMarket => !hashedMarket.HasValue())
                    .Take(1);
                var hasVehicleInfo = status.VehicleNumber.HasValue()
                                     && status.VehicleLatitude.HasValue
                                     && status.VehicleLongitude.HasValue;

                var isUsingGeoServices = isLocalMarket
                    ? Settings.LocalAvailableVehiclesMode == LocalAvailableVehiclesModes.Geo
                    : Settings.ExternalAvailableVehiclesMode == ExternalAvailableVehiclesModes.Geo;

                cancellationToken.ThrowIfCancellationRequested();
                if (Settings.ShowEta
                    && status.IBSStatusId.SoftEqual(VehicleStatuses.Common.Assigned)
                    && (hasVehicleInfo || isUsingGeoServices))
                {
                    long? eta = null;

                    if (isUsingGeoServices && status.DriverInfos.VehicleRegistration.HasValue())
                    {
                        var geoData =
                            await
                                _vehicleService.GetVehiclePositionInfoFromGeo(Order.PickupAddress.Latitude, Order.PickupAddress.Longitude,
                                    status.DriverInfos.VehicleRegistration, status.OrderId);

                        if (geoData != null)
                        {
                            eta = geoData.Eta;

                            if (geoData.IsPositionValid)
                            {
                                UpdatePosition(geoData.Latitude.Value, geoData.Longitude.Value, status.VehicleNumber, geoData.Market, cancellationToken, geoData.CompassCourse ?? 0);
                            }
                        }
                    }
                    else
                    {
                        var direction =
                            await
                                _vehicleService.GetEtaBetweenCoordinates(status.VehicleLatitude.Value, status.VehicleLongitude.Value,
                                    Order.PickupAddress.Latitude, Order.PickupAddress.Longitude);

                        // Log original eta value
                        if (direction.IsValidEta())
                        {
                            _metricsService.LogOriginalRideEta(Order.Id, direction.Duration);
                        }

                        eta = direction.Duration;
                    }
                    if (eta.HasValue)
                    {
                        statusInfoText += " " + FormatEta(eta);
                    }
                }

                cancellationToken.ThrowIfCancellationRequested();

                // Needed to do this here since cmtGeoService needs the device's location to calculate the Eta and does not have the ability to get the position of a specific vehicle(or a bach of vehicle) without the device location.
                if (isUsingGeoServices &&
                    (status.IBSStatusId.SoftEqual(VehicleStatuses.Common.Loaded)
                     || status.IBSStatusId.SoftEqual(VehicleStatuses.Common.Arrived)))
                {
                    //refresh vehicle position on the map from the geo data
                    var geoData = await _vehicleService.GetVehiclePositionInfoFromGeo(
                        Order.PickupAddress.Latitude,
                        Order.PickupAddress.Longitude,
                        status.DriverInfos.VehicleRegistration,
                        Order.Id);

                    if (geoData != null && geoData.IsPositionValid)
                    {
                        UpdatePosition(geoData.Latitude.Value, geoData.Longitude.Value, status.VehicleNumber, geoData.Market, cancellationToken, geoData.CompassCourse ?? 0);
                    }
                }
                else if (!isUsingGeoServices && hasVehicleInfo && VehicleStatuses.ShowOnMapStatuses.Any(vehicleStatus => vehicleStatus == status.IBSStatusId))
                {
                    UpdatePosition(status.VehicleLatitude.Value, status.VehicleLongitude.Value, status.VehicleNumber, string.Empty, cancellationToken);
                }

                cancellationToken.ThrowIfCancellationRequested();

                if (status.IBSStatusId.SoftEqual(VehicleStatuses.Common.Assigned))
                {
                    _vehicleService.SetAvailableVehicle(false);
                }

                StatusInfoText = statusInfoText;
                OrderStatusDetail = status;

                // Starts autofollowing vehicle.
                if (status.IBSStatusId.SoftEqual(VehicleStatuses.Common.Loaded) && !_canAutoFollowTaxi)
                {
                    _canAutoFollowTaxi = true;
                    _autoFollowTaxi = true;
                }
                else if (VehicleStatuses.CompletedStatuses.Any(ibsStatus => ibsStatus.Equals(status.IBSStatusId)))
                {
                    _canAutoFollowTaxi = false;
                    _autoFollowTaxi = false;
                }

                CenterMapIfNeeded();

                BottomBar.NotifyBookingStatusAppbarChanged();

                DisplayOrderNumber();

                if (isDone)
                {
                    this.Services().MessengerHub.Publish(new OrderStatusChanged(this, status.OrderId, OrderStatus.Completed, null));
                    GoToRideSummary(status.OrderId);

                    return;
                }

                //This is to prevent issue where taxi pin would still stay shown if the taxi driver bailed.
                if (VehicleStatuses.Common.Waiting.Equals(status.IBSStatusId))
                {
                    TaxiLocation = null;
                }

                if (VehicleStatuses.CancelStatuses.Any(cancelledStatus => cancelledStatus.Equals(status.IBSStatusId)))
                {
                    await GoToHomeScreen();
                }
            }
            catch (OperationCanceledException)
            {
                Logger.LogMessage("RefreshStatus ended: BookingStatusView was stopped.");
            }
            catch (Exception ex)
            {
                Logger.LogMessage("RefreshStatus ended: an exception occurred.");
                Logger.LogError(ex);
            }
        }

        private void DeviceOrientationChanged(DeviceOrientations deviceOrientation)
        {
            var orderStatusDetail = OrderStatusDetail;
            if (orderStatusDetail == null)
            {
                return;
            }

            var carNumber = orderStatusDetail.VehicleNumber;

            if ((deviceOrientation == DeviceOrientations.Left || deviceOrientation == DeviceOrientations.Right) && carNumber.HasValueTrimmed())
            {
                if (WaitingCarLandscapeViewModelParameters == null || (WaitingCarLandscapeViewModelParameters != null && WaitingCarLandscapeViewModelParameters.WaitingWindowClosed))
                {
                    WaitingCarLandscapeViewModelParameters = new WaitingCarLandscapeViewModelParameters
                    {
                        CarNumber = carNumber,
                        DeviceOrientations = deviceOrientation
                    };
                    ShowViewModel<WaitingCarLandscapeViewModel>(WaitingCarLandscapeViewModelParameters);
                }
                else
                {
                    WaitingCarLandscapeViewModelParameters.UpdateModelParameters(deviceOrientation, orderStatusDetail.VehicleNumber);
                }
            }
        }

        private async Task SwitchDispatchCompanyIfNecessary(OrderStatusDetail status)
        {
            if (status.Status != OrderStatus.TimedOut)
            {
                return;
            }

            bool alwayAcceptSwitch;
            bool.TryParse(this.Services().Cache.Get<string>("TaxiHailNetworkTimeOutAlwayAccept"), out alwayAcceptSwitch);

            var isAutomaticallyHandlingTimeout = alwayAcceptSwitch
                    || Settings.Network.AutoConfirmFleetChange
                    || status.CompanyKey == status.NextDispatchCompanyKey;

            if (status.NextDispatchCompanyKey != null && isAutomaticallyHandlingTimeout)
            {
                // Switch without user input
                await HandleNetworkTimeout(status);

                return;
            }

            if (status.NextDispatchCompanyKey != null && !_isDispatchPopupVisible && !isAutomaticallyHandlingTimeout)
            {
                _isDispatchPopupVisible = true;

                var tcs = new TaskCompletionSource<Unit>();

                await this.Services().Message.ShowMessage(
                    this.Services().Localize["TaxiHailNetworkTimeOutPopupTitle"],
                    string.Format(this.Services().Localize["TaxiHailNetworkTimeOutPopupMessage"], status.NextDispatchCompanyName),
                    this.Services().Localize["TaxiHailNetworkTimeOutPopupAccept"],
                    async () =>
                    {
                        await HandleNetworkTimeout(status);

                        tcs.SetResult(Unit.Default);
                    },
                    this.Services().Localize["TaxiHailNetworkTimeOutPopupRefuse"],
                    () =>
                    {
                        if (status.Status.Equals(OrderStatus.TimedOut))
                        {
                            _bookingService.IgnoreDispatchCompanySwitch(status.OrderId);
                            _isDispatchPopupVisible = false;
                        }

                        tcs.SetResult(Unit.Default);
                    },
                    this.Services().Localize["TaxiHailNetworkTimeOutPopupAlways"],
                    async () =>
                    {
                        this.Services().Cache.Set("TaxiHailNetworkTimeOutAlwayAccept", "true");
                        await HandleNetworkTimeout(status);

                        tcs.SetResult(Unit.Default);
                    });

                await tcs.Task;
            }
        }

        private async Task HandleNetworkTimeout(OrderStatusDetail status)
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

                if (orderStatusDetail.IBSStatusId == VehicleStatuses.Common.Timeout)
                {
                    StatusInfoText = orderStatusDetail.IBSStatusDescription;

                    BottomBar.NotifyBookingStatusAppbarChanged();

                    await GoToHomeScreen();
                    return;
                }
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
            catch (Exception ex)
            {
                Logger.LogError(ex);

                _isContactingNextCompany = false;
            }
        }

        private void DisplayOrderNumber()
        {
            if (OrderStatusDetail.IBSOrderId.HasValue)
            {
                ConfirmationNoTxt = string.Format(this.Services().Localize["StatusDescription"], OrderStatusDetail.IBSOrderId.Value);
            }
        }

        string FormatEta(long? eta)
        {
            if (!eta.HasValue)
            {
                return string.Empty;
            }

            var durationUnit = eta.Value <= 1 ? this.Services().Localize["EtaDurationUnit"] : this.Services().Localize["EtaDurationUnitPlural"];
            var etaDuration = eta.Value < 1 ? 1 : eta.Value;
            return string.Format(this.Services().Localize["StatusEta"], etaDuration, durationUnit);
        }

        private void GoToRideSummary(Guid orderId)
        {
            Logger.LogMessage("GoToSummary");

            ShowViewModel<RideSummaryViewModel>(new { orderId = orderId });

            ReturnToInitialState();
        }

        private async Task GoToHomeScreen()
        {
            await Task.Delay(TimeSpan.FromSeconds(10));
            ReturnToInitialState();
        }

        public void ReturnToInitialState()
        {
            StopBookingStatus();

            var homeViewModel = (HomeViewModel)Parent;

            homeViewModel.CurrentViewState = HomeViewModelState.Initial;
            homeViewModel.AutomaticLocateMeAtPickup.ExecuteIfPossible();
        }

        private void CenterMapIfNeeded()
        {

            var hasValidVehiclePosition = TaxiLocation != null &&
                TaxiLocation.Latitude.HasValue &&
                TaxiLocation.Longitude.HasValue;

            var isVehicleAssigned = OrderStatusDetail.SelectOrDefault(orderStatusDetail => orderStatusDetail.IBSStatusId.SoftEqual(VehicleStatuses.Common.Assigned));

            if (Order != null
                && isVehicleAssigned
                && hasValidVehiclePosition
                && !MapCenter.HasValue()
                )
            {
                var pickup = CoordinateViewModel.Create(Order.PickupAddress.Latitude, Order.PickupAddress.Longitude, true);
                var vehicle = CoordinateViewModel.Create(TaxiLocation.Latitude.Value, TaxiLocation.Longitude.Value);
                MapCenter = new[] { pickup, vehicle };

                return;
            }

            if (hasValidVehiclePosition && _canAutoFollowTaxi && _autoFollowTaxi)
            {
                var vehicle = CoordinateViewModel.Create(TaxiLocation.Latitude.Value, TaxiLocation.Longitude.Value);
                MapCenter = new[] { vehicle };

                return;
            }

            if (!isVehicleAssigned)
            {
                MapCenter = new CoordinateViewModel[0];
            }

        }

        #region Commands

        public ICommand CancelAutoFollow
        {
            get
            {
                return this.GetCommand(() => _autoFollowTaxi = false, CanCancelAutoFollowTaxi);
            }
        }

        private bool CanCancelAutoFollowTaxi()
        {
            return _canAutoFollowTaxi && _autoFollowTaxi;
        }

        public ICommand NewRide
        {
            get
            {
                return this.GetCommand(() => this.Services().Message.ShowMessage(
                    this.Services().Localize["StatusNewRideButton"],
                    this.Services().Localize["StatusConfirmNewBooking"],
                    this.Services().Localize["YesButton"],
                    () =>
                    {
                        _bookingService.ClearLastOrder();
                        ShowViewModel<HomeViewModel>(new { locateUser = true });
                    },
                    this.Services().Localize["NoButton"], delegate { }));
            }
        }

        public ICommand PrepareNewOrder
        {
            get
            {
                return this.GetCommand(async () =>
                {
                    _bookingService.ClearLastOrder();

                    var address = await _orderWorkflowService.SetAddressToUserLocation();
                    if (address.HasValidCoordinate())
                    {
                        ChangePresentation(new ZoomToStreetLevelPresentationHint(address.Latitude, address.Longitude));
                    }
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
                        () => { });

                    var messageSent = await _vehicleService.SendMessageToDriver(message, _vehicleNumber, Order.Id);
                    if (!messageSent)
                    {
                        this.Services().Message.ShowMessage(
                            this.Services().Localize["Error"],
                            this.Services().Localize["SendMessageToDriverErrorMessage"]);
                    }
                });
            }
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Ensuring that the subcription is disposed correctly to prevent memory leak.
                _subscriptions.Disposable = null;
            }

            base.Dispose(disposing);
        }
    }
}
