using System;
using System.Linq;
using System.Net;
using apcurium.MK.Booking.Google;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Configuration;

namespace apcurium.MK.Booking.Api.Services
{
    public class NearbyPlacesService : RestServiceBase<NearbyPlacesRequest>
    {
        private IMapsApiClient _client;
        private readonly IConfigurationManager _configurationManager;

        public NearbyPlacesService(IMapsApiClient client, IConfigurationManager configurationManager)
        {
            _client = client;
            _configurationManager = configurationManager;
        }

        public override object OnGet(NearbyPlacesRequest request)
        {
            if (request.Lat == null || request.Lng == null)
            {
                throw new HttpError(HttpStatusCode.BadRequest, ErrorCode.NearbyPlaces_LocationRequired.ToString());
            }

            int defaultRadius;
            if(!Int32.TryParse(_configurationManager.GetSetting("NearbyPlacesService.DefaultRadius"), out defaultRadius))
            {
                //fallback
                defaultRadius = 500;
            }

            var results = _client.GetNearbyPlaces(request.Lat.Value, request.Lng.Value, "en", false, request.Radius ?? defaultRadius);

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
