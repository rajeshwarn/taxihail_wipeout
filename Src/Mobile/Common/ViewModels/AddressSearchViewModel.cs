using System;
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

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class AddressSearchViewModel : BaseViewModel
    {
        private readonly string _ownerId;
        private readonly IGoogleService _googleService;
        private CancellationTokenSource _searchCancellationToken = new CancellationTokenSource();
        private bool _isSearching;

        public AddressSearchViewModel(string ownerId, string search, IGoogleService googleService)
        {
            _ownerId = ownerId;
            _googleService = googleService;
			TinyIoCContainer.Current.Resolve<IUserPositionService>().Refresh();
            SearchViewModelSelected = TinyIoCContainer.Current.Resolve<AddressSearchByGeoCodingViewModel>();
            Criteria = search;
        }

        public AddressSearchBaseViewModel SearchViewModelSelected { get; set; }

        public IEnumerable<AddressViewModel> AddressViewModels { get; set; }

		public IEnumerable<AddressViewModel> HistoricAddressViewModels { get; set; }

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

        public IMvxCommand SearchCommand
        {
            get
            {
                return new MvxRelayCommand(() =>
                {
                    if (string.IsNullOrEmpty(Criteria)) return;

                    if (_searchCancellationToken != null 
                        && _searchCancellationToken.Token.CanBeCanceled)
                    {
                        _searchCancellationToken.Cancel();
                    }
                    _searchCancellationToken = new CancellationTokenSource();
                    SearchViewModelSelected.Criteria = Criteria;
                    var task = SearchViewModelSelected.OnSearchExecute(_searchCancellationToken.Token);
                    task.ContinueWith(RefreshResults);
                    task.Start();
                    IsSearching = true;
                });
            }
        }

        public void RefreshResults(Task<IEnumerable<AddressViewModel>>  task)
        {
            if(task.IsCompleted)
            {
                InvokeOnMainThread(() =>
                                       {
                                           AddressViewModels = task.Result;
                                           FirePropertyChanged(() => AddressViewModels);
                                       });
                IsSearching = false;
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

            _searchSelected = btn == TopBarButton.SearchBtn;
            _favoritesSelected = btn == TopBarButton.FavoritesBtn;
            _contactsSelected = btn == TopBarButton.ContactsBtn;
            _placesSelected = btn == TopBarButton.PlacesBtn;

            FirePropertyChanged(() => SearchSelected);
            FirePropertyChanged(() => FavoritesSelected);
            FirePropertyChanged(() => ContactsSelected);
            FirePropertyChanged(() => PlacesSelected);

            SearchCommand.Execute();
		}

        private bool _searchSelected;
        public bool SearchSelected
        {
            get { return _searchSelected; }
            set
            {
                _searchSelected = value;
                FirePropertyChanged(() => SearchSelected);
                if (value) SetSelected(TopBarButton.SearchBtn);
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
                if (value) SetSelected(TopBarButton.FavoritesBtn);
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
                if (value) SetSelected(TopBarButton.ContactsBtn);
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
                if (value) SetSelected(TopBarButton.PlacesBtn);
            }
        }

        public IMvxCommand CloseViewCommand
        {
            get { return new MvxRelayCommand(() => RequestClose(this)); }
        }

    }
}

