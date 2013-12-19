using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;

namespace apcurium.MK.Booking.Api.Contract.Requests.Braintree
{
    [Authenticate]
    [Route("/payments/braintree/CommitPreauthorizedPaymentPayment", "POST")]
    public class CommitPreauthorizedPaymentBraintreeRequest : IReturn<CommitPreauthorizedPaymentResponse>
    {
        public string TransactionId { get; set; }
    }
}
