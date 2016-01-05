using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment.Braintree
{
    [Authenticate]
    [Route("/payments/braintree/generateclienttoken", "GET")]
	public class GenerateClientTokenBraintreeRequest : IReturn<GenerateClientTokenResponse>
    {
		
    }
}
