using System;
using Cirrious.MvvmCross.Commands;
using Cirrious.MvvmCross.Interfaces.Commands;
using Cirrious.MvvmCross.ViewModels;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Mobile.AppServices;
using Xamarin.Geolocation;
using System.Threading.Tasks;
namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class BookViewModel: MvxViewModel
    {
        private string _pickupLocation;
        private string _destinationLocation;
        public enum LocationType
        {
            Pickup,
            Destination
        }
        private LocationType _currentLocationType;
        private CreateOrder _order;
        private IAccountService _accountService;
        private Geolocator _geolocator;
        private  TaskScheduler _scheduler = TaskScheduler.FromCurrentSynchronizationContext();

        public BookViewModel(IAccountService accountService)
        {
            _accountService = accountService;
            _geolocator = new Geolocator{ DesiredAccuracy  = 100 };
        }

        public void Load()
        {
            _order = new CreateOrder();
            if (_accountService.CurrentAccount != null)
            {
                _order.Settings = _accountService.CurrentAccount.Settings;
            }
            else
            {
                _order.Settings = new BookingSettings{Passengers = 2};
            }
        }

        public Address Pickup
        {
            get
            { 
                return _order.PickupAddress;
            }

            set
            { 
                _order.PickupAddress = value;
                FirePropertyChanged(() => Pickup);
            }
        }

        public Address Dropoff
        {
            get
            { 
                return _order.DropOffAddress;
            }
            
            set
            {
                _order.DropOffAddress = value;
                FirePropertyChanged(() => Pickup);
            }
        }

        public string PickupLocation
        {
            get{ return _pickupLocation;}
            set
            { 
                _pickupLocation = value;
                PickupLocationChanged.RaiseCanExecuteChanged();
            }
        }

        public string DestinationLocation
        {
            get{ return _destinationLocation;}
            set
            { 
                _destinationLocation = value;
                PickupLocationChanged.RaiseCanExecuteChanged();
            }
        }

        public LocationType CurrentLocationType
        {
            get { return _currentLocationType; }
            set { _currentLocationType = value; }
        }

        private MvxRelayCommand _requestCurrentLocationCommand;

        public IMvxCommand  RequestCurrentLocationCommand
        {                   
            get
            {       
                if (_requestCurrentLocationCommand == null)
                {
                    _requestCurrentLocationCommand = new MvxRelayCommand(() => 
                    {       
                        _geolocator.GetPositionAsync(3000).ContinueWith(t =>
                        {
                            if (t.IsFaulted)
                            {
//                                PositionStatus.Text = ((GeolocationException)t.
//                                                       Exception.InnerException).Error.ToString();
                            }
                            else if (t.IsCanceled)
                            {
//                                PositionStatus.Text = "Canceled";
                            }
                            else
                            {
                                Console.WriteLine ( t.Result.Timestamp.ToString("G") );
                                Console.WriteLine ( t.Result.Latitude.ToString("N4"));
                                Console.WriteLine ( t.Result.Longitude.ToString("N4"));
                            }
                            
                        }, _scheduler);

                    });
                }
                return _requestCurrentLocationCommand;
            }
        }

        private MvxRelayCommand _pickupLocationChanged;

        public MvxRelayCommand PickupLocationChanged
        {                   
            get
            {       
                if (_pickupLocationChanged == null)
                {
                    _pickupLocationChanged = new MvxRelayCommand(() => 
                    {       


                    });
                }
                return _pickupLocationChanged;
            }
        }

    }
}

