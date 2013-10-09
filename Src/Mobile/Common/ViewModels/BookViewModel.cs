using System;
using Cirrious.MvvmCross.Commands;
using Cirrious.MvvmCross.Interfaces.Commands;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Mobile.AppServices;
using System.Threading.Tasks;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Infrastructure;
using TinyMessenger;
using apcurium.MK.Booking.Mobile.Messages;
using ServiceStack.Text;
using System.Threading;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Extensions;
using System.Collections.Generic;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Extensions;
using System.Globalization;
using apcurium.MK.Common.Entity;
using apcurium.Framework;
using System.Reactive.Linq;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class BookViewModel : BaseViewModel
    {
        private bool _initialized;

        private IEnumerable<CoordinateViewModel> _mapCenter;
        private string _fareEstimate;

        private bool _useExistingOrder;

        public BookViewModel()
        {
         
        }

        public BookViewModel(string order)
        {
            Order = order.FromJson<CreateOrder>();
             
            Order.Id = Guid.Empty;
            Order.PickupDate = null;

            _useExistingOrder = true;
        }

        protected async override void Initialize()
        {
            if (_initialized)
                throw new InvalidOperationException();
            _initialized = true;

            Panel = new PanelViewModel(this);

            var showEstimate = Task.Run(() =>  Boolean.Parse(TinyIoCContainer.Current.Resolve<IConfigurationManager>().GetSetting("Client.ShowEstimate")));

            try
            {
                ShowEstimate = await showEstimate;
            }
            catch
            {
                ShowEstimate = true;
            }

        }

		public override void Load()
        {
			base.Load();

            if ( Order == null || !_useExistingOrder )
            {
                InitializeOrder();           
            }

            CheckVersion();

            LoadLastActiveOrder();

            Pickup = new BookAddressViewModel(() => Order.PickupAddress, address => Order.PickupAddress = address)
            {
                IsExecuting = true,
                EmptyAddressPlaceholder = Resources.GetString("BookPickupLocationEmptyPlaceholder")
            };
            Dropoff = new BookAddressViewModel(() => Order.DropOffAddress, address => Order.DropOffAddress = address)
            {
                EmptyAddressPlaceholder = Resources.GetString("BookDropoffLocationEmptyPlaceholder")
            };
            
            Pickup.AddressChanged += AddressChanged;
            Dropoff.AddressChanged += AddressChanged;
            Dropoff.AddressCleared += RevertToPickupSelection;

            AddressSelectionMode = AddressSelectionMode.PickupSelection;

            if (! _useExistingOrder ) 
            {
                var tutorialWasDisplayed = CacheService.Get<string>("TutorialWasDisplayed");
                if (tutorialWasDisplayed.IsNullOrEmpty())
                {
                    Task.Run( () =>
                    {
                        //Thread.Sleep( 4000 );
                        Pickup.RequestCurrentLocationCommand.Execute();            
                    });
                }
                else
                {
                    Pickup.RequestCurrentLocationCommand.Execute();            
                }
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

        void RevertToPickupSelection (object sender, EventArgs e)
        {
            ActivatePickup.Execute(null);
        }

        void LoadLastActiveOrder( )
        {
            if (BookingService.HasLastOrder)
            {
                BookingService.GetLastOrderStatus().ContinueWith(t => 
                {
                    var isCompleted = BookingService.IsStatusCompleted(t.Result.IBSStatusId);
                    if (isCompleted)
                    {
                        BookingService.ClearLastOrder();
                    }
                    else
                    {
                        var order = AccountService.GetHistoryOrder(t.Result.OrderId);
                        ShowStatusActivity(order, t.Result);
                    }
                }, TaskContinuationOptions.OnlyOnRanToCompletion);
            }
        }

        private async void CheckVersion()
        {
            //The 2 second delay is required because the view might not be created.
            await Task.Delay(2000);
            if (AccountService.CurrentAccount != null)
            {
                TinyIoCContainer.Current.Resolve<IApplicationInfoService>().CheckVersionAsync();
            }
        }

        void AddressChanged(object sender, EventArgs e)
        {
            InvokeOnMainThread(() =>
                {
                    FirePropertyChanged(() => Pickup);
                    FirePropertyChanged(() => Dropoff);
                });

            CenterMap(sender is bool && !(bool)sender);

            LoadAvailableVehicles (Pickup.Model.Latitude, Pickup.Model.Longitude);

            Task.Factory.SafeStartNew(CalculateEstimate);
            FirePropertyChanged(() => CanClearAddress);
        }

        private void CalculateEstimate() 
        {
            _fareEstimate = this.Resources.GetString("EstimatingFare");

            FirePropertyChanged(() => FareEstimate);

            _fareEstimate = base.BookingService.GetFareEstimateDisplay(Order, "EstimatePriceFormat", "NoFareText", true, "EstimatedFareNotAvailable");
            
            FirePropertyChanged(() => FareEstimate);
        }

        public void InitializeOrder()
        {
            Order = new CreateOrder
                {
                    Settings = AccountService.CurrentAccount != null
                                   ? AccountService.CurrentAccount.Settings
                                   : new BookingSettings()
                };
        }

        public bool UseAmPmFormat
        {
            get
            {
                return new CultureInfo(CultureInfo).DateTimeFormat.LongTimePattern.ToLower().Contains("tt");
            }
        }

        public string CultureInfo
        {
            get
            {
                var culture = TinyIoCContainer.Current.Resolve<IConfigurationManager>().GetSetting("PriceFormat");
                return culture.IsNullOrEmpty() ? "en-US" : culture;
            }
        }

        private void NewOrder()
        {
            RequestMainThreadAction(() =>
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
            FirePropertyChanged(() => Order);
            FirePropertyChanged(() => Pickup);
            FirePropertyChanged(() => Dropoff);
            FirePropertyChanged(() => SelectedAddress);
            FirePropertyChanged(() => AddressSelectionMode);
            FirePropertyChanged(() => FareEstimate);
        }

        public string FareEstimate
        {
            get { return _fareEstimate; }
            set
            {
                _fareEstimate = value;
                FirePropertyChanged(() => FareEstimate);
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
                FirePropertyChanged(() => MapCenter);
			}
        }

        private IEnumerable<AvailableVehicle> _availableVehicles;
        public IEnumerable<AvailableVehicle> AvailableVehicles
        {
            get{return _availableVehicles;}
            set
            { 
				_availableVehicles = value;
				FirePropertyChanged(() => AvailableVehicles);
            }
        }

        private bool _showEstimate = true;
        public bool ShowEstimate
        {
            get { return _showEstimate; }
            private set
            {
                _showEstimate = value;
                FirePropertyChanged(() => ShowEstimate);
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
                    FirePropertyChanged(()=>AddressSelectionMode);
                    FirePropertyChanged(() => CanClearAddress);
                }
            }
        }

        public bool CanClearAddress
        {
            get
            { 
                return AddressSelectionMode == AddressSelectionMode.DropoffSelection && (Dropoff != null) && (Dropoff.Model != null) && Dropoff.Model.HasValidCoordinate(); 
            }
        }

        public IMvxCommand ShowTutorial
        {
            get
            {
                return new MvxRelayCommand(() =>
                {
                    if ( !Settings.TutorialEnabled )
                    {
                        return;
                    }

                    Task.Factory.SafeStartNew(() => 
                    {
                        Thread.Sleep(1000);
                        InvokeOnMainThread(() =>
                        {
                            var tutorialWasDisplayed = CacheService.Get<string>("TutorialWasDisplayed");
                            if (!tutorialWasDisplayed.IsNullOrEmpty()) return;

                            CacheService.Set("TutorialWasDisplayed", true.ToString());
                            MessageService.ShowDialogActivity(typeof(TutorialViewModel));
                        });
                    });
                });
            }
        }

        public IMvxCommand ActivatePickup
        {
            get
            {
                return GetCommand(() => this.InvokeOnMainThread(delegate
                    {
                        // Close the menu if it was open
                        Panel.MenuIsOpen = false;

                        AddressSelectionMode = AddressSelectionMode == AddressSelectionMode.PickupSelection
                                                        ? AddressSelectionMode.None
                                                        : AddressSelectionMode.PickupSelection;

                        if (AddressSelectionMode == AddressSelectionMode.PickupSelection && IsVisible)
                        {
                            MessageService.ShowToast(Resources.GetString("PickupWasActivatedToastMessage"), ToastDuration.Long);
                        }
                        FirePropertyChanged(() => SelectedAddress);
                        CenterMap(false);
                    }));
            }
        }
        
        public IMvxCommand ActivateDropoff
        {
            get
            {
                return GetCommand(() => RequestMainThreadAction(delegate
                    {
                        // Close the menu if it was open
                        Panel.MenuIsOpen = false;

                        AddressSelectionMode = AddressSelectionMode == AddressSelectionMode.DropoffSelection
                                                        ? AddressSelectionMode.None
                                                        : AddressSelectionMode.DropoffSelection;

                        if (AddressSelectionMode == AddressSelectionMode.DropoffSelection && IsVisible)
                        {
                            MessageService.ShowToast(Resources.GetString("DropoffWasActivatedToastMessage"), ToastDuration.Long);
                        }
                        FirePropertyChanged(() => SelectedAddress);
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
            var serverInfo = await TinyIoCContainer.Current.Resolve<IApplicationInfoService>().GetAppInfoAsync();
            var appVersion = TinyIoCContainer.Current.Resolve<IPackageInfo>().Version;
            var versionFormat = TinyIoCContainer.Current.Resolve<IAppResource>().GetString("Version");

            var version = string.Format(versionFormat, appVersion);
            if (serverInfo != null)
            {
                var serverVersionFormat = TinyIoCContainer.Current.Resolve<IAppResource>().GetString("ServerInfo");
                version += " " + string.Format(serverVersionFormat, serverInfo.SiteName, serverInfo.Version);
            }
            Panel.Version = version;
        }

        public void PickupDateSelected()
        {
            Task.Factory.SafeStartNew(CalculateEstimate);
        }

        public IMvxCommand PickupDateSelectedCommand
        {
            get
            {
                return GetCommand<DateTime?>(date =>
                {
                    if (date.HasValue && date < DateTime.Now)
                    {
                        MessageService.ShowMessage(Resources.GetString("InvalidChoiceTitle"), Resources.GetString("BookViewInvalidDate"));
                        Order.PickupDate = null;
                    }
                    else
                    {
                        Order.PickupDate = date;
                        InvokeOnMainThread(() => TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Publish(new DateTimePicked(this, Order.PickupDate)));

                        if (date.HasValue)
                        {
                            ProcessOrder();
                        }
                    }
                    PickupDateSelected();
                });
            }
        }

        public IMvxCommand BookNow
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

        public void ProcessOrder()
        {
            using(MessageService.ShowProgress())
			{
                bool isValid = BookingService.IsValid(Order);
                if (!isValid)
                {
                    Order.PickupDate = null;
                    var destinationIsRequired = TinyIoCContainer.Current.Resolve<IConfigurationManager>().GetSetting<bool>("Client.DestinationIsRequired", false);
                    if ( destinationIsRequired )
                    {
                        InvokeOnMainThread(() => MessageService.ShowMessage(Resources.GetString("InvalidBookinInfoTitle"), Resources.GetString("InvalidBookinInfoWhenDestinationIsRequired")));
                    }
                    else
                    {
                        InvokeOnMainThread(() => MessageService.ShowMessage(Resources.GetString("InvalidBookinInfoTitle"), Resources.GetString("InvalidBookinInfo")));
                    }
                    return;
                }

                if (Order.PickupDate.HasValue && Order.PickupDate.Value < DateTime.Now)
                {
                    Order.PickupDate = null;
                    InvokeOnMainThread(() => MessageService.ShowMessage(Resources.GetString("InvalidBookinInfoTitle"), Resources.GetString("BookViewInvalidDate")));
                    return;
                }

                TinyMessageSubscriptionToken token = null;
                token = MessengerHub.Subscribe<OrderConfirmed>(msg =>
                {
                    if (token != null)
                    {
                        MessengerHub.Unsubscribe<OrderConfirmed>(token);
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

                Order.Settings = AccountService.CurrentAccount.Settings;                        
                if ( Order.Settings.Passengers <= 0 )
                {
                    Order.Settings.Passengers = 1;
                }

                var estimate = BookingService.GetFareEstimate(Order.PickupAddress, Order.DropOffAddress, Order.PickupDate);
                Order.Estimate.Price = estimate.Price;

                var serialized = Order.ToJson();
                RequestNavigate<BookConfirmationViewModel>(new {order = serialized}, false, MvxRequestedBy.UserAction);
			}
        }

        public IMvxCommand NavigateToRateOrder
        {
            get
            {
                return GetCommand(() => RequestNavigate<BookRatingViewModel>(
                    new KeyValuePair<string, bool>("canRate", true)));
            }
        }
		       
        private void CompleteOrder()
        {   
            NewOrder();
        }

        private void ShowStatusActivity(Order data, OrderStatusDetail orderInfo)
        {
            RequestNavigate<BookingStatusViewModel>(new
            {
                order = data.ToJson(),
                orderStatus = orderInfo.ToJson()
            });
        }

        private IDisposable _getAvailableVehicles = NullDisposable.Instance;
        private void LoadAvailableVehicles(double latitude, double longitude)
        {
            _getAvailableVehicles.Dispose ();
            _getAvailableVehicles = Observable.Start (() => VehicleClient.GetAvailableVehicles (latitude, longitude))
                .Subscribe (result => InvokeOnMainThread(() =>{
                    AvailableVehicles = result;
                }));
        }
    }
}
