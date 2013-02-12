using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using ServiceStack.ServiceClient.Web;
using apcurium.MK.Booking.Google.Resources;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;

namespace apcurium.MK.Booking.Google.Impl
{
    public class MapsApiClient : IMapsApiClient
    {
        private const string PlaceDetailsServiceUrl = "https://maps.googleapis.com/maps/api/place/details/";
        private const string PlacesServiceUrl = "https://maps.googleapis.com/maps/api/place/search/";
        private const string PlacesTextServiceUrl = "https://maps.googleapis.com/maps/api/place/textsearch/";
        private const string MapsServiceUrl = "http://maps.googleapis.com/maps/api/";

        private IConfigurationManager _conifManager;
        private ILogger _logger;
        public MapsApiClient(IConfigurationManager conifManager, ILogger logger)
        {
            _logger = logger;
            _conifManager = conifManager;
        }

        protected string PlacesApiKey
        {
            get
            {
                return _conifManager.GetSettings()["Map.PlacesApiKey"];
            }
        }

        public Place[] GetNearbyPlaces(double? latitude, double? longitude, string name, string languageCode, bool sensor, int radius, string pipedTypeList = null)
        {
            pipedTypeList = pipedTypeList == null ? new PlaceTypes(_conifManager).GetPipedTypeList() : pipedTypeList;
            var url = name != null ? PlacesTextServiceUrl : PlacesServiceUrl;
            var client = new JsonServiceClient(url);

            var @params = new Dictionary<string, string>
            {
                { "sensor", sensor.ToString(CultureInfo.InvariantCulture).ToLowerInvariant() },
                { "key",  PlacesApiKey },                
                { "radius", radius.ToString(CultureInfo.InvariantCulture)  },
                { "language", languageCode  },
                { "types", pipedTypeList},
            };

            if ( name != null )
            {
                @params.Add ( "rankby", "distance");
            }

            if (latitude != null
                && longitude != null)
            {
                @params.Add("location", string.Join(",", latitude.Value.ToString(CultureInfo.InvariantCulture), longitude.Value.ToString(CultureInfo.InvariantCulture)));
            }

            if (name != null)
            {
                @params.Add("query", name);
            }

            var r = "json" + BuildQueryString(@params);


            _logger.LogMessage ("Places API : " + url + r );

            return client.Get<PlacesResponse>(r).Results.ToArray();

        }

        public GeoObj GetPlaceDetail(string reference)
        {
            var client = new JsonServiceClient(PlaceDetailsServiceUrl);
            var @params = new Dictionary<string, string>
            {
                { "reference", reference },
                 { "sensor", true.ToString().ToLower() },
                { "key",  PlacesApiKey },            
            };

            return client.Get<PlaceDetailResponse>("json" + BuildQueryString(@params)).Result;
        }

        public DirectionResult GetDirections(double originLat, double originLng, double destLat, double destLng)
        {
            var client = new JsonServiceClient(MapsServiceUrl);

            var resource = string.Format(CultureInfo.InvariantCulture, "directions/json?origin={0},{1}&destination={2},{3}&sensor=true", originLat, originLng, destLat, destLng);

            return client.Get<DirectionResult>(resource);
        }

        public GeoResult GeocodeAddress(string address)
        {
            var client = new JsonServiceClient(MapsServiceUrl);

            var resource = string.Format(CultureInfo.InvariantCulture, "geocode/json?address={0}&sensor=true", address);

            _logger.LogMessage ("GeocodeLocation : " + MapsServiceUrl + resource );


            return client.Get<GeoResult>(resource);
        }

        public GeoResult GeocodeLocation(double latitude, double longitude)
        {
            var client = new JsonServiceClient(MapsServiceUrl);

            var resource = string.Format(CultureInfo.InvariantCulture, "geocode/json?latlng={0},{1}&sensor=true", latitude, longitude);

            _logger.LogMessage ("GeocodeLocation : " + MapsServiceUrl + resource );

            return client.Get<GeoResult>(resource);
        }

        private string BuildQueryString(IDictionary<string, string> @params)
        {
            return "?" + string.Join("&", @params.Select(x => string.Join("=", x.Key, x.Value)));
        }
    }
}
