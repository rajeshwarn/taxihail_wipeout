using System.Linq;
using apcurium.MK.Booking.Mobile.AppServices;

namespace apcurium.MK.Booking.Mobile.ViewModels.SearchAddress
{
    public class AddressSearchByContactViewModel : AddressSearchBaseViewModel
    {
        private readonly IBookingService _bookingService;

        public AddressSearchByContactViewModel(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        protected override System.Collections.Generic.IEnumerable<AddressViewModel> SearchAddresses()
        {
            var addresses = _bookingService.GetAddressFromAddressBook();
            return addresses.Select(a => new AddressViewModel() { Address = a, ShowPlusSign = false, ShowRightArrow = false, IsFirst = a.Equals(addresses.First()), IsLast = a.Equals(addresses.Last()) }).ToList();
        }
    }
}