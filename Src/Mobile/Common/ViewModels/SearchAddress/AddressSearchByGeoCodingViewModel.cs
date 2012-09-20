using System.Collections.Generic;
using System.Linq;
using TinyIoC;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Mobile.ViewModels.SearchAddress
{
    public class AddressSearchByGeoCodingViewModel : AddressSearchBaseViewModel
    {
       private readonly IGeolocService _geolocService;

       public AddressSearchByGeoCodingViewModel(IGeolocService geolocService)
        {
            _geolocService = geolocService;
        }

        protected override IEnumerable<AddressViewModel> SearchAddresses()
        {
            var position = TinyIoCContainer.Current.Resolve<IUserPositionService>().LastKnownPosition;
            var addresses = _geolocService.SearchAddress(Criteria, position.Latitude, position.Longitude);
            return addresses.Select(a => new AddressViewModel() { Address = a, ShowPlusSign = false, ShowRightArrow = false, IsFirst = a.Equals(addresses.First()), IsLast = a.Equals(addresses.Last()) }).ToList();
        }
    }
}
