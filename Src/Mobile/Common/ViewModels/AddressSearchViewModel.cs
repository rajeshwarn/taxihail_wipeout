using System;
using Cirrious.MvvmCross.Commands;
using Cirrious.MvvmCross.Interfaces.Commands;
using Cirrious.MvvmCross.ViewModels;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Mobile.AppServices;
using System.Collections.Generic;
using Xamarin.Geolocation;
using System.Threading.Tasks;
using System.Linq;

using apcurium.MK.Common.Extensions;
using System.Threading;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Messages;
using TinyMessenger;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class AddressSearchViewModel : BaseViewModel
    {
        private IGoogleService _googleService;
        private IGeolocService _geolocService;
        private IBookingService _bookingService;
        private IAccountService _accountService;
        private IEnumerable<AddressViewModel> _addressViewModels;
        private Geolocator _geolocator;
        private TaskScheduler _scheduler = TaskScheduler.FromCurrentSynchronizationContext();
        private double _lat;
        private double _lg;
        private string _ownerId;
        public AddressSearchViewModel(string ownerId, string search, IGoogleService googleService, IGeolocService geolocService, IBookingService bookingService, IAccountService accountService, Geolocator geolocator)
        {
            _ownerId = ownerId;
            _googleService = googleService;
            _geolocService = geolocService;
            _bookingService = bookingService;
            _accountService = accountService;
            _addressViewModels = new List<AddressViewModel>();
            _geolocator = geolocator;
            _geolocator.GetPositionAsync(5000).ContinueWith(p =>
                {
                    if (p.IsCompleted)
                    {
                        _lat = p.Result.Latitude;
                        _lg = p.Result.Longitude;
                    }
                });
            SearchText = search;

            if (SearchText.HasValue())
            {
                SearchAddressCommand.Execute();
            }
        }

        public override void Load()
        {

        }


        public IMvxCommand GetPlacesCommand
        {
            get
            {
                return new MvxRelayCommand(() =>
                {
                    _geolocator.GetPositionAsync(3000).ContinueWith(t =>
                    {
                        if (t.IsFaulted)
                        {
                            Console.WriteLine("GetPosition : Faulted");
                        }
                        else if (t.IsCanceled)
                        {
                            Console.WriteLine("GetPosition : Cancelled");
                        }
                        else
                        {
                            var addresses = _googleService.GetNearbyPlaces(t.Result.Latitude, t.Result.Longitude);
                            AddressViewModels = addresses.Select(a => new AddressViewModel() { Address = a, ShowPlusSign = false, ShowRightArrow = false, IsFirst = a.Equals(addresses.First()), IsLast = a.Equals(addresses.Last()) }).ToList();
                        }

                    }, _scheduler);
                });
            }
        }

        private Thread _editThread;
        private MvxRelayCommand _searchAddressCommand;
        public IMvxCommand SearchAddressCommand
        {
            get
            {
                if (_searchAddressCommand == null)
                {
                    _searchAddressCommand = new MvxRelayCommand(() =>
                    {
                        Console.WriteLine("SearchAddressCommand: Start executing.");
                        if (_editThread != null && _editThread.IsAlive)
                        {
                            Console.WriteLine("SearchAddressCommand: Killing previous thread.");
                            _editThread.Abort();
                        }

                        _editThread = new Thread(() =>
                        {
                            Console.WriteLine("SearchAddressCommand: Starting new thread.");
                            Thread.Sleep(200);

                            var addresses = _geolocService.SearchAddress(SearchText, _lat, _lg);

                            AddressViewModels = addresses.Select(a => new AddressViewModel() { Address = a, ShowPlusSign = false, ShowRightArrow = false, IsFirst = a.Equals(addresses.First()), IsLast = a.Equals(addresses.Last()) }).ToList();
                            Console.WriteLine("SearchAddressCommand: Finishing executing command.");
                        });
                        _editThread.Start();
                    }, () => !SearchText.IsNullOrEmpty());
                }
                return _searchAddressCommand;
            }
        }

        
        public IMvxCommand GetContactsCommand
        {
            get
            {
                return new MvxRelayCommand(() =>
                   {
                       var addresses = _bookingService.GetAddressFromAddressBook();
                       AddressViewModels = addresses.Select(a => new AddressViewModel() { Address = a, ShowPlusSign = false, ShowRightArrow = false, IsFirst = a.Equals(addresses.First()), IsLast = a.Equals(addresses.Last()) }).ToList();

                   });

             
            }
        }

        
        public IMvxCommand GetFavoritesCommand
        {
            get
            {
                return new MvxRelayCommand(() =>
                    {
                        var addresses = _accountService.GetFavoriteAddresses();
                        AddressViewModels = addresses.Select(a => new AddressViewModel() { Address = a, ShowPlusSign = false, ShowRightArrow = false, IsFirst = a.Equals(addresses.First()), IsLast = a.Equals(addresses.Last()) }).ToList();
                    });               
            }
        }
        
        public IMvxCommand CloseViewCommand
        {
            get
            {
                return new MvxRelayCommand(() =>
                    {                        
                        RequestClose(this);
                    });                
            }
        }

        public IEnumerable<AddressViewModel> AddressViewModels
        {
            get { return _addressViewModels; }
            set
            {
                _addressViewModels = value;
                FirePropertyChanged(() => AddressViewModels);
            }
        }

        public IMvxCommand ResetCommand
        {
            get
            {
                return new MvxRelayCommand(() =>
                    {
                        AddressViewModels = new List<AddressViewModel>();
                    });
            }
        }


        public IMvxCommand RowSelectedCommand
        {
            get
            {
                return new MvxRelayCommand<AddressViewModel>(address =>
                    {
                        TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Publish(new AddressSelected(this, address.Address, _ownerId));
                        RequestClose(this);
                    });
            }
        }


        private string _searchText;
        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                SearchAddressCommand.Execute();
            }
        }



        public MvxRelayCommand PickupLocationChanged
        {
            get
            {
                return new MvxRelayCommand(() =>
                    {


                    });
            }
        }

    }
}

