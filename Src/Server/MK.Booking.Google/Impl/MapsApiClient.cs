using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using ServiceStack.ServiceClient.Web;
using apcurium.MK.Booking.Google.Resources;

namespace apcurium.MK.Booking.Google.Impl
{
    public class MapsApiClient : IMapsApiClient
    {
        private const string PlacesServiceUrl = "https://maps.googleapis.com/maps/api/place/search/";
        private const string MapsServiceUrl = "http://maps.googleapis.com/maps/api/";
        private const string PlacesApiKey = "AIzaSyBzHXvi9heL8opeThi_uCIBOETLCDk575I";

        public Place[] GetNearbyPlaces(double latitude, double longitude, string languageCode, bool sensor, int radius)
        {
            var client = new JsonServiceClient(PlacesServiceUrl);
            var @params = new Dictionary<string, string>
            {
                { "sensor", sensor.ToString(CultureInfo.InvariantCulture).ToLowerInvariant() },
                { "key",  PlacesApiKey },
                { "location", string.Join(",", latitude.ToString(CultureInfo.InvariantCulture), longitude.ToString(CultureInfo.InvariantCulture)) },
                { "radius", radius.ToString(CultureInfo.InvariantCulture)  },
                { "language", languageCode  },
                { "types", new PlaceTypes().GetPipedTypeList()  },
            };

            return client.Get<PlacesResponse>("json" + BuildQueryString(@params)).Results.ToArray();

        }

        public DirectionResult GetDirections(double originLat, double originLng, double destLat, double destLng)
        {

            var client = new JsonServiceClient(MapsServiceUrl);

            var resource = string.Format(CultureInfo.InvariantCulture, "directions/json?origin={0},{1}&destination={2},{3}&sensor=false", originLat, originLng, destLat, destLng);

            return client.Get<DirectionResult>(resource);

        }

        public GeoResult GeocodeAddress(string address)
        {
            var client = new JsonServiceClient(MapsServiceUrl);

            var resource = string.Format(CultureInfo.InvariantCulture, "geocode/json?address={0}&sensor=false", address);

            return client.Get<GeoResult>(resource);
        }

        public GeoResult GeocodeLocation(double latitude, double longitude)
        {
            var client = new JsonServiceClient(MapsServiceUrl);

            var resource = string.Format(CultureInfo.InvariantCulture, "geocode/json?latlng={0},{1}&sensor=false", latitude, longitude);

            return client.Get<GeoResult>(resource);
        }

        private string BuildQueryString(IDictionary<string,string> @params )
        {
            return "?" + string.Join("&", @params.Select(x => string.Join("=", x.Key, x.Value)));
        }
    }
}
