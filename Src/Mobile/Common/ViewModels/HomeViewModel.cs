using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Windows.Input;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Framework.Extensions;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.PresentationHints;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using apcurium.MK.Common.Entity;
using Cirrious.MvvmCross.Platform;
using Cirrious.MvvmCross.Plugins.WebBrowser;
using ServiceStack.Text;

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
		private readonly IPaymentService _paymentService;
		private string _lastHashedMarket = string.Empty;
		private bool _isShowingTermsAndConditions;
		private bool _isShowingCreditCardExpiredPrompt;
		private bool _locateUser;
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
            IBookingService bookingService)
		{
			_locationService = locationService;
			_orderWorkflowService = orderWorkflowService;
			_tutorialService = tutorialService;
			_pushNotificationService = pushNotificationService;
			_vehicleService = vehicleService;
			_termsService = termsService;
		    _mvxLifetime = mvxLifetime;
		    _bookingService = bookingService;
		    _accountService = accountService;
			_paymentService = paymentService;

            Panel = new PanelMenuViewModel(browserTask, orderWorkflowService, accountService, phoneService, paymentService, promotionService);

			Map = AddChild<MapViewModel>();
			OrderOptions = AddChild<OrderOptionsViewModel>();
			OrderReview = AddChild<OrderReviewViewModel>();
			OrderEdit = AddChild<OrderEditViewModel>();
			BottomBar = AddChild<BottomBarViewModel>();
			AddressPicker = AddChild<AddressPickerViewModel>();
			BookingStatus = AddChild<BookingStatusViewModel>();

			Observe(_vehicleService.GetAndObserveAvailableVehiclesWhenVehicleTypeChanges(), ZoomOnNearbyVehiclesIfPossible);
			Observe(_orderWorkflowService.GetAndObserveHashedMarket(), MarketChanged);
		}


        public void Init(bool locateUser, string defaultHintZoomLevel)
        {
			_locateUser = locateUser;
			_defaultHintZoomLevel = JsonSerializer.DeserializeFromString<ZoomToStreetLevelPresentationHint> (defaultHintZoomLevel);
		}

		public override void OnViewLoaded ()
		{
			base.OnViewLoaded ();
				
			BottomBar.Save = OrderEdit.Save;
			BottomBar.CancelEdit = OrderEdit.Cancel;
		}

		public override void OnViewStarted(bool firstTime)
		{
			base.OnViewStarted(firstTime);

			_locationService.Start();
            
			CheckActiveOrderAsync (firstTime);

            if (_orderWorkflowService.IsOrderRebooked())
            {
                _bottomBar.ReviewOrderDetails();
            }

			if (firstTime)
			{
				Panel.Start().FireAndForget();

                AddressPicker.RefreshFilteredAddress();

				CheckTermsAsync();

				CheckCreditCardExpiration();

                BottomBar.CheckManualRideLinqEnabledAsync(_lastHashedMarket.HasValue());

				this.Services().ApplicationInfo.CheckVersionAsync();

				_tutorialService.DisplayTutorialToNewUser();
				_pushNotificationService.RegisterDeviceForPushNotifications(force: true);
			}
			
			if (_locateUser)
			{
				AutomaticLocateMeAtPickup.Execute (null);
				_locateUser = false;
			}

			if (_defaultHintZoomLevel != null)
			{
				ChangePresentation(_defaultHintZoomLevel);
				_defaultHintZoomLevel = null;
			}

			_vehicleService.Start();
		}

		public async void CheckActiveOrderAsync(bool firstTime)
		{
			var lastOrder = await _orderWorkflowService.GetLastActiveOrder ();
			if(lastOrder != null)
			{
			    if (lastOrder.Item1.IsManualRideLinq)
			    {
                    var orderManualRideLinqDetail = await _bookingService.GetTripInfoFromManualRideLinq(lastOrder.Item1.Id);

                    ShowViewModelAndRemoveFromHistory<ManualRideLinqStatusViewModel>(new
                    {
                        orderManualRideLinqDetail = orderManualRideLinqDetail.ToJson()
                    });
			    }
			    else
			    {
					GotoBookingStatus(lastOrder.Item1, lastOrder.Item2);
			    }

				
			}
			else if (firstTime)
			{
				CheckUnratedRide ();
			}
		}
			
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
						ShowSubViewModel<UpdatedTermsAndConditionsViewModel, bool> (content, actionOnResult);
					},
					(locateUser, defaultHintZoomLevel) => 
					{
						_isShowingTermsAndConditions = false;

						// reset the viewmodel to the state it was before calling CheckTermsAsync()
						_locateUser = locateUser;
						_defaultHintZoomLevel = defaultHintZoomLevel;
					},
					_locateUser, 
					_defaultHintZoomLevel
				);
			}
		}

		public async void CheckCreditCardExpiration()
		{
			if (!_isShowingCreditCardExpiredPrompt)
			{
                // Update cached credit card
				await _accountService.GetCreditCard();

				if (!_accountService.CurrentAccount.IsPayPalAccountLinked
					&& _accountService.CurrentAccount.DefaultCreditCard != null
					&& _accountService.CurrentAccount.DefaultCreditCard.IsExpired())
				{
					_isShowingCreditCardExpiredPrompt = true;

					// remove card
					await _accountService.RemoveCreditCard();

					var paymentSettings = await _paymentService.GetPaymentSettings();
					if (!paymentSettings.IsPayInTaxiEnabled)
					{
						// we just remove the card and don't mention it to the user since he can't add one anyway
						return;
					}

					var title = this.Services().Localize["CreditCardExpiredTitle"];

					if (paymentSettings.IsOutOfAppPaymentDisabled)
					{
						// pay in car is disabled, user has only one choice and will not be able to leave the AddCreditCardViewModel without entering a valid card
						this.Services().Message.ShowMessage(title, 
							this.Services().Localize["CardExpiredMessage"], 
							() =>
						{
							_isShowingCreditCardExpiredPrompt = false;
							ShowViewModelAndClearHistory<CreditCardAddViewModel>(new { isMandatory = this.Services().Settings.CreditCardIsMandatory });
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

			Map.PickupAddress = order.PickupAddress;
			Map.DestinationAddress = order.DropOffAddress;

			BookingStatus.StartBookingStatus(order,orderStatusDetail);
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

					var addressSelectionMode = await _orderWorkflowService.GetAndObserveAddressSelectionMode ().Take (1).ToTask ();
					if (CurrentViewState == HomeViewModelState.Initial 
						&& addressSelectionMode == AddressSelectionMode.PickupSelection)
					{
						SetMapCenterToUserLocation(true, token);
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

					SetMapCenterToUserLocation(false, token);
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
						case HomeViewModelState.Review:
						case HomeViewModelState.PickDate:
						case HomeViewModelState.AddressSearch:
						case HomeViewModelState.AirportSearch:
						case HomeViewModelState.TrainStationSearch:
						case HomeViewModelState.BookingStatus:
							CurrentViewState = HomeViewModelState.Initial;
							break;
						case HomeViewModelState.Edit:
							CurrentViewState = HomeViewModelState.Review;
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
			get { return !Settings.IsAirportButtonEnabled || CurrentViewState == HomeViewModelState.BookingStatus; }
		}

		public bool IsTrainButtonHidden
		{
			get { return !Settings.IsTrainStationButtonEnabled || CurrentViewState == HomeViewModelState.BookingStatus; }
		}

		public HomeViewModelState CurrentViewState
		{
			get { return _currentViewState; }
			set
			{
				if (value == HomeViewModelState.BookingStatus)
				{
					_orderWorkflowService.ToggleBetweenPickupAndDestinationSelectionMode(AddressSelectionMode.None);
				}
				else if (value == HomeViewModelState.Initial && _currentViewState == HomeViewModelState.BookingStatus)
				{
					_orderWorkflowService.ToggleBetweenPickupAndDestinationSelectionMode(AddressSelectionMode.PickupSelection);
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

		private async void SetMapCenterToUserLocation(bool initialZoom, CancellationToken cancellationToken = default(CancellationToken))
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
							.Timeout(TimeSpan.FromSeconds (5))
							.Where(x => x.Any())
							.Take (1)
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
				var bounds = _vehicleService.GetBoundsForNearestVehicles(Map.PickupAddress, vehicles);	
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
				CheckCreditCardExpiration();
                AddressPicker.RefreshFilteredAddress();

				_accountService.LogApplicationStartUp ();
            }
        }

        private void MarketChanged(string hashedMarket)
        {
            // Market changed and not home market
            if (_lastHashedMarket != hashedMarket
                && hashedMarket.HasValue()
                && !Settings.Network.HideMarketChangeWarning)
            {
                this.Services().Message.ShowMessage(this.Services().Localize["MarketChangedMessageTitle"],
                    this.Services().Localize["MarketChangedMessage"]);
            }

            _lastHashedMarket = hashedMarket;

            if (BottomBar != null)
            {
                BottomBar.CheckManualRideLinqEnabledAsync(_lastHashedMarket.HasValue());
            }
        }
    }
}