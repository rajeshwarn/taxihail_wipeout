//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Xamarin.Contacts;
//using apcurium.Framework.Extensions;
//using apcurium.MK.Booking.Mobile.AppServices;
//
//namespace apcurium.MK.Booking.Mobile.ViewModels.SearchAddress
//{
//    public class AddressSearchByContactViewModel : AddressSearchBaseViewModel
//    {
//        private readonly IBookingService _bookingService;
//
//        public AddressSearchByContactViewModel(IBookingService bookingService)
//        {
//            _bookingService = bookingService;
//        }
//
//        public override Task<IEnumerable<AddressViewModel>> OnSearchExecute(System.Threading.CancellationToken cancellationToken)
//        {
//            var taskLoading = _bookingService.LoadContacts();
//            return taskLoading.ContinueWith(t => SearchAddresses(), cancellationToken);
//        }
//
//        protected override IEnumerable<AddressViewModel> SearchAddresses()
//        {
//            Predicate<Contact> predicate = c => true;
//            if(Criteria.HasValue())
//            {
//                predicate = c => c.DisplayName != null && c.DisplayName.ToLowerInvariant().Contains(Criteria);
//            }
//            var addresses = _bookingService.GetAddressFromAddressBook(predicate);
//            return addresses.Select(a => new AddressViewModel { Address = a, ShowPlusSign = false, ShowRightArrow = false, IsFirst = a.Equals(addresses.First()), IsLast = a.Equals(addresses.Last()) }).ToList();
//        }
//
//
//    }
//}