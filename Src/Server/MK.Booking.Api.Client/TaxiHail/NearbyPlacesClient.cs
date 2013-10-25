using System.Globalization;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class NearbyPlacesClient : BaseServiceClient
    {
        public NearbyPlacesClient(string url, string sessionId, string userAgent)
            : base(url, sessionId,userAgent)
        {
        }


        public Address[] GetNearbyPlaces(double? latitude, double? longitude, int? radius)
        {
            var result = Client.Get<Address[]>(string.Format(CultureInfo.InvariantCulture, "/places?lat={0}&lng={1}&radius={2}", latitude, longitude, radius));
            return result;
        }

        public Address[] GetNearbyPlaces(double? latitude, double? longitude)
        {
			var result = Client.Get<Address[]>(string.Format(CultureInfo.InvariantCulture, "/places?lat={0}&lng={1}", latitude, longitude));
            return result;
        }
    }
}
