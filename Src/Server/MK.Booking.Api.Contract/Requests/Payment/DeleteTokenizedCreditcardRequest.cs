using apcurium.MK.Common.Http;
using apcurium.MK.Common.Http.Response;
using apcurium.MK.Common.Resources;

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment
{
    [RouteDescription("/payments/deleteToken/{CardToken}", "DELETE")]
    public class DeleteTokenizedCreditcardRequest : IReturn<DeleteTokenizedCreditcardResponse>
    {
        public string CardToken { get; set; }
    }
}