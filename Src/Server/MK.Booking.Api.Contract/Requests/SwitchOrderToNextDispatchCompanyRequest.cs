using System;
using apcurium.MK.Common.Http;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RouteDescription("/accounts/orders/{OrderId}/switchDispatchCompany", "POST")]
    public class SwitchOrderToNextDispatchCompanyRequest : BaseDto
    {
        public Guid OrderId { get; set; }

        public string NextDispatchCompanyKey { get; set; }

        public string NextDispatchCompanyName { get; set; }
    }
}
