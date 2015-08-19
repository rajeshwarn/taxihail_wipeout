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
using apcurium.MK.Common;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using ServiceStack.Text;
using apcurium.MK.Booking.Maps;
using System.Net;
using apcurium.MK.Booking.Mobile.PresentationHints;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using Cirrious.MvvmCross.Platform;
using ServiceStack.ServiceClient.Web;
using apcurium.MK.Booking.Mobile.Data;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public sealed class BookingStatusViewModel : PageViewModel
    {
		private readonly IPhoneService _phoneService;
		private readonly IBookingService _bookingService;
		private readonly IVehicleService _vehicleService;
		private readonly IOrderWorkflowService _orderWorkflowService;

        private int _refreshPeriod = 5;              // in seconds
        private bool _waitingToNavigateAfterTimeOut;
        private string _vehicleNumber;
        private bool _isDispatchPopupVisible;
        private bool _isContactingNextCompany;
        private int? _currentIbsOrderId;

		public BookingStatusBottomBarViewModel BottomBar
		{
			get { return _bottomBar; }
			set
			{
				_bottomBar = value; 
				RaisePropertyChanged();
			}
		}

		public BookingStatusViewModel(IPhoneService phoneService, IBookingService bookingService, IVehicleService vehicleService, IOrderWorkflowService orderWorkflowService)
		{
		    _phoneService = phoneService;
			_bookingService = bookingService;
		    _vehicleService = vehicleService;
			_orderWorkflowService = orderWorkflowService;

			BottomBar = AddChild<BookingStatusBottomBarViewModel>();
		}
		
		public void StartBookingStatus(Order order, OrderStatusDetail orderStatusDetail)
		{
			Order = order;
			OrderStatusDetail = orderStatusDetail;
			DisplayOrderNumber();

			StatusInfoText = string.Format(this.Services().Localize["Processing"]);

			BottomBar.IsCancelButtonVisible = false;
			_waitingToNavigateAfterTimeOut = false;

			_orderWorkflowService.SetAddresses(order.PickupAddress, order.DropOffAddress);

			_refreshPeriod = Settings.OrderStatus.ClientPollingInterval;

			_subscriptions.Disposable = Observable.Timer(TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(_refreshPeriod))
				.ObserveOn(SynchronizationContext.Current)
				.Subscribe(_ => RefreshStatus(), Logger.LogError);
		}

		public void StopBookingStatus()
		{
			_subscriptions.Disposable = null;

			Order = null;
			OrderStatusDetail = null;

			_orderWorkflowService.PrepareForNewOrder();

			_vehicleService.SetAvailableVehicle(true);

			MapCenter = null;
		}

		private readonly SerialDisposable _subscriptions = new SerialDisposable();

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

		public bool IsContactTaxiVisible
		{
			get
			{
				return IsCallTaxiVisible
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

				bool isOrderStatusValid = OrderStatusDetail.IBSStatusId == VehicleStatuses.Common.Assigned
					|| OrderStatusDetail.IBSStatusId == VehicleStatuses.Common.Arrived
					|| OrderStatusDetail.IBSStatusId == VehicleStatuses.Common.Loaded;

				return Settings.ShowCallDriver && isOrderStatusValid && OrderStatusDetail.DriverInfos.MobilePhone.HasValue(); 
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

        public bool DriverPhotoHidden
        {
            get { return (string.IsNullOrWhiteSpace(OrderStatusDetail.DriverInfos.DriverPhotoUrl) || !IsDriverInfoAvailable); }
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
			set {
				_orderStatusDetail = value;
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
				RaisePropertyChanged(() => IsContactTaxiVisible);
				RaisePropertyChanged(() => CanGoBack);
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

		private bool CanRefreshStatus(OrderStatusDetail status)
		{
			return status.IBSOrderId.HasValue		// we can exit this loop only if we are assigned an IBSOrderId 
				|| status.IBSStatusId.HasValue();	// or if we get an IBSStatusId
		}

		private bool _refreshStatusIsExecuting;
		private BookingStatusBottomBarViewModel _bottomBar;

		public async void RefreshStatus()
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

				if(Settings.ShowEta 
					&& status.IBSStatusId.SoftEqual(VehicleStatuses.Common.Assigned) 
					&& status.VehicleNumber.HasValue()
					&& status.VehicleLatitude.HasValue
					&& status.VehicleLongitude.HasValue)
				{
					var d =  await _vehicleService.GetEtaBetweenCoordinates(status.VehicleLatitude.Value, status.VehicleLongitude.Value, Order.PickupAddress.Latitude, Order.PickupAddress.Longitude).ConfigureAwait(false);
					statusInfoText += " " + FormatEta(d);						
				}

				if (status.IBSStatusId.SoftEqual(VehicleStatuses.Common.Assigned) 
					&& status.VehicleNumber.HasValue() 
					&& status.VehicleLatitude.HasValue 
					&& status.VehicleLongitude.HasValue)
				{
					_vehicleService.SetAvailableVehicle(false);
				}

				StatusInfoText = statusInfoText;
                OrderStatusDetail = status;

                CenterMapIfNeeded ();

                BottomBar.UpdateActionsPossibleOnOrder(status.IBSStatusId);

                DisplayOrderNumber();

                if (isDone) 
                {
                    this.Services().MessengerHub.Publish(new OrderStatusChanged(this, status.OrderId, OrderStatus.Completed, null));
					GoToSummary();
                }

				if (_bookingService.IsStatusTimedOut(status.IBSStatusId)
					|| status.IBSStatusId.SoftEqual(VehicleStatuses.Common.CancelledDone))
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
				Logger.LogMessage("RefreshStatus ends");
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
	            ConfirmationNoTxt = string.Format(this.Services().Localize["StatusDescription"], OrderStatusDetail.IBSOrderId.Value);
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

		public void GoToSummary()
		{
			Logger.LogMessage ("GoToSummary");

			var @params = new
			{
				order = Order.ToJson(),
				orderStatus = OrderStatusDetail.ToJson()
			};

			StopBookingStatus();

			ShowViewModelAndRemoveFromHistory<RideSummaryViewModel> (@params.ToSimplePropertyDictionary());

			((HomeViewModel)Parent).CurrentViewState = HomeViewModelState.Initial;
		}

        private async void GoToBookingScreen()
		{
            if (!_waitingToNavigateAfterTimeOut)
            {
				_waitingToNavigateAfterTimeOut = true;
				await Task.Delay (TimeSpan.FromSeconds (10));
				ReturnToInitialState();
            }
        }

		public void ReturnToInitialState()
		{
			StopBookingStatus();

			_bookingService.ClearLastOrder();

			var homeViewModel = Parent as HomeViewModel;

			homeViewModel.CurrentViewState = HomeViewModelState.Initial;
			homeViewModel.AutomaticLocateMeAtPickup.ExecuteIfPossible();
		}

		private void CenterMapIfNeeded()
        {   
			if (Order == null) 
			{
				return;
			}

			if (VehicleStatuses.Common.Assigned.Equals(OrderStatusDetail.IBSStatusId) 
				&& OrderStatusDetail.VehicleLatitude.HasValue 
				&& OrderStatusDetail.VehicleLongitude.HasValue
				&& MapCenter == null)
			{
				var pickup = CoordinateViewModel.Create(Order.PickupAddress.Latitude, Order.PickupAddress.Longitude, true);
				var vehicle = CoordinateViewModel.Create(OrderStatusDetail.VehicleLatitude.Value, OrderStatusDetail.VehicleLongitude.Value);
				MapCenter = new[] { pickup, vehicle };

				return;
			}

			if (!VehicleStatuses.Common.Assigned.Equals(OrderStatusDetail.IBSStatusId))
			{
				MapCenter = new CoordinateViewModel[0];
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

	    #endregion
    }
}
