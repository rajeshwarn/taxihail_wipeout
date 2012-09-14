using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
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
using apcurium.MK.Common;

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

        public BookAddressViewModel(Func<Address> getAddress , Action<Address> setAddress, Geolocator geolocator)
        {
            _getAddress = getAddress;
            _setAddress = setAddress;
            _id = Guid.NewGuid().ToString();            
            _geolocator = geolocator;
            
            TinyIoCContainer.Current.Resolve<TinyMessenger.ITinyMessengerHub>().Subscribe<AddressSelected>(OnAddressSelected, selected => selected.OwnerId == _id);
        }

        public string Title { get; set; }
        
        public string EmptyAddressPlaceholder { get; set; }

        public string Display
        {
            get
            {
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

        public Address Model { get { return _getAddress(); } set { _setAddress(value); } }        

        public IMvxCommand SearchCommand
        {
            get
            {
                return new MvxRelayCommand<Address>(coordinate =>
                {
                    var address = TinyIoC.TinyIoCContainer.Current.Resolve<IGeolocService>().SearchAddress(coordinate.Latitude, coordinate.Longitude);
                    if (address.Count() > 0)
                    {
                        SetAddress(address[0]);
                    }
                    else
                    {
                        ClearAddress();
                    }
                    //Console.WriteLine("S");
                });
            }
        }

        public IMvxCommand PickAddress
        {
            get
            {
                return new MvxRelayCommand(() =>
                {                    
                    //RequestNavigate<AddressSearchViewModel>(new { search  = Params.Get<string>( Model.StreetNumber, Model.Street ).Where( p => p.HasValue() ).JoinBy( " " ), ownerId = _id});                    
                    RequestNavigate<AddressSearchViewModel>(new { search = Model.FullAddress , ownerId = _id });                    
                });
            }
        }

        private void OnAddressSelected(AddressSelected selected)
        {
            SetAddress(selected.Content);
        }

        public bool IsExecuting
        {
            get { return _isExecuting; }
            set 
            { 
                _isExecuting = value;
                FirePropertyChanged(() => IsExecuting);
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
                        }
                    });
            }
        }

        public void SetAddress(Address address)
        {
            Model.FullAddress = address.FullAddress;
            Model.Longitude = address.Longitude;
            Model.Latitude = address.Latitude;

            FirePropertyChanged(() => Display);
            FirePropertyChanged(() => Model);
        }


        public void ClearAddress()
        {
            Model.FullAddress = null;
            Model.Longitude = 0;
            Model.Latitude = 0;
            FirePropertyChanged(() => Display);
            FirePropertyChanged(() => Model);
        }

        public IMvxCommand RequestCurrentLocationCommand
        {
            get
            {
                
                return new MvxRelayCommand(() =>
                {
                    IsExecuting = true;
                    _cancellationToken = new CancellationTokenSource();
                    _geolocator.GetPositionAsync(10000, _cancellationToken.Token ).ContinueWith(t =>
                    {
                        try
                        {
                            if (t.IsFaulted)
                            {
                                // PositionStatus.Text = ((GeolocationException)t.Exception.InnerException).Error.ToString();
                            }
                            else
                            {                                
                                var address = TinyIoC.TinyIoCContainer.Current.Resolve<IGeolocService>().SearchAddress(t.Result.Latitude, t.Result.Longitude);
                                if (address.Count() > 0)
                                {
                                    SetAddress(address[0]);                                    
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