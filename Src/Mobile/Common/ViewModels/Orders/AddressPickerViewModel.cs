using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using apcurium.MK.Booking.MapDataProvider;
using apcurium.MK.Booking.Maps;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.PresentationHints;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Infrastructure;

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
		private readonly ILocationService _locationService;
	    private readonly IPostalCodeService _postalCodeService;

	    private bool _isInLocationDetail;
		private Address _currentAddress;	
		private bool _ignoreTextChange;
		private string _currentLanguage;
		private AddressViewModel[] _defaultHistoryAddresses = new AddressViewModel[0];
		private AddressViewModel[] _defaultFavoriteAddresses = new AddressViewModel[0];
		private AddressViewModel[] _defaultNearbyPlaces = new AddressViewModel[0];

        public AddressViewModel[] FilteredPlaces { get; private set; }

		private AddressLocationType _currentActiveFilter;

	    private string _previousPostCode = string.Empty;

		public event EventHandler<HomeViewModelStateRequestedEventArgs> PresentationStateRequested;

		public AddressPickerViewModel(IOrderWorkflowService orderWorkflowService,
			IPlaces placesService,
			IGeolocService geolocService,
			IAccountService accountService,
			ILocationService locationService, 
            IPostalCodeService postalCodeService)
		{
			_orderWorkflowService = orderWorkflowService;
			_geolocService = geolocService;
			_placesService = placesService;
			_accountService = accountService;
			_locationService = locationService;
		    _postalCodeService = postalCodeService;


		    FilteredPlaces = new AddressViewModel[0];
		}

		public void Init(string searchCriteria)
		{
			AllAddresses = new ObservableCollection<AddressViewModel>();
			_currentLanguage = TinyIoCContainer.Current.Resolve<ILocalization> ().CurrentLanguage;

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

	    private async Task LoadAddressesUnspecified()
	    {
            ShowDefaultResults = true;
            _currentAddress = await GetCurrentAddressOrUserPosition();
            StartingText = _currentAddress != null
                ? _currentAddress.GetFirstPortionOfAddress()
                : string.Empty;

            var favoritePlaces = _accountService.GetFavoriteAddresses();
            var historyPlaces = _accountService.GetHistoryAddresses();
            var neabyPlaces = Task.Run(() => _placesService
                .SearchPlaces(
                    null,
                    _currentAddress != null ? _currentAddress.Latitude : (double?)null,
                    _currentAddress != null ? _currentAddress.Longitude : (double?)null,
                    null,
                    _currentLanguage
                )
            );
            
            using (this.Services().Message.ShowProgressNonModal())
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

	    

	    public async void RefreshFilteredAddress()
	    {
	        try
	        {
                var filteredPlaces = await _placesService.GetFilteredPlacesList();

                FilteredPlaces = ConvertToAddressViewModel(filteredPlaces, AddressType.Places);
	        }
	        catch (Exception ex)
	        {
	            this.Logger.LogError(ex);
	        }
	    }

	    private void LoadFilteredAddress(AddressLocationType filter)
	    {
			using (this.Services().Message.ShowProgressNonModal())
			{
				AllAddresses.Clear();

			    _isShowingFilteredList = true;

                AllAddresses.AddRange(FilteredPlaces.Where(place => place.Address.AddressLocationType == filter));
			}
	    }

	    public async Task LoadAddresses(AddressLocationType filter)
		{
            _ignoreTextChange = true;
	        try
	        {
				_currentActiveFilter = filter;

	            if (filter == AddressLocationType.Unspeficied)
	            {
                    await LoadAddressesUnspecified();
	            }
	            else
	            {
                    LoadFilteredAddress(filter);
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
				.Where(f => f.FullAddress.HasValue())
				.Select(a => new AddressViewModel(a, type)).Distinct().ToArray();

			if (_currentAddress != null)
			{
				var currentPosition = new Maps.Geo.Position(_currentAddress.Latitude, _currentAddress.Longitude);
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
	    private bool _isShowingFilteredList;

		void LoadDefaultList()
		{
		    if (_isShowingFilteredList)
		    {
				//Needed to prevent a race condition where LoadDefaultList would be called right after we set the filtered places list.
		        _isShowingFilteredList = false;
		        return;
		    }

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

		private async Task<Address> UpdateAddressWithPlaceDetail(Address value)
		{
			if ((value != null) && (value.AddressType == "place"))
			{
				var place = _placesService.GetPlaceDetail(value.FriendlyName, value.PlaceId);
				return place;
			}

            if ((value != null) && (value.AddressType == "craftyclicks"))
            {
                var geoLoc = await _geolocService.SearchAddress(value.FullAddress, value.Latitude, value.Longitude);

                if (geoLoc.Any())
                {
                    return geoLoc.First();
                }
            }

            return value;
		}

		public ICommand AddressSelected
		{
			get
			{
				return this.GetCommand<AddressViewModel>(vm =>
				{
				    this.Services().Message.ShowProgressNonModal(false);
				    SelectAddress(vm.Address, true);
				}); 
			}
		}

	    public async void SelectAddress(Address address, bool returnToHome = false)
	    {
			if (address == null)
			{
				return;
			}

			try
			{
				var detailedAddress = await UpdateAddressWithPlaceDetail(address);

				if (_isInLocationDetail)
				{
					this.ReturnResult(detailedAddress);

					return;
				}

				((HomeViewModel)Parent).LocateMe.Cancel();

				await _orderWorkflowService.SetAddress(detailedAddress);

                if (_currentActiveFilter == AddressLocationType.Airport)
                {
                    PresentationStateRequested.Raise(this, new HomeViewModelStateRequestedEventArgs(HomeViewModelState.AirportDetails));
                }
                else
                {

					if (returnToHome)
					{
						// This needs to be called if we are displaying the AddressPickerViewModel from the home view.
						PresentationStateRequested.Raise(this, new HomeViewModelStateRequestedEventArgs(HomeViewModelState.Initial));
					}
	
					ChangePresentation(new ZoomToStreetLevelPresentationHint(detailedAddress.Latitude, detailedAddress.Longitude));
				}
			}
			catch(Exception ex)
			{
				Logger.LogMessage("An error occurred while selecting address.");
				Logger.LogError(ex);
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

            
            if (criteria.HasValue() && criteria != StartingText && criteria != _previousPostCode)
			{
				using (this.Services().Message.ShowProgressNonModal())
				{
					ShowDefaultResults = false;
					AllAddresses.Clear();

					var fhAdrs = SearchFavoriteAndHistoryAddresses(criteria);
					var pAdrs = Task.Run(() => SearchPlaces(criteria));
                    var gAdrs = Task.Run(() => SearchGeocodeAddresses(criteria));
                    if (this.Services().Settings.CraftyClicksApiKey.HasValue() && _postalCodeService.IsValidPostCode(criteria))
				    {
                        var ccAdrs = SearchPostalCode(criteria);
				        _previousPostCode = criteria;
                        
                        AllAddresses.AddRangeDistinct(await ccAdrs, (x, y) => x.Equals(y));
				    }

                    AllAddresses.AddRangeDistinct(await fhAdrs, (x, y) => x.Equals(y));

					if (char.IsDigit(criteria[0]))
					{
                        AllAddresses.AddRangeDistinct(await gAdrs, (x, y) => x.Equals(y));	
						AllAddresses.AddRangeDistinct(await pAdrs, (x, y) => x.Equals(y));
					}
					else
					{
						AllAddresses.AddRangeDistinct(await pAdrs, (x, y) => x.Equals(y));
						AllAddresses.AddRangeDistinct(await gAdrs, (x, y) => x.Equals(y));	
					}


				}
			}
			else
			{
				LoadDefaultList();
			}
		}

		protected AddressViewModel[] SearchPlaces(string criteria)
		{           
			var fullAddresses = _placesService.SearchPlaces(criteria, 
				_currentAddress != null ? _currentAddress.Latitude : (double?)null, 
				_currentAddress != null ? _currentAddress.Longitude : (double?)null, 
				null, _currentLanguage);

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
				.Select(f => new AddressViewModel(f, AddressType.Favorites) { IsSearchResult = true });
			var a2 = (await historicAddresses)
				.Where(predicate)
				.Select(f => new AddressViewModel(f, AddressType.History) { IsSearchResult = true });

			return a1.Concat(a2).ToArray(); 
		}

	    protected async Task<AddressViewModel[]> SearchPostalCode(string criteria)
	    {
	        try
	        {
                var postalCodeAddresses = await Task.Run(() => _postalCodeService.GetAddressFromPostalCodeAsync(criteria));

                return postalCodeAddresses
                    .Select(adrs => new AddressViewModel(adrs, AddressType.Places))
                    .ToArray();
	        }
	        catch (Exception ex)
	        {
	            Logger.LogMessage("Unable to obtain postalcode information from CraftyClicks.");
                Logger.LogError(ex);

	            return new AddressViewModel[0];
	        }
            
	    }


		protected async Task<AddressViewModel[]> SearchGeocodeAddresses(string criteria)
		{
			Logger.LogMessage("Starting SearchAddresses : " + criteria);
			var position = _currentAddress;

			Address[] addresses;

			if (position == null)
			{
				Logger.LogMessage("No Position SearchAddresses : " + criteria);
				addresses = await _geolocService.SearchAddress(criteria);                
			}
			else
			{
				Logger.LogMessage("Position SearchAddresses : " + criteria);
				addresses = await _geolocService.SearchAddress(criteria, position.Latitude, position.Longitude);
			}

			return addresses
                .Select(a => new AddressViewModel(a, AddressType.Places) { IsSearchResult = true })
                .ToArray();
		}

		private async Task<Address> GetCurrentAddressOrUserPosition()
		{
			var currentAddress = await _orderWorkflowService.GetCurrentAddress();
			if (currentAddress.HasValidCoordinate())
			{
				return currentAddress;
			}

			var userPosition = _locationService.BestPosition;
			if (userPosition == null)
			{
				return null;
			}

			return new Address
			{ 
				Latitude = userPosition.Latitude,
				Longitude = userPosition.Longitude
			};
		}
	}
}

