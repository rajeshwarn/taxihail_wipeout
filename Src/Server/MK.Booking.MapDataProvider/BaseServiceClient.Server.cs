using System;
using apcurium.MK.Common;
using ServiceStack.ServiceClient.Web;

namespace apcurium.MK.Booking.MapDataProvider
{
    public class BaseServiceClient
    {
        public BaseServiceClient(IConnectivityService connectivityService)
        {
        }

        protected JsonServiceClient GetClient(string url = "")
        {
            return new JsonServiceClient(url)
            {
                Timeout = new TimeSpan(0, 0, 2, 0, 0)
            };
        }
    }
}
