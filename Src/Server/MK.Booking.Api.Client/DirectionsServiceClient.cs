using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using apcurium.MK.Booking.Api.Contract.Resources;
using System.Globalization;

namespace apcurium.MK.Booking.Api.Client
{
    public class DirectionsServiceClient : BaseServiceClient
    {
        public DirectionsServiceClient(string url, AuthInfo credential)
            : base(url, credential)
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
