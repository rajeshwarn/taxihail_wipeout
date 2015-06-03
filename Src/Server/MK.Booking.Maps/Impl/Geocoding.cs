﻿#region

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
using MK.Common.Configuration;
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

		public Address[] Search(string addressName, string currentLanguage, GeoResult geoResult = null)
        {
            var popularPlaces = new Address[0];

            if (addressName.HasValue())
            {
                popularPlaces = SearchPopularAddresses(addressName);
            }

            if (geoResult == null)
            {
				var addresses = SearchUsingName(addressName, true, currentLanguage);
                return addresses == null 
                    ? popularPlaces 
                    : popularPlaces.Concat(addresses.Select(a => new GeoObjToAddressMapper().ConvertToAddress(a, null, true))).ToArray();
            }
            else            
            {
				var addresses = ResourcesExtensions.ConvertGeoResultToAddresses(geoResult);
                 
                return addresses == null 
                    ? popularPlaces 
                    : popularPlaces.Concat(addresses.Select(a => new GeoObjToAddressMapper().ConvertToAddress(a, null, true))).ToArray();
            }
        }

        public async Task<Address[]> SearchAsync(string addressName, string currentLanguage, GeoResult geoResult = null)
        {
            var popularPlaces = new Address[0];

            if (addressName.HasValue())
            {
                popularPlaces = SearchPopularAddresses(addressName);
            }

            if (geoResult == null)
            {
                var addresses = await SearchUsingNameAsync(addressName, true, currentLanguage);
                return addresses == null
                    ? popularPlaces
                    : popularPlaces.Concat(addresses.Select(a => new GeoObjToAddressMapper().ConvertToAddress(a, null, true))).ToArray();
            }
            else
            {
                var addresses = ResourcesExtensions.ConvertGeoResultToAddresses(geoResult);

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

            if (geoResult != null)
            {
				var addresses = ResourcesExtensions.ConvertGeoResultToAddresses(geoResult);
                return addressesInRange.Concat(addresses.Select(a => new GeoObjToAddressMapper().ConvertToAddress(a, null, false))).ToArray();
            }
            else
            {
				var addresses = _mapApi.GeocodeLocation(latitude, longitude, currentLanguage);
                var rr = addresses.Select(r => new GeoObjToAddressMapper().ConvertToAddress(r, null, false));
                return addressesInRange.Concat(rr).ToArray();
            }
        }

		private GeoAddress[] SearchUsingName(string name, bool useFilter, string currentLanguage)
        {
		    if (name == null)
		    {
		        return null;
		    }

		    GeoAddress[] results;
            var filter = _appSettings.Data.GeoLoc.SearchFilter;

		    if (filter.HasValue() && useFilter)
		    {
		        var filteredName = string.Format(filter, name.Split(' ').JoinBy("+"));
		        results = _mapApi.GeocodeAddress(filteredName, currentLanguage);

		        return FilterGeoCodingResults(results);
		    }
		    results = _mapApi.GeocodeAddress(name.Split(' ').JoinBy("+"), currentLanguage);

		    return FilterGeoCodingResults(results);
        }

        private async Task<GeoAddress[]> SearchUsingNameAsync(string name, bool useFilter, string currentLanguage)
        {
            if (name == null)
            {
                return null;
            }

            var filter = _appSettings.Data.GeoLoc.SearchFilter;

            var addressName = filter.HasValue() && useFilter
                ? string.Format(filter, name.Split(' ').JoinBy("+"))
                : name.Split(' ').JoinBy("+");
            
            var addresses = await _mapApi.GeocodeAddressAsync(addressName, currentLanguage);

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