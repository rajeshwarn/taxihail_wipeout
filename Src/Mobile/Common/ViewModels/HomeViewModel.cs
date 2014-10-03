using System.Windows.Input;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.PresentationHints;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using Cirrious.MvvmCross.Plugins.WebBrowser;
using ServiceStack.Text;
using apcurium.MK.Booking.Mobile.Messages;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class HomeViewModel : PageViewModel
    {
		private readonly IOrderWorkflowService _orderWorkflowService;
		private readonly ILocationService _locationService;
		private readonly ITutorialService _tutorialService;
		private readonly IPushNotificationService _pushNotificationService;
		private readonly IVehicleService _vehicleService;
		private readonly IAccountService _accountService;
		private readonly IBookingService _bookingService;
		private readonly ITermsAndConditionsService _termsService;
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
			IBookingService bookingService) : base()
		{
			_locationService = locationService;
			_orderWorkflowService = orderWorkflowService;
			_tutorialService = tutorialService;
			_pushNotificationService = pushNotificationService;
			_vehicleService = vehicleService;
			_accountService = accountService;
			_termsService = termsService;
			_bookingService = bookingService;

			Panel = new PanelMenuViewModel(this, browserTask, orderWorkflowService, accountService, phoneService);
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
			OrderReview = AddChild<OrderReviewViewModel>();
			OrderEdit = AddChild<OrderEditViewModel>();
			BottomBar = AddChild<BottomBarViewModel>();
			AddressPicker = AddChild<AddressPickerViewModel>();

			BottomBar.Save = OrderEdit.Save;
			BottomBar.CancelEdit = OrderEdit.Cancel;
		}

		public override void OnViewStarted(bool firstTime)
		{
			base.OnViewStarted(firstTime);

			_locationService.Start();

			if (_accountService.CurrentAccount  == null) {
				return;
			}


			CheckTermsAsync();
			CheckActiveOrderAsync ();
			if (firstTime)
			{
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
				var mode =  _orderWorkflowService.GetAndObserveAddressSelectionMode ().Take (1).ToTask ().Result;
				if (_currentState == HomeViewModelState.Initial && mode == apcurium.MK.Booking.Mobile.Data.AddressSelectionMode.PickupSelection)
				{
					LocateMe.Execute();
				}                    


				_locateUser = false;
			}

			if (_defaultHintZoomLevel != null)
			{
				this.ChangePresentation(_defaultHintZoomLevel);
				_defaultHintZoomLevel = null;
			}
			_vehicleService.Start();
		}


		public async void CheckActiveOrderAsync()
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

		public ICommand LocateMe
		{
			get
			{
				return this.GetCommand(async () =>
				{
					var address = await _orderWorkflowService.SetAddressToUserLocation();
					if(address.HasValidCoordinate())
					{
                        this.ChangePresentation(new ZoomToStreetLevelPresentationHint(address.Latitude, address.Longitude));
					}
				});
			}
		}

		protected override TViewModel AddChild<TViewModel>()
		{
			var child = base.AddChild<TViewModel>();
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

            if (e.State == HomeViewModelState.Initial)
            {
                _vehicleService.Start ();
            }
            else
            {
                _vehicleService.Stop ();
            }

		}
    }
}