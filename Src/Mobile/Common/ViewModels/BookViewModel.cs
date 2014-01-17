using System;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Contract.Requests;
using System.Threading.Tasks;
using apcurium.MK.Booking.Mobile.Infrastructure;
using TinyMessenger;
using apcurium.MK.Booking.Mobile.Messages;
using ServiceStack.Text;
using System.Threading;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Extensions;
using System.Collections.Generic;
using System.Globalization;
using apcurium.MK.Common.Entity;
using System.Reactive.Disposables;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class BookViewModel : BaseViewModel
    {
        private readonly IPushNotificationService _pushNotificationService;
        private readonly IPackageInfo _packageInfo;
        private bool _initialized;

        private IEnumerable<CoordinateViewModel> _mapCenter;
        private string _fareEstimate;

        private bool _useExistingOrder;

        public BookViewModel(IPushNotificationService pushNotificationService, IPackageInfo packageInfo)
        {
            _pushNotificationService = pushNotificationService;
            _packageInfo = packageInfo;

            Initialize();
        }

		public void Init(string order)
        {
			if (order == null)
			{
				// Default navigation without parameter
				return;
			}

            Order = order.FromJson<CreateOrder>();
             
            Order.Id = Guid.Empty;
            Order.PickupDate = null;

            _useExistingOrder = true;
        }

        protected async void Initialize()
        {
            if (_initialized)
                throw new InvalidOperationException();

            _initialized = true;

            Panel = new PanelViewModel(this);

            this.Services().MessengerHub.Subscribe<AppActivated>(_ => AppActivated());

            var showEstimate = Task.Run(() => Boolean.Parse(this.Services().Config.GetSetting("Client.ShowEstimate")));

            try
            {
                ShowEstimate = await showEstimate;
            }
            catch
            {
                ShowEstimate = true;
            }

        }

		public override void OnViewLoaded()
        {
			base.OnViewLoaded();

            if ( Order == null || !_useExistingOrder )
            {
                InitializeOrder();           
            }

            CheckVersion();

            SetupPushNotification();

            LoadLastActiveOrder();

            ClearAddresses();
            
            Pickup.AddressChanged += AddressChanged;
            Dropoff.AddressChanged += AddressChanged;
            Dropoff.AddressCleared += RevertToPickupSelection;

            AddressSelectionMode = AddressSelectionMode.PickupSelection;

            if (! _useExistingOrder ) 
            {
                Pickup.RequestCurrentLocationCommand.Execute();
            }
            else
            {                
                Pickup.IsExecuting = false;
                Dropoff.IsExecuting = false;
            }

            CalculateEstimate ();

            _useExistingOrder = false;

            CenterMap(true);


            UpdateServerInfo();
 
            ForceRefresh();
        }

        void ClearAddresses()
        {
			Pickup = new BookAddressViewModel(){
				IsExecuting = true,
				EmptyAddressPlaceholder = this.Services().Localize["BookPickupLocationEmptyPlaceholder"]
			};
			Pickup.Init(() => Order.PickupAddress, address => Order.PickupAddress = address);

			Dropoff = new BookAddressViewModel() {
				EmptyAddressPlaceholder = this.Services().Localize["BookDropoffLocationEmptyPlaceholder"]
			};
			Dropoff.Init(() => Order.DropOffAddress, address => Order.DropOffAddress = address);
        }

        private void AppActivated()
        {
            this.Services().Location.GetNextPosition(new TimeSpan(0, 0, 2), 20).Subscribe(p =>
            {
                NewOrder();
            });
        }

		public override void OnViewStarted(bool firstTime = false)
        {
			base.OnViewStarted(firstTime);
            ObserveAvailableVehicles();
        }
        
        void SetupPushNotification()
        {
            InvokeOnMainThread(() => _pushNotificationService.RegisterDeviceForPushNotifications(true));
        }


        protected readonly CompositeDisposable Subscriptions = new CompositeDisposable ();
        public override void OnViewStopped ()
        {
            base.OnViewStopped ();
            Subscriptions.DisposeAll ();
        }

        void RevertToPickupSelection (object sender, EventArgs e)
        {
            ActivatePickup.Execute();
        }

        void LoadLastActiveOrder( )
        {
            if (this.Services().Booking.HasLastOrder)
            {
                this.Services().Booking.GetLastOrderStatus().ContinueWith(t => 
                {
                    var isCompleted = this.Services().Booking.IsStatusCompleted(t.Result.IbsStatusId);
                    if (isCompleted)
                    {
                        this.Services().Booking.ClearLastOrder();
                    }
                    else
                    {
                        var order = this.Services().Account.GetHistoryOrder(t.Result.OrderId);
                        ShowStatusActivity(order, t.Result);
                    }
                }, TaskContinuationOptions.OnlyOnRanToCompletion);
            }
        }

        private async void CheckVersion()
        {
            await Task.Delay(1000);
            this.Services().ApplicationInfo.CheckVersionAsync();
        }

        void AddressChanged(object sender, EventArgs e)
        {
            InvokeOnMainThread(() =>
                {
					RaisePropertyChanged(() => Pickup);
					RaisePropertyChanged(() => Dropoff);
                });

            var isUserInitiated = sender is bool && (bool)sender;
            if (!isUserInitiated)
            {
                CenterMap(true);
            }

            Task.Factory.SafeStartNew(CalculateEstimate);
			RaisePropertyChanged(() => CanClearAddress);
        }

        private void CalculateEstimate() 
        {
            _fareEstimate = this.Services().Localize["EstimatingFare"];

			RaisePropertyChanged(() => FareEstimate);

            _fareEstimate = this.Services().Booking.GetFareEstimateDisplay(Order, "EstimatePriceFormat", "NoFareText", true, "EstimatedFareNotAvailable");
            
			RaisePropertyChanged(() => FareEstimate);
        }

        public void InitializeOrder()
        {
            Order = new CreateOrder
                {
                    Settings = this.Services().Account.CurrentAccount != null
                                   ? this.Services().Account.CurrentAccount.Settings
                                   : new BookingSettings()
                };
        }

        public bool UseAmPmFormat
        {
            get
            {
                return new CultureInfo(CultureProvider.CultureInfoString).DateTimeFormat.LongTimePattern.ToLower().Contains("tt");
            }
        }

        public bool DisableFutureBooking
        {
            get
            {
                return this.Services().Config.GetSetting("Client.DisableFutureBooking", false);
            }
        }

        public bool HideDestination
        {
            get
            {
                return this.Services().Config.GetSetting("Client.HideDestination", false);
            }
        }
        
        private void NewOrder()
        {
			InvokeOnMainThread(() =>
            {
                InitializeOrder();

                ForceRefresh();

                if (AddressSelectionMode != AddressSelectionMode.PickupSelection)
                {
                    ActivatePickup.Execute();
                    Thread.Sleep(300);
                }

                Pickup.RequestCurrentLocationCommand.Execute();
            });
        }

        public void Reset()
        {
            InitializeOrder();
            ForceRefresh();
        }

        private void ForceRefresh()
        {
			RaisePropertyChanged(() => Order);
			RaisePropertyChanged(() => Pickup);
			RaisePropertyChanged(() => Dropoff);
			RaisePropertyChanged(() => SelectedAddress);
			RaisePropertyChanged(() => AddressSelectionMode);
			RaisePropertyChanged(() => FareEstimate);
        }

        public string FareEstimate
        {
            get { return _fareEstimate; }
            set
            {
                _fareEstimate = value;
				RaisePropertyChanged();
            }
        }

        public CreateOrder Order
        {
            get;
            private set;
        }

        public BookAddressViewModel SelectedAddress
        {
            get
            {
                if (AddressSelectionMode == AddressSelectionMode.PickupSelection)
                {
                    return Pickup;
                }
                return AddressSelectionMode == AddressSelectionMode.DropoffSelection
                        ? Dropoff 
                        : null;
            }
        }

        public IEnumerable<CoordinateViewModel> MapCenter
        {
            get { return _mapCenter; }
            private set
            {
                _mapCenter = value;
				RaisePropertyChanged();
			}
        }

        private IEnumerable<AvailableVehicle> _availableVehicles;
        public IEnumerable<AvailableVehicle> AvailableVehicles
        {
            get{return _availableVehicles;}
            set
            { 
				_availableVehicles = value;
				RaisePropertyChanged();
            }
        }

        private bool _showEstimate = true;
        public bool ShowEstimate
        {
			get { return _showEstimate && !HideDestination; }
            private set
            {
                _showEstimate = value;
				RaisePropertyChanged();
            } 
        }

        public PanelViewModel Panel { get; set; }

        public BookAddressViewModel Pickup { get; set; }

        public BookAddressViewModel Dropoff { get; set; }

        private AddressSelectionMode _addressSelectionMode;

        public AddressSelectionMode AddressSelectionMode
        {
            get
            {
                return _addressSelectionMode;
            }
            set
            {
                if (_addressSelectionMode != value)
                {
                    _addressSelectionMode = value;
					RaisePropertyChanged();
					RaisePropertyChanged(() => CanClearAddress);
                }
            }
        }

        public bool CanClearAddress
        {
            get
            { 
                return AddressSelectionMode == AddressSelectionMode.DropoffSelection 
                                && (Dropoff != null) 
                                && (Dropoff.Model != null) 
                                && Dropoff.Model.HasValidCoordinate(); 
            }
        }

		private bool _canShowTutorial = true;
        public AsyncCommand ShowTutorial
        {
            get
            {
				return GetCommand(async () =>
                {
                    if (!this.Services().Config.GetSetting("Client.TutorialEnabled", true))
                    {
                        return;
                    }

					await Task.Delay(1000);

                    var tutorialWasDisplayed = this.Services().AppCache.Get<string>("TutorialWasDisplayed");
                    if (tutorialWasDisplayed.SoftEqual(this.Services().Account.CurrentAccount.Email)) return;

                    this.Services().AppCache.Set("TutorialWasDisplayed", this.Services().Account.CurrentAccount.Email);
                    this.Services().Message.ShowDialogActivity(typeof(TutorialViewModel));
				},() => _canShowTutorial );
            }
        }

        public AsyncCommand ActivatePickup
        {
            get
            {
                return GetCommand(() => InvokeOnMainThread(delegate
                    {
                        // Close the menu if it was open
                        Panel.MenuIsOpen = false;

                        AddressSelectionMode = AddressSelectionMode == AddressSelectionMode.PickupSelection
                                                        ? AddressSelectionMode.None
                                                        : AddressSelectionMode.PickupSelection;

						RaisePropertyChanged(() => SelectedAddress);
                        CenterMap(false);
                    }));
            }
        }

        public AsyncCommand ActivateDropoff
        {
            get
            {
				return GetCommand(() => InvokeOnMainThread(delegate
                    {
                        // Close the menu if it was open
                        Panel.MenuIsOpen = false;

                        AddressSelectionMode = AddressSelectionMode == AddressSelectionMode.DropoffSelection
                                                        ? AddressSelectionMode.None
                                                        : AddressSelectionMode.DropoffSelection;

						RaisePropertyChanged(() => SelectedAddress);
                        CenterMap(false);
                    }));
            }
        }
               
        public void CenterMap(bool changeZoom)
        {
            if (AddressSelectionMode == AddressSelectionMode.DropoffSelection && Dropoff.Model.HasValidCoordinate())
            {
                MapCenter = new[] { new CoordinateViewModel { Coordinate = new Coordinate { Latitude = Dropoff.Model.Latitude, Longitude = Dropoff.Model.Longitude }, Zoom = changeZoom ? ZoomLevel.Close : ZoomLevel.DontChange } };
            }
            else if (AddressSelectionMode == AddressSelectionMode.PickupSelection && Pickup.Model.HasValidCoordinate())
            {
                MapCenter = new[] { new CoordinateViewModel { Coordinate = new Coordinate { Latitude = Pickup.Model.Latitude, Longitude = Pickup.Model.Longitude }, Zoom = changeZoom ? ZoomLevel.Close : ZoomLevel.DontChange } };
            }
            else if (AddressSelectionMode == AddressSelectionMode.None && Pickup.Model.HasValidCoordinate() && Dropoff.Model.HasValidCoordinate())
            {
                MapCenter = new[]
                {
                    new CoordinateViewModel { Coordinate = new Coordinate { Latitude = Dropoff.Model.Latitude, Longitude = Dropoff.Model.Longitude }, Zoom = changeZoom ? ZoomLevel.Close : ZoomLevel.DontChange } , 
                    new CoordinateViewModel { Coordinate = new Coordinate { Latitude = Pickup.Model.Latitude, Longitude = Pickup.Model.Longitude }, Zoom = ZoomLevel.DontChange }
                };
            }
        }

        private async void UpdateServerInfo()
        {
            var serverInfo = await this.Services().ApplicationInfo.GetAppInfoAsync();
            var appVersion = _packageInfo.Version;
            var versionFormat = this.Services().Localize["Version"];

            var version = string.Format(versionFormat, appVersion);
            if (serverInfo != null)
            {
                var serverVersionFormat = this.Services().Localize["ServerInfo"];
                version += " " + string.Format(serverVersionFormat, serverInfo.SiteName, serverInfo.Version);
            }
            Panel.Version = version;
        }

        public void PickupDateSelected()
        {
            Task.Factory.SafeStartNew(CalculateEstimate);
        }

        public AsyncCommand<DateTime?> PickupDateSelectedCommand
        {
            get
            {
                return GetCommand<DateTime?>(date =>
                {
                    if (date.HasValue && date < DateTime.Now)
                    {
                        this.Services().Message.ShowMessage(this.Services().Localize["InvalidChoiceTitle"], this.Services().Localize["BookViewInvalidDate"]);
                        Order.PickupDate = null;
                    }
                    else
                    {
                        Order.PickupDate = date;
                        InvokeOnMainThread(() => this.Services().MessengerHub.Publish(new DateTimePicked(this, Order.PickupDate)));

                        if (date.HasValue)
                        {
                            ProcessOrder();
                        }
                    }
                    PickupDateSelected();
                });
            }
        }

        public AsyncCommand BookNow
        {
            get
            {
                return GetCommand(() => {

                    // Ensure PickupDate is null (= now). It will be set to DateTime.Now later
                    Order.PickupDate = null;
                    ProcessOrder();

                });
            }
        }

        public AsyncCommand ClosePanelCommand
        {
            get
            {
                return GetCommand(() => {
                    Panel.MenuIsOpen = false;
                });
            }
        }

        public AsyncCommand GooglePlayServicesNotAvailable
		{
			get
			{
				return GetCommand(() => 
				{
					// Android specific
					// Prevent tutorial from appearing above the message box
					_canShowTutorial = false;
				});
			}
		}

        public void ProcessOrder()
        {
            using (this.Services().Message.ShowProgress())
			{
                bool isValid = this.Services().Booking.IsValid(Order);
                if (!isValid)
                {
                    Order.PickupDate = null;
                    var destinationIsRequired = this.Services().Config.GetSetting("Client.DestinationIsRequired", false);
                    if ( destinationIsRequired )
                    {
                        InvokeOnMainThread(() => this.Services().Message.ShowMessage(this.Services().Localize["InvalidBookinInfoTitle"], this.Services().Localize["InvalidBookinInfoWhenDestinationIsRequired"]));
                    }
                    else
                    {
                        InvokeOnMainThread(() => this.Services().Message.ShowMessage(this.Services().Localize["InvalidBookinInfoTitle"], this.Services().Localize["InvalidBookinInfo"]));
                    }
                    return;
                }

                if (Order.PickupDate.HasValue && Order.PickupDate.Value < DateTime.Now)
                {
                    Order.PickupDate = null;
                    InvokeOnMainThread(() => this.Services().Message.ShowMessage(this.Services().Localize["InvalidBookinInfoTitle"], this.Services().Localize["BookViewInvalidDate"]));
                    return;
                }

                TinyMessageSubscriptionToken token = null;
// ReSharper disable once RedundantAssignment
                token = this.Services().MessengerHub.Subscribe<OrderConfirmed>(msg =>
                {

// ReSharper disable AccessToModifiedClosure
                    if (token != null)
                    {
                        this.Services().MessengerHub.Unsubscribe<OrderConfirmed>(token);
// ReSharper enable AccessToModifiedClosure
                    }
                    if (msg.IsCancelled)
                    {
                        //User cancelled
                        Order.PickupDate = null;
                    }
                    else
                    {
                        Task.Factory.StartNew(CompleteOrder);
                    }
                });

                Order.Settings = this.Services().Account.CurrentAccount.Settings;                        
                if ( Order.Settings.Passengers <= 0 )
                {
                    Order.Settings.Passengers = 1;
                }

                var estimate = this.Services().Booking.GetFareEstimate(Order.PickupAddress, Order.DropOffAddress, Order.PickupDate);
                Order.Estimate.Price = estimate.Price;

                var serialized = Order.ToJson();
                ShowViewModel<BookConfirmationViewModel>(new {order = serialized});
			}
        }

        public AsyncCommand NavigateToRateOrder
        {
            get
            {
				return GetCommand(() => ShowViewModel<BookRatingViewModel>(new {
					canRate = true,
				}));
                    
            }
        }
		       
        private void CompleteOrder()
        {   
            NewOrder();
        }

        private void ShowStatusActivity(Order data, OrderStatusDetail orderInfo)
        {
            ShowViewModel<BookingStatusViewModel>(new
            {
                order = data.ToJson(),
                orderStatus = orderInfo.ToJson()
            });
        }

        private async void ObserveAvailableVehicles()
        {
            var subscription = new BooleanDisposable();
            Subscriptions.Add(subscription);
            do
            {
				AvailableVehicles = await this.Services().Vehicle.GetAvailableVehiclesAsync(Pickup.Model.Latitude, Pickup.Model.Longitude);
                await Task.Delay(5000);
            }
            while(!subscription.IsDisposed);
        }

    }
}
