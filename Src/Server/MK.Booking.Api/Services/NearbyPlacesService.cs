using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using apcurium.MK.Booking.Google;
using ServiceStack.Common.Web;
using ServiceStack.ServiceClient.Web;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Api.Services
{
    public class NearbyPlacesService : RestServiceBase<NearbyPlacesRequest>
    {
        private const int DefaulRadius = 100;
        private IPlacesClient _client;
        public NearbyPlacesService(IPlacesClient client)
        {
            _client = client;
        }

        public override object OnGet(NearbyPlacesRequest request)
        {
            if (request.Lat == null || request.Lng == null)
            {
                throw new HttpError(HttpStatusCode.BadRequest, ErrorCode.NearbyPlaces_LocationRequired.ToString());
            }

            var results = _client.GetNearbyPlaces(request.Lat.Value, request.Lng.Value, "en", false, request.Radius ?? DefaulRadius);

            return results.Select(x => new Address
            {
                FriendlyName = x.Name,
                FullAddress = x.Vicinity,
                Latitude = x.Geometry.Location.Lat,
                Longitude = x.Geometry.Location.Lng,
            });
        }

    }
}
