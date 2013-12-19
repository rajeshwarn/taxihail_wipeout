using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;

namespace apcurium.MK.Booking.Api.Contract.Requests.Cmt
{
    [Authenticate]
    [Route("/payments/cmt/commitPreAuthorizePayment", "POST")]
    public class CommitPreauthorizedPaymentCmtRequest : IReturn<CommitPreauthorizedPaymentResponse>
    {
        public string TransactionId { get; set; }
    }
}
