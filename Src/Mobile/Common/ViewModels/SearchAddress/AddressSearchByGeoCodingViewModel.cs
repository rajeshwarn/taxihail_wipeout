using System.Collections.Generic;
using System.Linq;
using TinyIoC;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Diagnostic;

namespace apcurium.MK.Booking.Mobile.ViewModels.SearchAddress
{
    public class AddressSearchByGeoCodingViewModel : AddressSearchBaseViewModel
    {
       private readonly IGeolocService _geolocService;

       public AddressSearchByGeoCodingViewModel(IGeolocService geolocService)
        {
            _geolocService = geolocService;
        }

        protected override IEnumerable<AddressViewModel> SearchAddresses ()
		{
            TinyIoCContainer.Current.Resolve<ILogger>().LogMessage( "Starting SearchAddresses : " + Criteria.ToSafeString()   );
			var position = TinyIoCContainer.Current.Resolve<ILocationService> ().LastKnownPosition;

            var addresses = new apcurium.MK.Common.Entity.Address[0];

            if (position == null)
            {
                TinyIoCContainer.Current.Resolve<ILogger>().LogMessage( "No Position SearchAddresses : " + Criteria.ToSafeString()   );
                addresses = _geolocService.SearchAddress(Criteria);                
            }
            else
            {
                TinyIoCContainer.Current.Resolve<ILogger>().LogMessage( "Position SearchAddresses : " + Criteria.ToSafeString()   );
                addresses = _geolocService.SearchAddress(Criteria, position.Latitude, position.Longitude);
            }
            return addresses.Select(a => new AddressViewModel() { Address = a, ShowPlusSign = false, ShowRightArrow = false, IsFirst = a.Equals(addresses.First()), IsLast = a.Equals(addresses.Last()) }).ToList();
        }
    }
}
