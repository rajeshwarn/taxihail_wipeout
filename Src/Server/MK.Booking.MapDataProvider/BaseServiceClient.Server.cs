using System;
using System.Net.Http;
using apcurium.MK.Common;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.MapDataProvider
{
    public class BaseServiceClient
    {
        public BaseServiceClient(IConnectivityService connectivityService)
        {
        }

        public HttpClient GetClient(string url = "")
        {
            var client = new HttpClient()
            {
                Timeout = new TimeSpan(0, 0, 2, 0, 0)
            };

            if (url.HasValueTrimmed())
            {
                client.BaseAddress = new Uri(url);
            }

            return client;
        }
    }
}
