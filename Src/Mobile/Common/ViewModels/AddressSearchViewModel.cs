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
using apcurium.MK.Booking.Mobile.ListViewStructure;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class AddressSearchViewModel : BaseViewModel 
	{
        private IGoogleService _googleService;
		private IGeolocService _geolocService;
		private IBookingService _bookingService;
		private IAccountService _accountService;
		private IEnumerable<Address> _addressList;
		private Geolocator _geolocator;
        private TaskScheduler _scheduler = TaskScheduler.FromCurrentSynchronizationContext();

        public AddressSearchViewModel(IGoogleService googleService, IGeolocService geolocService, IBookingService bookingService, IAccountService accountService)
        {
            _googleService = googleService;
			_geolocService = geolocService;
			_bookingService = bookingService;
			_accountService = accountService;
			_addressList = new List<Address>();
			_geolocator = new Geolocator{ DesiredAccuracy  = 100 };
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
								AddressList = _googleService.GetNearbyPlaces( t.Result.Latitude, t.Result.Longitude );
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
						AddressList = _geolocService.SearchAddress( SearchText );
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
						AddressList = _bookingService.GetAddressFromAddressBook();
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
						AddressList = _accountService.GetFavoriteAddresses();
					});
				}
				return _getFavoritesCommand;
			}
		}

		public IEnumerable<Address> AddressList { 
			get { return _addressList; }
			set { 
				_addressList = value;
				DataSourceStructure = LoadStructure( _addressList );
				FirePropertyChanged( () => AddressList ); }
		}

		public InfoStructure DataSourceStructure { get; set; }

		private InfoStructure LoadStructure( IEnumerable<Address> addressList )
		{
			var structure = new InfoStructure( 44, false );

			var sect = structure.AddSection();
			addressList.ForEach( item => sect.AddItem( new TwoLinesAddressItem( item.Id, item.FriendlyName, item.FullAddress ) { Data = item } ) );

			return structure;
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

