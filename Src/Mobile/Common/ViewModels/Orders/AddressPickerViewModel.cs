using System;
using System.Collections.ObjectModel;
using apcurium.MK.Booking.Mobile.AppServices;
using System.Linq;
using apcurium.MK.Booking.Mobile.Extensions;
using System.Threading.Tasks;
using apcurium.MK.Booking.Maps;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Booking.Maps.Geo;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.PresentationHints;

namespace apcurium.MK.Booking.Mobile.ViewModels.Orders
{
	public class AddressPickerViewModel : PageViewModel,
		IRequestPresentationState<HomeViewModelStateRequestedEventArgs>,
		ISubViewModel<Address>
	{
		private readonly IOrderWorkflowService _orderWorkflowService;
		private readonly IPlaces _placesService;
		private readonly IGeolocService _geolocService;
		private readonly IAccountService _accountService;
		private bool _isInLocationDetail;
		private Address _currentAddress;	
		private bool _ignoreTextChange;
		private AddressViewModel[] _defaultHistoryAddresses = new AddressViewModel[0];
		private AddressViewModel[] _defaultFavoriteAddresses = new AddressViewModel[0];
		private AddressViewModel[] _defaultNearbyPlaces = new AddressViewModel[0];

		public event EventHandler<HomeViewModelStateRequestedEventArgs> PresentationStateRequested;

		public AddressPickerViewModel(IOrderWorkflowService orderWorkflowService,
			IPlaces placesService,
			IGeolocService geolocService,
			IAccountService accountService)
		{
			_orderWorkflowService = orderWorkflowService;
			_geolocService = geolocService;
			_placesService = placesService;
			_accountService = accountService;
		}

		public void Init(string searchCriteria)
		{
			AllAddresses = new ObservableCollection<AddressViewModel>();

			if (searchCriteria != null) 
			{
				_isInLocationDetail = true;
				SearchAddress (searchCriteria);
				StartingText = searchCriteria;
			} 
			else 
			{
				_ignoreTextChange = true;
			}
		}

		public ObservableCollection<AddressViewModel> AllAddresses { get; set; }

		public async void LoadAddresses()
		{            
            _ignoreTextChange = true;
			ShowDefaultResults = true;
			_currentAddress = await _orderWorkflowService.GetCurrentAddress();
			StartingText = _currentAddress.GetFirstPortionOfAddress();

			var favoritePlaces = _accountService.GetFavoriteAddresses();
			var historyPlaces = _accountService.GetHistoryAddresses();
			var neabyPlaces = Task.Run(() => _placesService.SearchPlaces(null, _currentAddress.Latitude, _currentAddress.Longitude, null));

			try
			{
				using(this.Services().Message.ShowProgressNonModal())
				{
					AllAddresses.Clear();

					_defaultFavoriteAddresses = ConvertToAddressViewModel(await favoritePlaces, AddressType.Favorites);
					AllAddresses.AddRange(_defaultFavoriteAddresses);

					_defaultHistoryAddresses = ConvertToAddressViewModel(await historyPlaces, AddressType.History);
					AllAddresses.AddRange(_defaultHistoryAddresses);

					_defaultNearbyPlaces = ConvertToAddressViewModel(await neabyPlaces, AddressType.Places);
					AllAddresses.AddRange(_defaultNearbyPlaces);
				}
			}
			catch (Exception e)
			{
				Logger.LogError(e);
			}
            finally
            {
                _ignoreTextChange = false;
            }
		}

		private AddressViewModel[] ConvertToAddressViewModel(Address[] addresses, AddressType type)
		{
			var addressViewModels = addresses
				.Where(f => f.BookAddress.HasValue())
				.Select(a => new AddressViewModel(a, type)).Distinct().ToArray();

			if (_currentAddress != null)
			{
				var currentPosition = new Position(_currentAddress.Latitude, _currentAddress.Longitude);
				addressViewModels = addressViewModels.OrderBy(a => a.ToPosition().DistanceTo(currentPosition)).ToArray();
			}

			if(addressViewModels.Any())
			{
				addressViewModels.Last().IsLast = true;
			}

			return addressViewModels;
		}		

		bool _showDefaultResults;
		public bool ShowDefaultResults
		{
			get
			{
				return _showDefaultResults;
			}
			set
			{
				_showDefaultResults = value;
				RaisePropertyChanged();
			}
		}

		void LoadDefaultList()
		{            
            using (this.Services().Message.ShowProgressNonModal())
			{
				AllAddresses.Clear();
				ShowDefaultResults = true;
				AllAddresses.AddRange(_defaultFavoriteAddresses.Concat(_defaultHistoryAddresses).Concat(_defaultNearbyPlaces));				
			}
		}

		public ICommand TextSearchCommand
		{
			get
			{
				return this.GetCommand<string>(criteria => SearchAddress(criteria)); 
			}
		}

		private Address UpdateAddressWithPlaceDetail(Address value)
		{
			if ((value != null) && (value.AddressType == "place"))
			{
				var place = _placesService.GetPlaceDetail(value.FriendlyName, value.PlaceReference);
				return place;
			}
			else
			{
				return value;
			}
		}

		public ICommand AddressSelected
		{
			get
			{
				return this.GetCommand<AddressViewModel>(vm => 
				{
                    this.Services().Message.ShowProgressNonModal(false);
					var detailedAddress = UpdateAddressWithPlaceDetail(vm.Address);

					if(_isInLocationDetail)
					{
						this.ReturnResult(detailedAddress);
					}
					else
					{
						_orderWorkflowService.SetAddress(detailedAddress);
                    	PresentationStateRequested.Raise(this, new HomeViewModelStateRequestedEventArgs(HomeViewModelState.Initial));
						ChangePresentation(new ZoomToStreetLevelPresentationHint(detailedAddress.Latitude, detailedAddress.Longitude));
					}
				}); 
			}
		}

		public ICommand Cancel
		{
			get
			{
				return this.GetCommand(() => 
				{
                    this.Services().Message.ShowProgressNonModal(false );

					if(_isInLocationDetail)
					{
						Close(this);
					}
					else
					{
						PresentationStateRequested.Raise(this, new HomeViewModelStateRequestedEventArgs(HomeViewModelState.Initial));
					}
				}); 
			}
		}

		private string _startingText;
		public string StartingText
		{
			get { return _startingText; }
			set 
			{
				_startingText = value;
				RaisePropertyChanged();
			}
		}

		public async Task SearchAddress(string criteria)
		{
            if (_ignoreTextChange)
            {
                return;
            }

            if (criteria.HasValue() && criteria != StartingText)
			{
				using (this.Services().Message.ShowProgressNonModal())
				{
					ShowDefaultResults = false;
					AllAddresses.Clear();

					var fhAdrs = SearchFavoriteAndHistoryAddresses(criteria);
					var pAdrs = Task.Run(() => SearchPlaces(criteria));
					var gAdrs = Task.Run(() => SearchGeocodeAddresses(criteria));

					AllAddresses.AddRangeDistinct(await fhAdrs, (x, y) => x.Equals(y));
					AllAddresses.AddRangeDistinct(await pAdrs, (x, y) => x.Equals(y));
					AllAddresses.AddRangeDistinct(await gAdrs, (x, y) => x.Equals(y));

				}
			}
			else
			{
				LoadDefaultList();
			}
		}

		protected AddressViewModel[] SearchPlaces(string criteria)
		{           
			var position = _currentAddress;

			if (position == null)
			{
				return new AddressViewModel[0];
			}

			var fullAddresses = _placesService.SearchPlaces(criteria, position.Latitude, position.Longitude, null);

			var addresses = fullAddresses.ToList();
			return addresses.Select(a => new AddressViewModel(a, AddressType.Places) { IsSearchResult = true }).ToArray();
		}

		protected async Task<AddressViewModel[]> SearchFavoriteAndHistoryAddresses(string criteria)
		{
			var addresses = _accountService.GetFavoriteAddresses().ConfigureAwait(false);
			var historicAddresses = _accountService.GetHistoryAddresses().ConfigureAwait(false);

			Func<Address, bool> predicate = x => (x.FriendlyName != null
			                                && x.FriendlyName.ToLowerInvariant().Contains(criteria))
			                                || (x.FullAddress != null
			                                && x.FullAddress.ToLowerInvariant().Contains(criteria));         

			var a1 = (await addresses)
				.Where(predicate)
				.Select(f => new AddressViewModel(f, f.FriendlyName.HasValue() ? AddressType.Favorites : AddressType.History) { IsSearchResult = true });
			var a2 = (await historicAddresses)
				.Where(predicate)
				.Select(f => new AddressViewModel(f, f.FriendlyName.HasValue() ? AddressType.Favorites : AddressType.History) { IsSearchResult = true });

			return a1.Concat(a2).ToArray(); 
		}

		protected AddressViewModel[] SearchGeocodeAddresses(string criteria)
		{
			Logger.LogMessage("Starting SearchAddresses : " + criteria);
			var position = _currentAddress;

			Address[] addresses;

			if (position == null)
			{
				Logger.LogMessage("No Position SearchAddresses : " + criteria);
				addresses = _geolocService.SearchAddress(criteria);                
			}
			else
			{
				Logger.LogMessage("Position SearchAddresses : " + criteria);
				addresses = _geolocService.SearchAddress(criteria, position.Latitude, position.Longitude);
			}

			return addresses.Select(a => new AddressViewModel(a, AddressType.Places) { IsSearchResult = true }).ToArray();
		}
	}
}

