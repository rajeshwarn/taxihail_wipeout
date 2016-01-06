using System;
using System.Net.Http;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common;

namespace apcurium.MK.Booking.MapDataProvider
{
    public class BaseServiceClient
    {
        private readonly IConnectivityService _connectivityService;

        public BaseServiceClient(IConnectivityService connectivityService)
        {
            _connectivityService = connectivityService;
        }

        public HttpClient GetClient(string url = "")
        {
            var client = new HttpClient(new CustomNativeMessageHandler(_connectivityService))
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