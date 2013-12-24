#region

using System;
using System.Globalization;
using apcurium.MK.Common.Entity;

#endregion

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class SearchLocationsServiceClient : BaseServiceClient
    {
        public SearchLocationsServiceClient(string url, string sessionId, string userAgent)
            : base(url, sessionId, userAgent)
        {
        }


        public Address[] Search(string name, double latitude, double longitude)
        {
            var resource = string.Format(CultureInfo.InvariantCulture, "/searchlocation?Name={0}&Lat={1}&Lng={2}", name,
                latitude, longitude);

            Console.WriteLine(resource);
            var result = Client.Post<Address[]>(resource, null);
            return result;
        }
    }
}