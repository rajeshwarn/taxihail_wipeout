#region

using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Http;
using apcurium.MK.Common.Http.Response;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RouteDescription("/accounts", "GET")]
    public class CurrentAccount : BaseDto
    {
    }

    public class CurrentAccountResponse : Account, IHasResponseStatus
    {
        public CurrentAccountResponse()
        {
            ResponseStatus = new ResponseStatus();
        }

        public ResponseStatus ResponseStatus { get; set; }
    }
}