using apcurium.MK.Booking.Google;
using apcurium.MK.Booking.Google.Resources;
using apcurium.MK.Booking.Maps.Impl.Mappers;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Maps.Impl
{
    public class Places : IPlaces
    {
        private IMapsApiClient _client;
        private readonly IConfigurationManager _configurationManager;

        public Places(IMapsApiClient client, IConfigurationManager configurationManager)
        {
            _client = client;
            _configurationManager = configurationManager;
        }

        public Address GetPlaceDetail(string name, string referenceId)
        {
            var place = _client.GetPlaceDetail(referenceId);

            var result = new GeoObjToAddressMapper().ConvertToAddress(place, name);

            result.PlaceReference = referenceId;

            return result;
        }

        public Address[] SearchPlaces(string name, double? latitude, double? longitude, int? radius)
        {

            int defaultRadius;
            if (!Int32.TryParse(_configurationManager.GetSetting("NearbyPlacesService.DefaultRadius"), out defaultRadius))
            {
                //fallback
                defaultRadius = 500;
            }

            var results = _client.GetNearbyPlaces(latitude, longitude, name, "en", false, radius.HasValue? radius.Value : defaultRadius);

            return results.Select(ConvertToAddress).ToArray();
        }

        private Address ConvertToAddress(Place place)
        {
            var address = new Address
            {
                
                Id = Guid.NewGuid(),
                PlaceReference = place.Reference,
                FriendlyName = place.Name + " (" + place.Types.FirstOrDefault().ToSafeString()+")",
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
