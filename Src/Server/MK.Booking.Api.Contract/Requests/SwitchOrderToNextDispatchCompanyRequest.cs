using System;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [Route("/account/orders/{OrderId}/switchDispatchCompany", "POST")]
    public class SwitchOrderToNextDispatchCompanyRequest : BaseDto
    {
        public Guid OrderId { get; set; }

        public string NextDispatchCompanyKey { get; set; }

        public string NextDispatchCompanyName { get; set; }
    }
}
