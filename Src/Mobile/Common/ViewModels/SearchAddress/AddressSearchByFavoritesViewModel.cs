using System.Collections.Generic;
using System.Linq;
using TinyIoC;
using apcurium.MK.Booking.Mobile.AppServices;

namespace apcurium.MK.Booking.Mobile.ViewModels.SearchAddress
{
    public class AddressSearchByFavoritesViewModel : AddressSearchBaseViewModel
    {
        private readonly IAccountService _accountService;


        public AddressSearchByFavoritesViewModel(IAccountService accountService)
        {
            _accountService = accountService;
        }

        protected override IEnumerable<AddressViewModel> SearchAddresses()
       {
           var addresses = _accountService.GetFavoriteAddresses();
           var historicAddresses = _accountService.GetHistoryAddresses();

           var a1 = addresses.Select(a => new AddressViewModel { Address = a, ShowPlusSign = false, ShowRightArrow = false, IsFirst = a.Equals(addresses.First()), IsLast = a.Equals(addresses.Last()) });
           var a2 = historicAddresses.Select(a => new AddressViewModel { Address = a, ShowPlusSign = false, ShowRightArrow = false, IsFirst = a.Equals(historicAddresses.First()), IsLast = a.Equals(historicAddresses.Last()) });
           var r =  a1.Concat(a2).ToArray(); //   addresses.Concat(historicAddresses).Select(a => new AddressViewModel { Address = a, ShowPlusSign = false, ShowRightArrow = false, IsFirst = a.Equals(addresses.First()), IsLast = a.Equals(addresses.Last()) }).ToList();
           return r;
       }
    }
}