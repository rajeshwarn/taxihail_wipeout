using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using apcurium.MK.Booking.Api.Contract.Resources;
using Cirrious.MvvmCross.Commands;
using Cirrious.MvvmCross.Interfaces.Commands;
using System.Threading.Tasks;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Common.Extensions;
using System.Threading;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Diagnostic;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using Cirrious.MvvmCross.ExtensionMethods;
using apcurium.MK.Booking.Mobile.Extensions;
using ServiceStack.Text;
using apcurium.MK.Common;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class BookAddressViewModel : BaseViewModel,
        IMvxServiceConsumer<ILocationService>,
        IMvxServiceConsumer<IAccountService>,
        IMvxServiceConsumer<IGeolocService>
    {
        private ILocationService _geolocator;
        private CancellationTokenSource _cancellationToken;
        private TaskScheduler _scheduler = TaskScheduler.FromCurrentSynchronizationContext();
        private bool _isExecuting;
        private Func<Address> _getAddress;
        private Action<Address> _setAddress;
        private string _id;
        private string _searchingTitle;

        public event EventHandler AddressChanged;

        public event EventHandler AddressCleared;

        public BookAddressViewModel(Func<Address> getAddress, Action<Address> setAddress, ILocationService geolocator)
        {
            _getAddress = getAddress;
            _setAddress = setAddress;
            _id = Guid.NewGuid().ToString();
            _geolocator = geolocator;
            _searchingTitle = Resources.GetString("AddressSearchingText");
            MessengerHub.Subscribe<AddressSelected>(OnAddressSelected, selected => selected.OwnerId == _id);
        }

        public string AddressLine2
        {
            get
            {
                if (IsExecuting)
                {
                    return "";
                }
                else
                {
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
        }

        public string EmptyAddressPlaceholder { get; set; }

        public string AddressLine1
        {
            get
            {
                FirePropertyChanged(() => IsPlaceHolder);
                if (IsExecuting)
                {
                    return _searchingTitle;
                }
                if (GetAddress().HasValue())
                {
                    return GetAddress();
                }
                else
                {
                    return EmptyAddressPlaceholder;
                }
            }
        }

        private string GetAddress()
        {
            var adr = _getAddress();
            if (adr == null)
            {
                return "";
            }
            else
            {
                if (adr.BuildingName.HasValue())
                {
                    return Params.Get(adr.BuildingName, adr.Street).Where(s => s.HasValue() && s.Trim().HasValue()).JoinBy(", ");
                }
                else if (Params.Get(adr.StreetNumber, adr.Street).Where(s => s.HasValue() && s.Trim().HasValue()).Count() > 0)
                {
                    return Params.Get(adr.StreetNumber, adr.Street).Where(s => s.HasValue() && s.Trim().HasValue()).JoinBy(" ");
                }
                else
                {
                    return adr.FullAddress;
                }
            }
        }



        public bool IsPlaceHolder
        {
            get
            {
                return Model.FullAddress.IsNullOrEmpty();
            }
        }

        public Address Model { get { return _getAddress(); } set { _setAddress(value); } }

        public IMvxCommand SearchCommand
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
                            var accountAddress = this.GetService<IAccountService>().FindInAccountAddresses(coordinate.Latitude, coordinate.Longitude);
                            if (accountAddress != null)
                            {
                                return new Address[] { accountAddress};
                            }
                            else
                            {
                                return this.GetService<IGeolocService>().SearchAddress(coordinate.Latitude, coordinate.Longitude).ToArray();
                            }
                        }
                        return null;

                    }, token);

                    task.ContinueWith(t =>
                    { 
                        InvokeOnMainThread(() => {
                            if (t.Result != null && t.Result.Any())
                            {
                                var address = t.Result[0];
                                Console.WriteLine(address.FullAddress);
                                // Replace result coordinates  by search coordinates (= user position)
                                address.Latitude = coordinate.Latitude;
                                address.Longitude = coordinate.Longitude;
                                SetAddress(address, true);
                            }
                            else
                            {
                                TinyIoCContainer.Current.Resolve<ILogger>().LogMessage("No address found for coordinate : La : {0} , Lg: {1} ", coordinate.Latitude, coordinate.Longitude);
                                ClearAddress();
                            }
                        });
                        
                    }, TaskContinuationOptions.OnlyOnRanToCompletion);

                });
            }
        }

        public IMvxCommand PickAddress
        {
            get
            {
                return GetCommand(() =>
                {
                    CancelCurrentLocation();
                    if (Settings.StreetNumberScreenEnabled 
                        && Model.BookAddress.HasValue())
                    {
                        RequestNavigate<BookStreetNumberViewModel>(new { address = JsonSerializer.SerializeToString<Address>(Model), ownerId = _id });
                    }
                    else
                    {
                        RequestNavigate<AddressSearchViewModel>(new { search = Model.FullAddress, ownerId = _id, places = "false" });
                    }

                });
            }
        }

        private void OnAddressSelected(AddressSelected selected)
        {
            SetAddress(selected.Content, true);
        }

        public bool IsExecuting
        {
            get { return _isExecuting; }
            set
            {
                _isExecuting = value;
                FirePropertyChanged(() => IsExecuting);
                FirePropertyChanged(() => AddressLine1);
                FirePropertyChanged(() => AddressLine2);
            }
        }

        public IMvxCommand CancelCurrentLocationCommand
        {
            get
            {
                return new MvxRelayCommand<string>(_notUsed => {
                    CancelCurrentLocation();
                });
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
                try
                {

                    if (IsExecuting)
                    {
                        CancelCurrentLocation();
                    }

                    if ( ( address.Street.IsNullOrEmpty() ) && (address.ZipCode.IsNullOrEmpty () ) && (address.AddressType != "place")  && (address.AddressType != "popular")) // This should only be true when using an address from a version smaller than 1.3                    
                    {
                        var a = this.GetService<IGeolocService>().SearchAddress(address.FullAddress, null , null );
                        if ( a.Count() > 0 )
                        {
                            address = a.First();
                        }
                    }

                    address.CopyTo(Model);

                    FirePropertyChanged(() => AddressLine1);
                    FirePropertyChanged(() => AddressLine2);
                    FirePropertyChanged(() => Model);


                    if (AddressChanged != null)
                    {
                        AddressChanged(userInitiated, EventArgs.Empty);
                    }

                }
                finally
                {


                    IsExecuting = false;
                }

            });
        }

        public void ClearAddress()
        {
            InvokeOnMainThread(() =>
            {
                var clearAddress = new Address();
                clearAddress.CopyTo(Model);
                FirePropertyChanged(() => AddressLine1);
                FirePropertyChanged(() => AddressLine2);
                FirePropertyChanged(() => Model);
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

        public IMvxCommand ClearPositionCommand
        {
            get
            {

                return GetCommand(ClearAddress);
            }
        }

        public IMvxCommand RequestCurrentLocationCommand {
            get {
                return new MvxRelayCommand (() =>
                {

                    CancelCurrentLocationCommand.Execute ();

                    if ( !_geolocator.IsServiceEnabled )
                    {
                        TinyIoCContainer.Current.Resolve<IMessageService>().ShowMessage ( TinyIoCContainer.Current.Resolve<IAppResource>().GetString ("LocationServiceErrorTitle"),TinyIoCContainer.Current.Resolve<IAppResource>().GetString ("LocationServiceErrorMessage") );
                        return ;
                    }
                    IsExecuting = true;
                    _cancellationToken = new CancellationTokenSource ();
                    _geolocator.GetPositionAsync (6000, 50, 2000, 2000, _cancellationToken.Token).ContinueWith (t =>
                    {
                        try
                        {
                            Logger.LogMessage("Request Location Command");
                            if (t.IsFaulted)
                            {
                                Logger.LogMessage("Request Location Command : FAULTED");
                                IsExecuting = false;
                            }
                            else if (t.IsCompleted && !t.IsCanceled)
                            {
                                Logger.LogMessage("Request Location Command :SUCCESS La {0}, Ln{1}", t.Result.Latitude, t.Result.Longitude);
                                ThreadPool.QueueUserWorkItem(pos => SearchAddressForCoordinate((Position)pos), t.Result);
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex);
                            IsExecuting = false;
                        }

                    }, _scheduler);
                });
            }

        }

        private void SearchAddressForCoordinate(Position p)
        {
            IsExecuting = true;
            TinyIoCContainer.Current.Resolve<ILogger>().LogMessage("Start Call SearchAddress : " + p.Latitude.ToString() + ", " + p.Longitude.ToString());

            var accountAddress = TinyIoCContainer.Current.Resolve<IAccountService>().FindInAccountAddresses(p.Latitude, p.Longitude);
            if (accountAddress != null)
            {
                TinyIoCContainer.Current.Resolve<ILogger>().LogMessage("Address found in account");
                SetAddress(accountAddress, false);
            }
            else
            {
                var address = TinyIoC.TinyIoCContainer.Current.Resolve<IGeolocService>().SearchAddress(p.Latitude, p.Longitude);
                TinyIoCContainer.Current.Resolve<ILogger>().LogMessage("Call SearchAddress finsihed, found {0} addresses", address.Count());
                if (address.Count() > 0)
                {
                    TinyIoCContainer.Current.Resolve<ILogger>().LogMessage(" found {0} addresses", address.Count());
                    SetAddress(address[0], false);
                }
                else
                {
                    TinyIoCContainer.Current.Resolve<ILogger>().LogMessage(" clear addresses");
                    ClearAddress();
                }
                TinyIoCContainer.Current.Resolve<ILogger>().LogMessage("Exiting SearchAddress thread");
            }

            IsExecuting = false;
        }

    }
}
