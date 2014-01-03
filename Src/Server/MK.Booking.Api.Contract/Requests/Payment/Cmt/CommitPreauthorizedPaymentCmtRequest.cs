#region

using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment.Cmt
{
    [Authenticate]
    [Route("/payments/cmt/commitPreAuthorizePayment", "POST")]
    public class CommitPreauthorizedPaymentCmtRequest : IReturn<CommitPreauthorizedPaymentResponse>
    {
        public string TransactionId { get; set; }
    }
}