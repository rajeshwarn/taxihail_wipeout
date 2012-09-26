using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if !IOS
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
#endif
using apcurium.MK.Booking.Api.Contract.Resources;
using Cirrious.MvvmCross.Commands;
using Cirrious.MvvmCross.Interfaces.Commands;
using Xamarin.Geolocation;
using System.Threading.Tasks;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Common.Extensions;
using System.Threading;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Mobile.Infrastructure;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class BookAddressViewModel : BaseViewModel
    {
        private Geolocator _geolocator;
        private CancellationTokenSource _cancellationToken;
        private TaskScheduler _scheduler = TaskScheduler.FromCurrentSynchronizationContext();
        private bool _isExecuting;
        private Func<Address> _getAddress;
        private Action<Address> _setAddress;
        private string _id;
        private string _searchingTitle;

        public event EventHandler AddressChanged;
        public BookAddressViewModel(Func<Address> getAddress, Action<Address> setAddress, Geolocator geolocator)
        {
            _getAddress = getAddress;
            _setAddress = setAddress;
            _id = Guid.NewGuid().ToString();
            _geolocator = geolocator;
            _searchingTitle = TinyIoC.TinyIoCContainer.Current.Resolve<IAppResource>().GetString("AddressSearchingText");
            TinyIoCContainer.Current.Resolve<TinyMessenger.ITinyMessengerHub>().Subscribe<AddressSelected>(OnAddressSelected, selected => selected.OwnerId == _id);
        }

        public string Title { get; set; }

        public string EmptyAddressPlaceholder { get; set; }

        public string Display
        {
            get
            {
                FirePropertyChanged(() => IsPlaceHolder);
                if (IsExecuting)
                {
                    return _searchingTitle;
                }
                if (Model.FullAddress.HasValue())
                {
                    return Model.FullAddress;
                }
                else
                {
                    return EmptyAddressPlaceholder;
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

        private Task _searchTask;

        public IMvxCommand SearchCommand
        {
            get
            {


                return new MvxRelayCommand<Address>(coordinate =>
                {
                    CancelCurrentLocationCommand.Execute();
                                      
                    _cancellationToken = new CancellationTokenSource();

                    var token = _cancellationToken.Token;
                    _searchTask = Task.Factory.StartNew(() =>
                    {
                        IsExecuting = true;
                        return TinyIoC.TinyIoCContainer.Current.Resolve<IGeolocService>().SearchAddress(coordinate.Latitude, coordinate.Longitude);

                    }, token)
                    .ContinueWith(t =>
                        {
                            if ( (t.IsCompleted) && ( !t.IsCanceled ) )
                            {
                                RequestMainThreadAction(() =>
                                {
                                    if (t.Result.Count() > 0)
                                    {
                                        SetAddress(t.Result[0], true);
                                    }
                                    else
                                    {
                                        ClearAddress();
                                    }
                                });
                            }
                            IsExecuting = false;
                        });

                });
            }
        }

        public IMvxCommand PickAddress
        {
            get
            {
                return new MvxRelayCommand(() =>
                {
                    CancelCurrentLocationCommand.Execute();
                    RequestNavigate<AddressSearchViewModel>(new { search = Model.FullAddress, ownerId = _id });
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
                FirePropertyChanged(() => Display);
            }
        }


        public IMvxCommand CancelCurrentLocationCommand
        {
            get
            {
                return new MvxRelayCommand(() =>
                    {
                        IsExecuting = false;
                        if (_cancellationToken != null)
                        {
                            _cancellationToken.Cancel();
                            _cancellationToken.Dispose();
                            _cancellationToken = null;
                        }
                    });
            }
        }


        public void SetAddress(Address address, bool userInitiated)
        {
            Model.FullAddress = address.FullAddress;
            Model.Longitude = address.Longitude;
            Model.Latitude = address.Latitude;

            FirePropertyChanged(() => Display);
            FirePropertyChanged(() => Model);


            if (AddressChanged != null)
            {
                AddressChanged(userInitiated, EventArgs.Empty);
            }
        }


        public void ClearAddress()
        {
            Model.FullAddress = null;
            Model.Longitude = 0;
            Model.Latitude = 0;
            FirePropertyChanged(() => Display);
            FirePropertyChanged(() => Model);
        }



        public IMvxCommand ClearPositionCommand
        {
            get
            {

                return new MvxRelayCommand(() =>
                {
                    ClearAddress();

                });
            }
        }
        public IMvxCommand RequestCurrentLocationCommand
        {
            get
            {
                return new MvxRelayCommand(() =>
                {
                    CancelCurrentLocationCommand.Execute();
                    IsExecuting = true;
                    _cancellationToken = new CancellationTokenSource();
                    _geolocator.GetPositionAsync(30000, _cancellationToken.Token).ContinueWith(t =>
                    {
                        try
                        {
                            if (t.IsFaulted)
                            {
                                // PositionStatus.Text = ((GeolocationException)t.Exception.InnerException).Error.ToString();
                            }
                            else if ( t.IsCompleted && !t.IsCanceled )
                            {
                                var address = TinyIoC.TinyIoCContainer.Current.Resolve<IGeolocService>().SearchAddress(t.Result.Latitude, t.Result.Longitude);
                                if (address.Count() > 0)
                                {
                                    SetAddress(address[0], false);
                                }
                                else
                                {
                                    ClearAddress();
                                }
                            }
                        }
                        finally
                        {
                            IsExecuting = false;
                        }

                    }, _scheduler);

                });
            }
        }

    }
}