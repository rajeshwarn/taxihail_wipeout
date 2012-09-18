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
		private IEnumerable<AddressViewModel> _historicAddressViewModels;
		private TaskScheduler _scheduler = TaskScheduler.FromCurrentSynchronizationContext();        
        private string _ownerId;

        public AddressSearchViewModel(string ownerId, string search, IGoogleService googleService, IGeolocService geolocService, IBookingService bookingService, IAccountService accountService)
        {
            _ownerId = ownerId;
            _googleService = googleService;
            _geolocService = geolocService;
            _bookingService = bookingService;
            _accountService = accountService;
            _addressViewModels = new List<AddressViewModel>();
			_historicAddressViewModels = new List<AddressViewModel>();
			TinyIoCContainer.Current.Resolve<IUserPositionService>().Refresh();

			_searchFilter = new string[0];
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
				FirePropertyChanged(() => HistoricIsHidden);
			}
		}
		private bool _favoritesSelected;
		public bool FavoritesSelected { 
			get { return _favoritesSelected; }
			set { _favoritesSelected = value; 
				FirePropertyChanged( () => FavoritesSelected );
				FirePropertyChanged(() => HistoricIsHidden);
			}
		}
		private bool _contactsSelected;
		public bool ContactsSelected { 
			get { return _contactsSelected; }
			set { _contactsSelected = value;
				FirePropertyChanged( () => ContactsSelected );
				FirePropertyChanged(() => HistoricIsHidden);
			}
		}
		private bool _placesSelected;
		public bool PlacesSelected { 
			get { return _placesSelected; }
			set { _placesSelected = value; 
				FirePropertyChanged( () => PlacesSelected );
				FirePropertyChanged(() => HistoricIsHidden);
			}
		}

        public IMvxCommand GetPlacesCommand
        {
            get
            {
                return new MvxRelayCommand(() =>
                {
					SearchText = "";
					SetSelected( TopBarButton.PlacesBtn );
					IsSearching = true;

                    ThreadPool.QueueUserWorkItem( o=>
                        {
                            try
                            {
                                var position = TinyIoCContainer.Current.Resolve<IUserPositionService>().LastKnownPosition;
                                var addresses = _googleService.GetNearbyPlaces(position.Latitude, position.Longitude);
                                if( PlacesSelected && addresses!=null )
					            {
								AddressViewModels = addresses.Select(a => new AddressViewModel() { Address = a, ShowPlusSign = false, ShowRightArrow = false, IsFirst = a.Equals(addresses.First()), IsLast = a.Equals(addresses.Last()) }).ToList();
                                }   
                            }
                            finally
                            {
                                IsSearching = false;
                            }
                        });                   
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
                        
                        if (_editThread != null && _editThread.IsAlive)
                        {                        
                            _editThread.Abort();
							IsSearching = false;
                        }

						if( SearchText.Count( c => char.IsLetter( c ) ) > 2 )
						{
	                        _editThread = new Thread(() =>
	                        {
	                            Console.WriteLine("SearchAddressCommand: Starting new thread.");
	                            Thread.Sleep(200);

								new Thread( () => {
									IsSearching = true;
                                    var position = TinyIoCContainer.Current.Resolve<IUserPositionService>().LastKnownPosition;
									var addresses = _geolocService.SearchAddress(SearchText, position.Latitude, position.Longitude );
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
					SearchText = "";
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
					SearchText = "";
					SetSelected( TopBarButton.FavoritesBtn );
					new Thread( () => {
						IsSearching = true;
						var addresses = _accountService.GetFavoriteAddresses();
						var historicAddresses = _accountService.GetHistoryAddresses();

						if( FavoritesSelected )
						{
							AddressViewModels = addresses.Select(a => new AddressViewModel() { Address = a, ShowPlusSign = false, ShowRightArrow = false, IsFirst = a.Equals(addresses.First()), IsLast = a.Equals(addresses.Last()) }).ToList();
							HistoricAddressViewModels = historicAddresses.Select(a => new AddressViewModel() { Address = a, ShowPlusSign = false, ShowRightArrow = false, IsFirst = a.Equals(historicAddresses.First()), IsLast = a.Equals(historicAddresses.Last()) }).ToList();
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
			get {
				if( _searchFilter.Count() == 0)
				{
					return _addressViewModels;
				}
				else
				{
					return _addressViewModels.Where( a => _searchFilter.All ( s => a.Address.FriendlyName.ToLower().Contains( s ) || a.Address.FullAddress.ToLower().Contains( s ) ) ).ToList();
				}
			}
			set
            {
                _addressViewModels = value;
                FirePropertyChanged(() => AddressViewModels);
            }
        }

		public IEnumerable<AddressViewModel> HistoricAddressViewModels
		{
			get { 
				if( _searchFilter.Count() == 0)
					{
					return _historicAddressViewModels;
					}
					else
					{
						return _historicAddressViewModels.Where( a => _searchFilter.All ( s => (!a.Address.FriendlyName.IsNullOrEmpty() && a.Address.FriendlyName.ToLower().Contains( s )) || a.Address.FullAddress.ToLower().Contains( s ) ) ).ToList();
					}
			}
			set
			{
				_historicAddressViewModels = value;
				FirePropertyChanged(() => HistoricAddressViewModels);
				FirePropertyChanged(() => HistoricIsHidden);
			}
		}
		
		public bool HistoricIsHidden{ get { return HistoricAddressViewModels.Count() == 0 || !FavoritesSelected; } }

        public IMvxCommand SearchCommand
        {
            get
            {
                return new MvxRelayCommand(() =>
                    {
						SearchText = "";
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
                        ThreadPool.QueueUserWorkItem(o =>
                            {
                                if (address.Address != null)
                                {
                                    if (address.Address.FullAddress.IsNullOrEmpty() && (address.Address.AddressType == "place") && (address.Address.PlaceReference.HasValue()))
                                    {
                                        var placeAddress = _googleService.GetPlaceDetail(address.Address.PlaceReference);
                                        placeAddress.FriendlyName = address.Address.FriendlyName;
                                        InvokeOnMainThread(() => TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Publish(new AddressSelected(this, placeAddress, _ownerId)));
                                    }
                                    else
                                    {
                                        InvokeOnMainThread(() => TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Publish(new AddressSelected(this, address.Address, _ownerId)));
                                    }
                                }
                            });
                        RequestClose(this);
                    });
            }
        }


        private string _searchText;
		private string[] _searchFilter;
		public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
				FirePropertyChanged( () => SearchText );
				if( SearchSelected )
				{
					_searchFilter = new string[0];
                	SearchAddressCommand.Execute();
				}
				else
				{
					if( value.IsNullOrEmpty() )
					{
						_searchFilter = new string[0];
					}
					else
					{
						_searchFilter = value.ToLower().Split( new string[]{" "}, StringSplitOptions.RemoveEmptyEntries );
					}
					FirePropertyChanged( () => AddressViewModels );
					FirePropertyChanged( () => HistoricAddressViewModels );
				}
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

