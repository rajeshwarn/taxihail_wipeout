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

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class BookAddressViewModel : BaseViewModel
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

        public BookAddressViewModel(Func<Address> getAddress, Action<Address> setAddress, ILocationService geolocator)
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
                        if (!token.IsCancellationRequested)
                        {
                            IsExecuting = true;
                            return TinyIoC.TinyIoCContainer.Current.Resolve<IGeolocService>().SearchAddress(coordinate.Latitude, coordinate.Longitude);
                        }
                        return null;

                    }, token).ContinueWith(t =>
                    {
                        if (t.IsCompleted && !t.IsCanceled && !t.IsFaulted)
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
                    }, token);

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
                    if ((_cancellationToken != null) && (_cancellationToken.Token.CanBeCanceled))
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
            if (IsExecuting)
            {
                CancelCurrentLocationCommand.Execute();
            }
            Model.FullAddress = address.FullAddress;
            Model.Longitude = address.Longitude;
            Model.Latitude = address.Latitude;
            Model.Apartment = address.Apartment;
            Model.RingCode = address.RingCode;
            Model.BuildingName = address.BuildingName;

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
                    _geolocator.GetPositionAsync(30000, 200, _cancellationToken.Token).ContinueWith(t =>
                    {
                        try
                        {
                            if (t.IsFaulted)
                            {
                                IsExecuting = false;                                
                            }
                            else if (t.IsCompleted && !t.IsCanceled)
                            {
                                ThreadPool.QueueUserWorkItem(pos => SearchAddressForCoordinate((Position)pos), t.Result);

                            }
                        }
                        catch
                        {
                            IsExecuting = false;
                        }
                        finally
                        {
                            
                        }

                    }, _scheduler);

                });
            }

        }

        private void SearchAddressForCoordinate(Position p)
        {
            Console.WriteLine("Start Call SearchAddress : " + p.Latitude.ToString() + ", " + p.Longitude.ToString());
            var address = TinyIoC.TinyIoCContainer.Current.Resolve<IGeolocService>().SearchAddress(p.Latitude, p.Longitude);
            Console.WriteLine("Call SearchAddress finsihed");
            if (address.Count() > 0)
            {
                SetAddress(address[0], false);
            }
            else
            {
                ClearAddress();
            }
            Console.WriteLine("Exiting SearchAddress thread");
            IsExecuting = false;
        }



    }
}