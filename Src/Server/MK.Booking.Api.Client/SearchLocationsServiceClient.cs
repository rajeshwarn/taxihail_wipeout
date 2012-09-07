using apcurium.MK.Booking.Api.Contract.Resources;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace apcurium.MK.Booking.Api.Client
{
    public class SearchLocationsServiceClient : BaseServiceClient
    {
        public SearchLocationsServiceClient(string url, string sessionId)
            : base(url, sessionId)
        {
        }       


        public AddressList Search(string name, double latitude, double longitude )
        {
            var resource = string.Format(CultureInfo.InvariantCulture, "/searchlocation?Name={0}&Lat={1}&Lng={2}", name, latitude, longitude);
            var result = Client.Get<AddressList>(resource);
            return result;
        }
    }
}
