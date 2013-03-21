using System;
using Cirrious.MvvmCross.Commands;
using Cirrious.MvvmCross.ExtensionMethods;
using Cirrious.MvvmCross.Interfaces.Commands;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using Cirrious.MvvmCross.Interfaces.Views;
using Cirrious.MvvmCross.ViewModels;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Mobile.AppServices;
using System.Threading.Tasks;
using apcurium.MK.Booking.Mobile.Models;
using apcurium.MK.Booking.Mobile.Navigation;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Infrastructure;
using TinyMessenger;
using apcurium.MK.Booking.Mobile.Messages;
using ServiceStack.Text;
using System.Threading;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Extensions;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.ObjectModel;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Extensions;
using System.Globalization;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Mobile.Extensions;
using Cirrious.MvvmCross.Interfaces.Platform.Lifetime;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class BookViewModel : BaseViewModel,
        IMvxServiceConsumer<IAccountService>,
        IMvxServiceConsumer<ILocationService>,
        IMvxServiceConsumer<IBookingService>,
        IMvxServiceConsumer<IApplicationInfoService>,
        IMvxServiceConsumer<ICacheService>
    {
        private bool _initialized;
        private IAccountService _accountService;
        private ILocationService _geolocator;
        private IBookingService _bookingService;
        private IApplicationInfoService _applicationInfoService;
        private IEnumerable<CoordinateViewModel> _mapCenter;
        private string _fareEstimate;

        private bool _useExistingOrder = false;

        public BookViewModel()
        {
         
        }

        public BookViewModel(string order)
        {
            Order = JsonSerializer.DeserializeFromString<CreateOrder>(order);   
             
            Order.Id = Guid.Empty;
            Order.PickupDate = null;

            _useExistingOrder = true;
         

        }

        protected override void Initialize()
        {
            if (_initialized)
                throw new InvalidOperationException();
            _initialized = true;

            _accountService = this.GetService<IAccountService>();
            _accountService = this.GetService<IAccountService>();
            _geolocator = this.GetService<ILocationService>();
            _bookingService = this.GetService<IBookingService>();
            _applicationInfoService= this.GetService<IApplicationInfoService>();
            Panel = new PanelViewModel(this);
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

            Pickup = new BookAddressViewModel(() => Order.PickupAddress, address => Order.PickupAddress = address, _geolocator)
            {
                IsExecuting = true,
                //Title = Resources.GetString("BookPickupLocationButtonTitle"),
                EmptyAddressPlaceholder = Resources.GetString("BookPickupLocationEmptyPlaceholder")
            };
            Dropoff = new BookAddressViewModel(() => Order.DropOffAddress, address => Order.DropOffAddress = address, _geolocator)
            {
                //Title = Resources.GetString("BookDropoffLocationButtonTitle"),
                EmptyAddressPlaceholder = Resources.GetString("BookDropoffLocationEmptyPlaceholder")
            };
            
            Pickup.AddressChanged += AddressChanged;
            Dropoff.AddressChanged += AddressChanged;
            Dropoff.AddressCleared += RevertToPickupSelection;

            AddressSelectionMode = AddressSelectionMode.PickupSelection;

            if (! _useExistingOrder ) 
            {
                var tutorialWasDisplayed = this.GetService<ICacheService>().Get<string>("TutorialWasDisplayed");
                if (tutorialWasDisplayed.IsNullOrEmpty())
                {
                    Task.Factory.SafeStartNew( () =>
                                              {
                        Thread.Sleep( 4000 );
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

            Task.Factory.SafeStartNew( ()=> UpdateServerInfo());
 
            ForceRefresh();
        }

        void RevertToPickupSelection (object sender, EventArgs e)
        {
            ActivatePickup.Execute(null);
        }

        void LoadLastActiveOrder( )
        {
            if (_bookingService.HasLastOrder)
            {
                _bookingService.GetLastOrderStatus().ContinueWith(t => 
                {
                    var isCompleted = _bookingService.IsStatusCompleted(t.Result.IBSStatusId);
                    if (isCompleted)
                    {
                        _bookingService.ClearLastOrder();
                    }
                    else
                    {
                        var order = TinyIoCContainer.Current.Resolve<IAccountService>().GetHistoryOrder(t.Result.OrderId);
                        ShowStatusActivity(order, t.Result);
                    }
                }, TaskContinuationOptions.OnlyOnRanToCompletion);
            }
        }

        private void CheckVersion()
        {
            Task.Factory.SafeStartNew(() =>
            {
                //The 2 second delay is required because the view might not be created.
                Thread.Sleep(2000);
                
                if (_accountService.CurrentAccount != null)
                {
                    TinyIoCContainer.Current.Resolve<IApplicationInfoService>().CheckVersion();
                }
            });

        }

        void AddressChanged(object sender, EventArgs e)
        {
            InvokeOnMainThread(() => FirePropertyChanged(() => Pickup));
            InvokeOnMainThread(() => FirePropertyChanged(() => Dropoff));
            CenterMap(sender is bool ? !(bool)sender : false);

            Task.Factory.SafeStartNew(() => CalculateEstimate());
            FirePropertyChanged(() => CanClearAddress);
        }

        private void CalculateEstimate()
        {
            _fareEstimate = TinyIoCContainer.Current.Resolve<IBookingService>().GetFareEstimateDisplay(Order, "EstimatePrice", "NoFareText", true, "EstimatedFareNotAvailable");
            
            InvokeOnMainThread(() => FirePropertyChanged(() => FareEstimate));

        }

        public void InitializeOrder()
        {
            Order = new CreateOrder();
            if (_accountService.CurrentAccount != null)
            {
                Order.Settings = _accountService.CurrentAccount.Settings;
            }
            else
            {
                Order.Settings = new BookingSettings();
            }
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
                if (culture.IsNullOrEmpty())
                {
                    return "en-US";
                }
                else
                {
                    return culture;                
                }
            }
        }

        private void NewOrder()
        {
            RequestMainThreadAction(() =>
            {
                InitializeOrder();

                ForceRefresh();

                if (this.AddressSelectionMode != Data.AddressSelectionMode.PickupSelection)
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
                if (this.AddressSelectionMode == Data.AddressSelectionMode.PickupSelection)
                {
                    return Pickup;
                }
                else if (this.AddressSelectionMode == Data.AddressSelectionMode.DropoffSelection)
                {
                    return Dropoff;
                }
                else
                {
                    return null;
                }
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

        public bool ShowEstimate
        {
            get
            {
                var ret = true;
                try
                {
                     ret =  Boolean.Parse(TinyIoCContainer.Current.Resolve<IConfigurationManager>().GetSetting("Client.ShowEstimate"));
                }
                catch (Exception)
                {
                    return true;
                }
                return ret;
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
                    FirePropertyChanged("AddressSelectionMode");
                    FirePropertyChanged(() => CanClearAddress);
                }
            }
        }

        public bool CanClearAddress
        {
            get
            { 
                return this.AddressSelectionMode == Data.AddressSelectionMode.DropoffSelection && (Dropoff != null) && (Dropoff.Model != null) && Dropoff.Model.HasValidCoordinate(); 
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
                            var tutorialWasDisplayed = this.GetService<ICacheService>().Get<string>("TutorialWasDisplayed");
                            if (tutorialWasDisplayed.IsNullOrEmpty())
                            {
                                this.GetService<ICacheService>().Set<string>("TutorialWasDisplayed", true.ToString());
                                MessageService.ShowDialogActivity(typeof(TutorialViewModel));
                            }
                        });
                    });
                });
            }
        }

        public IMvxCommand ActivatePickup
        {
            get
            {
                return GetCommand(() =>
                {
                    this.InvokeOnMainThread(delegate
                    {
                        // Close the menu if it was open
                        Panel.MenuIsOpen = false;

                        this.AddressSelectionMode = this.AddressSelectionMode == Data.AddressSelectionMode.PickupSelection
                            ? Data.AddressSelectionMode.None
                            : Data.AddressSelectionMode.PickupSelection;

                        if (this.AddressSelectionMode == Data.AddressSelectionMode.PickupSelection && this.IsVisible)
                        {
                            MessageService.ShowToast(Resources.GetString("PickupWasActivatedToastMessage"), ToastDuration.Long);
                        }
                        FirePropertyChanged(() => SelectedAddress);
                        CenterMap(false);
                    });
                });
            }

        }
        
        public IMvxCommand ActivateDropoff
        {
            get
            {
                return GetCommand(() =>
                {
                    this.RequestMainThreadAction(delegate
                    {
                        // Close the menu if it was open
                        Panel.MenuIsOpen = false;

                        this.AddressSelectionMode = this.AddressSelectionMode == Data.AddressSelectionMode.DropoffSelection
                                ? Data.AddressSelectionMode.None
                                : Data.AddressSelectionMode.DropoffSelection;

                        if (this.AddressSelectionMode == Data.AddressSelectionMode.DropoffSelection && this.IsVisible)
                        {
                            MessageService.ShowToast(Resources.GetString("DropoffWasActivatedToastMessage"), ToastDuration.Long);
                        }
                        FirePropertyChanged(() => SelectedAddress);
                        CenterMap(false);
                    });
                });
            }
        }
               
        private void CenterMap(bool changeZoom)
        {
            if (this.AddressSelectionMode == Data.AddressSelectionMode.DropoffSelection && Dropoff.Model.HasValidCoordinate())
            {
                MapCenter = new CoordinateViewModel[] { new CoordinateViewModel { Coordinate = new Coordinate { Latitude = Dropoff.Model.Latitude, Longitude = Dropoff.Model.Longitude }, Zoom = changeZoom ? ZoomLevel.Close : ZoomLevel.DontChange } };
            }
            else if (this.AddressSelectionMode == Data.AddressSelectionMode.PickupSelection && Pickup.Model.HasValidCoordinate())
            {
                MapCenter = new CoordinateViewModel[] { new CoordinateViewModel { Coordinate = new Coordinate { Latitude = Pickup.Model.Latitude, Longitude = Pickup.Model.Longitude }, Zoom = changeZoom ? ZoomLevel.Close : ZoomLevel.DontChange } };
            }
            else if (this.AddressSelectionMode == Data.AddressSelectionMode.None && Pickup.Model.HasValidCoordinate() && Dropoff.Model.HasValidCoordinate())
            {
                MapCenter = new CoordinateViewModel[]
                {
                    new CoordinateViewModel { Coordinate = new Coordinate { Latitude = Dropoff.Model.Latitude, Longitude = Dropoff.Model.Longitude }, Zoom = changeZoom ? ZoomLevel.Close : ZoomLevel.DontChange } , 
                    new CoordinateViewModel { Coordinate = new Coordinate { Latitude = Pickup.Model.Latitude, Longitude = Pickup.Model.Longitude }, Zoom = ZoomLevel.DontChange }
                };
            }

        }

        private void UpdateServerInfo()
        {
            string appVersion = TinyIoCContainer.Current.Resolve<IPackageInfo>().Version;
            var versionFormat = TinyIoCContainer.Current.Resolve<IAppResource>().GetString("Version");
            var serverInfo = TinyIoCContainer.Current.Resolve<IApplicationInfoService>().GetAppInfo();
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
                            BookTaxi.Execute();
                            
                        }
                    }
                    PickupDateSelected();
                });
            }
        }

        public IMvxCommand BookTaxi
        {
            get
            {
                return ConfirmOrder;
            }
        }

        public IMvxCommand ConfirmOrder
        {
            get
            {
                return GetCommand(() =>
                {
                   
                    bool isValid = _bookingService.IsValid(Order);
                    if (!isValid)
                    {
                        Order.PickupDate = null;
                        InvokeOnMainThread(() => MessageService.ShowMessage(Resources.GetString("InvalidBookinInfoTitle"), Resources.GetString("InvalidBookinInfo")));
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
                            Task.Factory.StartNew(() => CompleteOrder(msg.Content));
                        }
                    });

                    InvokeOnMainThread(() =>
                    {
                        Order.Settings = _accountService.CurrentAccount.Settings;                        
                        if ( Order.Settings.Passengers <= 0 )
                        {
                            Order.Settings.Passengers = 1;
                        }

                        var serialized = Order.ToJson();
                        RequestNavigate<BookConfirmationViewModel>(new { order = serialized }, false, MvxRequestedBy.UserAction);
                    });
                });
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
		       

        private void CompleteOrder(CreateOrder order)
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

    }
}
