#region

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using apcurium.MK.Booking.Maps.Geo;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.Provider;
using apcurium.MK.Booking.MapDataProvider;
using apcurium.MK.Booking.MapDataProvider.Resources;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Booking.MapDataProvider.Google.Resources;

#endregion

namespace apcurium.MK.Booking.Maps.Impl
{
    public class Addresses : IAddresses
    {
		private readonly IGeocoder _geocoder;
        private readonly IAppSettings _appSettings;
        private readonly IPopularAddressProvider _popularAddressProvider;
        private readonly ILogger _logger;
		private readonly IPlaceDataProvider _placeProvider;

        public Addresses(IGeocoder geocoder, IPlaceDataProvider placeProvider, IAppSettings appSettings,
            IPopularAddressProvider popularAddressProvider, ILogger logger)
        {
			_placeProvider = placeProvider;
            _logger = logger;
			_geocoder = geocoder;
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
		public Address[] Search(string name, double? latitude, double? longitude, string currentLanguage, GeoResult geoResult = null)
        {
            if (name.IsNullOrEmpty())
            {
                return new Address[0];
            }

            var geoCodingService = new Geocoding(_geocoder, _appSettings, _popularAddressProvider, _logger);

            var allResults = geoCodingService.Search(name, latitude, longitude, currentLanguage, geoResult);

            return ProcessAddresses(name, allResults, latitude, longitude, currentLanguage, geoResult);
        }

        private Address[] ProcessAddresses(string name, IEnumerable<Address> allResults, double? latitude, double? longitude, string currentLanguage, GeoResult geoResult = null)
        {
            IEnumerable<Address> addressesGeocode;
            IEnumerable<Address> addressesPlaces = new Address[0];

            // ReSharper disable CompareOfFloatsByEqualityOperator
            if (latitude.HasValue && longitude.HasValue && (latitude.Value != 0 || longitude.Value != 0))
            // ReSharper restore CompareOfFloatsByEqualityOperator
            {
                addressesGeocode = allResults
                    .OrderBy(adrs => Position.CalculateDistance(adrs.Latitude, adrs.Longitude, latitude.Value, longitude.Value));
            }
            else
            {
                addressesGeocode = allResults;
            }

            var term = name.FirstOrDefault();
            int n;
            if (!int.TryParse(term + "", out n))
            {
                var nearbyService = new Places(_placeProvider, _appSettings, _popularAddressProvider);

                addressesPlaces = nearbyService.SearchPlaces(name, latitude, longitude, currentLanguage);
            }

            return addressesGeocode
                .Take(20)
                .Concat(addressesPlaces.Take(20))
                .OrderBy(p => AddressSortingHelper.GetRelevance(p, name, latitude, longitude))
                .ToArray();
        }

        public async Task<Address[]> SearchAsync(string name, double? latitude, double? longitude, string currentLanguage, GeoResult geoResult = null)
        {
            var geoCodingService = new Geocoding(_geocoder, _appSettings, _popularAddressProvider, _logger);

            var allResults = await geoCodingService.SearchAsync(name, latitude, longitude, currentLanguage, geoResult);

            return ProcessAddresses(name, allResults, latitude, longitude, currentLanguage, geoResult);
        }
    }
}