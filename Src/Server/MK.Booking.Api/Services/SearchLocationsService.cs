#region

using System.Net;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Maps;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class SearchLocationsService : Service
    {
        private readonly IAddresses _client;

        public SearchLocationsService(IAddresses client)
        {
            _client = client;
        }

        public object Post(SearchLocationsRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Name))
            {
                throw new HttpError(HttpStatusCode.BadRequest, ErrorCode.Search_Locations_NameRequired.ToString());
            }
            return _client.Search(request.Name, request.Lat.GetValueOrDefault(), request.Lng.GetValueOrDefault(),
                "en", request.GeoResult);
        }
    }
}