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

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class BookAddressViewModel : BaseViewModel
    {
        private Geolocator _geolocator;
        private TaskScheduler _scheduler = TaskScheduler.FromCurrentSynchronizationContext();

        private string _id;

        public BookAddressViewModel(Address address, Geolocator geolocator)
        {
            
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

        public IMvxCommand PickAddress
        {
            get
            {
                return new MvxRelayCommand(() =>
                {
                    //Pickup = new Address { FriendlyName = Guid.NewGuid().ToString(), FullAddress = Guid.NewGuid().ToString() };
                    RequestNavigate<AddressSearchViewModel>(new { ownerId = _id});
                    //TinyIoCContainer.Current.Resolve<INavigationService>().Navigate<AddressSearchViewModel>( "apcurium.MK.Booking.Mobile.Client.AddressSearchView" );
                });
            }
        }

        private void OnAddressSelected(AddressSelected selected)
        {
            Model.FullAddress = selected.Content.FullAddress;
            FirePropertyChanged(() => Display);
        }

        public IMvxCommand RequestCurrentLocationCommand
        {
            get
            {
                return new MvxRelayCommand(() =>
                {
                    _geolocator.GetPositionAsync(3000).ContinueWith(t =>
                    {
                        Console.WriteLine(Title);
                        if (t.IsFaulted)
                        {
                            //                                PositionStatus.Text = ((GeolocationException)t.
                            //                                                       Exception.InnerException).Error.ToString();
                        }
                        else if (t.IsCanceled)
                        {
                            //                                PositionStatus.Text = "Canceled";
                        }
                        else
                        {
                            Console.WriteLine(t.Result.Timestamp.ToString("G"));
                            Console.WriteLine(t.Result.Latitude.ToString("N4"));
                            Console.WriteLine(t.Result.Longitude.ToString("N4"));
                        }

                    }, _scheduler);

                });
            }
        }
    }
}