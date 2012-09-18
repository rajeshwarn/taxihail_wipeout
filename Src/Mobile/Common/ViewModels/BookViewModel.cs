using System;
using Cirrious.MvvmCross.Commands;
using Cirrious.MvvmCross.Interfaces.Commands;
using Cirrious.MvvmCross.ViewModels;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Mobile.AppServices;
using Xamarin.Geolocation;
using System.Threading.Tasks;
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
    public class BookViewModel : BaseViewModel
    {
        private IAccountService _accountService;
        private Geolocator _geolocator;
        private bool _pickupIsActive = true;
        private bool _dropoffIsActive = false;
        private IAppResource _appResource;
        private string _version;
        private IEnumerable<CoordinateViewModel> _mapCenter;
        private string _fareEstimate;

        public BookViewModel(IAccountService accountService, IAppResource appResource, Geolocator geolocator)
        {
            _appResource = appResource;
            _accountService = accountService;
            _geolocator = geolocator;
            _appResource = appResource;

            Load();
            Pickup = new BookAddressViewModel(() => Order.PickupAddress, address => Order.PickupAddress = address, _geolocator) { Title = appResource.GetString("BookPickupLocationButtonTitle"), EmptyAddressPlaceholder = appResource.GetString("BookPickupLocationEmptyPlaceholder") };
            Dropoff = new BookAddressViewModel(() => Order.DropOffAddress, address => Order.DropOffAddress = address, _geolocator) { Title = appResource.GetString("BookDropoffLocationButtonTitle"), EmptyAddressPlaceholder = appResource.GetString("BookDropoffLocationEmptyPlaceholder") };

            Pickup.PropertyChanged -= new PropertyChangedEventHandler(Address_PropertyChanged);
            Pickup.PropertyChanged += new PropertyChangedEventHandler(Address_PropertyChanged);

            Dropoff.PropertyChanged -= new PropertyChangedEventHandler(Address_PropertyChanged);
            Dropoff.PropertyChanged += new PropertyChangedEventHandler(Address_PropertyChanged);

            Pickup.AddressChanged -= new EventHandler(AddressChanged);
            Pickup.AddressChanged += new EventHandler(AddressChanged);

            Dropoff.AddressChanged -= new EventHandler(AddressChanged);
            Dropoff.AddressChanged += new EventHandler(AddressChanged);

            _fareEstimate = appResource.GetString("NoFareText");

            CenterMap(true);

            ThreadPool.QueueUserWorkItem(UpdateServerInfo);
        }

        void AddressChanged(object sender, EventArgs e)
        {
            InvokeOnMainThread(() => FirePropertyChanged(() => Pickup));
            InvokeOnMainThread(() => FirePropertyChanged(() => Dropoff));
            CenterMap(sender is bool ? !(bool)sender : false );
        }

        void Address_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Display")
            {
                ThreadPool.QueueUserWorkItem(CalculateEstimate);                                
            }
        }

        private void CalculateEstimate(object state)
        {
            _fareEstimate = _appResource.GetString("NoFareText");



            if (Order.PickupAddress.HasValidCoordinate() && Order.DropOffAddress.HasValidCoordinate())
            {
                var directionInfo = TinyIoCContainer.Current.Resolve<IGeolocService>().GetDirectionInfo(Order.PickupAddress.Latitude, Order.PickupAddress.Longitude, Order.DropOffAddress.Latitude, Order.DropOffAddress.Longitude);
                if (directionInfo != null)
                {
                    if (directionInfo.Price.HasValue)
                    {
                        if (directionInfo.Price.Value > 100)
                        {
                            _fareEstimate = _appResource.GetString("EstimatePriceOver100");
                        }
                        else
                        {
                            _fareEstimate = String.Format(_appResource.GetString("EstimatePrice"), directionInfo.FormattedPrice);
                        }

                        if (directionInfo.Distance.HasValue)
                        {
                            _fareEstimate += String.Format(_appResource.GetString("EstimateDistance"), directionInfo.FormattedDistance);

                        }
                    }
                    else
                    {
                        _fareEstimate = String.Format(_appResource.GetString("EstimatedFareNotAvailable"));
                    }


                }

            }

            InvokeOnMainThread(() => FirePropertyChanged(() => FareEstimate));

        }


        public override void Load()
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
                    Load();

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
            Load();
        }

        public void Rebook(Order order)
        {
            var serialized = JsonSerializer.SerializeToString<Order>(order);
            Order = JsonSerializer.DeserializeFromString<CreateOrder>(serialized);
            Order.Id = Guid.Empty;
            Order.PickupDate = null;
            Order.PickupDate = null;
            ForceRefresh();
        }

        private void ForceRefresh()
        {
            FirePropertyChanged(() => Order);
            FirePropertyChanged(() => Pickup);
            FirePropertyChanged(() => Dropoff);
            FirePropertyChanged(() => SelectedAddress);
            FirePropertyChanged(() => FareEstimate);
            FirePropertyChanged(() => IsInTheFuture);
            FirePropertyChanged(() => PickupIsActive);
            FirePropertyChanged(() => DropoffIsActive);
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
            }
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
                        
                        RequestNavigate<LoginViewModel>();
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
            else
            {
                //var position = TinyIoCContainer.Current.Resolve<IUserPositionService>().LastKnownPosition;
                //if (position.IsUsable)
                //{
                //    MapCenter = new CoordinateViewModel[] { new CoordinateViewModel { Coordinate = new Coordinate { Latitude = position.Latitude, Longitude = position.Longitude }, Zoom = changeZoom ? ZoomLevel.Close : ZoomLevel.DontChange } };
                //}
                //else if ((position.RefreshTime == CoordinateRefreshTime.Recently) || (position.RefreshTime == CoordinateRefreshTime.NotRecently))
                //{
                //    MapCenter = new CoordinateViewModel[] { new CoordinateViewModel { Coordinate = new Coordinate { Latitude = position.Latitude, Longitude = position.Longitude }, Zoom = = changeZoom ? ZoomLevel.Medium : ZoomLevel.DontChange } };
                //}
                //else
                //{
                //    MapCenter = new CoordinateViewModel[] { new CoordinateViewModel { Coordinate = new Coordinate { Latitude = position.Latitude, Longitude = position.Longitude }, Zoom = = changeZoom ? ZoomLevel.Overview: ZoomLevel.DontChange } };
                //}

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



        public void Initialize()
        {
            PickupIsActive = true;
            Pickup.RequestCurrentLocationCommand.Execute();
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

        }


        
        
    }
}