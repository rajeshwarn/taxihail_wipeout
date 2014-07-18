#region

using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Maps.Geo;
using apcurium.MK.Booking.Maps.Impl.Mappers;
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
                if ( addresses == null )
                {
                    return popularPlaces;
                }
                else
                {
                    return popularPlaces.Concat(addresses.Select(a => new GeoObjToAddressMapper().ConvertToAddress(a, null, true))).ToArray();
                }
            }
            else            
            {
				var addresses = new MapDataProvider.Google.GoogleApiClient(_appSettings, _logger).ConvertGeoResultToAddresses(geoResult);
                 
                if ( addresses == null )
                {
                    return popularPlaces;
                }
                else
                {
                    return popularPlaces.Concat(addresses.Select(a => new GeoObjToAddressMapper().ConvertToAddress(a, null, true))).ToArray();
                }
                
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
				var addresses = new MapDataProvider.Google.GoogleApiClient( _appSettings, _logger ).ConvertGeoResultToAddresses(geoResult);
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
            var filter = _appSettings.Data.SearchFilter;
            if (name != null)
            {
                if ((filter.HasValue()) && (useFilter))
                {
                    var filteredName = string.Format(filter, name.Split(' ').JoinBy("+"));
					return _mapApi.GeocodeAddress(filteredName, currentLanguage);
                }
				return _mapApi.GeocodeAddress(name.Split(' ').JoinBy("+"), currentLanguage);
            }
            return null;
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

       
    }
}