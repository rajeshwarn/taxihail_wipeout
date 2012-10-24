using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Api.Client
{
    class GeolocationServiceClient: BaseServiceClient
    {
        public GeolocationServiceClient(string url, string sessionId)
            : base(url, sessionId)
        {
        }


        public PopularAddress GetPlaceDetail(double Longitude, double Latitude)
        {
            var result = Client.Get<PopularAddress>(string.Format(CultureInfo.InvariantCulture, "/popularaddresses/{0}/{1}", Longitude,Latitude));
            return result;
        }
    }
}
