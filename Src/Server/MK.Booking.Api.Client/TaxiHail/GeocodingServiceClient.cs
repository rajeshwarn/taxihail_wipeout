using System.Globalization;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class GeocodingServiceClient : BaseServiceClient
    {
        public GeocodingServiceClient(string url, string sessionId)
            : base(url, sessionId)
        {
        }


        public Address[] Search( string addressName )
        {
            var resource = string.Format("/geocode?Name={0}", addressName);
            var result = Client.Get<Address[]>(resource);
            return result;
        }


        public Address[] Search(double latitude, double longitude)
        {
            var resource = string.Format(CultureInfo.InvariantCulture, "/geocode?Lat={0}&Lng={1}", latitude, longitude );
            var result = Client.Get<Address[]>(resource);
            return result;
        }

        public Address DefaultLocation()
        {
            var result = Client.Get<Address>("/settings/defaultlocation");
            return result;
        }
        
    }
}
