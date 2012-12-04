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

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class BookAddressViewModel : BaseViewModel,
        IMvxServiceConsumer<ILocationService>,
        IMvxServiceConsumer<IAccountService>,
        IMvxServiceConsumer<IGeolocService>
    {
        private ILocationService _geolocator;
        private CancellationTokenSource _cancellationToken;
        private TaskScheduler _scheduler = TaskScheduler.FromCurrentSynchronizationContext ();
        private bool _isExecuting;
        private Func<Address> _getAddress;
        private Action<Address> _setAddress;
        private string _id;
        private string _searchingTitle;

        public event EventHandler AddressChanged;

        public BookAddressViewModel (Func<Address> getAddress, Action<Address> setAddress, ILocationService geolocator)
        {
            _getAddress = getAddress;
            _setAddress = setAddress;
            _id = Guid.NewGuid ().ToString ();
            _geolocator = geolocator;
            _searchingTitle = Resources.GetString ("AddressSearchingText");
            MessengerHub.Subscribe<AddressSelected> (OnAddressSelected, selected => selected.OwnerId == _id);
        }

        public string Title { get; set; }

        public string EmptyAddressPlaceholder { get; set; }

        public string Display {
            get {
                FirePropertyChanged (() => IsPlaceHolder);
                if (IsExecuting) {
                    return _searchingTitle;
                }
                if (Model.BookAddress.HasValue ()) {
                    return Model.BookAddress;
                } else {
                    return EmptyAddressPlaceholder;
                }
            }
        }

        public bool IsPlaceHolder {
            get {
                return Model.FullAddress.IsNullOrEmpty ();
            }
        }

        public Address Model { get { return _getAddress (); } set { _setAddress (value); } }

        public IMvxCommand SearchCommand {
            get {
                return new MvxRelayCommand<Address> (coordinate =>
                {
                    CancelCurrentLocationCommand.Execute ();

                    _cancellationToken = new CancellationTokenSource ();

                    var token = _cancellationToken.Token;
                    var task = Task.Factory.SafeStartNew (() =>
                    {
                        if (!token.IsCancellationRequested) {
                            IsExecuting = true;
                            var accountAddress = this.GetService<IAccountService> ().FindInAccountAddresses (coordinate.Latitude, coordinate.Longitude);
                            if (accountAddress != null) 
                            {
                                return new Address[] { accountAddress};
                            }
                            else
                            {
                                return this.GetService<IGeolocService>().SearchAddress (coordinate.Latitude, coordinate.Longitude).ToArray ();
                            }
                        }
                        return null;

                    }, token);

                    task.ContinueWith (t =>
                    { 
						InvokeOnMainThread(() => {
							if (t.Result != null && t.Result.Any ()) {
								var address = t.Result[0];
                                Console.WriteLine ( address.FullAddress );
								// Replace result coordinates  by search coordinates (= user position)
								address.Latitude = coordinate.Latitude;
								address.Longitude = coordinate.Longitude;
								SetAddress (address, true);
							} else {
                                TinyIoCContainer.Current.Resolve<ILogger>().LogMessage( "No address found for coordinate : La : {0} , Lg: {1} ", coordinate.Latitude , coordinate.Longitude );
								ClearAddress ();
							}
						});
                        
                    }, TaskContinuationOptions.OnlyOnRanToCompletion);

                });
            }
        }

        public IMvxCommand PickAddress {
            get {
                return new MvxRelayCommand (() =>
                {
                    CancelCurrentLocationCommand.Execute ();
                    if(Settings.StreetNumberScreenEnabled)
                    {
                        RequestNavigate<BookStreetNumberViewModel> (new { address = JsonSerializer.SerializeToString<Address>(Model), ownerId = _id });
                    }else{
                        RequestNavigate<AddressSearchViewModel> (new { search = Model.FullAddress, ownerId = _id });
                    }

                });
            }
        }

        private void OnAddressSelected (AddressSelected selected)
        {
            SetAddress (selected.Content, true);
        }

        public bool IsExecuting {
            get { return _isExecuting; }
            set {
                _isExecuting = value;
                FirePropertyChanged (() => IsExecuting);
                FirePropertyChanged (() => Display);
            }
        }

        public IMvxCommand CancelCurrentLocationCommand {
            get {
                return new MvxRelayCommand (() =>
                {
                    IsExecuting = false;
                    if ((_cancellationToken != null) && (_cancellationToken.Token.CanBeCanceled)) {
                        _cancellationToken.Cancel ();
                        _cancellationToken = null;
                    }
                });
            }
        }

        public void SetAddress (Address address, bool userInitiated)
        {
            InvokeOnMainThread (() =>
            {
                IsExecuting = true;
                try {


                    if (IsExecuting) {
                        CancelCurrentLocationCommand.Execute ();
                    }

                    address.Copy(Model);

                    FirePropertyChanged (() => Display);
                    FirePropertyChanged (() => Model);


                    if (AddressChanged != null) {
                        AddressChanged (userInitiated, EventArgs.Empty);
                    }

                } finally {


                    IsExecuting = false;
                }

            });
        }

        public void ClearAddress ()
        {
            InvokeOnMainThread (() =>
            {

                Model.FullAddress = null;
                Model.Longitude = 0;
                Model.Latitude = 0;
                FirePropertyChanged (() => Display);
                FirePropertyChanged (() => Model);
                IsExecuting = false;
            });
        }

        public IMvxCommand ClearPositionCommand {
            get {

                return new MvxRelayCommand (ClearAddress);
            }
        }

        public IMvxCommand RequestCurrentLocationCommand {
            get {
                return new MvxRelayCommand (() =>
                {

                    CancelCurrentLocationCommand.Execute ();
                    IsExecuting = true;
                    _cancellationToken = new CancellationTokenSource ();
                    _geolocator.GetPositionAsync (5000, 50, 2000, 2000, _cancellationToken.Token).ContinueWith (t =>
                    {
                        try {
                            Logger.LogMessage ("Request Location Command");
                            if (t.IsFaulted) {
                                Logger.LogMessage ("Request Location Command : FAULTED");
                                IsExecuting = false;
                            } else if (t.IsCompleted && !t.IsCanceled) {
                                Logger.LogMessage ("Request Location Command :SUCCESS La {0}, Ln{1}", t.Result.Latitude, t.Result.Longitude);
                                ThreadPool.QueueUserWorkItem (pos => SearchAddressForCoordinate ((Position)pos), t.Result);
                            }
                        } catch (Exception ex) {
                            Logger.LogError (ex);
                            IsExecuting = false;
                        }

                    }, _scheduler);
                });
            }

        }

        private void SearchAddressForCoordinate (Position p)
        {
            IsExecuting = true;
            TinyIoCContainer.Current.Resolve<ILogger> ().LogMessage ("Start Call SearchAddress : " + p.Latitude.ToString () + ", " + p.Longitude.ToString ());

            var accountAddress = TinyIoCContainer.Current.Resolve<IAccountService> ().FindInAccountAddresses (p.Latitude, p.Longitude);
            if (accountAddress != null) 
            {
                TinyIoCContainer.Current.Resolve<ILogger> ().LogMessage ("Address found in account");
                SetAddress (accountAddress, false);
            } 
            else 
            {
                var address = TinyIoC.TinyIoCContainer.Current.Resolve<IGeolocService> ().SearchAddress (p.Latitude, p.Longitude);
                TinyIoCContainer.Current.Resolve<ILogger> ().LogMessage ("Call SearchAddress finsihed, found {0} addresses", address.Count ());
                if (address.Count () > 0) {
                    TinyIoCContainer.Current.Resolve<ILogger> ().LogMessage (" found {0} addresses", address.Count ());
                    SetAddress (address [0], false);
                } else {
                    TinyIoCContainer.Current.Resolve<ILogger> ().LogMessage (" clear addresses");
                    ClearAddress ();
                }
                TinyIoCContainer.Current.Resolve<ILogger> ().LogMessage ("Exiting SearchAddress thread");
            }

            IsExecuting = false;
        }

    }
}