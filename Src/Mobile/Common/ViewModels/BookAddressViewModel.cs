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

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class BookAddressViewModel : BaseViewModel
    {
        private Geolocator _geolocator;
        private CancellationTokenSource _cancellationToken;
        private TaskScheduler _scheduler = TaskScheduler.FromCurrentSynchronizationContext();
        private bool _isExecuting;

        private string _id;

        public BookAddressViewModel(Address address, Geolocator geolocator)
        {
            SearchCoordinate = new CoordinateViewModel();
            _id = Guid.NewGuid().ToString();            
            _geolocator = geolocator;
            Model = address;
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
            

        public Address Model { get; private set; }

        public CoordinateViewModel SearchCoordinate { get; set; }

        public IMvxCommand SearchCommand
        {
            get
            {
                return new MvxRelayCommand(() =>
                {
                    Console.WriteLine("S");
                });
            }
        }

        public IMvxCommand PickAddress
        {
            get
            {
                return new MvxRelayCommand(() =>
                {                    
                    RequestNavigate<AddressSearchViewModel>(new { ownerId = _id});                    
                });
            }
        }

        private void OnAddressSelected(AddressSelected selected)
        {
            Model.FullAddress = selected.Content.FullAddress;
            FirePropertyChanged(() => Display);
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


        //public CoordinateViewModel CoordinateViewModel
        //{
        //    get
        //    {
        //        return new CoordinateViewModel(Model.Latitude, Model.Longitude);
        //    }
        //}


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

                                //                                PositionStatus.Text = ((GeolocationException)t.
                                //                                                       Exception.InnerException).Error.ToString();
                            }
                            else
                            {
                                Console.WriteLine(t.Result.Timestamp.ToString("G"));
                                Console.WriteLine(t.Result.Latitude.ToString("N4"));
                                Console.WriteLine(t.Result.Longitude.ToString("N4"));

                                var address = TinyIoC.TinyIoCContainer.Current.Resolve<IGeolocService>().SearchAddress(t.Result.Latitude, t.Result.Longitude);
                                if (address.Count() > 0)
                                {
                                    this.Model.FullAddress = address[0].FullAddress;
                                    this.Model.Longitude = address[0].Longitude;
                                    this.Model.Latitude = address[0].Latitude;
                                }
                                else
                                {
                                    this.Model.FullAddress = null;
                                    this.Model.Longitude = 0;
                                    this.Model.Latitude = 0;
                                }
                                FirePropertyChanged(() => Display);
                                FirePropertyChanged(() => Model);

                                
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