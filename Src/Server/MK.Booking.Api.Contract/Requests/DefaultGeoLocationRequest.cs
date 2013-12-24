#region

using ServiceStack.ServiceHost;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/settings/defaultlocation", "GET")]
    public class DefaultGeoLocationRequest
    {
    }
}