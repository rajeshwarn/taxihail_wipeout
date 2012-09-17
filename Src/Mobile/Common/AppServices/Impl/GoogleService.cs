using System;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
	public class GoogleService : BaseService, IGoogleService
	{
		public GoogleService ()
		{

		}
        public Address GetPlaceDetail(string reference)
        {

            Address result = null;
            UseServiceClient<PlaceDetailServiceClient>(service =>
            {
                result = service.GetPlaceDetail(reference);
            });

            return result;
        }
		public Address[] GetNearbyPlaces(double? latitude, double? longitude, int? radius)
		{
			Address[] places = null;
			UseServiceClient<NearbyPlacesClient>(service =>
			{
				places = service.GetNearbyPlaces( latitude, longitude, radius );                   
			});

			return places;
		}

		public Address[] GetNearbyPlaces(double? latitude, double? longitude)
		{
			Address[] places = null;
			UseServiceClient<NearbyPlacesClient>(service =>
			{
				places = service.GetNearbyPlaces( latitude, longitude );                   
			});

			return places;
		}
	}
}

