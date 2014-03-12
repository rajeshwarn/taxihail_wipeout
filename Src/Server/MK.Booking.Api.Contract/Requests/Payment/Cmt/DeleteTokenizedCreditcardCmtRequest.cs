#region

using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using ServiceStack.ServiceHost;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment.Cmt
{
    [Authenticate]
    [Route("/payments/cmt/deleteToken/{CardToken}/", "DELETE")]
    public class DeleteTokenizedCreditcardCmtRequest : IReturn<DeleteTokenizedCreditcardResponse>
    {
        public string CardToken { get; set; }
    }
}