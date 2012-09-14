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
        private double _lat;
        private double _lg;
        private string _ownerId;
        
		public AddressSearchViewModel(string ownerId, string search, IGoogleService googleService, IGeolocService geolocService, IBookingService bookingService, IAccountService accountService, Geolocator geolocator)
        {
            _ownerId = ownerId;
            _googleService = googleService;
            _geolocService = geolocService;
            _bookingService = bookingService;
            _accountService = accountService;
            _addressViewModels = new List<AddressViewModel>();
            _geolocator = geolocator;
            _geolocator.GetPositionAsync(5000).ContinueWith(p =>
                {
                    if (p.IsCompleted)
                    {
                        _lat = p.Result.Latitude;
                        _lg = p.Result.Longitude;
                    }
                });
            SearchText = search;

            if (SearchText.HasValue())
            {
                SearchAddressCommand.Execute();
            }
			SearchSelected = true;
        }

        public override void Load()
        {

        }

		private bool _isSearching;
		public bool IsSearching {
			get { return _isSearching; }
			set { _isSearching = value; 
				FirePropertyChanged( () => IsSearching );
			}
		}

		private bool _searchSelected;
		public bool SearchSelected { 
			get { return _searchSelected; }
			set { _searchSelected = value;
				FirePropertyChanged( () => SearchSelected );
			}
		}
		private bool _favoritesSelected;
		public bool FavoritesSelected { 
			get { return _favoritesSelected; }
			set { _favoritesSelected = value; 
				FirePropertyChanged( () => FavoritesSelected );
			}
		}
		private bool _contactsSelected;
		public bool ContactsSelected { 
			get { return _contactsSelected; }
			set { _contactsSelected = value;
				FirePropertyChanged( () => ContactsSelected );
			}
		}
		private bool _placesSelected;
		public bool PlacesSelected { 
			get { return _placesSelected; }
			set { _placesSelected = value; 
				FirePropertyChanged( () => PlacesSelected );
			}
		}

        public IMvxCommand GetPlacesCommand
        {
            get
            {
                return new MvxRelayCommand(() =>
                {
					SetSelected( TopBarButton.PlacesBtn );
					IsSearching = true;
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
                            var addresses = _googleService.GetNearbyPlaces(t.Result.Latitude, t.Result.Longitude);
							if( PlacesSelected )
							{
                            	AddressViewModels = addresses.Select(a => new AddressViewModel() { Address = a, ShowPlusSign = false, ShowRightArrow = false, IsFirst = a.Equals(addresses.First()), IsLast = a.Equals(addresses.Last()) }).ToList();
							}
                        }
						IsSearching = false;

                    }, _scheduler);
                });
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

                        Console.WriteLine("SearchAddressCommand: Start executing.");
                        if (_editThread != null && _editThread.IsAlive)
                        {
                            Console.WriteLine("SearchAddressCommand: Killing previous thread.");
                            _editThread.Abort();
							IsSearching = false;
                        }

						if( SearchText.Where( c => char.IsLetter( c ) ).Count() > 3 )
						{
	                        _editThread = new Thread(() =>
	                        {
	                            Console.WriteLine("SearchAddressCommand: Starting new thread.");
	                            Thread.Sleep(200);

								new Thread( () => {
									IsSearching = true;
									var addresses = _geolocService.SearchAddress(SearchText, _lat, _lg);
									if( SearchSelected )
									{
		                            	AddressViewModels = addresses.Select(a => new AddressViewModel() { Address = a, ShowPlusSign = false, ShowRightArrow = false, IsFirst = a.Equals(addresses.First()), IsLast = a.Equals(addresses.Last()) }).ToList();
									}
		                            Console.WriteLine("SearchAddressCommand: Finishing executing command.");
									IsSearching = false;
								} ).Start();
	                        });
	                        _editThread.Start();
						}
						else
						{
							AddressViewModels = new List<AddressViewModel>();
						}
					}, () => !SearchText.IsNullOrEmpty() );
                }
                return _searchAddressCommand;
            }
        }

        
        public IMvxCommand GetContactsCommand
        {
            get
            {
				return new MvxRelayCommand(() => {
					SetSelected( TopBarButton.ContactsBtn );
					new Thread( ()=> {
						IsSearching = true;
						var addresses = _bookingService.GetAddressFromAddressBook();
						if(ContactsSelected )
						{
	                       AddressViewModels = addresses.Select(a => new AddressViewModel() { Address = a, ShowPlusSign = false, ShowRightArrow = false, IsFirst = a.Equals(addresses.First()), IsLast = a.Equals(addresses.Last()) }).ToList();
						}  
						IsSearching = false;
					}).Start();
					Console.WriteLine( "Thread started");
				});
            }
        }

        
        public IMvxCommand GetFavoritesCommand
        {
            get
            {
                return new MvxRelayCommand(() => {
					SetSelected( TopBarButton.FavoritesBtn );
					new Thread( () => {
						IsSearching = true;
						var addresses = _accountService.GetFavoriteAddresses();

						if( FavoritesSelected )
						{
	                        AddressViewModels = addresses.Select(a => new AddressViewModel() { Address = a, ShowPlusSign = false, ShowRightArrow = false, IsFirst = a.Equals(addresses.First()), IsLast = a.Equals(addresses.Last()) }).ToList();
						}
						IsSearching = false;
					} ).Start();
                    });               
            }
        }
        
        public IMvxCommand CloseViewCommand
        {
            get
            {
                return new MvxRelayCommand(() =>
                    {                        
                        RequestClose(this);
                    });                
            }
        }

        public IEnumerable<AddressViewModel> AddressViewModels
        {
            get { return _addressViewModels; }
            set
            {
                _addressViewModels = value;
                FirePropertyChanged(() => AddressViewModels);
            }
        }

        public IMvxCommand SearchCommand
        {
            get
            {
                return new MvxRelayCommand(() =>
                    {
						SetSelected( TopBarButton.SearchBtn );
						AddressViewModels = new List<AddressViewModel>();

						SearchAddressCommand.Execute();
                        
                    });
            }
        }


        public IMvxCommand RowSelectedCommand
        {
            get
            {
                return new MvxRelayCommand<AddressViewModel>(address =>
                    {
                        TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Publish(new AddressSelected(this, address.Address, _ownerId));
                        RequestClose(this);
                    });
            }
        }


        private string _searchText;
        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                SearchAddressCommand.Execute();
            }
        }


		private enum TopBarButton { SearchBtn, FavoritesBtn, ContactsBtn, PlacesBtn }

		private void SetSelected( TopBarButton btn )
		{
			SearchSelected = btn == TopBarButton.SearchBtn;
			FavoritesSelected = btn == TopBarButton.FavoritesBtn;
			ContactsSelected = btn == TopBarButton.ContactsBtn;
			PlacesSelected = btn == TopBarButton.PlacesBtn;
		}

    }
}

