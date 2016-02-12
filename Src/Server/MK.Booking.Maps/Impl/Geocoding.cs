#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using apcurium.MK.Booking.Maps.Geo;
using apcurium.MK.Booking.Maps.Impl.Mappers;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.Geography;
using apcurium.MK.Common.Provider;
using apcurium.MK.Booking.MapDataProvider;
using apcurium.MK.Booking.MapDataProvider.Resources;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Booking.MapDataProvider.Google.Resources;
using apcurium.MK.Booking.MapDataProvider.Extensions;

#endregion

namespace apcurium.MK.Booking.Maps.Impl
{
    public class Geocoding : IGeocoding
    {
        private readonly IAppSettings _appSettings;
		private readonly IGeocoder  _mapApi;
        private readonly ILogger _logger;
        private readonly IPopularAddressProvider _popularAddressProvider;

        public Geocoding(IGeocoder mapApi, IAppSettings appSettings,
            IPopularAddressProvider popularAddressProvider, ILogger logger)
        {
            _logger = logger;
            _mapApi = mapApi;
            _appSettings = appSettings;
            _popularAddressProvider = popularAddressProvider;
        }

        public Address GetPlaceDetail(string placeId)
        {
            return new GeoObjToAddressMapper().ConvertToAddress(_mapApi.GetAddressDetail(placeId), null, true);
        }

        public Address[] Search(string query, double? pickupLatitude, double? pickupLongitude, string currentLanguage, GeoResult geoResult = null)
        {
            var popularPlaces = new Address[0];

            if (query.HasValueTrimmed())
            {
                popularPlaces = SearchPopularAddresses(query);
            }

            if (geoResult == null)
            {
                var addresses = SearchUsingName(query, currentLanguage, pickupLatitude, pickupLongitude);
                return addresses == null 
                    ? popularPlaces 
                    : popularPlaces.Concat(addresses.Select(a => new GeoObjToAddressMapper().ConvertToAddress(a, null, true))).ToArray();
            }
            else            
            {
				var addresses = geoResult.ConvertGeoResultToAddresses();
                 
                return addresses == null 
                    ? popularPlaces 
                    : popularPlaces.Concat(addresses.Select(a => new GeoObjToAddressMapper().ConvertToAddress(a, null, true))).ToArray();
            }
        }

        public async Task<Address[]> SearchAsync(string query, double? pickupLatitude, double? pickupLongitude, string currentLanguage, GeoResult geoResult = null)
        {
            var popularPlaces = new Address[0];

            if (query.HasValue())
            {
                popularPlaces = SearchPopularAddresses(query);
            }

            if (geoResult == null)
            {
                var addresses = await SearchUsingNameAsync(query, currentLanguage, pickupLatitude, pickupLongitude);
                return addresses == null
                    ? popularPlaces
                    : popularPlaces.Concat(addresses.Select(a => new GeoObjToAddressMapper().ConvertToAddress(a, null, true))).ToArray();
            }
            else
            {
                var addresses = geoResult.ConvertGeoResultToAddresses();

                return addresses == null
                    ? popularPlaces
                    : popularPlaces.Concat(addresses.Select(a => new GeoObjToAddressMapper().ConvertToAddress(a, null, true))).ToArray();
            }
        }

        public Address[] Search(double latitude, double longitude, string currentLanguage, GeoResult geoResult = null,
            bool searchPopularAddress = false)
        {
            var addressesInRange = new Address[0];
            if (searchPopularAddress)
            {
                addressesInRange = GetPopularAddressesInRange(new Position(latitude, longitude));
            }
           
            var addresses = geoResult != null
                ? geoResult.ConvertGeoResultToAddresses()
                : _mapApi.GeocodeLocation(latitude, longitude, currentLanguage);

            return addressesInRange
                .Concat(addresses.Select(ToAddress))
                .ToArray();
        }

        public async Task<Address[]> SearchAsync(double latitude, double longitude, string currentLanguage, GeoResult geoResult = null, 
            bool searchPopularAddress = false)
        {
            var addressesInRange = new Address[0];
            if (searchPopularAddress)
            {
                addressesInRange = GetPopularAddressesInRange(new Position(latitude, longitude));
            }

            var addresses = geoResult != null
                ? geoResult.ConvertGeoResultToAddresses()
                : await _mapApi.GeocodeLocationAsync(latitude, longitude, currentLanguage);

            return addressesInRange
                .Concat(addresses.Select(ToAddress))
                .ToArray();
        }

        private static Address ToAddress(GeoAddress geoAddress)
        {
            return new GeoObjToAddressMapper().ConvertToAddress(geoAddress, null, false);
        }

