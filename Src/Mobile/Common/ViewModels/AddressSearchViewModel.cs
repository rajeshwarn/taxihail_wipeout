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
using System.Threading;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Messages;
using TinyMessenger;

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
        private string _ownerId;
        public AddressSearchViewModel(string ownerId,IGoogleService googleService, IGeolocService geolocService, IBookingService bookingService, IAccountService accountService, Geolocator geolocator)
        {
            _ownerId = ownerId;
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

		private Thread _editThread;
		private MvxRelayCommand _searchAddressCommand;
		public IMvxCommand SearchAddressCommand
		{
            get
            {   
                if (_searchAddressCommand == null)
                {
                    _searchAddressCommand = new MvxRelayCommand(() => 
                    {      
						Console.WriteLine( "SearchAddressCommand: Start executing." );
						if(_editThread!= null && _editThread.IsAlive )
						{
							Console.WriteLine( "SearchAddressCommand: Killing previous thread." );
							_editThread.Abort();
						}

						_editThread = new Thread( () => {
							Console.WriteLine( "SearchAddressCommand: Starting new thread." );
							Thread.Sleep(1000);
							var addresses = _geolocService.SearchAddress( SearchText );
							AddressViewModels = addresses.Select( a => new AddressViewModel(){ Address = a, ShowPlusSign = false, ShowRightArrow = false, IsFirst = a.Equals(addresses.First()), IsLast = a.Equals(addresses.Last()) } ).ToList();
							Console.WriteLine( "SearchAddressCommand: Finishing executing command." );
						});
						_editThread.Start();
					}, () => !SearchText.IsNullOrEmpty() );
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

		private MvxRelayCommand _closeViewCommand;
		public IMvxCommand CloseViewCommand
		{
            get
            {       
                if (_closeViewCommand == null)
                {
                    _closeViewCommand = new MvxRelayCommand(() => 
                    {
                        TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Publish(new AddressSelected(this, new Address { FullAddress = Guid.NewGuid().ToString() }, _ownerId));
						RequestClose( this );
					});
				}
				return _closeViewCommand;
			}
		}

		public IEnumerable<AddressViewModel> AddressViewModels { 
			get { return _addressViewModels; }
			set { 
				_addressViewModels = value;
				FirePropertyChanged( () => AddressViewModels ); }
		}

		private MvxRelayCommand _resetCommand;
		public IMvxCommand ResetCommand
		{
            get
            {       
                if (_resetCommand == null)
                {
                    _resetCommand = new MvxRelayCommand(() => 
                    {
                        
						AddressViewModels = new List<AddressViewModel>();
					});
				}
				return _resetCommand;
			}
		}

		private MvxRelayCommand _rowSelectedCommand;
		public IMvxCommand RowSelectedCommand
		{
            get
            {       
                if (_rowSelectedCommand == null)
                {
                    _rowSelectedCommand = new MvxRelayCommand(() => 
                    {       
						RequestClose( this );
					});
				}
				return _rowSelectedCommand;
			}
		}


		private string _searchText;
		public string SearchText { get { return _searchText; }
			set { _searchText = value;
				SearchAddressCommand.Execute();
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

