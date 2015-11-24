
using System;
using System.Net.Http;
using ModernHttpClient;

namespace apcurium.MK.Booking.MapDataProvider
{
    public class BaseServiceClient
    {
        public HttpClient GetClient(string url)
        {
            var client = new HttpClient(new NativeMessageHandler())
            {
                Timeout = new TimeSpan(0, 0, 2, 0, 0),
#if DEBUG
                BaseAddress = new Uri(url)
#else
                BaseAddress = new Uri("");
#endif
            };

            return client;
        }
    }
}