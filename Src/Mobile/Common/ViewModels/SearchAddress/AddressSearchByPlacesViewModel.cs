using System.Collections.Generic;
using System.Linq;
using TinyIoC;
using apcurium.MK.Booking.Mobile.AppServices;

namespace apcurium.MK.Booking.Mobile.ViewModels.SearchAddress
{
    public class AddressSearchByPlacesViewModel : AddressSearchBaseViewModel
    {
        private IGoogleService _googleService;

       public AddressSearchByPlacesViewModel(IGoogleService googleService)
        {
            _googleService = googleService;
        }

       protected override IEnumerable<AddressViewModel> SearchAddresses()
       {
           var position = TinyIoCContainer.Current.Resolve<IUserPositionService>().LastKnownPosition;
           var addresses = _googleService.GetNearbyPlaces(position.Latitude, position.Longitude);
           return addresses.Select(a => new AddressViewModel() { Address = a, ShowPlusSign = false, ShowRightArrow = false, IsFirst = a.Equals(addresses.First()), IsLast = a.Equals(addresses.Last()) }).ToList();
       }
    }
}