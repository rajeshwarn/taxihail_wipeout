#region

using System;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment.Cmt
{
    [Authenticate]
    [Route("/payments/cmt/preAuthorizePayment", "POST")]
    public class PreAuthorizePaymentCmtRequest : IReturn<PreAuthorizePaymentResponse>
    {
        public double Tip { get; set; }
        public double Meter { get; set; }

        public double Amount { get; set; }

        public string CardToken { get; set; }

        public string OrderNumber { get; set; }
        public Guid OrderId { get; set; }
    }
}