        private GeoAddress[] SearchUsingName(string searchQuery, string currentLanguage, double? pickupLatitude, double? pickupLongitude)
        {
            if (searchQuery == null || !searchQuery.HasValueTrimmed())
            {
                return null;
            }

            var filter = _appSettings.Data.GeoLoc.SearchFilter;

            var query = filter.HasValue()
                ? string.Format(filter, searchQuery)
                : searchQuery;
		    
            var searchRadius = _appSettings.Data.GeoLoc.SearchRadius <= 0 ? 45000 : _appSettings.Data.GeoLoc.SearchRadius;
            var results = _mapApi.GeocodeAddress(query, currentLanguage, pickupLatitude, pickupLongitude, searchRadius);

		    return FilterGeoCodingResults(results);
        }

        private async Task<GeoAddress[]> SearchUsingNameAsync(string searchQuery, string currentLanguage, double? pickupLatitude, double? pickupLongitude)
        {
            if (searchQuery == null)
            {
                return null;
            }

            var filter = _appSettings.Data.GeoLoc.SearchFilter;

            var query = filter.HasValue()
                ? string.Format(filter, searchQuery)
                : searchQuery;
            
            var searchRadius = _appSettings.Data.GeoLoc.SearchRadius <= 0 ? 45000 : _appSettings.Data.GeoLoc.SearchRadius;

            #if DEBUG
            Console.WriteLine(string.Format("Geocoding.SearchUsingNameAsync with query: {0} radius: {1}", query, searchRadius));
            #endif

            var addresses = await _mapApi.GeocodeAddressAsync(query, currentLanguage, pickupLatitude, pickupLongitude, searchRadius);

            #if DEBUG
            Console.WriteLine("Geocoding.SearchUsingNameAsync results");
            foreach(var address in addresses)
            {
                Console.WriteLine(string.Format("    {0}", address.FullAddress));
            }
            #endif

            return FilterGeoCodingResults(addresses);
        }

        private Address[] SearchPopularAddresses(string name)
        {
            var words = name.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            var popularAddresses = from a in _popularAddressProvider.GetPopularAddresses()
                where
                    words.All(
                        w =>
                            a.FriendlyName.ToUpper().Contains(w.ToUpper()) ||
                            a.FullAddress.ToUpper().Contains(w.ToUpper()))
                select a;

            return popularAddresses.ToArray();
        }

        private Address[] GetPopularAddressesInRange(Position position)
        {
            const int range = 150;

            var addressesInRange = from a in _popularAddressProvider.GetPopularAddresses()
                let distance = position.DistanceTo(new Position(a.Latitude, a.Longitude))
                where distance <= range
                orderby distance ascending
                select a;
            var inRange = addressesInRange as Address[] ?? addressesInRange.ToArray();
            inRange.ForEach(a => a.AddressType = "popular");
            return inRange.ToArray();
        }

        private GeoAddress[] FilterGeoCodingResults(GeoAddress[] resultsToFilter)
        {
            if (!ShouldFilterResults())
            {
                return resultsToFilter;
            }

            // Make sure that the geocoding results are part of the region covered by the app
            return resultsToFilter.Where(result =>
                GeographyHelper.RegionContainsCoordinate(
                    _appSettings.Data.UpperRightLatitude.Value,  // x1
                    _appSettings.Data.UpperRightLongitude.Value, // y1
                    _appSettings.Data.LowerLeftLatitude.Value,   // x2
                    _appSettings.Data.LowerLeftLongitude.Value,  // y2
                    result.Latitude, result.Longitude))
                .ToArray();
        }

        private bool ShouldFilterResults()
        {
            var settingsHaveValues = _appSettings.Data.LowerLeftLatitude.HasValue
                                  && _appSettings.Data.LowerLeftLongitude.HasValue
                                  && _appSettings.Data.UpperRightLatitude.HasValue
                                  && _appSettings.Data.UpperRightLongitude.HasValue;

            if (!settingsHaveValues)
            {
                return false;
            }

            var lowerLeft = new Address
            {
                Latitude = _appSettings.Data.LowerLeftLatitude.Value,
                Longitude = _appSettings.Data.LowerLeftLongitude.Value
            };
            var upperRight = new Address
            {
                Latitude = _appSettings.Data.UpperRightLatitude.Value,
                Longitude = _appSettings.Data.UpperRightLongitude.Value
            };

            return HasValidCoordinate(lowerLeft) && HasValidCoordinate(upperRight);
        }

        private bool HasValidCoordinate(Address instance)
        {
            return instance.Longitude != 0 
                && instance.Latitude != 0 
                && instance.Latitude >= -90 
                && instance.Latitude <= 90 
                && instance.Longitude >= -180 
                && instance.Longitude <= 180;
        }
    }
}