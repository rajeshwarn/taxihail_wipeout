#region

using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Google;
using apcurium.MK.Booking.Google.Resources;
using apcurium.MK.Booking.Maps.Geo;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.Provider;

#endregion

namespace apcurium.MK.Booking.Maps.Impl
{
    public class Addresses : IAddresses
    {
        private readonly IMapsApiClient _client;
        private readonly IAppSettings _appSettings;
        private readonly IPopularAddressProvider _popularAddressProvider;

        public Addresses(IMapsApiClient client, IAppSettings appSettings,
            IPopularAddressProvider popularAddressProvider)
        {
            _client = client;
            _appSettings = appSettings;
            _popularAddressProvider = popularAddressProvider;
        }

        /// <summary>
        ///     Search addresses for the specified name, latitude and longitude.
        /// </summary>
        /// <param name='name'>
        ///     Search criteria, address fragment. Cannot be null or empty
        /// </param>
        /// <param name='latitude'>
        ///     Latitude
        /// </param>
        /// <param name='longitude'>
        ///     Longitude
        /// </param>
        /// <param name="geoResult">
        ///     
        /// </param>
        public Address[] Search(string name, double? latitude, double? longitude, GeoResult geoResult = null)
        {
            if (name.IsNullOrEmpty())
            {
                return new Address[0];
            }

            IEnumerable<Address> addressesGeocode;
            IEnumerable<Address> addressesPlaces = new Address[0];

            var geoCodingService = new Geocoding(_client, _appSettings, _popularAddressProvider);

            var allResults = geoCodingService.Search(name, geoResult);

// ReSharper disable CompareOfFloatsByEqualityOperator
            if (latitude.HasValue && longitude.HasValue && (latitude.Value != 0 || longitude.Value != 0))
// ReSharper restore CompareOfFloatsByEqualityOperator
            {
                addressesGeocode = allResults
                    .OrderBy(
                        adrs =>
                            Position.CalculateDistance(adrs.Latitude, adrs.Longitude, latitude.Value, longitude.Value));
            }
            else
            {
                addressesGeocode = allResults;
            }

            var term = name.FirstOrDefault();
            int n;
            if (!int.TryParse(term + "", out n))
            {
                var nearbyService = new Places(_client, _appSettings, _popularAddressProvider);
                addressesPlaces = nearbyService.SearchPlaces(name, latitude, longitude, null);
            }

//TODO not sure what this code is doing
            return addressesPlaces
                    .Take(20)
                    .Concat(addressesGeocode.Take(20))
                    .ToArray(); //todo Take 20!? api's consern
        }
    }
}