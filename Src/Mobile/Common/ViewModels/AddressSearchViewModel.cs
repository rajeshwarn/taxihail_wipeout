using System;
using Cirrious.MvvmCross.Commands;
using Cirrious.MvvmCross.Interfaces.Commands;
using Cirrious.MvvmCross.ViewModels;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Mobile.AppServices;
using System.Collections.Generic;
using Xamarin.Geolocation;
using System.Threading.Tasks;
using System.Linq;

using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class AddressSearchViewModel : BaseViewModel 
	{
        private IGoogleService _googleService;
		private IGeolocService _geolocService;
		private IBookingService _bookingService;
		private IAccountService _accountService;
		private IEnumerable<AddressViewModel> _addressViewModels;
		private Geolocator _geolocator;
        private TaskScheduler _scheduler = TaskScheduler.FromCurrentSynchronizationContext();

        public AddressSearchViewModel(IGoogleService googleService, IGeolocService geolocService, IBookingService bookingService, IAccountService accountService, Geolocator geolocator)
        {
            _googleService = googleService;
			_geolocService = geolocService;
			_bookingService = bookingService;
			_accountService = accountService;
			_addressViewModels = new List<AddressViewModel>();
            _geolocator = geolocator;
        }

        public override void Load()
        {
         
        }

		private MvxRelayCommand _getPlacesCommand;
		public IMvxCommand GetPlacesCommand
		{
            get
            {       
                if (_getPlacesCommand == null)
                {
                    _getPlacesCommand = new MvxRelayCommand(() => 
                    {       
						_geolocator.GetPositionAsync(3000).ContinueWith(t =>
			            {
			                if (t.IsFaulted)
			                {
								Console.WriteLine("GetPosition : Faulted");
			                }
			                else if (t.IsCanceled)
			                {
								Console.WriteLine("GetPosition : Cancelled");
			                }
			                else
			                {
								var addresses = _googleService.GetNearbyPlaces( t.Result.Latitude, t.Result.Longitude );
								AddressViewModels = addresses.Select( a => new AddressViewModel(){ Address = a, ShowPlusSign = false, ShowRightArrow = false, IsFirst = a.Equals(addresses.First()), IsLast = a.Equals(addresses.Last()) } ).ToList();
			                }
			                
			            }, _scheduler);
					});
				}
				return _getPlacesCommand;
			}
		}

		private MvxRelayCommand _searchAddressCommand;
		public IMvxCommand SearchAddressCommand
		{
            get
            {       
                if (_searchAddressCommand == null)
                {
                    _searchAddressCommand = new MvxRelayCommand(() => 
                    {       
						var addresses = _geolocService.SearchAddress( SearchText );
						AddressViewModels = addresses.Select( a => new AddressViewModel(){ Address = a, ShowPlusSign = false, ShowRightArrow = false, IsFirst = a.Equals(addresses.First()), IsLast = a.Equals(addresses.Last()) } ).ToList();
					});
				}
				return _searchAddressCommand;
			}
		}

		private MvxRelayCommand _getContactsCommand;
		public IMvxCommand GetContactsCommand
		{
            get
            {       
                if (_getContactsCommand == null)
                {
                    _getContactsCommand = new MvxRelayCommand(() => 
                    {       
						var addresses = _bookingService.GetAddressFromAddressBook();
						AddressViewModels = addresses.Select( a => new AddressViewModel(){ Address = a, ShowPlusSign = false, ShowRightArrow = false, IsFirst = a.Equals(addresses.First()), IsLast = a.Equals(addresses.Last()) } ).ToList();

					});
				}
				return _getContactsCommand;
			}
		}

		private MvxRelayCommand _getFavoritesCommand;
		public IMvxCommand GetFavoritesCommand
		{
            get
            {       
                if (_getFavoritesCommand == null)
                {
                    _getFavoritesCommand = new MvxRelayCommand(() => 
                    {       
						var addresses = _accountService.GetFavoriteAddresses();
						AddressViewModels = addresses.Select( a => new AddressViewModel(){ Address = a, ShowPlusSign = false, ShowRightArrow = false, IsFirst = a.Equals(addresses.First()), IsLast = a.Equals(addresses.Last()) } ).ToList();
					});
				}
				return _getFavoritesCommand;
			}
		}

		public IEnumerable<AddressViewModel> AddressViewModels { 
			get { return _addressViewModels; }
			set { 
				_addressViewModels = value;
				FirePropertyChanged( () => AddressViewModels ); }
		}

		public string SearchText { get; set; }

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

