using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using ServiceStack.ServiceClient.Web;
using apcurium.MK.Booking.Google.Resources;

namespace apcurium.MK.Booking.Google.Impl
{
    public class PlacesClient : IPlacesClient
    {
        private const string ServiceUrl = "https://maps.googleapis.com/maps/api/place/search/";
        private const string ApiKey = "AIzaSyBzHXvi9heL8opeThi_uCIBOETLCDk575I";

        public Place[] GetNearbyPlaces(double latitude, double longitude, string languageCode, bool sensor, int radius)
        {
            var client = new JsonServiceClient(ServiceUrl);
            var @params = new Dictionary<string, string>
            {
                { "sensor", sensor.ToString(CultureInfo.InvariantCulture).ToLowerInvariant() },
                { "key",  ApiKey },
                { "location", string.Join(",", latitude.ToString(CultureInfo.InvariantCulture), longitude.ToString(CultureInfo.InvariantCulture)) },
                { "radius", radius.ToString(CultureInfo.InvariantCulture)  },
                { "language", languageCode  },
                { "types", new PlaceTypes().GetPipedTypeList()  },
            };

            return client.Get<PlacesResponse>("json" + BuildQueryString(@params)).Results.ToArray();

        }

        private string BuildQueryString(IDictionary<string,string> @params )
        {
            return "?" + string.Join("&", @params.Select(x => string.Join("=", x.Key, x.Value)));
        }
    }
}
