#region

using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment.Braintree
{
    [Authenticate]
    [Route("/payments/braintree/CommitPreauthorizedPaymentPayment", "POST")]
    public class CommitPreauthorizedPaymentBraintreeRequest : IReturn<CommitPreauthorizedPaymentResponse>
    {
        public string TransactionId { get; set; }
    }
}