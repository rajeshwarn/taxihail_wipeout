using System;
using System.Linq;
using System.Net;
using apcurium.MK.Booking.Google;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Booking.Google.Resources;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;

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
            if (string.IsNullOrEmpty(request.Name) 
                && request.IsLocationEmpty())
            {
                throw new HttpError(HttpStatusCode.BadRequest, ErrorCode.NearbyPlaces_LocationRequired.ToString());
            }

            int defaultRadius;
            if(!Int32.TryParse(_configurationManager.GetSetting("NearbyPlacesService.DefaultRadius"), out defaultRadius))
            {
                //fallback
                defaultRadius = 500;
            }

            var results = _client.GetNearbyPlaces(request.Lat.Value, request.Lng.Value, request.Name, "en", false, request.Radius ?? defaultRadius);

            return results.Select(ConvertToAddress).ToArray();
        }

        private Address ConvertToAddress(Place place)
        {
            var address = new Address
            {
                Id = Guid.NewGuid(),
                PlaceReference = place.Reference,
                FriendlyName = place.Name,
                FullAddress = place.Vicinity,
                Latitude = place.Geometry.Location.Lat,
                Longitude = place.Geometry.Location.Lng,
                AddressType = "place"
            };

            if (address.FullAddress.HasValue() &&
                address.FullAddress.Contains("-"))
            {
                var firstWordStreetNumber = address.FullAddress.Split(' ')[0];
                if (firstWordStreetNumber.Contains("-"))
                {
                    var newStreetNUmber = firstWordStreetNumber.Split('-')[0].Trim();
                    address.FullAddress = address.FullAddress.Replace(firstWordStreetNumber, newStreetNUmber);
                }
            }

           return address;
        }

    }
}
