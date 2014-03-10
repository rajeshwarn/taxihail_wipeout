using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using System.Windows.Input;
using Cirrious.MvvmCross.Plugins.WebBrowser;
using ServiceStack.Text;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Booking.Mobile.PresentationHints;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class HomeViewModel : BaseViewModel
    {
		private readonly IOrderWorkflowService _orderWorkflowService;
		private readonly ILocationService _locationService;
		private readonly ITutorialService _tutorialService;
		private readonly IPushNotificationService _pushNotificationService;
		private readonly IVehicleService _vehicleService;
		private readonly IAccountService _accountService;
		private readonly ITermsAndConditionsService _termsService;

		public HomeViewModel(IOrderWorkflowService orderWorkflowService, 
			IMvxWebBrowserTask browserTask,
			ILocationService locationService,
			ITutorialService tutorialService,
			IPushNotificationService pushNotificationService,
			IVehicleService vehicleService,
			IAccountService accountService,
			IPhoneService phoneService,
			ITermsAndConditionsService termsService) : base()
		{
			_locationService = locationService;
			_orderWorkflowService = orderWorkflowService;
			_tutorialService = tutorialService;
			_pushNotificationService = pushNotificationService;
			_vehicleService = vehicleService;
			_accountService = accountService;
			_termsService = termsService;

			Panel = new PanelMenuViewModel(this, browserTask, orderWorkflowService, accountService, phoneService);
		}

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
			if (firstTime)
			{
				this.Services().ApplicationInfo.CheckVersionAsync();

				CheckTermsAsync();

				_tutorialService.DisplayTutorialToNewUser();
				_pushNotificationService.RegisterDeviceForPushNotifications(force: true);
			}

			if (_locateUser)
			{
				LocateMe.Execute();
				_locateUser = false;
			}

			if (_defaultHintZoomLevel != null)
			{
				this.ChangePresentation(_defaultHintZoomLevel);
				_defaultHintZoomLevel = null;
			}
			_vehicleService.Start();
		}


		public async void CheckTermsAsync()
		{
			if (!Settings.ShowTermsAndConditions) 
			{
				return;
			}

			var response = await _termsService.GetTerms();
			var termsAckKey = string.Format("TermsAck{0}", _accountService.CurrentAccount.Email);
			var termsAcknowledged = this.Services().Cache.Get<string>(termsAckKey);

			if (response.Updated || !(termsAcknowledged == "yes"))
			{				 
				this.Services().Cache.Clear(termsAckKey);
				ShowSubViewModel<UpdatedTermsAndConditionsViewModel, bool>(new 
					{														
						content = response.Content
					}.ToStringDictionary(), 
					async acknowledged =>
					{
						this.Services().Cache.Set<string>(termsAckKey, acknowledged ? "yes" : "no");
						if (!acknowledged)
						{
							_accountService.SignOut();
						}
					});
			}
		}

		public override void OnViewStopped()
		{
			base.OnViewStopped();
			_locationService.Stop();
			_vehicleService.Stop();
		}
			
		public void VehicleServiceStateManager(HomeViewModelPresentationHint hint)
		{
			if (hint.State == HomeViewModelState.Initial) {
				_vehicleService.Start ();
			} else {
				_vehicleService.Stop ();
			}
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
    }
}