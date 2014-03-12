#region

using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment.Braintree
{
    [Authenticate]
    [Route("/payments/braintree/deleteToken/{CardToken}", "DELETE")]
    public class DeleteTokenizedCreditcardBraintreeRequest : IReturn<DeleteTokenizedCreditcardResponse>
    {
        public string CardToken { get; set; }
    }
}