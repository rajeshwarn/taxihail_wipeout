#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Extensions;
using ServiceStack.ServiceClient.Web;
using apcurium.MK.Booking.MapDataProvider.Resources;

#endregion

namespace apcurium.MK.Booking.MapDataProvider.OpenStreetMap
{
    public class MapsApiClient : IMapsApiClient
    {
        

        private const string ResverseGeocodingServiceUrl = "http://nominatim.openstreetmap.org/reverse?format=json&lat={0}&lon={1}&zoom=18&addressdetails=1";
        private const string GeocodingServiceUrl = "http://nominatim.openstreetmap.org/search?q={0}&countrycodes={1}&limit=100&format=json&polygon=0&addressdetails=1";
        

        private readonly IAppSettings _settings;
        private readonly ILogger _logger;
        private readonly IMapsApiClient _googleApi;  //We use google for method not implemented by OpenStreetMap
        public MapsApiClient(IAppSettings settings, ILogger logger)
        {
            _logger = logger;
            _settings = settings;
            _googleApi = new apcurium.MK.Booking.MapDataProvider.Google.MapsApiClient(settings, logger);
        }

        protected string PlacesApiKey
        {
            get { return _settings.Data.PlacesApiKey; }
        }

        public Place[] GetNearbyPlaces(double? latitude, double? longitude, string languageCode, bool sensor, int radius,
            string pipedTypeList = null)
        {
            return _googleApi.GetNearbyPlaces(latitude, longitude, languageCode, sensor, radius, pipedTypeList );
        }

        public Place[] SearchPlaces(double? latitude, double? longitude, string name, string languageCode, bool sensor,
            int radius, string countryCode)
        {
            return _googleApi.SearchPlaces(latitude, longitude, name, languageCode, sensor, radius, countryCode );            
        }

        public GeoAddress GetPlaceDetail(string reference)
        {
            return _googleApi.GetPlaceDetail(reference);  
        }

        public DirectionResult GetDirections(double originLat, double originLng, double destLat, double destLng)
        {
            return _googleApi.GetDirections(originLat, originLng, destLat, destLng);
        }

        public GeoAddress[] GeocodeAddress(string address)
        {
            var priceFormat = new RegionInfo(_settings.Data.PriceFormat);
            var countryCode = priceFormat.TwoLetterISORegionName.ToLower();

            var url = string.Format(CultureInfo.InvariantCulture, GeocodingServiceUrl, address, countryCode );
            var client = new JsonServiceClient(url);
            var results = client.Get<IEnumerable<OSMResult>>(url);

            if ((results != null) && (results.Count()>0))
            {
                return results.Select( ConvertOSMAddressToGeoAddress).ToArray();
            }
            else
            {
                return new GeoAddress[0];
            }
            

        }

        public GeoAddress[] GeocodeLocation(double latitude, double longitude)
        {

            var url = string.Format(CultureInfo.InvariantCulture, ResverseGeocodingServiceUrl, latitude, longitude);
            var client = new JsonServiceClient(url);


            _logger.LogMessage("GeocodeLocation : " + url);

            var r = client.Get<OSMResult>(url);
            if  ( ( r != null ) && (r.address != null ) )
            {
                
                return new GeoAddress[] { ConvertOSMAddressToGeoAddress( r ) };
            }
            else
            {
                return new GeoAddress[0];
            }
            
            
            return null;
        }

        private GeoAddress ConvertOSMAddressToGeoAddress( OSMResult result )
        {
            return new GeoAddress { City = result.address.city , FullAddress =  result.display_name, Latitude = result.lat, Longitude = result.lon, LocationType= "OSM", State= result.address.state, Street = result.address.road, StreetNumber = result.address.house_number, ZipCode = result.address.postcode  };
        }
        
    }
}