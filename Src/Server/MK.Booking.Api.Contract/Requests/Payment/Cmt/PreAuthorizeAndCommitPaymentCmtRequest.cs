#region

using System;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment.Cmt
{
    [Authenticate]
    [Route("/payments/cmt/preAuthorizeAndCommitPayment", "POST")]
    public class PreAuthorizeAndCommitPaymentCmtRequest : IReturn<CommitPreauthorizedPaymentResponse>
    {
        public double Amount { get; set; }
        public double MeterAmount { get; set; }
        public double TipAmount { get; set; }
        public string CardToken { get; set; }
        public Guid OrderId { get; set; }
    }
}