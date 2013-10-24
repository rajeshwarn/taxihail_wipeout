using System;
using ServiceStack.ServiceHost;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests.Cmt
{
    [Authenticate]
    [Route("/payments/cmt/preAuthorizePayment", "POST")]
    public class PreAuthorizePaymentCmtRequest : IReturn<PreAuthorizePaymentResponse>
    {
        public double Amount { get; set; }

        public string CardToken { get; set; }

        public string OrderNumber { get; set; }
        public Guid OrderId { get; set; }
    }
}
