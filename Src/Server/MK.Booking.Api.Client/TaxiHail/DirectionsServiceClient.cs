using System.Globalization;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class DirectionsServiceClient : BaseServiceClient
    {
        public DirectionsServiceClient(string url, string sessionId)
            : base(url, sessionId)
        {
        }


        public DirectionInfo GetDirectionDistance ( double originLatitude , double originLongitude , double destinationLatitude , double destinationLongitude  )
        {                           
            var resource = string.Format(CultureInfo.InvariantCulture, "/directions?OriginLat={0}&OriginLng={1}&DestinationLat={2}&DestinationLng={3}", originLatitude, originLongitude,destinationLatitude ,destinationLongitude);            
            var result = Client.Get<DirectionInfo>(resource);
            return result;
        }


    }
}
