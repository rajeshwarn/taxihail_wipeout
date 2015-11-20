#region


#endregion

using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [Route("/account/addresses", "GET")]
    public class Addresses
    {
    }
}