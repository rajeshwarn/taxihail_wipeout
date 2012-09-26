using System;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.MvvmCross.Commands;
using Cirrious.MvvmCross.Interfaces.Commands;
using apcurium.MK.Booking.Mobile.AppServices;
using System.Collections.Generic;
using apcurium.MK.Booking.Mobile.ViewModels.SearchAddress;
using apcurium.MK.Common.Extensions;
using System.Threading;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Messages;
using TinyMessenger;
using apcurium.MK.Booking.Mobile.Infrastructure;


namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class AddressSearchViewModel : BaseViewModel
    {
        private readonly string _ownerId;
        private readonly IGoogleService _googleService;
		private readonly IAppResource _appResource;
		private CancellationTokenSource _searchCancellationToken = new CancellationTokenSource();
        private bool _isSearching;

		public AddressSearchViewModel(string ownerId, string search, IGoogleService googleService, IAppResource appResource)
        {
            _ownerId = ownerId;
            _googleService = googleService;
			_appResource = appResource;
			TinyIoCContainer.Current.Resolve<IUserPositionService>().Refresh();
            SearchViewModelSelected = TinyIoCContainer.Current.Resolve<AddressSearchByGeoCodingViewModel>();
            Criteria = search;
            SearchSelected = true;
            HistoricIsHidden = true;
			AllAddresses = new SectionAddressViewModel[0];
        }

        public AddressSearchBaseViewModel SearchViewModelSelected { get; set; }

        public IEnumerable<AddressViewModel> AddressViewModels { get; set; }

		public IEnumerable<AddressViewModel> HistoricAddressViewModels { get; set; }

		public IEnumerable<SectionAddressViewModel> AllAddresses {get; set;	}

        public string Criteria
        {
            get { return _criteria; }
            set
            {
                _criteria = value;
                FirePropertyChanged(() => Criteria);
            }
        }

        public bool IsSearching
        {
            get { return _isSearching; }
            set
            {
                _isSearching = value;
                FirePropertyChanged(() => IsSearching);
            }
        }

        public bool HistoricIsHidden { get; set; }

        public IMvxCommand SearchCommand
        {
            get
            {
                return new MvxRelayCommand<string>(criteria =>
                {
                    SearchViewModelSelected.Criteria = criteria != null ? criteria.ToLowerInvariant() : null;

                    if (!SearchViewModelSelected.CriteriaValid)
                    {
                        return;
                    }

                    CancelCurrentSearch();

                    _searchCancellationToken = new CancellationTokenSource();
                    
                    var task = SearchViewModelSelected.OnSearchExecute(_searchCancellationToken.Token);
                    task.ContinueWith(RefreshResults);
                    if(!(SearchViewModelSelected is AddressSearchByContactViewModel))
                    {
						Console.WriteLine( "Show Progress" );
						TinyIoCContainer.Current.Resolve<IMessageService>().ShowProgress(true, () => CancelCurrentSearch() );
                        task.Start();
                    }
                    IsSearching = true;
                });
            }
        }

        private void CancelCurrentSearch()
        {
            if (_searchCancellationToken != null
                && _searchCancellationToken.Token.CanBeCanceled)
            {
                _searchCancellationToken.Cancel();
                _searchCancellationToken.Dispose();
                _searchCancellationToken = null;
            }
        }

        public void RefreshResults(Task<IEnumerable<AddressViewModel>>  task)
        {
            if(task.IsCompleted
                && !task.IsCanceled)
            {
                InvokeOnMainThread(() =>
                {
                    IsSearching = false;
                    AddressViewModels = task.Result.Where(x => !x.Address.IsHistoric).ToList();
                    HistoricAddressViewModels = task.Result.Where(x => x.Address.IsHistoric).ToList();
                    HistoricIsHidden = !HistoricAddressViewModels.Any();
					var allAddresses = new List<SectionAddressViewModel>();
					if( SearchViewModelSelected is AddressSearchByFavoritesViewModel )
					{
						allAddresses.Add( new SectionAddressViewModel(){SectionTitle =  _appResource.GetString("FavoriteLocationsTitle"), Addresses = AddressViewModels} );
						allAddresses.Add( new SectionAddressViewModel(){SectionTitle = _appResource.GetString("HistoryViewTitle"), Addresses = HistoricAddressViewModels} );
					}
					else
					{
						allAddresses.Add( new SectionAddressViewModel(){SectionTitle =  "", Addresses = AddressViewModels} );
					}

					AllAddresses = allAddresses;

					FirePropertyChanged(() => AddressViewModels);

					FirePropertyChanged(() => HistoricAddressViewModels);
					FirePropertyChanged(() => AllAddresses);
					FirePropertyChanged(() => HistoricIsHidden);
                });
            }
			Console.WriteLine( "Hide Progress" );
			TinyIoCContainer.Current.Resolve<IMessageService>().ShowProgress(false);
        }

        private void ClearResults()
        {
            AddressViewModels = new AddressViewModel[0];
            HistoricAddressViewModels = new AddressViewModel[0];
			AllAddresses = new SectionAddressViewModel[0];
            HistoricIsHidden = true;

            FirePropertyChanged(() => AddressViewModels);
            FirePropertyChanged(() => HistoricAddressViewModels);
			FirePropertyChanged(() => AllAddresses);
			FirePropertyChanged(() => HistoricIsHidden);
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

		private enum TopBarButton { SearchBtn, FavoritesBtn, ContactsBtn, PlacesBtn }

        
		private void SetSelected(TopBarButton btn)
		{
		    switch (btn)
		    {
		        case TopBarButton.SearchBtn:
		            SearchViewModelSelected = TinyIoCContainer.Current.Resolve<AddressSearchByGeoCodingViewModel>();
                    break;
		        case TopBarButton.FavoritesBtn:
                    SearchViewModelSelected = TinyIoCContainer.Current.Resolve<AddressSearchByFavoritesViewModel>();
		            break;
		        case TopBarButton.ContactsBtn:
		            SearchViewModelSelected = TinyIoCContainer.Current.Resolve<AddressSearchByContactViewModel>();
		            break;
		        case TopBarButton.PlacesBtn:
                    SearchViewModelSelected = TinyIoCContainer.Current.Resolve<AddressSearchByPlacesViewModel>();
		            break;
		        default:
		            throw new ArgumentOutOfRangeException("btn");
		    }

            SearchSelected = btn == TopBarButton.SearchBtn;
            FavoritesSelected = btn == TopBarButton.FavoritesBtn;
            ContactsSelected = btn == TopBarButton.ContactsBtn;
            PlacesSelected = btn == TopBarButton.PlacesBtn;
            
			if( Criteria == null )
			{
				SearchCommand.Execute(Criteria);
			}
			else
			{
		    	Criteria = null;
			}
		}

        public IMvxCommand SelectedChangedCommand
        {
            get { 
                return new MvxRelayCommand<object>(param => param.Maybe(tag =>
                {
                    ClearResults();
                    TopBarButton btSelected;
                    if(Enum.TryParse(tag.ToString(), true, out btSelected))
                    {
                        SetSelected(btSelected);
                    }
                })); 
            }
        }

        private bool _searchSelected;
        public bool SearchSelected
        {
            get { return _searchSelected; }
            set
            {
                _searchSelected = value;
                FirePropertyChanged(() => SearchSelected);
            }
        }

        private bool _favoritesSelected;
        public bool FavoritesSelected
        {
            get { return _favoritesSelected; }
            set
            {
                _favoritesSelected = value;
                FirePropertyChanged(() => FavoritesSelected);
            }
        }

        private bool _contactsSelected;
        public bool ContactsSelected
        {
            get { return _contactsSelected; }
            set
            {
                _contactsSelected = value;
                FirePropertyChanged(() => ContactsSelected);
            }
        }

        private bool _placesSelected;
        private string _criteria;

        public bool PlacesSelected
        {
            get { return _placesSelected; }
            set
            {
                _placesSelected = value;
                FirePropertyChanged(() => PlacesSelected);
            }
        }

        public IMvxCommand CloseViewCommand
        {
            get { return new MvxRelayCommand(() => RequestClose(this)); }
        }

        

    }
}

