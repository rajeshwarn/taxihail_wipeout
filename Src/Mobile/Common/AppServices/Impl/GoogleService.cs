using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Maps;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
	public class GoogleService : BaseService, IGoogleService
	{
        public Address GetPlaceDetail(string name, string reference)
        {

            Address result = null;
            UseServiceClient<IPlaces>(service =>
            {
                result = service.GetPlaceDetail( name, reference);
            });

            return result;
        }
        public Address[] GetNearbyPlaces(double? latitude, double? longitude, string name = null, int? radius = null)
		{
			Address[] places = null;
			//UseServiceClient<NearbyPlacesClient>(service =>
              
            UseServiceClient<IPlaces>(service =>
			{
				places = service.SearchPlaces( name, latitude, longitude, radius );                   
			});

			return places;
		}
	}
}

