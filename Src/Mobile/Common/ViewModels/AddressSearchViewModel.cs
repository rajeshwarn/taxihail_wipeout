using System;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.MvvmCross.Interfaces.Commands;
using apcurium.MK.Booking.Mobile.AppServices;
using System.Collections.Generic;
using apcurium.MK.Common.Extensions;
using System.Threading;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Messages;
using TinyMessenger;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using System.Reactive.Linq;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class AddressSearchViewModel : BaseViewModel
    {
        private readonly string _ownerId;
        private readonly IGoogleService _googleService;
        private readonly ObservableCollection<AddressViewModel> _addressViewModels = new ObservableCollection<AddressViewModel> ();
        private CancellationTokenSource _searchCancellationToken = new CancellationTokenSource ();
        private bool _isSearching;
        private string _criteria;

        public AddressSearchViewModel (string ownerId, string search, IGoogleService googleService, string places = "false")
        {
            _ownerId = ownerId;
            _googleService = googleService;
        
            IsPlaceSearch = places == "true";

            Criteria = search;

            var searchTextChanged = Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs> (
                ev => PropertyChanged += ev,
                ev => PropertyChanged -= ev)
                .Where (ev => ev.EventArgs.PropertyName == "Criteria");

            searchTextChanged.Subscribe (o => OnSearchStart ());
            searchTextChanged.Throttle (TimeSpan.FromMilliseconds (700)).Subscribe (o => OnSearch ());

            OnSearch ();
        }

        public bool IsPlaceSearch{ get; set; }

        private void OnSearchStart ()
        {
            IsSearching = true;
            Logger.LogMessage ("OnSearchStarted");
            CancelSearch ();
        }

        private void CancelSearch ()
        {
            if (_searchCancellationToken == null) return;
            _searchCancellationToken.Cancel ();
            _searchCancellationToken.Dispose ();
            _searchCancellationToken = null;
        }

        private void OnSearch ()
        {
            IsSearching = true;

            Logger.LogMessage ("OnSearch");
            CancelSearch ();

            _searchCancellationToken = new CancellationTokenSource ();

            if (!IsPlaceSearch) {
                var task = new Task<IEnumerable<AddressViewModel>> (SearchFavoriteAndHistoryAddresses, _searchCancellationToken.Token);
                var task2 = new Task<IEnumerable<AddressViewModel>> (SearchGeocodeAddresses, _searchCancellationToken.Token);

                task.ContinueWith (r => RefreshResults (r, task2));
                task2.ContinueWith (r => RefreshResults (r, task));

                task2.Start ();
                task.Start ();
            } else {
                var task = new Task<IEnumerable<AddressViewModel>> (SearchPlaces, _searchCancellationToken.Token);                
                task.ContinueWith (r => RefreshResults (r, null));
                task.Start ();
            }
        }

        public ObservableCollection<AddressViewModel> AddressViewModels { 
            get { return _addressViewModels; }
        }

        public string Criteria {
            get { return _criteria; }
            set {
                _criteria = value;
                FirePropertyChanged (() => Criteria);
            }
        }

        public bool IsSearching {
            get { return _isSearching; }
            set {
                _isSearching = value;
                FirePropertyChanged (() => IsSearching);
            }
        }


     
        protected IEnumerable<AddressViewModel> SearchPlaces ()
        {           
            var position = LocationService.BestPosition;

            if (position == null) {

                position = LocationService.GetNextPosition(new TimeSpan(0,0,1),1000).FirstOrDefault();

                if(position == null && LocationService.BestPosition != null)
                {
                    position =  LocationService.BestPosition;
                }

                if (position == null) {
                    return Enumerable.Empty<AddressViewModel> ();
                }
            }
            var fullAddresses = _googleService.GetNearbyPlaces(position.Latitude, position.Longitude, Criteria.HasValue () ? Criteria : null);
            var addresses = fullAddresses.ToList ();
            return addresses.Select (a => new AddressViewModel () { Address = a , Icon = "address"}).ToList ();
            
        }

        protected IEnumerable<AddressViewModel> SearchFavoriteAndHistoryAddresses ()
        {
            var addresses = AccountService.GetFavoriteAddresses ();
            var historicAddresses = AccountService.GetHistoryAddresses ();

            Func<Address, bool> predicate = c => true;
            if (Criteria.HasValue ()) {
                predicate = x => (x.FriendlyName != null && x.FriendlyName.ToLowerInvariant ().Contains (Criteria)) || (x.FullAddress != null && x.FullAddress.ToLowerInvariant ().Contains (Criteria));
            }
            var a1 = addresses.Where (predicate).Select (a => new AddressViewModel { Address = a, Icon = "favorites"});
            var a2 = historicAddresses.Where (predicate).Select (a => new AddressViewModel { Address = a,  Icon = "history" });
            var r = a1.Concat (a2).ToArray (); 
            return r;
        }

        protected IEnumerable<AddressViewModel> SearchGeocodeAddresses ()
        {
            Logger.LogMessage ("Starting SearchAddresses : " + Criteria.ToSafeString ());
            var position = TinyIoCContainer.Current.Resolve<AbstractLocationService> ().LastKnownPosition;

            Address[] addresses;
            
            if (position == null) {
                Logger.LogMessage ("No Position SearchAddresses : " + Criteria.ToSafeString ());
                addresses = GeolocService.SearchAddress (Criteria);                
            } else {
                Logger.LogMessage ("Position SearchAddresses : " + Criteria.ToSafeString ());
                addresses = GeolocService.SearchAddress(Criteria, position.Latitude, position.Longitude);
            }
            return addresses.Select (a => new AddressViewModel { Address = a, Icon="address"}).ToList ();
        }
     
        public void RefreshResults (Task<IEnumerable<AddressViewModel>> task, Task concurentTask)
        {          
            InvokeOnMainThread (() =>
            {                   
                if (task.IsCompleted && !task.IsCanceled && !task.IsFaulted) {
                    if ((concurentTask == null) || (concurentTask.IsCompleted)) {
                        IsSearching = false;
                    }
                    if ((concurentTask == null) || (!concurentTask.IsCompleted)) {
                        AddressViewModels.Clear ();
                    }

                    AddressViewModels.AddRange (task.Result);
                    BubbleSort (AddressViewModels);
                    AddressViewModels.ForEach (a => 
                    {
                        a.IsFirst = a.Equals (AddressViewModels.First ());
                        a.IsLast = a.Equals (AddressViewModels.Last ());
                    });
                }
            });
        }

        [Obsolete("This is terrible - why would you do this?")]
        public  void BubbleSort (ObservableCollection<AddressViewModel> o)
        {
            for (var i = o.Count - 1; i >= 0; i--) {
                for (var j = 1; j <= i; j++) {
                    var o1 = o [j - 1];
                    var o2 = o [j];
                    if (!Compare(o2, o1)) continue;
                    o.Remove (o1);
                    o.Insert (j, o1);
                }
            }
        }

        private bool Compare (AddressViewModel o1, AddressViewModel o2)
        {
            if (o1.Icon.SoftEqual ("favorites")) {
                return  !o2.Icon.SoftEqual ("favorites");                
            }
            if (o1.Icon.SoftEqual ("history")) {

                return (!o2.Icon.SoftEqual ("favorites")) && (!o2.Icon.SoftEqual ("history"));
            }
            return false;
        }

        public void ClearResults ()
        {
            AddressViewModels.Clear ();
        }

        public IMvxCommand RowSelectedCommand {
            get {
                return GetCommand<AddressViewModel> (address => ThreadPool.QueueUserWorkItem (o =>
                    {
                        if (address.Address == null) return;

                        if ((address.Address.AddressType == "place") && (address.Address.PlaceReference.HasValue ())) {
                            var placeAddress = _googleService.GetPlaceDetail (address.Address.FriendlyName, address.Address.PlaceReference);
                            placeAddress.FriendlyName = address.Address.FriendlyName;
                            placeAddress.BuildingName = address.Address.FriendlyName;
                            placeAddress.AddressType = address.Address.AddressType;
                            if ( address.Address.FullAddress.HasValue () )
                            {
                                placeAddress.FullAddress = address.Address.FullAddress;
                            }
                            RequestClose (this);
                            InvokeOnMainThread (() => TinyIoCContainer.Current.Resolve<ITinyMessengerHub> ().Publish (new AddressSelected (this, placeAddress, _ownerId)));
                        } else if (address.Address.AddressType == "localContact") {

                            var addresses = GeolocService.SearchAddress (address.Address.FullAddress);
                            if (addresses.Any()) {
                                RequestClose (this);
                                InvokeOnMainThread (() => TinyIoCContainer.Current.Resolve<ITinyMessengerHub> ().Publish (new AddressSelected (this, addresses.ElementAt (0), _ownerId)));
                            } else {
                                    
                                var title = TinyIoCContainer.Current.Resolve<IAppResource> ().GetString ("LocalContactCannotBeResolverTitle");
                                var msg = TinyIoCContainer.Current.Resolve<IAppResource> ().GetString ("LocalContactCannotBeResolverMessage");
                                TinyIoCContainer.Current.Resolve<IMessageService> ().ShowMessage (title, msg);
                            }
                        } else {
                            RequestClose (this);
                                
                            InvokeOnMainThread (() => TinyIoCContainer.Current.Resolve<ITinyMessengerHub> ().Publish (new AddressSelected (this, address.Address, _ownerId)));
                        }
                    }));
            }
        }

        public IMvxCommand CloseViewCommand {
            get { return GetCommand (() => RequestClose (this)); }
        }



    }
}

