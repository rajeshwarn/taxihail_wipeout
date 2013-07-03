using MK.Common.Android.Configuration;
using apcurium.MK.Booking.Google;
using apcurium.MK.Booking.Google.Resources;
using apcurium.MK.Booking.Maps.Geo;
using apcurium.MK.Booking.Maps.Impl.Mappers;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using apcurium.MK.Common.Provider;
using System.Globalization;

namespace apcurium.MK.Booking.Maps.Impl
{
    public class Places : IPlaces
    {
        private IMapsApiClient _client;
        private readonly IConfigurationManager _configurationManager;
        private readonly IPopularAddressProvider _popularAddressProvider;

        public Places(IMapsApiClient client, IConfigurationManager configurationManager, IPopularAddressProvider popularAddressProvider)
        {
            _client = client;
            _configurationManager = configurationManager;
            _popularAddressProvider = popularAddressProvider;
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

            var popularAddresses = Enumerable.Empty<Address>();
            if(name != null && latitude.HasValue && longitude.HasValue)
            {
                var words = name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                popularAddresses = from a in _popularAddressProvider.GetPopularAddresses()
                    where words.All(w => a.FriendlyName.ToUpper().Contains(w.ToUpper()) || a.FullAddress.ToUpper().Contains(w.ToUpper()))
                    select a;
                popularAddresses =popularAddresses.ForEach ( p=> p.AddressType = "popular" );
               // popularAddresses = _popularAddressProvider.GetPopularAddresses().Where(c => c.FullAddress.ToLower().Contains(words.ToLower()));

            }

            var googlePlaces = _client.GetNearbyPlaces(latitude, longitude, name, "en", false, radius.HasValue? radius.Value : defaultRadius).Take(15);

            return popularAddresses.Concat(googlePlaces.Select(ConvertToAddress)).ToArray();
        }

        private Address ConvertToAddress(Place place)
        {
            var txtInfo = new CultureInfo("en-US", false).TextInfo;
            
            var address = new Address
            {
                
                Id = Guid.NewGuid(),
                PlaceReference = place.Reference,
                FriendlyName = place.Name + " (" + txtInfo.ToTitleCase(  place.Types.FirstOrDefault().ToSafeString().Replace("_", " ")) + ")",
                FullAddress = place.Formatted_Address.IsNullOrEmpty ()? place.Vicinity : place.Formatted_Address,
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
