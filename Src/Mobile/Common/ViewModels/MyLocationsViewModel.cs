using System;
using apcurium.MK.Booking.Mobile.ViewModels;
using System.Threading.Tasks;
using apcurium.MK.Common.Entity;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using apcurium.MK.Booking.Mobile.AppServices;
using TinyIoC;
using System.Linq;
using System.Threading;
using apcurium.MK.Common.Extensions;
using Cirrious.MvvmCross.Interfaces.Commands;
using Cirrious.MvvmCross.Commands;
using ServiceStack.Text;

namespace apcurium.MK.Booking.Mobile
{
    public class MyLocationsViewModel: BaseViewModel
    {
        private IAccountService _accountService;
        protected override void Initialize ()
        {
            base.Initialize ();
            _accountService = TinyIoCContainer.Current.Resolve<IAccountService>();
        }

        public override void OnViewLoaded ()
        {
            base.OnViewLoaded ();

            LoadAllAddresses();
        }

        private ObservableCollection<Address> _favoriteAddresses;
        public ObservableCollection<Address> FavoriteAddresses {
            get {
                return _favoriteAddresses;
            }
            set {
                if(value != _favoriteAddresses)
                {
                    _favoriteAddresses = value;
                    FirePropertyChanged("FavoriteAddresses");
                }
            }
        }

        private ObservableCollection<Address> _historyAddresses;
        public ObservableCollection<Address> HistoryAddresses {
            get {
                return _historyAddresses;
            }
            set {
                if(value != _historyAddresses)
                {
                    _historyAddresses = value;
                    FirePropertyChanged("HistoryAddresses");
                }
            }
        }

        private ObservableCollection<SectionAddressViewModel> _allAddresses;
        public ObservableCollection<SectionAddressViewModel> AllAddresses { 
            get {
                return _allAddresses;
            }
            set {
                if(value != _allAddresses) {
                    _allAddresses = value;
                    FirePropertyChanged("AllAddresses");
                }
            }
        }

        public IMvxCommand NavigateToLocationDetailPage{
            get{
                return new MvxRelayCommand<AddressViewModel>(a => {

                    if(a.Address.Id == Guid.Empty)
                    {
                        // New address
                        RequestNavigate<LocationDetailViewModel>();
                    } else {
                        RequestNavigate<LocationDetailViewModel>(new Dictionary<string,string>{
                            { "address", a.Address.ToJson() }
                        });
                    }
                });
            }
        }
        public Task LoadAllAddresses ()
        {
            var tasks = new [] {
                LoadFavoriteAddresses(),
                LoadHistoryAddresses()
            };
            return Task.Factory.ContinueWhenAll<AddressViewModel[]>(tasks, t => {

                var allAddresses = new ObservableCollection<SectionAddressViewModel>();
                if(t[0].Status == TaskStatus.RanToCompletion)
                {
                    allAddresses.Add(new SectionAddressViewModel{
                        Addresses = t[0].Result,
                        SectionTitle = Resources.GetString("FavoriteLocationsTitle")
                    });
                }

                if(t[1].Status == TaskStatus.RanToCompletion)
                {
                    allAddresses.Add(new SectionAddressViewModel{
                        Addresses = t[1].Result,
                        SectionTitle = Resources.GetString("LocationHistoryTitle")
                    });
                }
                AllAddresses = allAddresses;

            }, new CancellationTokenSource().Token, TaskContinuationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
        }
        

        private Task<AddressViewModel[]> LoadFavoriteAddresses()
        {
            return Task<AddressViewModel[]>.Factory.StartNew(()=>{
                var adrs = _accountService.GetFavoriteAddresses().ToList();
                adrs.Add (new Address
                {
                    FriendlyName=Resources.GetString("LocationAddFavoriteTitle"),
                    FullAddress = Resources.GetString("LocationAddFavoriteSubtitle"),
                });

                return adrs.Select(a => new AddressViewModel
                { 
                    Address = a,
                    IsAddNew =  a.Id.IsNullOrEmpty(),
                    ShowPlusSign = a.Id.IsNullOrEmpty(),
                    ShowRightArrow = !a.Id.IsNullOrEmpty(),
                    IsFirst = a.Equals(adrs.First()),
                    IsLast = a.Equals(adrs.Last())
                }).ToArray();
            }).HandleErrors();
        }

        private Task<AddressViewModel[]> LoadHistoryAddresses()
        {
            return Task<AddressViewModel[]>.Factory.StartNew(()=>{
                var adrs = _accountService.GetHistoryAddresses();
                return adrs.Select(a => new AddressViewModel
                { 
                    Address = a,
                    ShowPlusSign = a.Id.IsNullOrEmpty(),
                    ShowRightArrow = !a.Id.IsNullOrEmpty(),
                    IsFirst = a.Equals(adrs.First()),
                    IsLast = a.Equals(adrs.Last()) 
                }).ToArray();
            }).HandleErrors();
        }
    }
}

