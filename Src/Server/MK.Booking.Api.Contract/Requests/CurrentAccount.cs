#region

using apcurium.MK.Booking.Api.Contract.Http;
using apcurium.MK.Booking.Api.Contract.Resources;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface.ServiceModel;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [Route("/account", "GET")]
    public class CurrentAccount : BaseDto
    {
    }

    [NoCache]
    public class CurrentAccountResponse : Account, IHasResponseStatus
    {
        public CurrentAccountResponse()
        {
            ResponseStatus = new ResponseStatus();
        }

        public ResponseStatus ResponseStatus { get; set; }
    }
}