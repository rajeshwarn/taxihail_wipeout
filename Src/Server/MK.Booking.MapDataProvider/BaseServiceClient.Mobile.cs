
using System;
using System.Net.Http;
using apcurium.MK.Common.Extensions;
using ModernHttpClient;

namespace apcurium.MK.Booking.MapDataProvider
{
    public class BaseServiceClient
    {
        public HttpClient GetClient(string url)
        {
            var client = new HttpClient(new NativeMessageHandler())
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