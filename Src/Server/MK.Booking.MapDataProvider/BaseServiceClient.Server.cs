using System;
using ServiceStack.ServiceClient.Web;

namespace apcurium.MK.Booking.MapDataProvider
{
    public class BaseServiceClient
    {
        protected JsonServiceClient GetClient(string url = "")
        {
            return new JsonServiceClient(url)
            {
                Timeout = new TimeSpan(0, 0, 2, 0, 0)
            };
        }
    }
}
