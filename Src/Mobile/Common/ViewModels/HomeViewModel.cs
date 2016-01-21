using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.PresentationHints;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using apcurium.MK.Common.Entity;
using Cirrious.MvvmCross.Platform;
using Cirrious.MvvmCross.Plugins.WebBrowser;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.Configuration.Impl;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public sealed class HomeViewModel : PageViewModel
    {
		private readonly IOrderWorkflowService _orderWorkflowService;
		private readonly ILocationService _locationService;
		private readonly ITutorialService _tutorialService;
		private readonly IPushNotificationService _pushNotificationService;
		private readonly IVehicleService _vehicleService;
		private readonly ITermsAndConditionsService _termsService;
	    private readonly IMvxLifetime _mvxLifetime;
		private readonly IAccountService _accountService;
		private readonly IBookingService _bookingService;
	    private readonly IMetricsService _metricsService;
	    private readonly IPaymentService _paymentService;
		private readonly INetworkRoamingService _networkRoamingService;
	    private string _lastHashedMarket = string.Empty;
		private bool _isShowingTermsAndConditions;
		private bool _isShowingCreditCardExpiredPrompt;
		private bool _locateUser;
		private bool _isShowingTutorial;
		private ZoomToStreetLevelPresentationHint _defaultHintZoomLevel;

		private HomeViewModelState _currentViewState = HomeViewModelState.Initial;

		public HomeViewModel(IOrderWorkflowService orderWorkflowService, 
			IMvxWebBrowserTask browserTask,
			ILocationService locationService,
			ITutorialService tutorialService,
			IPushNotificationService pushNotificationService,
			IVehicleService vehicleService,
			IAccountService accountService,
			IPhoneService phoneService,
			ITermsAndConditionsService termsService,
			IPaymentService paymentService, 
            IMvxLifetime mvxLifetime,
            IPromotionService promotionService,
            IMetricsService metricsService,
			IBookingService bookingService, IPaymentProviderClientService paymentProviderClientService)
			INetworkRoamingService networkRoamingService)
		{
		    _locationService = locationService;
			_orderWorkflowService = orderWorkflowService;
			_tutorialService = tutorialService;
			_pushNotificationService = pushNotificationService;
			_vehicleService = vehicleService;
			_termsService = termsService;
		    _mvxLifetime = mvxLifetime;
		    _metricsService = metricsService;
			_bookingService = bookingService;
		    _accountService = accountService;
			_paymentService = paymentService;
			_networkRoamingService = networkRoamingService;

            Panel = new PanelMenuViewModel(browserTask, orderWorkflowService, accountService, phoneService, paymentService, promotionService, paymentProviderClientService);

			Map = AddChild<MapViewModel>();
			OrderOptions = AddChild<OrderOptionsViewModel>();
			OrderReview = AddChild<OrderReviewViewModel>();
			OrderEdit = AddChild<OrderEditViewModel>();
            OrderAirport = AddChild<OrderAirportViewModel>();
			BottomBar = AddChild<BottomBarViewModel>();
			AddressPicker = AddChild<AddressPickerViewModel>();
			BookingStatus = AddChild<BookingStatusViewModel>();
            DropOffSelection = AddChild<DropOffSelectionMidTripViewModel>();

			Observe(_vehicleService.GetAndObserveAvailableVehiclesWhenVehicleTypeChanges(), ZoomOnNearbyVehiclesIfPossible);
			Observe(_networkRoamingService.GetAndObserveMarketSettings(), MarketChanged);
		}

		private bool _firstTime;

		public void Init(bool locateUser, string defaultHintZoomLevel = null, string order = null, string orderStatusDetail = null, string manualRidelinqDetail = null)
        {
			_locateUser = locateUser;
		    _defaultHintZoomLevel = defaultHintZoomLevel.FromJson<ZoomToStreetLevelPresentationHint>();

			if (manualRidelinqDetail != null)
			{
			    var deserializedRidelinqDetails = manualRidelinqDetail.FromJson<OrderManualRideLinqDetail>();

				CurrentViewState = HomeViewModelState.ManualRidelinq;

				BookingStatus.StartBookingStatus(deserializedRidelinqDetails, true);

				return;
			}

			if (order != null && orderStatusDetail != null)
			{
			    var deserializedOrder = order.FromJson<Order>();
			    var deserializeOrderStatus = orderStatusDetail.FromJson<OrderStatusDetail>();

				CurrentViewState = HomeViewModelState.BookingStatus;

				BookingStatus.StartBookingStatus(deserializedOrder, deserializeOrderStatus);	
			}
		}

		public Task GoToBookingStatusFromOrderId(Guid orderId)
		{
			return Task.Run(async () =>
			{
				var orderStatus = await _bookingService.GetOrderStatusAsync(orderId);
				var order = await _accountService.GetHistoryOrderAsync(orderId);

				CurrentViewState = HomeViewModelState.BookingStatus;

				BookingStatus.StartBookingStatus(order, orderStatus);
			});
		}

		public override void OnViewLoaded ()
		{
			base.OnViewLoaded ();
				
			BottomBar.Save = OrderEdit.Save;
			BottomBar.CancelEdit = OrderEdit.Cancel;
            BottomBar.NextAirport = OrderAirport.NextCommand;
		}



		public override void OnViewStarted(bool firstTime)
		{
			base.OnViewStarted(firstTime);

			try
			{
				_firstTime = firstTime;

				_locationService.Start();

				if (_orderWorkflowService.IsOrderRebooked())
				{
					_bottomBar.ReviewOrderDetails().FireAndForget();
				}

				if (firstTime)
				{
					Panel.Start().FireAndForget();

					AddressPicker.RefreshFilteredAddress();

					this.Services().ApplicationInfo.CheckVersionAsync().FireAndForget();

					CheckTermsAsync();

					CheckCreditCardExpiration().FireAndForget();

					BottomBar.CheckManualRideLinqEnabledAsync();

					_isShowingTutorial = _tutorialService.DisplayTutorialToNewUser(() =>
						{
							_isShowingTutorial = false;
							LocateUserIfNeeded();
						});
					_pushNotificationService.RegisterDeviceForPushNotifications(force: true);
				}

				LocateUserIfNeeded();

				if (_defaultHintZoomLevel != null)
				{
					ChangePresentation(_defaultHintZoomLevel);
					_defaultHintZoomLevel = null;
				}

				_vehicleService.Start();
			}
			catch(Exception ex)
			{
				Logger.LogError(ex);
			}
		}

		private void LocateUserIfNeeded()
		{
			if (_locateUser && !_isShowingTutorial)
			{
				AutomaticLocateMeAtPickup.Execute(null);
				_locateUser = false;
			}
		}

		public bool FirstTime{ get; set;}

		private bool _isShowingUnratedPopup;
	    private void CheckUnratedRide()
	    {
			if (Settings.RatingEnabled && !_isShowingUnratedPopup)
			{
				var unratedRideId = _orderWorkflowService.GetLastUnratedRide();
				if (unratedRideId != null
					&& _orderWorkflowService.ShouldPromptUserToRateLastRide())
				{
					_isShowingUnratedPopup = true;

					var title = this.Services().Localize["RateLastRideTitle"];
					var message = this.Services().Localize["RateLastRideMessage"];
					Action goToRate = () =>
					{
						_isShowingUnratedPopup = false;
						ShowViewModel<BookRatingViewModel> (new
						{
							orderId = unratedRideId.ToString (),
							canRate = true
						});
					};

					if (Settings.RatingRequired)
					{
						if (Settings.CanSkipRatingRequired)
						{
							this.Services().Message.ShowMessage(title, message,
								this.Services().Localize["RateLastRide"],
								goToRate,
								this.Services().Localize["NotNow"],
								() => { _isShowingUnratedPopup = false; });
						}
						else
						{
							this.Services().Message.ShowMessage(title, message, goToRate);
						}
					}
					else
					{
						this.Services().Message.ShowMessage(title, message,
							this.Services().Localize["RateLastRide"],
							goToRate,
							this.Services().Localize["DontAsk"],
							() => { 
								_isShowingUnratedPopup = false;
								this.Services().Cache.Set("RateLastRideDontPrompt", "yes");
							},
							this.Services().Localize["NotNow"],
							() => { _isShowingUnratedPopup = false; });
					}
				}
			}
	    }

		public async void CheckTermsAsync()
		{
			// if we're already showing the terms and conditions, do nothing
			if (!_isShowingTermsAndConditions)
			{
				await _termsService.CheckIfNeedsToShowTerms (
					(content, actionOnResult) => 
					{
						_isShowingTermsAndConditions = true;
						ShowSubViewModel<UpdatedTermsAndConditionsViewModel, bool>(content, actionOnResult);
					},
					(locateUser, defaultHintZoomLevel) => 
					{
						// reset the viewmodel to the state it was before calling CheckTermsAsync()
						_locateUser = locateUser;
						_defaultHintZoomLevel = defaultHintZoomLevel;

						_isShowingTermsAndConditions = false;
					},
					_locateUser, 
					_defaultHintZoomLevel
				).HandleErrors();
			}
		}

		public async Task CheckCreditCardExpiration()
		{
			if (!_isShowingCreditCardExpiredPrompt)
			{
                // Update cached credit card
				await _accountService.GetDefaultCreditCard();

				if (!_accountService.CurrentAccount.IsPayPalAccountLinked
					&& _accountService.CurrentAccount.DefaultCreditCard != null
					&& _accountService.CurrentAccount.DefaultCreditCard.IsExpired())
				{
					_isShowingCreditCardExpiredPrompt = true;

					// remove card
					await _accountService.RemoveCreditCard(_accountService.CurrentAccount.DefaultCreditCard.CreditCardId);

					var paymentSettings = await _paymentService.GetPaymentSettings();
					if (!paymentSettings.IsPayInTaxiEnabled)
					{
						// we just remove the card and don't mention it to the user since he can't add one anyway
						return;
					}

					var title = this.Services().Localize["CreditCardExpiredTitle"];

					if (paymentSettings.IsPaymentOutOfAppDisabled != OutOfAppPaymentDisabled.None)
					{
						// pay in car is disabled, user has only one choice and will not be able to leave the AddCreditCardViewModel without entering a valid card
						this.Services().Message.ShowMessage(title, 
							this.Services().Localize["CardExpiredMessage"], 
							() =>
						{
							_isShowingCreditCardExpiredPrompt = false;
								ShowViewModelAndClearHistory<CreditCardAddViewModel>(new { isMandatory = paymentSettings.CreditCardIsMandatory });
						});
					}
					else
					{
						this.Services().Message.ShowMessage(title, 
							this.Services().Localize["CardExpiredNonMandatoryMessage"],
							this.Services().Localize["CreditCardExpiredUpdateNow"],
							() =>
						{
							_isShowingCreditCardExpiredPrompt = false;
							ShowViewModel<CreditCardAddViewModel>();
						},
							this.Services().Localize["NotNow"],
							() =>
						{
							_isShowingCreditCardExpiredPrompt = false;
						});
					}
				}
			}
		}

		public override void OnViewStopped()
		{
			base.OnViewStopped();
			_locationService.Stop();
			_vehicleService.Stop();
		}

	    public bool IsManualRideLinqEnabled
	    {
	        get { return _isManualRideLinqEnabled; }
	        set
	        {
	            _isManualRideLinqEnabled = value;
	            RaisePropertyChanged();
	        }
	    }

		public BookingStatusViewModel BookingStatus
		{
			get { return _bookingStatus; }
			set
			{
				_bookingStatus = value; 
				RaisePropertyChanged();
			}
		}

		public void GotoBookingStatus(Order order, OrderStatusDetail orderStatusDetail)
		{
			CurrentViewState = HomeViewModelState.BookingStatus;

			BookingStatus.StartBookingStatus(order, orderStatusDetail);
		}

		public void GoToManualRideLinq(OrderManualRideLinqDetail detail, bool isRestoringFromBackground = false)
		{
			CurrentViewState = HomeViewModelState.ManualRidelinq;

			_orderWorkflowService.SetAddresses(new Address(), new Address());

			BookingStatus.StartBookingStatus(detail, isRestoringFromBackground);
		}

		public PanelMenuViewModel Panel { get; set; }

		private MapViewModel _map;
		public MapViewModel Map
		{ 
			get { return _map; }
			private set
			{ 
				_map = value;
				RaisePropertyChanged();
			}
		}

	    private OrderOptionsViewModel _orderOptions;
		public OrderOptionsViewModel OrderOptions
		{ 
			get { return _orderOptions; }
			private set
			{ 
				_orderOptions = value;
				RaisePropertyChanged();
			}
		}

		private OrderReviewViewModel _orderReview;
		public OrderReviewViewModel OrderReview
		{
			get { return _orderReview; }
			set
			{
				_orderReview = value;
				RaisePropertyChanged();
			}
		}

		private OrderEditViewModel _orderEdit;
		public OrderEditViewModel OrderEdit
		{
			get { return _orderEdit; }
			set
			{
				_orderEdit = value;
				RaisePropertyChanged();
			}
		}

        private BottomBarViewModel _bottomBar;
		public BottomBarViewModel BottomBar
		{
			get { return _bottomBar; }
			set
			{
				_bottomBar = value;
				RaisePropertyChanged();
			}
		}

		private AddressPickerViewModel _addressPickerViewModel;
		public AddressPickerViewModel AddressPicker
		{
			get { return _addressPickerViewModel; }
			set
			{
				_addressPickerViewModel = value;
				RaisePropertyChanged();
			}
		}

        private OrderAirportViewModel _orderAirport;
        public OrderAirportViewModel OrderAirport
        {
            get { return _orderAirport; }
            set
            {
                _orderAirport = value;
                RaisePropertyChanged();
            }
        }

        private DropOffSelectionMidTripViewModel _dropOffSelection;
        public DropOffSelectionMidTripViewModel DropOffSelection
        { 
            get { return _dropOffSelection; }
            private set
            { 
                _dropOffSelection = value;
                RaisePropertyChanged();
            }
        }

		private CancellableCommand _automaticLocateMeAtPickup;
		public CancellableCommand AutomaticLocateMeAtPickup
		{
			get
			{
				return _automaticLocateMeAtPickup ?? (_automaticLocateMeAtPickup = new CancellableCommand(async token =>
				{				
					// we want this command to be top priority, so cancel previous map-related commands
					LocateMe.Cancel();
					Map.UserMovedMap.Cancel();

					var addressSelectionMode = await _orderWorkflowService.GetAndObserveAddressSelectionMode().Take(1).ToTask();
					if (CurrentViewState == HomeViewModelState.Initial 
						&& addressSelectionMode == AddressSelectionMode.PickupSelection)
					{
						// if we are entering for the first time in HomeViewModel, do not allow interuption on zoom.
						var ct = _firstTime 
							? CancellationToken.None 
							: token;

						await SetMapCenterToUserLocation(true, ct);
					}									
				}));
			}
		}

		/**
		 * Should ONLY be called by the "Locate me" button
		 * Use AutomaticLocateMeAtPickup if it's an automatic trigger after app event (appactivated, etc.)
		 **/
		private CancellableCommand _locateMe;
		public CancellableCommand LocateMe
		{
			get
			{
				return _locateMe ?? (_locateMe = new CancellableCommand(token =>
				{
					AutomaticLocateMeAtPickup.Cancel();
					Map.UserMovedMap.Cancel();

					return SetMapCenterToUserLocation(false, token);
				}));
			}
		}

	    private void ProcessSingleOrNoFilteredAddresses(AddressLocationType filter, HomeViewModelState state)
	    {
            var filteredAddress = AddressPicker.FilteredPlaces
                    .Where(address => address.Address.AddressLocationType == filter)
                    .ToArray();

            if (!filteredAddress.Any())
            {
                var localize = this.Services().Localize;
                this.Services().Message.ShowMessage( localize["FilteredAddresses_Error_Title"], localize["FilteredAddresses_Error_Message"]);

                return;
            }

            if (filteredAddress.Length == 1)
            {
                var address = filteredAddress
                    .Select(place => place.Address)
                    .First();

                AddressPicker.SelectAddress(address);

                return;
            }

		    CurrentViewState = state;
	    }

		public new ICommand CloseCommand
		{
			get
			{
				return this.GetCommand(() =>
				{
					switch (CurrentViewState)
					{
						case HomeViewModelState.BookingStatus:
							_bookingStatus.ReturnToInitialState();
							break;
						case HomeViewModelState.Review:
						case HomeViewModelState.PickDate:
						case HomeViewModelState.AddressSearch:
						case HomeViewModelState.AirportSearch:
						case HomeViewModelState.TrainStationSearch:
                        case HomeViewModelState.BookATaxi:
                            CurrentViewState = HomeViewModelState.Initial;
                            break;
						case HomeViewModelState.AirportDetails:
							CurrentViewState = HomeViewModelState.Initial;
							break;
						case HomeViewModelState.Edit:
							CurrentViewState = HomeViewModelState.Review;
							break;
						case HomeViewModelState.AirportPickDate:
							CurrentViewState = HomeViewModelState.AirportDetails;
							break;
						default:
							base.CloseCommand.ExecuteIfPossible();
							break;
					}
				});
			}
		}

	    public ICommand AirportSearch
	    {
	        get
	        {
	            return this.GetCommand(() => ProcessSingleOrNoFilteredAddresses(AddressLocationType.Airport, HomeViewModelState.AirportSearch));
	        }
	    }

	    public ICommand TrainStationSearch
	    {
	        get
	        {
	            return this.GetCommand(() => ProcessSingleOrNoFilteredAddresses(AddressLocationType.Train, HomeViewModelState.TrainStationSearch));
	        }
	    }

		public bool IsAirportButtonHidden
		{
			get { return !Settings.IsAirportButtonEnabled || CurrentViewState == HomeViewModelState.BookingStatus || CurrentViewState == HomeViewModelState.ManualRidelinq; }
		}

		public bool IsTrainButtonHidden
		{
			get { return !Settings.IsTrainStationButtonEnabled || CurrentViewState == HomeViewModelState.BookingStatus || CurrentViewState == HomeViewModelState.ManualRidelinq; }
		}

		public HomeViewModelState CurrentViewState
		{
			get { return _currentViewState; }
			set
			{
				var disableAddressSelectionMode = value == HomeViewModelState.BookingStatus 
					|| value == HomeViewModelState.ManualRidelinq;

				var isCurrentStateSelectionModeOff = _currentViewState == HomeViewModelState.BookingStatus 
					|| _currentViewState == HomeViewModelState.ManualRidelinq;

				if (disableAddressSelectionMode)
				{
					_orderWorkflowService.SetAddressSelectionMode();
				}
				else if (value == HomeViewModelState.Initial && isCurrentStateSelectionModeOff)
				{
					_orderWorkflowService.SetAddressSelectionMode(AddressSelectionMode.PickupSelection);
					_bottomBar.EstimateSelected = false;
				}

				if (value == HomeViewModelState.DropOffAddressSelection)
				{
					if (DropOffSelection.DestinationAddress.Id == Guid.Empty)
					{
						LocateMe.ExecuteIfPossible();
					}
					_orderWorkflowService.SetAddressSelectionMode(AddressSelectionMode.DropoffSelection);
					_orderWorkflowService.SetDropOffSelectionMode(true);
				}

				_currentViewState = value;

				RaisePropertyChanged();
				RaisePropertyChanged(() => IsAirportButtonHidden);
				RaisePropertyChanged(() => IsTrainButtonHidden);
			}
		}

		public bool CanUseCloseCommand()
		{
			return CurrentViewState != HomeViewModelState.Initial && BookingStatus.CanGoBack;
		}

		private async Task SetMapCenterToUserLocation(bool initialZoom, CancellationToken cancellationToken)
		{
			try
			{
				var address = await _orderWorkflowService.SetAddressToUserLocation(cancellationToken);
				if(address.HasValidCoordinate())
				{
					// zoom like uber means start at user location with street level zoom and when and only when you have vehicle, zoom out
					// otherwise, this causes problems on slow networks where the address is found but the pin is not placed correctly and we show the entire map of the world until we get the timeout
					ChangePresentation(new ZoomToStreetLevelPresentationHint(address.Latitude, address.Longitude, initialZoom));

					// do the uber zoom
					try 
					{
						var availableVehicles = await _vehicleService.GetAndObserveAvailableVehicles()
							.Timeout(TimeSpan.FromSeconds(5))
							.Where(x => x.Any())
							.Take(1)
							.ToTask(cancellationToken);

						ZoomOnNearbyVehiclesIfPossible(availableVehicles);
					}
					catch (TimeoutException)
					{ 
						Console.WriteLine(@"ZoomOnNearbyVehiclesIfPossible: Timeout occured while waiting for available vehicles");
					}
				}
			}
			catch(OperationCanceledException)
			{
				// Operation Cancelled exception is suppressed because we needed to stop the current process but we don't have to handle this.
			}
			catch(Exception ex)
			{
				Logger.LogError(ex);
			}
		}

		private void ZoomOnNearbyVehiclesIfPossible(AvailableVehicle[] vehicles)
		{
			if(Settings.ZoomOnNearbyVehicles)
			{
			    var isUsingGeoServices = !_lastHashedMarket.HasValue()
			        ? Settings.LocalAvailableVehiclesMode == LocalAvailableVehiclesModes.Geo
			        : Settings.ExternalAvailableVehiclesMode == ExternalAvailableVehiclesModes.Geo;

                var bounds = _vehicleService.GetBoundsForNearestVehicles(isUsingGeoServices, Map.PickupAddress, vehicles);	
				if (bounds != null)
				{
					ChangePresentation(new ChangeZoomPresentationHint(bounds));
				}
			}
		}

		private bool _subscribedToLifetimeChanged;
	    private bool _isManualRideLinqEnabled;
		private BookingStatusViewModel _bookingStatus;

		public void SubscribeLifetimeChangedIfNecessary()
		{
			if (!_subscribedToLifetimeChanged)
			{
				_mvxLifetime.LifetimeChanged += OnApplicationLifetimeChanged;
				_subscribedToLifetimeChanged = true;
			}
		}

		public void UnsubscribeLifetimeChangedIfNecessary()
		{
			if (_subscribedToLifetimeChanged)
			{
				_mvxLifetime.LifetimeChanged -= OnApplicationLifetimeChanged;
				_subscribedToLifetimeChanged = false;
			}
		}

        private void OnApplicationLifetimeChanged(object sender, MvxLifetimeEventArgs args)
        {
            if (args.LifetimeEvent == MvxLifetimeEvent.ActivatedFromDisk || args.LifetimeEvent == MvxLifetimeEvent.ActivatedFromMemory)
            {
				// since this is called before OnViewStarted and AutomaticLocateMe needs it, do it here, otherwise AutomaticLocateMe will be very slow
				_locationService.Start();

				AutomaticLocateMeAtPickup.ExecuteIfPossible();
                CheckUnratedRide();
				CheckTermsAsync();
				CheckCreditCardExpiration().FireAndForget();
                AddressPicker.RefreshFilteredAddress();

				_metricsService.LogApplicationStartUp ();
            }
        }

        private void MarketChanged(MarketSettings marketSettings)
        {
			// Market changed and not home market
			if (_lastHashedMarket != marketSettings.HashedMarket
				&& !marketSettings.IsLocalMarket
				&& !Settings.Network.HideMarketChangeWarning)
			{
				this.Services().Message.ShowMessage(this.Services().Localize["MarketChangedMessageTitle"],
					this.Services().Localize["MarketChangedMessage"]);
			}

			_lastHashedMarket = marketSettings.HashedMarket;

            if (BottomBar != null)
            {
                BottomBar.CheckManualRideLinqEnabledAsync();
            }
        }

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				BookingStatus.Dispose();
			}

			base.Dispose(disposing);
		}
    }
}