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
    public class BookViewModel : BaseViewModel
    {      
        private IAccountService _accountService;
        private Geolocator _geolocator;
        private bool _pickupIsActive = true;
        private bool _dropoffIsActive = false;
        private  TaskScheduler _scheduler = TaskScheduler.FromCurrentSynchronizationContext();

        public BookViewModel(IAccountService accountService)
        {
            _accountService = accountService;
            _geolocator = new Geolocator{ DesiredAccuracy  = 100 };
            Load();
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
                Order.Settings = new BookingSettings{Passengers = 2};
            }           
        }

        public void Reset()
        {
            Load();
        }

        public void Rebook(CreateOrder rebookData)
        {
            Order = rebookData;
        }

        public CreateOrder Order
        {
            get;
            private set;
        }

        public Address Pickup
        {
            get
            { 
                return Order.PickupAddress;
            }
            set
            { 
                Order.PickupAddress = value;
                FirePropertyChanged(() => Pickup);
            }
        }

        public Address Dropoff
        {
            get
            { 
                return Order.DropOffAddress;
            }            
            set
            {
                Order.DropOffAddress = value;
                FirePropertyChanged(() => Dropoff);
            }
        }

        public bool PickupIsActive
        {
            get{ return _pickupIsActive;}
            set
            {
                _pickupIsActive = value;
                FirePropertyChanged(() => PickupIsActive);
                if (DropoffIsActive && PickupIsActive)
                {
                    _dropoffIsActive = false;
                    FirePropertyChanged(() => DropoffIsActive);
                }
            }
        }

        public bool DropoffIsActive
        {
            get{ return _dropoffIsActive;}
            set
            { 
                _dropoffIsActive = value;
                FirePropertyChanged(() => DropoffIsActive);
                if (DropoffIsActive && PickupIsActive)
                {
                    _pickupIsActive = false;
                    FirePropertyChanged(() => PickupIsActive);
                }
            }
        }

        public MvxRelayCommand ActivatePickup
        {                   
            get
            {       
                return new MvxRelayCommand(() => PickupIsActive = !PickupIsActive);            
            }
        }

        public MvxRelayCommand ActivateDropoff
        {                   
            get
            {   
                return new MvxRelayCommand(() => DropoffIsActive = !DropoffIsActive);      
            }
        }

        public IMvxCommand  PickPickupLocation
        {                   
            get
            {       
                return new MvxRelayCommand(() => 
                                           {   
                    Pickup = new Address{ FriendlyName = Guid.NewGuid().ToString() };
                });
            }
        }

        public IMvxCommand  PickDropOffLocation
        {                   
            get
            {       
                return new MvxRelayCommand(() => 
                                           {
                                           Dropoff = new Address{ FriendlyName = Guid.NewGuid().ToString() };                          
                });
            }
        }

        public IMvxCommand  RequestCurrentLocationCommand
        {                   
            get
            {       
                return new MvxRelayCommand(() => 
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
                            Console.WriteLine(t.Result.Timestamp.ToString("G"));
                            Console.WriteLine(t.Result.Latitude.ToString("N4"));
                            Console.WriteLine(t.Result.Longitude.ToString("N4"));
                        }
                        
                    }, _scheduler);
                    
                });
            }
        }
        


    }
}

