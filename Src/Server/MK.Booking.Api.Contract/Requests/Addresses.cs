#region


#endregion

using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [Route("/account/addresses", "GET")]
    public class Addresses
    {
    }
}