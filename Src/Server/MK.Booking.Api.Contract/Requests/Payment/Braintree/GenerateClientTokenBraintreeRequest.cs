using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment.Braintree
{
    [Authenticate]
    [Route("/payments/braintree/generateclienttoken", "GET")]
    public class GenerateClientTokenBraintreeRequest
    {
    }
}
