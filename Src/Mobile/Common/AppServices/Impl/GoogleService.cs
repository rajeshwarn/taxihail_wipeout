using apcurium.MK.Booking.Maps;
using apcurium.MK.Common.Entity;
using TinyIoC;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
	public class GoogleService : BaseService, IGoogleService
	{
        public Address GetPlaceDetail(string name, string reference)
        {
			var result = TinyIoCContainer.Current.Resolve<IPlaces>().GetPlaceDetail( name, reference);
            return result;
        }
        public Address[] GetNearbyPlaces(double? latitude, double? longitude, string name = null, int? radius = null)
		{
			var places = TinyIoCContainer.Current.Resolve<IPlaces>().SearchPlaces( name, latitude, longitude, radius);
			return places;
		}
	}
}

