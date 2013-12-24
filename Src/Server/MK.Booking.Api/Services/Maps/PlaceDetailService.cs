#region

using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Maps;
using apcurium.MK.Common.Entity;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Services.Maps
{
    public class PlaceDetailService : Service
    {
        private readonly IPlaces _client;

        public PlaceDetailService(IPlaces client)
        {
            _client = client;
        }


        public Address Get(PlaceDetailRequest request)
        {
            return _client.GetPlaceDetail(request.PlaceName, request.ReferenceId);
        }
    }
}