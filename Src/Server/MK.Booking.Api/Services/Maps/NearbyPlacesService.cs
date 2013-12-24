#region

using System.Net;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Maps;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Services.Maps
{
    public class NearbyPlacesService : Service
    {
        private readonly IPlaces _client;

        public NearbyPlacesService(IPlaces client)
        {
            _client = client;
        }

        public object Get(NearbyPlacesRequest request)
        {
            if (string.IsNullOrEmpty(request.Name)
                && request.IsLocationEmpty())
            {
                throw new HttpError(HttpStatusCode.BadRequest, ErrorCode.NearbyPlaces_LocationRequired.ToString());
            }

            return _client.SearchPlaces(request.Name, request.Lat, request.Lng, request.Radius);
        }
    }
}