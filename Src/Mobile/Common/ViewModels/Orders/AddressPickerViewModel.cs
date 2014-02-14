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
	public class AddressPickerViewModel : ChildViewModel
	{
		readonly IAccountService _accountService;
		readonly IOrderWorkflowService _orderWorkflowService;
		readonly IPlaces _placeService;
		readonly IGeolocService _geoCoding;

		Address _currentAddress;

		public AddressPickerViewModel(IAccountService accountService, 
			IOrderWorkflowService orderWorkflowService,
			IPlaces placeService,
			IGeolocService geoCoding)
		{
			_geoCoding = geoCoding;
			_placeService = placeService;
			_orderWorkflowService = orderWorkflowService;
			_accountService = accountService;
			AllAddresses = new ObservableCollection<AddressViewModel>();
		}

		public ObservableCollection<AddressViewModel> AllAddresses { get; set; }

		private AddressViewModel[] _defaultHistoryAddresses = new AddressViewModel[0];
		private AddressViewModel[] _defaultFavoriteAddresses = new AddressViewModel[0];
		private AddressViewModel[] _defaultNearbyPlaces = new AddressViewModel[0];

		public async void LoadAddresses()
		{            
			ShowDefaultResults = true;
			_currentAddress = await _orderWorkflowService.GetCurrentAddress();
			StartingText = _currentAddress.GetFirstPortionOfAddress();
			var neabyPlaces = Task.Factory.StartNew(() => _placeService.SearchPlaces(null, 
																_currentAddress.Latitude, 
																_currentAddress.Longitude, null));
			var favoritePlaces = Task.Factory.StartNew(() => _accountService.GetFavoriteAddresses().ToArray());
			var historyPlaces = Task.Factory.StartNew(() => _accountService.GetHistoryAddresses().ToArray());

			try
			{
				using(this.Services().Message.ShowProgressNonModal())
				{
					AllAddresses.Clear();

					favoritePlaces.ContinueWith(t =>
						{
							_defaultFavoriteAddresses = OnAddressesArrived(t.Result, AddressType.Favorites);
						});
					historyPlaces.ContinueWith(t =>
						{
							_defaultHistoryAddresses = OnAddressesArrived(t.Result, AddressType.History);
						});
					neabyPlaces.ContinueWith(t =>
						{
							_defaultNearbyPlaces = OnAddressesArrived(t.Result, AddressType.Places);
						});

					await favoritePlaces;
					await historyPlaces;
					await neabyPlaces;
				}
			}
			catch (Exception e)
			{
				Logger.LogError(e);
			}
		}

		private AddressViewModel[] OnAddressesArrived(Address[] addresses, AddressType type)
		{
			var addressViewModels = addresses
				.Where(f => f.BookAddress.HasValue())
				.Select(a => new AddressViewModel(a, type)).Distinct().ToArray();

			if (_currentAddress != null)
			{
				var currentPosition = new Position(_currentAddress.Latitude, _currentAddress.Longitude);
				addressViewModels = addressViewModels.OrderBy(a => a.ToPosition().DistanceTo(currentPosition)).ToArray();
			}

			addressViewModels.Last().IsLast = true;

			InvokeOnMainThread(() =>
				{
					AllAddresses.AddRange(addressViewModels);
				});

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

		public ICommand AddressSelected
		{
			get
			{
				return this.GetCommand<AddressViewModel>(vm => {
					_orderWorkflowService.SetAddress(vm.Address);
					ChangePresentation(new HomeViewModelPresentationHint(HomeViewModelState.Initial));
				}); 
			}
		}

		public ICommand Cancel
		{
			get
			{
				return this.GetCommand(() => {
					ChangePresentation(new HomeViewModelPresentationHint(HomeViewModelState.Initial));
				}); 
			}
		}

		private string _startingText;
		public string StartingText
		{
			get { return _startingText; }
			set 
			{
				if(value != _startingText)
				{
					_startingText = value;
					RaisePropertyChanged();
				}
			}
		}

		public void SearchAddress(string criteria)
		{
			if (criteria.HasValue())
			{                
				InvokeOnMainThread(() =>
					{
						this.Services().Message.ShowProgressNonModal(true);
						ShowDefaultResults = false;
						AllAddresses.Clear();
					});

				var t1 = Task.Factory.StartNew(() =>
					{
						var fhAdrs = SearchFavoriteAndHistoryAddresses(criteria);
						InvokeOnMainThread(() =>
							{
								AllAddresses.AddRangeDistinct(fhAdrs, (x, y) => x.Equals(y));
							});
					});
				var t2 = Task.Factory.StartNew(() =>
					{
						var pAdrs = SearchPlaces(criteria);
						InvokeOnMainThread(() =>
							{
								AllAddresses.AddRangeDistinct(pAdrs, (x, y) => x.Equals(y));
							});
					});
				var t3 = Task.Factory.StartNew(() =>
					{
						var gAdrs = SearchGeocodeAddresses(criteria);   
						InvokeOnMainThread(() =>
							{
								AllAddresses.AddRangeDistinct(gAdrs, (x, y) => x.Equals(y));
							});
					});

				t1.Wait();
				t2.Wait();
				t3.Wait();

				InvokeOnMainThread(() =>
					{
						this.Services().Message.ShowProgressNonModal(false);
					});
			}
			else
			{
				InvokeOnMainThread(() =>
					{
						LoadDefaultList();
					});
			}
		}

		protected AddressViewModel[] SearchPlaces(string criteria)
		{           
			var position = _currentAddress;

			if (position == null)
			{
				return new AddressViewModel[0];
			}

			var fullAddresses = _placeService.SearchPlaces(criteria, position.Latitude, position.Longitude, null);

			var addresses = fullAddresses.ToList();
			return addresses.Select(a => new AddressViewModel(a, AddressType.Places) { IsSearchResult = true }).ToArray();
		}

		protected AddressViewModel[] SearchFavoriteAndHistoryAddresses(string criteria)
		{
			var addresses = _accountService.GetFavoriteAddresses();
			var historicAddresses = _accountService.GetHistoryAddresses();

			Func<Address, bool> predicate = c => true;

			predicate = x => (x.FriendlyName != null
				&& x.FriendlyName.ToLowerInvariant().Contains(criteria))
			            || (x.FullAddress != null
				            && x.FullAddress.ToLowerInvariant().Contains(criteria));           

			var a1 = addresses.Where(predicate).Select(f => new AddressViewModel(f, f.FriendlyName.HasValue() ? AddressType.Favorites : AddressType.History) { IsSearchResult = true });
			var a2 = historicAddresses.Where(predicate).Select(f => new AddressViewModel(f, f.FriendlyName.HasValue() ? AddressType.Favorites : AddressType.History) { IsSearchResult = true });
			var r = a1.Concat(a2).ToArray(); 
			return r;
		}

		protected AddressViewModel[] SearchGeocodeAddresses(string criteria)
		{
			Logger.LogMessage("Starting SearchAddresses : " + criteria);
			var position = _currentAddress;

			Address[] addresses;

			if (position == null)
			{
				Logger.LogMessage("No Position SearchAddresses : " + criteria);
				addresses = _geoCoding.SearchAddress(criteria);                
			}
			else
			{
				Logger.LogMessage("Position SearchAddresses : " + criteria);
				addresses = _geoCoding.SearchAddress(criteria, position.Latitude, position.Longitude);
			}

			return addresses.Select(a => new AddressViewModel(a, AddressType.Places) { IsSearchResult = true }).ToArray();
		}
	}
}

