using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment
{
    [Authenticate]
    [Route("/payments/deleteToken/{CardToken}", "DELETE")]
    public class DeleteTokenizedCreditcardRequest : IReturn<DeleteTokenizedCreditcardResponse>
    {
        public string CardToken { get; set; }
    }
}