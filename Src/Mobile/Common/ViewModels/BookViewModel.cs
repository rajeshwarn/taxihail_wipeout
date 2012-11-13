using System;
using Cirrious.MvvmCross.Commands;
using Cirrious.MvvmCross.ExtensionMethods;
using Cirrious.MvvmCross.Interfaces.Commands;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
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

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class BookViewModel : BaseViewModel,
        IMvxServiceConsumer<IAccountService>,
        IMvxServiceConsumer<IAppResource>,
        IMvxServiceConsumer<ILocationService>
    {
        private bool _initialized;
        private IAccountService _accountService;
        private ILocationService _geolocator;
        private bool _pickupIsActive = true;
        private bool _dropoffIsActive = false;
        private IAppResource _appResource;
        private string _version;
        private IEnumerable<CoordinateViewModel> _mapCenter;
        private string _fareEstimate;

        public BookViewModel()
        {
            InitializeViewModel();

            MessengerHub.Subscribe<LogOutRequested>(msg => Logout.Execute());

            PickupIsActive = true;
            DropoffIsActive = false;
            Pickup.RequestCurrentLocationCommand.Execute();
            
        }

        public BookViewModel(string order)
        {
            InitializeViewModel(JsonSerializer.DeserializeFromString<CreateOrder>(order));
            
            Rebook(JsonSerializer.DeserializeFromString<Order>(order));
        }

        private void InitializeViewModel(CreateOrder order = null)
        {
            if(_initialized) throw new InvalidOperationException();
            _initialized = true;

            _appResource = this.GetService<IAppResource>();
            _accountService = this.GetService<IAccountService>();
            _geolocator = this.GetService<ILocationService>();

            if (order != null)
            {
                Order = order;
            }
            else
            {
                InitializeOrder();
            }

            Pickup = new BookAddressViewModel(() => Order.PickupAddress, address => Order.PickupAddress = address, _geolocator)
            {
                Title = _appResource.GetString("BookPickupLocationButtonTitle"),
                EmptyAddressPlaceholder = _appResource.GetString("BookPickupLocationEmptyPlaceholder")
            };
            Dropoff = new BookAddressViewModel(() => Order.DropOffAddress, address => Order.DropOffAddress = address, _geolocator)
            {
                Title = _appResource.GetString("BookDropoffLocationButtonTitle"),
                EmptyAddressPlaceholder = _appResource.GetString("BookDropoffLocationEmptyPlaceholder")
            };

            Pickup.PropertyChanged += Address_PropertyChanged;
            Dropoff.PropertyChanged += Address_PropertyChanged;
            Pickup.AddressChanged += AddressChanged;
            Dropoff.AddressChanged += AddressChanged;

            _fareEstimate = _appResource.GetString("NoFareText");

            CenterMap(true);

            ThreadPool.QueueUserWorkItem(UpdateServerInfo);
        }

        void AddressChanged(object sender, EventArgs e)
        {
            InvokeOnMainThread(() => FirePropertyChanged(() => Pickup));
            InvokeOnMainThread(() => FirePropertyChanged(() => Dropoff));
            CenterMap(sender is bool ? !(bool)sender : false );
        }

        void Address_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Display")
            {
                ThreadPool.QueueUserWorkItem(CalculateEstimate);
                FirePropertyChanged(() => CanClearAddress);
            }
        }

        private void CalculateEstimate(object state)
        {
            _fareEstimate = TinyIoCContainer.Current.Resolve<IBookingService>().GetFareEstimateDisplay(Order, "EstimatePrice" , "NoFareText");
            
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
                Order.Settings = new BookingSettings { Passengers = 2 };
            }
        }


        public void NewOrder()
        {
            RequestMainThreadAction(() =>
                {
                    InitializeOrder();

                    ForceRefresh();

                    if (!PickupIsActive)
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

        public void Rebook(Order order)
        {

            var serialized = JsonSerializer.SerializeToString<Order>(order);
            Order = JsonSerializer.DeserializeFromString<CreateOrder>(serialized);
            Order.Id = Guid.Empty;
            Order.PickupDate = null;
			Pickup.SetAddress( Order.PickupAddress, false );
			Dropoff.SetAddress( Order.DropOffAddress, false );
			PickupIsActive=true;
			DropoffIsActive=false;
			CenterMap(false);
			ForceRefresh();
        }

        private void ForceRefresh()
        {
            FirePropertyChanged(() => Order);
            FirePropertyChanged(() => Pickup);
            FirePropertyChanged(() => Dropoff);
            FirePropertyChanged(() => SelectedAddress);


            FirePropertyChanged(() => PickupIsActive);
            FirePropertyChanged(() => DropoffIsActive);


			FirePropertyChanged(() => FareEstimate);

            FirePropertyChanged(() => IsInTheFuture);
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
                if (PickupIsActive)
                {
                    return Pickup;
                }
                else if (DropoffIsActive)
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

        public BookAddressViewModel Pickup { get; set; }
        public BookAddressViewModel Dropoff { get; set; }

        public bool PickupIsActive
        {
            get { return _pickupIsActive; }
            set
            {
                _pickupIsActive = value;
                FirePropertyChanged(() => PickupIsActive);

            }
        }

        public bool DropoffIsActive
        {
            get { return _dropoffIsActive; }
            set
            {
                _dropoffIsActive = value;
                FirePropertyChanged(() => DropoffIsActive);
                FirePropertyChanged(() => CanClearAddress);
            }
        }

        public bool CanClearAddress
        {
            get { return DropoffIsActive && ( Dropoff != null) && (Dropoff.Model != null) && Dropoff.Model.HasValidCoordinate(); }
        }

        public bool NoAddressActiveSelection
        {
            get { return !DropoffIsActive && !PickupIsActive; }
        }
        public MvxRelayCommand ActivatePickup
        {
            get
            {
                return new MvxRelayCommand(() =>
                {
                    PickupIsActive = !PickupIsActive;
                    if (DropoffIsActive && PickupIsActive)
                    {
                        DropoffIsActive = false;
                    }
                    if (PickupIsActive)
                    {
                        var res = TinyIoCContainer.Current.Resolve<IAppResource>();
                        TinyIoCContainer.Current.Resolve<IMessageService>().ShowToast(res.GetString("PickupWasActivatedToastMessage"), ToastDuration.Long );
                    }
                    FirePropertyChanged(() => SelectedAddress);
                    FirePropertyChanged(() => NoAddressActiveSelection);
                    CenterMap(false);
                });
            }

        }

        
        public MvxRelayCommand ActivateDropoff
        {
            get
            {
                return new MvxRelayCommand(() =>
                    {
                        DropoffIsActive = !DropoffIsActive;
                        if (DropoffIsActive && PickupIsActive)
                        {
                            PickupIsActive = false;
                        }
                        if (DropoffIsActive)
                        {
                            var res = TinyIoCContainer.Current.Resolve<IAppResource>();
                            TinyIoCContainer.Current.Resolve<IMessageService>().ShowToast(res.GetString("DropoffWasActivatedToastMessage"), ToastDuration.Long);
                        }
                        FirePropertyChanged(() => SelectedAddress);
                        FirePropertyChanged(() => NoAddressActiveSelection);

                        CenterMap(false);

                    });
            }
        }

        public MvxRelayCommand Logout
        {
            get
            {
                return new MvxRelayCommand(() =>
                    {
                        TinyIoCContainer.Current.Resolve<IAccountService>().SignOut();                        
                        RequestNavigate<LoginViewModel>(true);
                        RequestClose(this);
                    });
            }
        }

        
        private void CenterMap(bool changeZoom)
        {
            var c = new ObservableCollection<string>();
            
            

            if (DropoffIsActive && Dropoff.Model.HasValidCoordinate())
            {
                MapCenter = new CoordinateViewModel[] { new CoordinateViewModel { Coordinate = new Coordinate { Latitude = Dropoff.Model.Latitude, Longitude = Dropoff.Model.Longitude }, Zoom = changeZoom ? ZoomLevel.Close : ZoomLevel.DontChange } };
            }
            else if (PickupIsActive && Pickup.Model.HasValidCoordinate())
            {
                MapCenter = new CoordinateViewModel[] { new CoordinateViewModel { Coordinate = new Coordinate { Latitude = Pickup.Model.Latitude, Longitude = Pickup.Model.Longitude }, Zoom = changeZoom ? ZoomLevel.Close : ZoomLevel.DontChange } };
            }
            else if ((!PickupIsActive && Pickup.Model.HasValidCoordinate()) && (!DropoffIsActive && Dropoff.Model.HasValidCoordinate()))
            {
                MapCenter = new CoordinateViewModel[] { new CoordinateViewModel { Coordinate = new Coordinate { Latitude = Dropoff.Model.Latitude, Longitude = Dropoff.Model.Longitude }, Zoom = changeZoom ? ZoomLevel.Close : ZoomLevel.DontChange } , 
                                            new CoordinateViewModel { Coordinate = new Coordinate { Latitude = Pickup.Model.Latitude, Longitude = Pickup.Model.Longitude }, Zoom = ZoomLevel.DontChange }};
            }

        }




        private void UpdateServerInfo(object state)
        {

            _serverInfo = TinyIoCContainer.Current.Resolve<IApplicationInfoService>().GetAppInfo();
            _version = null;
            FirePropertyChanged(() => Version);

        }
        private ApplicationInfo _serverInfo;

        public string Version
        {
            get
            {
                if (_version == null)
                {
                    string appVersion = TinyIoCContainer.Current.Resolve<IPackageInfo>().Version;
                    var versionFormat = TinyIoCContainer.Current.Resolve<IAppResource>().GetString("Version");
                    _version = string.Format(versionFormat, appVersion);

                    if (_serverInfo != null)
                    {
                        var serverVersionFormat = TinyIoCContainer.Current.Resolve<IAppResource>().GetString("ServerInfo");
                        _version += " " + string.Format(serverVersionFormat, _serverInfo.SiteName, _serverInfo.Version);
                    }
                }
                return _version;
                //android:text="v1.0.10 (Atlanta Checker v1.0.1)"           
            }
        }

        public bool IsInTheFuture { get { return Order.PickupDate.HasValue; } }

        public string PickupDateDisplay
        {
            get
            {
                if (Order.PickupDate.HasValue)
                {
                    var format = _appResource.GetString("PickupDateDisplay");
                    return String.Format(format, Order.PickupDate.Value);
                }
                else
                {
                    return "";
                }

            }
        }
        public void PickupDateSelected()
        {
            FirePropertyChanged(() => IsInTheFuture);
            FirePropertyChanged(() => PickupDateDisplay);
            ThreadPool.QueueUserWorkItem(CalculateEstimate);
        }
		public IMvxCommand PickupDateSelectedCommand
		{
			get
			{
				return new MvxRelayCommand<DateTime?>(date => {
					if( date.HasValue && date < DateTime.Now )
					{
						var res = TinyIoCContainer.Current.Resolve<IAppResource>();
						TinyIoCContainer.Current.Resolve<IMessageService>().ShowMessage( res.GetString("InvalidChoiceTitle"), res.GetString("BookViewInvalidDate") );
						Order.PickupDate = null;
					}
					else
					{
						Order.PickupDate = date;
						InvokeOnMainThread(() => TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Publish(new DateTimePicked(this, Order.PickupDate )));
					}
					PickupDateSelected();
				});
			}
		}

        public IMvxCommand NavigateToRateOrder
        {
            get
            {
                return new MvxRelayCommand(() => RequestNavigate<BookRatingViewModel>(
                    new KeyValuePair<string, bool>("canRate", true)));
               
            }
        }

        public IMvxCommand  NavigateToConfirmationCommand
        {
            get
            {

                return new MvxRelayCommand(() => 
                                           {

                    var serialized = Order.ToJson (); // JsonSerializer.SerializeToString<.SerializeToString(, typeof(OrderWithStatusModel));
                    RequestNavigate<BookConfirmationViewModel>(new {order = serialized});
                });
            }
        }

        public IMvxCommand NavigateToHistoryList
        {
            get
            {
                return new MvxRelayCommand(() => RequestNavigate<HistoryViewModel>());

            }
        }

        public IMvxCommand NavigateToOrderStatus 
		{
			get {
				return new MvxRelayCommand<Dictionary<string,object>> (order =>
				{					
					var orderGet = (Order)order ["order"];                  
					var orderInfoGet = (OrderStatusDetail)order ["orderInfo"];
					var orderWithStatus = new OrderWithStatusModel () { Order = orderGet, OrderStatusDetail = orderInfoGet };
					var serialized = JsonSerializer.SerializeToString (orderWithStatus, typeof(OrderWithStatusModel));
                    RequestNavigate<BookingStatusViewModel> (new {order = serialized});
				});
			}
		}

   }
}
