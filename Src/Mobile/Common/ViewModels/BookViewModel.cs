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

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class BookViewModel : BaseViewModel
    {      
        private IAccountService _accountService;
        private Geolocator _geolocator;
        private bool _pickupIsActive = true;
        private bool _dropoffIsActive = false;
        private IAppResource _appResource;
        

        public BookViewModel(IAccountService accountService, IAppResource appResource, Geolocator geolocator)
        {
            _appResource = appResource;
            _accountService = accountService;
            _geolocator = geolocator;
            _appResource = appResource;
            
            Load();
            Pickup = new BookAddressViewModel( ()=> Order.PickupAddress, address => Order.PickupAddress = address, _geolocator) { Title = appResource.GetString("BookPickupLocationButtonTitle"), EmptyAddressPlaceholder = appResource.GetString("BookPickupLocationEmptyPlaceholder") };
            Dropoff = new BookAddressViewModel(() => Order.DropOffAddress, address => Order.DropOffAddress = address, _geolocator) { Title = appResource.GetString("BookDropoffLocationButtonTitle"), EmptyAddressPlaceholder = appResource.GetString("BookDropoffLocationEmptyPlaceholder") };

            
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

        public void Rebook(Order order)
        {
            var serialized = JsonSerializer.SerializeToString<Order>(order);
            Order = JsonSerializer.DeserializeFromString<CreateOrder>(serialized);
            Order.Id = Guid.Empty;
            Order.PickupDate = null;
            Order.PickupDate = null;
            FirePropertyChanged(() => Order);
            FirePropertyChanged(() => Pickup);
            FirePropertyChanged(() => Dropoff);
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




        public BookAddressViewModel Pickup { get; set; }
        public BookAddressViewModel Dropoff { get; set; }

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
                FirePropertyChanged(() => SelectedAddress);
                FirePropertyChanged(() => NoAddressActiveSelection);
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
                FirePropertyChanged(() => SelectedAddress);
                FirePropertyChanged(() => NoAddressActiveSelection);
                
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
                                               //Pickup = new Address { FriendlyName = Guid.NewGuid().ToString(), FullAddress = Guid.NewGuid().ToString() };
                    //RequestNavigate(typeof(AddressSearchViewModel));
                    //TinyIoCContainer.Current.Resolve<INavigationService>().Navigate<AddressSearchViewModel>( "apcurium.MK.Booking.Mobile.Client.AddressSearchView" );
                });
            }
        }

        public IMvxCommand  PickDropOffLocation
        {                   
            get
            {       
                return new MvxRelayCommand(() => 
                                           {
                                           //Dropoff = new Address{ FriendlyName = Guid.NewGuid().ToString() };                          
                });
            }
        }





        public void Initialize()
        {
            PickupIsActive = true;
            Pickup.RequestCurrentLocationCommand.Execute();
        }
    }
}