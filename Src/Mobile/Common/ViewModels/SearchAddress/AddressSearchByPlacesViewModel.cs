using System;
using System.Collections.Generic;
using System.Linq;
using TinyIoC;
using apcurium.Framework.Extensions;
using apcurium.MK.Booking.Api.Contract.Resources;
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
           Func<Address, bool> predicate = c => true;
           if (Criteria.HasValue())
           {
               predicate = x => (x.FriendlyName != null && x.FriendlyName.ToLowerInvariant().Contains(Criteria)) || (x.FullAddress != null && x.FullAddress.ToLowerInvariant().Contains(Criteria));
           }
           var position = TinyIoCContainer.Current.Resolve<IUserPositionService>().LastKnownPosition;
           var fullAddresses = _googleService.GetNearbyPlaces(position.Latitude, position.Longitude);
           var addresses = fullAddresses.Where(predicate).ToList();
           return addresses.Select(a => new AddressViewModel() { Address = a, ShowPlusSign = false, ShowRightArrow = false, IsFirst = a.Equals(addresses.First()), IsLast = a.Equals(addresses.Last()) }).ToList();
       }
    }
}