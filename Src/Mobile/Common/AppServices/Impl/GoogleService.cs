using System;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Maps;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
	public class GoogleService : BaseService, IGoogleService
	{
		public GoogleService ()
		{

		}
        public Address GetPlaceDetail(string name, string reference)
        {

            Address result = null;
            //UseServiceClient<PlaceDetailServiceClient>(service =>
            UseServiceClient<IPlaces>(service =>
            {
                result = service.GetPlaceDetail( name, reference);
            });

            return result;
        }
		public Address[] GetNearbyPlaces(double? latitude, double? longitude, int? radius = null)
		{
			Address[] places = null;
			//UseServiceClient<NearbyPlacesClient>(service =>
              
            UseServiceClient<IPlaces>(service =>
			{
				places = service.SearchPlaces( null, latitude, longitude, radius );                   
			});

			return places;
		}

        //public Address[] GetNearbyPlaces(double? latitude, double? longitude)
        //{
        //    Address[] places = null;
        //    UseServiceClient<IPlaces>(service =>
        //    {
        //        places = service.Search( latitude, longitude );                   
        //    });

        //    return places;
        //}
	}
}

