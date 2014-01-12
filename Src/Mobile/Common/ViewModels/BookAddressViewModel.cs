using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Common.Extensions;
using System.Threading;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.Extensions;
using ServiceStack.Text;
using apcurium.MK.Common;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class BookAddressViewModel : BaseViewModel
       {
        private CancellationTokenSource _cancellationToken;
        private bool _isExecuting;
        private readonly Func<Address> _getAddress;
        private readonly Action<Address> _setAddress;
        private readonly string _id;
        private readonly string _searchingTitle;

        public event EventHandler AddressChanged;
        public event EventHandler AddressCleared;

        public BookAddressViewModel(Func<Address> getAddress, Action<Address> setAddress)
        {
            _getAddress = getAddress;
            _setAddress = setAddress;
            _id = Guid.NewGuid().ToString();
            _searchingTitle = this.Services().Localize["AddressSearchingText"];
            this.Services().MessengerHub.Subscribe<AddressSelected>(OnAddressSelected, selected => selected.OwnerId == _id);
        }

        public string AddressLine2
        {
            get
            {
                if (IsExecuting)
                {
                    return "";
                }
                var addressDisplay = "";
                var adr = _getAddress();
                if (adr != null)
                {
                    if ((adr.AddressType == "place") || (Params.Get(adr.City, adr.State, adr.ZipCode).Count(s => s.HasValue()) == 0))
                    {
                        addressDisplay =  adr.FullAddress;
                    }
                    else
                    {
                        addressDisplay =  Params.Get(adr.City, adr.State, adr.ZipCode).Where(s => s.HasValue()).JoinBy(", ");
                    }
                }
                return addressDisplay;
            }
        }

        public string EmptyAddressPlaceholder { get; set; }

        public string AddressLine1
        {
            get
            {
				RaisePropertyChanged(() => IsPlaceHolder);
                if (IsExecuting)
                {
                    return _searchingTitle;
                }
                return GetAddress().HasValue() ? GetAddress() : EmptyAddressPlaceholder;
            }
        }

        private string GetAddress()
        {
            var adr = _getAddress();
            if (adr == null)
            {
                return "";
            }
            if (adr.BuildingName.HasValue())
            {
                return Params.Get(adr.BuildingName, adr.Street).Where(s => s.HasValue() && s.Trim().HasValue()).JoinBy(", ");
            }
            return Params.Get(adr.StreetNumber, adr.Street).Any(s => s.HasValue() && s.Trim().HasValue())
                 ? Params.Get(adr.StreetNumber, adr.Street).Where(s => s.HasValue() && s.Trim().HasValue()).JoinBy(" ") 
                 : adr.FullAddress;
        }



        public bool IsPlaceHolder
        {
            get
            {
                return Model.FullAddress.IsNullOrEmpty();
            }
        }

        public Address Model { get { return _getAddress(); } set { _setAddress(value); } }

        public AsyncCommand<Address> SearchCommand
        {
            get
            {
                return GetCommand<Address>(coordinate =>
                {
                    CancelCurrentLocation();

                    _cancellationToken = new CancellationTokenSource();

                    var token = _cancellationToken.Token;
                    var task = Task.Factory.SafeStartNew(() =>
                    {
                        if (!token.IsCancellationRequested)
                        {
                            IsExecuting = true;
                            var accountAddress = this.Services().Account.FindInAccountAddresses(coordinate.Latitude, coordinate.Longitude);
                            if (accountAddress != null)
                            {
                                return new[] { accountAddress};
                            }
                            return this.Services().Geoloc.SearchAddress(coordinate.Latitude, coordinate.Longitude).ToArray();
                        }
                        return null;

                    }, token);

                    task.ContinueWith(t => InvokeOnMainThread(() => {
                    if (t.Result != null && t.Result.Any())
                    {
                        var address = t.Result[0];                                
                        // Replace result coordinates  by search coordinates (= user position)
                        address.Latitude = coordinate.Latitude;
                        address.Longitude = coordinate.Longitude;
                        SetAddress(address, true);
                    }
                    else
                    {
                        Logger.LogMessage("No address found for coordinate : La : {0} , Lg: {1} ", coordinate.Latitude, coordinate.Longitude);
                        ClearAddress();
                    }
                    }), TaskContinuationOptions.OnlyOnRanToCompletion);

                });
            }
        }

        public AsyncCommand PickAddress
        {
            get
            {
                return GetCommand(() =>
                {
                    CancelCurrentLocation();


                    if (this.Services().Config.GetSetting("Client.StreetNumberScreenEnabled", true)
                        && Model.BookAddress.HasValue())
                    {
                        ShowViewModel<BookStreetNumberViewModel>(new
                            {
                                address = JsonSerializer.SerializeToString(Model),
                                ownerId = _id
                            });
                    }
                    else
                    {
                        ShowViewModel<AddressSearchViewModel>(new
                            {
                                search = Model.FullAddress,
                                ownerId = _id, 
                                places = "false"
                            });
                    }

                });
            }
        }

        private void OnAddressSelected(AddressSelected selected)
        {
            if ( selected.Content != null )
            {
                SetAddress(selected.Content, !selected.ShouldRecenter);
            }
            else
            {
                ClearAddress ();
            }

        }

        public bool IsExecuting
        {
            get { return _isExecuting; }
            set
            {
                _isExecuting = value;
				RaisePropertyChanged();
				RaisePropertyChanged(() => AddressLine1);
				RaisePropertyChanged(() => AddressLine2);
            }
        }

        //todo il sert a quoi le param?
        public AsyncCommand<string> CancelCurrentLocationCommand
        {
            get
            {
                return new AsyncCommand<string>(_ => CancelCurrentLocation());
            }
        }

        private void CancelCurrentLocation()
        {
            if ((_cancellationToken != null) && (_cancellationToken.Token.CanBeCanceled))
            {
                _cancellationToken.Cancel();
                _cancellationToken = null;
            }
            IsExecuting = false;
        }

        public void SetAddress(Address address, bool userInitiated)
        {
            InvokeOnMainThread(() =>
            {
                IsExecuting = true;

                CancelCurrentLocation();


                if ((address.Street.IsNullOrEmpty()) && (address.ZipCode.IsNullOrEmpty()) &&
                    (address.AddressType != "place") && (address.AddressType != "popular"))
                    // This should only be true when using an address from a version smaller than 1.3                    
                {
                    var a = this.Services().Geoloc.SearchAddress(address.FullAddress);
                    if (a.Any())
                    {
                        address = a.First();
                    }
                }

                address.CopyTo(Model);

				RaisePropertyChanged(() => AddressLine1);
				RaisePropertyChanged(() => AddressLine2);
				RaisePropertyChanged(() => Model);


                if (AddressChanged != null)
                {
                    AddressChanged(userInitiated, EventArgs.Empty);
                }
            });
        }

        public void ClearAddress()
        {
            InvokeOnMainThread(() =>
            {
                var clearAddress = new Address();
                clearAddress.CopyTo(Model);
				RaisePropertyChanged(() => AddressLine1);
				RaisePropertyChanged(() => AddressLine2);
				RaisePropertyChanged(() => Model);
                IsExecuting = false;
                if (AddressChanged != null)
                {
                    AddressChanged(true, EventArgs.Empty);
                }

                if (AddressCleared != null)
                {
                    AddressCleared(this, EventArgs.Empty);
                }
            });
        }

        public AsyncCommand ClearPositionCommand
        {
            get
            {
                return GetCommand(ClearAddress);
            }
        }

        public AsyncCommand RequestCurrentLocationCommand
        {
            get {
                return GetCommand(() =>
                {

                    CancelCurrentLocationCommand.Execute ();

                    if (!this.Services().Location.IsLocationServicesEnabled)
                    {
                        this.Services().Message.ShowMessage(this.Services().Localize["LocationServiceErrorTitle"], this.Services().Localize["LocationServiceErrorMessage"]);
                        return ;
                    }

                    IsExecuting = true;
                    var positionSet = false;
                    if (this.Services().Location.BestPosition != null)
                    {
                        InvokeOnMainThread(() => SearchAddressForCoordinate(this.Services().Location.BestPosition));
                        return;
                    }

                    this.Services().Location.GetNextPosition(TimeSpan.FromSeconds(6), 50).Subscribe(
                    pos=>
                    {
                        positionSet =true;
                        InvokeOnMainThread(()=>SearchAddressForCoordinate(pos));
                    },
                    ()=>
                    {  
						positionSet = false;
                        
                        if(!positionSet)
                        {
                            InvokeOnMainThread(() =>
                                {
                                    if (this.Services().Location.BestPosition == null)
                                    {
                                        this.Services().Message.ShowToast("Cant find location, please try again", ToastDuration.Short);
                                    }
                                    else
                                    {
                                        SearchAddressForCoordinate(this.Services().Location.BestPosition);    
                                    }
                                    
                                });
                        }
                    });
                });
            }

        }

        private async void SearchAddressForCoordinate(Position p)
        {
            IsExecuting = true;
            Logger.LogMessage("Start Call SearchAddress : " + p.Latitude.ToString(CultureInfo.InvariantCulture) + ", " + p.Longitude.ToString(CultureInfo.InvariantCulture));

            var accountAddress = await Task.Run(() => this.Services().Account.FindInAccountAddresses(p.Latitude, p.Longitude));
            if (accountAddress != null)
            {
                Logger.LogMessage("Address found in account");
                SetAddress(accountAddress, false);
            }
            else
            {
                var address = await Task.Run(() => this.Services().Geoloc.SearchAddress(p.Latitude, p.Longitude));
                Logger.LogMessage("Call SearchAddress finsihed, found {0} addresses", address.Count());
                if (address.Any())
                {
                    Logger.LogMessage(" found {0} addresses", address.Count());
                    SetAddress(address[0], false);
                }
                else
                {
                    Logger.LogMessage(" clear addresses");
                    ClearAddress();
                }
                Logger.LogMessage("Exiting SearchAddress thread");
            }

            IsExecuting = false;
        }

    }
}
