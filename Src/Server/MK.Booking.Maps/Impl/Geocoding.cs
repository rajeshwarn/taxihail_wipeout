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
    public class Geocoding : IGeocoding
    {
        private string[] _otherTypesAllowed = new string[] { "airport", "transit_station", "bus_station", "train_station" };
        private IMapsApiClient _mapApi;        
        private IConfigurationManager _configManager;

        public Geocoding(IMapsApiClient mapApi, IConfigurationManager configManager)
        {
            _mapApi = mapApi;
            _configManager = configManager;
        }

        public Address[] Search(string addressName)
        {
            Address[] result = null;

            var geoResult = SearchUsingName(addressName, true);

            if ((geoResult.Status == ResultStatus.OK) || ( geoResult.Results.Count > 0 ))
            {
                result = ConvertGeoResultToAddresses(geoResult);
            }

            if (( result == null ) || ( result.Count() == 0 ) )
            {
                result = ConvertGeoResultToAddresses(SearchUsingName(addressName, false));                                                
            }
            return result;

        }

        private GeoResult SearchUsingName(string name, bool useFilter)
        {
            var filter = _configManager.GetSetting("GeoLoc.SearchFilter");

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


        public Address[] Search(double latitude, double longitude)
        {
            var geoResult = _mapApi.GeocodeLocation(latitude, longitude);
            if (geoResult.Status == ResultStatus.OK)
            {
                return ConvertGeoResultToAddresses(geoResult);
            }
            else
            {
                return new Address[0];
            }


         
        }

        private Address[] ConvertGeoResultToAddresses(GeoResult geoResult)
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
                                     .Select(new GeoObjToAddressMapper().ConvertToAddress).ToArray();
        }

      



       


    }
}
