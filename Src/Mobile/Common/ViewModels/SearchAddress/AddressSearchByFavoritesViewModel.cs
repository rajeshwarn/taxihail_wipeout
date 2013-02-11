//using System;
//using System.Collections.Generic;
//using System.Linq;
//using apcurium.Framework.Extensions;
//using apcurium.MK.Booking.Mobile.AppServices;
//using apcurium.MK.Common.Entity;
//
//namespace apcurium.MK.Booking.Mobile.ViewModels.SearchAddress
//{
//    public class AddressSearchByFavoritesViewModel : AddressSearchBaseViewModel
//    {
//        private readonly IAccountService _accountService;
//
//
//        public AddressSearchByFavoritesViewModel(IAccountService accountService)
//        {
//            _accountService = accountService;
//        }
//
//        protected override IEnumerable<AddressViewModel> SearchAddresses()
//       {
//           var addresses = _accountService.GetFavoriteAddresses();
//           var historicAddresses = _accountService.GetHistoryAddresses();
//
//           Func<Address, bool> predicate = c => true;
//           if (Criteria.HasValue())
//           {
//               predicate = x => (x.FriendlyName != null && x.FriendlyName.ToLowerInvariant().Contains(Criteria)) || (x.FullAddress != null && x.FullAddress.ToLowerInvariant().Contains(Criteria));
//           }
//            var a1 = addresses.Where(predicate).Select(a => new AddressViewModel { Address = a, ShowPlusSign = false, ShowFavoriteIcon = true, ShowRightArrow = false, IsFirst = a.Equals(addresses.First()), IsLast = a.Equals(addresses.Last()) && (historicAddresses.Count() ==0) });
//           var a2 = historicAddresses.Where(predicate).Select(a => new AddressViewModel { Address = a, ShowPlusSign = false, ShowHistoryIcon=true, ShowRightArrow = false, IsFirst = a.Equals(historicAddresses.First()) && (addresses.Count() ==0), IsLast = a.Equals(historicAddresses.Last()) });
//           var r =  a1.Concat(a2).ToArray(); 
//           return r;
//       }
//    }
//}