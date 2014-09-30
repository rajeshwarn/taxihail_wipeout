using System.Diagnostics;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.PresentationHints;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using Cirrious.MvvmCross.Platform;
using Cirrious.MvvmCross.Plugins.WebBrowser;
using Cirrious.MvvmCross.ViewModels;
using ServiceStack.Common.Web;
using ServiceStack.Text;
using apcurium.MK.Booking.Mobile.Messages;
using System.Linq;
using System.Reactive.Linq;
using System;
using System.Threading;
using System.Reactive.Threading.Tasks;
using apcurium.MK.Booking.Mobile.Data;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class HomeViewModel : PageViewModel
    {
		private readonly IOrderWorkflowService _orderWorkflowService;
		private readonly ILocationService _locationService;
		private readonly ITutorialService _tutorialService;
		private readonly IPushNotificationService _pushNotificationService;
		private readonly IVehicleService _vehicleService;
		private readonly ITermsAndConditionsService _termsService;
	    private readonly IMvxLifetime _mvxLifetime;
		private readonly IAccountService _accountService;

		private HomeViewModelState _currentState = HomeViewModelState.Initial;

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
            IMvxLifetime mvxLifetime) : base()
		{
			_locationService = locationService;
			_orderWorkflowService = orderWorkflowService;
			_tutorialService = tutorialService;
			_pushNotificationService = pushNotificationService;
			_vehicleService = vehicleService;
			_termsService = termsService;
		    _mvxLifetime = mvxLifetime;
			_accountService = accountService;

			Panel = new PanelMenuViewModel(this, browserTask, orderWorkflowService, accountService, phoneService, paymentService);
		}

		private bool _isShowingTermsAndConditions;
		private bool _locateUser;
		private ZoomToStreetLevelPresentationHint _defaultHintZoomLevel;

        public void Init(bool locateUser, string defaultHintZoomLevel)
        {
			_locateUser = locateUser;
			_defaultHintZoomLevel = JsonSerializer.DeserializeFromString<ZoomToStreetLevelPresentationHint> (defaultHintZoomLevel);
		}

		public override void OnViewLoaded ()
		{
			base.OnViewLoaded ();
					            
			Map = AddChild<MapViewModel>();
			OrderOptions = AddChild<OrderOptionsViewModel>();
			OrderReview = AddChild<OrderReviewViewModel>(true);
			OrderEdit = AddChild<OrderEditViewModel>(true);
			BottomBar = AddChild<BottomBarViewModel>();
			AddressPicker = AddChild<AddressPickerViewModel>();

			BottomBar.Save = OrderEdit.Save;
			BottomBar.CancelEdit = OrderEdit.Cancel;
		}

		public override void OnViewStarted(bool firstTime)
		{
			base.OnViewStarted(firstTime);

			_locationService.Start();
			CheckTermsAsync();
			CheckActiveOrderAsync (firstTime);

            if (_orderWorkflowService.IsOrderRebooked())
            {
                _bottomBar.ReviewOrderDetails();
            }

			if (firstTime)
			{
                // Don't await side panel creation
				Panel.Start();

				this.Services().ApplicationInfo.CheckVersionAsync();

				_tutorialService.DisplayTutorialToNewUser();
				_pushNotificationService.RegisterDeviceForPushNotifications(force: true);

				this.Services().MessengerHub.Subscribe<AppActivated>(m => 
				{
					_locateUser = true;
				});
			}

			if (_locateUser)
			{
				AutomaticLocateMeAtPickup.Execute(null);				
				_locateUser = false;
			}

			if (_defaultHintZoomLevel != null)
			{
				this.ChangePresentation(_defaultHintZoomLevel);
				_defaultHintZoomLevel = null;
			}

			_vehicleService.Start();
		}

		public async void CheckActiveOrderAsync(bool firstTime)
		{
			var lastOrder = await _orderWorkflowService.GetLastActiveOrder ();
			if(lastOrder != null)
			{
				ShowViewModel<BookingStatusViewModel> (new
				{
					order = lastOrder.Item1.ToJson (),
					orderStatus = lastOrder.Item2.ToJson ()
				});
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

		public override void OnViewStopped()
		{
			base.OnViewStopped();
			_locationService.Stop();
			_vehicleService.Stop();
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

		public ICommand AutomaticLocateMeAtPickup
		{
			get
			{
				return this.GetCommand(async () =>
				{					
					if (_accountService.CurrentAccount != null)
					{
						var addressSelectionMode = await _orderWorkflowService.GetAndObserveAddressSelectionMode ().Take (1).ToTask ();
						if (_currentState == HomeViewModelState.Initial 
							&& addressSelectionMode == AddressSelectionMode.PickupSelection)
						{
							var address = await _orderWorkflowService.SetAddressToUserLocation();
							if(address.HasValidCoordinate())
							{
								// zoom like uber means start at user location with street level zoom and when and only when you have vehicle, zoom out
								// otherwise, this causes problems on slow networks where the address is found but the pin is not placed correctly and we show the entire map of the world until we get the timeout
								this.ChangePresentation(new ZoomToStreetLevelPresentationHint(address.Latitude, address.Longitude, null));

								if(Settings.ZoomOnNearbyVehicles)
								{
									try 
									{
										var availableVehicles = await _vehicleService.GetAndObserveAvailableVehicles ().Timeout (TimeSpan.FromSeconds (5)).Where (x => x.Count () > 0).Take (1).ToTask();
										var bounds = _vehicleService.GetBoundsForNearestVehicles(Map.PickupAddress, availableVehicles);	
										if (bounds != null)
										{
                                            this.ChangePresentation(new ZoomToStreetLevelPresentationHint(address.Latitude, address.Longitude, bounds));
										}
									}
									catch (TimeoutException)
									{ 
										Console.WriteLine("LocateMe: Timeout occured while waiting for available vehicles");
									}
								}
							}
						}
					}									
				});
			}
		}

		/**
		 * Should ONLY be called by the "Locate me" button
		 * Use AutomaticLocateMeAtPickup if it's an automatic trigger after app event (appactivated, etc.)
		 **/
		public ICommand LocateMe
		{
			get
			{
				return this.GetCommand(async () =>
				{					
					if (_accountService.CurrentAccount != null)
					{
						var address = await _orderWorkflowService.SetAddressToUserLocation();
						if(address.HasValidCoordinate())
						{
							this.ChangePresentation(new ZoomToStreetLevelPresentationHint(address.Latitude, address.Longitude, null));
						}
					}									
				});
			}
		}

		protected override TViewModel AddChild<TViewModel>(bool lazyLoad = false)
		{
            var child = base.AddChild<TViewModel>(lazyLoad);
			var rps = child as IRequestPresentationState<HomeViewModelStateRequestedEventArgs>;
			if (rps != null)
			{
				rps.PresentationStateRequested += OnPresentationStateRequested;
			}

            return child;
		}

		private void OnPresentationStateRequested(object sender, HomeViewModelStateRequestedEventArgs e)
		{
			_currentState = e.State;

			this.ChangePresentation(new HomeViewModelPresentationHint(e.State, e.IsNewOrder));
		    if (e.State == HomeViewModelState.Review)
		    {
		        if (OrderReview.IsDeferredLoaded)
		        {
                    OrderReview.Init();
		        }  
		    }
            else if (e.State == HomeViewModelState.Edit)
            {
                if (OrderEdit.IsDeferredLoaded)
                {
                    OrderEdit.Init();
                } 
            }

            if (e.State == HomeViewModelState.Initial)
            {
                _vehicleService.Start ();
            }
            else
            {
                _vehicleService.Stop ();
            }
		}

		private bool _subscribedToLifetimeChanged;

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
            if (args.LifetimeEvent == MvxLifetimeEvent.ActivatedFromDisk
                || args.LifetimeEvent == MvxLifetimeEvent.ActivatedFromMemory)
            {
                CheckUnratedRide();
            }
        }
    }
}