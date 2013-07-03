﻿using MK.Common.Android.Configuration;
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

namespace apcurium.MK.Booking.Maps.Impl
{
    public class Geocoding : IGeocoding
    {
        private string[] _otherTypesAllowed = new string[] { "airport", "transit_station", "bus_station", "train_station", "route", "postal_code" };
        private IMapsApiClient _mapApi;        
        private IConfigurationManager _configManager;
        private readonly IPopularAddressProvider _popularAddressProvider;

        public Geocoding(IMapsApiClient mapApi, IConfigurationManager configManager, IPopularAddressProvider popularAddressProvider)
        {
            _mapApi = mapApi;
            _configManager = configManager;
            _popularAddressProvider = popularAddressProvider;
        }

        public Address[] Search(string addressName, GeoResult geoResult = null)
        {
            geoResult = geoResult ?? SearchUsingName(addressName, true);

            if ((geoResult.Status == ResultStatus.OK) || ( geoResult.Results.Count > 0 ))
            {
                return ConvertGeoResultToAddresses(geoResult,null);
            }

			// No result
            return new Address[0];

        }

        private GeoResult SearchUsingName(string name, bool useFilter)
        {
            var filter = _configManager.GetSetting("GeoLoc.SearchFilter");
            if (name != null)
            {
                if ((filter.HasValue()) && (useFilter))
                {
                    var filteredName = string.Format(filter, name.Split(' ').JoinBy("+"));
                    return _mapApi.GeocodeAddress(filteredName);
                }
                else
                {
                    return _mapApi.GeocodeAddress(name.Split(' ').JoinBy("+"));
                }
            }
            return null;
        }


        public Address[] Search(double latitude, double longitude, GeoResult geoResult = null, bool searchPopularAddress = false)
        {
            var addressesInRange = new Address[0];
            if (searchPopularAddress)
            {
               addressesInRange = GetPopularAddressesInRange(new Position(latitude, longitude));
            }

            geoResult = geoResult ?? _mapApi.GeocodeLocation(latitude, longitude);
            if (geoResult.Status == ResultStatus.OK)
            {
                return addressesInRange.Concat(ConvertGeoResultToAddresses(geoResult, null)).ToArray();
            }
            else
            {
                return addressesInRange;
            }
        }

      

        private Address[] GetPopularAddressesInRange(Position position)
        {
            const int range = 150;
            const double R = 6378137;

            var addressesInRange = from a in _popularAddressProvider.GetPopularAddresses()
                                   let distance = position.DistanceTo(new Position(a.Latitude, a.Longitude))
                                   where distance <= range
                                   orderby distance ascending
                                   select a;
            addressesInRange.ForEach(a => a.AddressType = "popular");
            return addressesInRange.ToArray();
        }

        private Address[] ConvertGeoResultToAddresses(GeoResult geoResult, string placeName)
        {
            if ((geoResult.Status != ResultStatus.OK) || (geoResult.Results == null) || (geoResult.Results.Count == 0))
            {
                return new Address[0];
            }

            return geoResult.Results.Where(r => r.Formatted_address.HasValue() &&
                                                  r.Geometry != null && r.Geometry.Location != null &&
                                                  r.Geometry.Location.Lng != 0 && r.Geometry.Location.Lat != 0 &&
                                                  (r.AddressComponentTypes.Any(type => type == AddressComponentType.Street_address) ||
                                                  (r.Types.Any(t => _otherTypesAllowed.Any(o => o.ToLower() == t.ToLower())))))
                                     .Select(r=>new GeoObjToAddressMapper().ConvertToAddress(r,placeName)).ToArray();
        }

      



       


    }
}
