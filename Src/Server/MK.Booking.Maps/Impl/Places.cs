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
            if(latitude.HasValue && longitude.HasValue)
            {
                if (string.IsNullOrEmpty(name))
                {
                    popularAddresses = from a in _popularAddressProvider.GetPopularAddresses()                                       
                                       select a;
                }
                else
                {
                    var words = name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    popularAddresses = from a in _popularAddressProvider.GetPopularAddresses()
                                       where words.All(w => a.FriendlyName.ToUpper().Contains(w.ToUpper()) || a.FullAddress.ToUpper().Contains(w.ToUpper()))
                                       select a;
                }

                popularAddresses =popularAddresses.ForEach ( p=> p.AddressType = "popular" );               
            }

            IEnumerable<Place> googlePlaces = new Place[0];

            if (string.IsNullOrWhiteSpace(name))
            {
                googlePlaces = _client.GetNearbyPlaces(latitude, longitude,  "en", false, radius.HasValue ? radius.Value : defaultRadius).Take(15);
            }
            else
            {

                var priceFormat = new RegionInfo(_configurationManager.GetSetting("PriceFormat"));
                
                googlePlaces = _client.SearchPlaces(latitude, longitude, name, "en", false, radius.HasValue ? radius.Value : defaultRadius, priceFormat.TwoLetterISORegionName.ToLower()).Take(15);
            }

            if (latitude.HasValue && longitude.HasValue)
            {
                var places = popularAddresses.Concat(googlePlaces.Select(ConvertToAddress)).OrderBy(p => Position.CalculateDistance(p.Latitude, p.Longitude, latitude.Value, longitude.Value)).ToArray();
                return places;
            }
            else
            {
                var places = popularAddresses.Concat(googlePlaces.Select(ConvertToAddress)).ToArray();
                return places;
            }
        }

        private Address ConvertToAddress(Place place)
        {
            var txtInfo = new CultureInfo("en-US", false).TextInfo;
            
            var typesToHide = new[] { "establishment", "geocode" };
            var placeType = place.Types.FirstOrDefault().ToSafeString();
            bool hidePlaceType = ( string.IsNullOrWhiteSpace( placeType ) || typesToHide.Contains( placeType ) );
            var displayPlaceType = hidePlaceType ? "" : " (" + txtInfo.ToTitleCase(placeType.Replace("_", " ")) + ")";

            

            //string diplay = placeType//

            var address = new Address
            {
                
                Id = Guid.NewGuid(),
                PlaceReference = place.Reference,
                FriendlyName = place.Name + displayPlaceType,
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
